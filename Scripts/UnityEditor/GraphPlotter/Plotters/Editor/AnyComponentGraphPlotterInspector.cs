using System;
using System.Collections.Generic;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(AnyComponentGraphPlotter))]
	public class AnyComponentGraphPlotterInspector : ExtenityEditorBase<AnyComponentGraphPlotter>
	{
		private int componentIndex = -1;

		private List<string> addField = new List<string>();

		private TypeInspectors inspectors;

		public AnyComponentGraphPlotterInspector() : base()
		{
			inspectors = TypeInspectors.Instance;
		}

		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			var components = new List<Component>(Me.GetComponents<Component>());

			// find index of (previously) selected component.
			if (Me.component != null)
			{
				componentIndex = -1;

				for (int i = 0; i < components.Count; i++)
				{
					if (components[i] == Me.component)
					{
						componentIndex = i;
						break;
					}
				}
			}

			// populate list of component names. 
			var componentsStrings = new string[components.Count];
			for (int i = 0; i < components.Count; i++)
			{
				componentsStrings[i] = i + ". " + components[i].GetType().Name;
			}

			EditorGUILayout.Space();

			// component selection.
			componentIndex = EditorGUILayout.Popup("Component", componentIndex, componentsStrings);

			// value axis mode.
			float newMin, newMax;
			Utils.AxisSettings(Me, ref Me.mode, Me.min, out newMin, Me.max, out newMax);

			if (newMin != Me.min)
			{
				Me.min = newMin;

				if (Me.monitor != null)
				{
					Me.monitor.Min = Me.min;
				}
			}

			if (newMax != Me.max)
			{
				Me.max = newMax;

				if (Me.monitor != null)
				{
					Me.monitor.Max = Me.max;
				}
			}

			// Sample Time
			var newSampleTime = (SampleTime)EditorGUILayout.EnumPopup("Sample time", Me.SampleTime);
			if (newSampleTime != Me.SampleTime)
			{
				Undo.RecordObject(target, "Change sample time");
				Me.SampleTime = newSampleTime;
			}

			EditorGUILayout.Space();

			if (componentIndex > -1 && components.Count > 0)
			{
				Me.component = components[componentIndex];
			}
			else
			{
				Me.component = null;
			}

			if (Me.component != null)
			{
				EditorGUILayout.LabelField("Fields");

				EditorGUILayout.BeginHorizontal("Box");
				EditorGUILayout.BeginVertical();

				for (int j = 0; j < Me.channelFields.Count; j++)
				{
					var field = Me.channelFields[j];

					EditorGUILayout.BeginHorizontal();

					GUILayout.Label(field.FieldName + " : " + TypeInspectors.GetReadableName(field.fieldTypeName));
					var newColor = EditorGUILayout.ColorField(field.color, GUILayout.Width(40));
					if (newColor != field.color)
					{
						Undo.RecordObject(Me, "Change field color");
						field.color = newColor;
					}

					EditorGUILayout.Space();

					if (GUILayout.Button("remove", GUILayout.Width(60)))
					{
						Undo.RecordObject(Me, "Remove field");
						Me.channelFields.RemoveAt(j);
						break;
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();
				}

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				var instanceType = Me.component.GetType();

				int level = 0;

				while (instanceType != null && !inspectors.IsSampleType(instanceType))
				{
					var inspector = inspectors.GetTypeInspector(instanceType);
					var fieldNameStrings = inspector.FieldNameStrings;
					var fields = inspector.Fields;

					int selectedIndex = -1;

					if (level < addField.Count)
					{
						var previouslySelectedFieldName = addField[level];
						selectedIndex = Array.FindIndex(fields, field => (field.name == previouslySelectedFieldName));
					}

					selectedIndex = EditorGUILayout.Popup(level == 0 ? "Add field" : "...", selectedIndex, fieldNameStrings);
					if (selectedIndex > -1)
					{
						string fieldName = fields[selectedIndex].name;
						if (level < addField.Count)
						{
							addField[level] = fieldName;
						}
						else
						{
							addField.Add(fieldName);
						}

						instanceType = fields[selectedIndex].type;

						level++;
					}
					else
					{
						instanceType = null;
					}
				}

				if (instanceType != null)
				{
					var field = new AnyComponentGraphPlotter.ChannelField();
					field.field = addField.ToArray();
					field.fieldTypeName = instanceType.FullName;
					field.color = PlotColors.AllColors[Me.channelFields.Count % PlotColors.AllColors.Length];

					Undo.RecordObject(Me, "Add field");
					Me.channelFields.Add(field);

					addField.RemoveAt(addField.Count - 1);
				}
				else
				{
					if (level > 1)
					{
						EditorGUILayout.HelpBox("Using many levels of reflection can have a significant impact on runtime-time performance.", MessageType.Warning);
					}

				}
			}

			Utils.OpenButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateMonitors();
		}
	}

}