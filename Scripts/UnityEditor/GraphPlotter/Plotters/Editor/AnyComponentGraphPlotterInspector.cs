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
			if (Me.Component != null)
			{
				componentIndex = -1;

				for (int i = 0; i < components.Count; i++)
				{
					if (components[i] == Me.Component)
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
			CommonEditor.DrawAxisRangeConfiguration(Me, Me.Graph, ref Me.Range);

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
				Me.Component = components[componentIndex];
			}
			else
			{
				Me.Component = null;
			}

			if (Me.Component != null)
			{
				EditorGUILayout.LabelField("Fields");

				EditorGUILayout.BeginHorizontal("Box");
				EditorGUILayout.BeginVertical();

				for (int j = 0; j < Me.ChannelFields.Count; j++)
				{
					var field = Me.ChannelFields[j];

					EditorGUILayout.BeginHorizontal();

					GUILayout.Label(field.FieldName + " : " + TypeInspectors.GetReadableName(field.FieldTypeName));
					var newColor = EditorGUILayout.ColorField(field.Color, GUILayout.Width(40));
					if (newColor != field.Color)
					{
						Undo.RecordObject(Me, "Change field color");
						field.Color = newColor;
					}

					EditorGUILayout.Space();

					if (GUILayout.Button("remove", GUILayout.Width(60)))
					{
						Undo.RecordObject(Me, "Remove field");
						Me.ChannelFields.RemoveAt(j);
						break;
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();
				}

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				var instanceType = Me.Component.GetType();

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
					var field = new AnyComponentGraphPlotter.ChannelField
					{
						Field = addField.ToArray(),
						FieldTypeName = instanceType.FullName,
						Color = PlotColors.AllColors[Me.ChannelFields.Count % PlotColors.AllColors.Length]
					};

					Undo.RecordObject(Me, "Add field");
					Me.ChannelFields.Add(field);

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

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateGraph();
		}
	}

}