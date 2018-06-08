using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(MonitorComponent))]
	public class MonitorComponentEditor : UnityEditor.Editor
	{
		private int componentIndex = -1;

		private List<string> addField = new List<string>();

		private TypeInspectors inspectors;

		public MonitorComponentEditor() : base()
		{
			inspectors = TypeInspectors.Instance;
		}

		public override void OnInspectorGUI()
		{
			var monitorComponent = target as MonitorComponent;
			var go = monitorComponent.gameObject;

			var components = new List<Component>(go.GetComponents<Component>());

			// find index of (previously) selected component.
			if (monitorComponent.component != null)
			{
				componentIndex = -1;

				for (int i = 0; i < components.Count; i++)
				{
					if (components[i] == monitorComponent.component)
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
			Utils.AxisSettings(monitorComponent, ref monitorComponent.mode, monitorComponent.min, out newMin, monitorComponent.max, out newMax);

			if (newMin != monitorComponent.min)
			{
				monitorComponent.min = newMin;

				if (monitorComponent.monitor != null)
				{
					monitorComponent.monitor.Min = monitorComponent.min;
				}
			}

			if (newMax != monitorComponent.max)
			{
				monitorComponent.max = newMax;

				if (monitorComponent.monitor != null)
				{
					monitorComponent.monitor.Max = monitorComponent.max;
				}
			}

			// Sample Mode
			var newSampleMode = (MonitorComponent.SampleMode)EditorGUILayout.EnumPopup("Sample time", monitorComponent.sampleMode);
			if (newSampleMode != monitorComponent.sampleMode)
			{
				Undo.RecordObject(target, "Change sample time");
				monitorComponent.sampleMode = newSampleMode;
			}

			EditorGUILayout.Space();

			if (componentIndex > -1 && components.Count > 0)
			{
				monitorComponent.component = components[componentIndex];
			}
			else
			{
				monitorComponent.component = null;
			}

			if (monitorComponent.component != null)
			{
				EditorGUILayout.LabelField("Fields");

				EditorGUILayout.BeginHorizontal("Box");
				EditorGUILayout.BeginVertical();

				for (int j = 0; j < monitorComponent.monitorInputFields.Count; j++)
				{
					var field = monitorComponent.monitorInputFields[j];

					EditorGUILayout.BeginHorizontal();

					GUILayout.Label(field.FieldName + " : " + TypeInspectors.GetReadableName(field.fieldTypeName));
					var newColor = EditorGUILayout.ColorField(field.color, GUILayout.Width(40));
					if (newColor != field.color)
					{
						Undo.RecordObject(monitorComponent, "Change field color");
						field.color = newColor;
					}

					EditorGUILayout.Space();

					if (GUILayout.Button("remove", GUILayout.Width(60)))
					{
						Undo.RecordObject(monitorComponent, "Remove field");
						monitorComponent.monitorInputFields.RemoveAt(j);
						break;
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();
				}

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				var instanceType = monitorComponent.component.GetType();

				int level = 0;

				while (instanceType != null && !inspectors.IsSampleType(instanceType))
				{
					var inspector = inspectors.GetTypeInspector(instanceType);
					var fieldNameStrings = inspector.FieldNameStrings;
					var fields = inspector.Fields;

					int selectedIndex = -1;

					if (level < addField.Count)
					{
						string previouslySelectedFieldName = addField[level];
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
					var field = new MonitorComponent.MonitorInputField();
					field.field = addField.ToArray();
					field.fieldTypeName = instanceType.FullName;
					field.color = PlotColors.AllColors[monitorComponent.monitorInputFields.Count % PlotColors.AllColors.Length];

					Undo.RecordObject(monitorComponent, "Add field");
					monitorComponent.monitorInputFields.Add(field);

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

			Utils.OpenButton(go);

			monitorComponent.UpdateMonitors();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

}