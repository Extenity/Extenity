// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonitorComponents 
{
	[CustomEditor(typeof(MonitorComponent))]
	public class MonitorComponentEditor : Editor
	{
		private int componentIndex = -1;

		private List<string> addField = new List<string>();

		private Color[] colors = new Color[] { Colors.green, Colors.blue, Colors.purple, Colors.yellow, Colors.red };

		private TypeInspectors inspectors;

		public MonitorComponentEditor() : base()
		{
			inspectors = TypeInspectors.Instance;
		}

		public override void OnInspectorGUI()
		{
			MonitorComponent monitorComponent = target as MonitorComponent;
			GameObject go = monitorComponent.gameObject;

			List<Component> components = new List<Component>(go.GetComponents<Component>());

			components.RemoveAll((Component c) => {
				string _namespace = c.GetType().Namespace;
				return (_namespace == "UnityEngine") || (_namespace == "MonitorComponents");
			});

			// find index of (previously) selected component.
			if(monitorComponent.component != null)
			{
				componentIndex = -1;

				for(int i = 0; i < components.Count; i++)
				{
					if(components[i] == monitorComponent.component)
					{
						componentIndex = i;
						break;
					}
				}
			}

			// populate list of component names. 
			string[] componentsStrings = new string[components.Count];
			for(int i = 0; i < components.Count; i++)
			{
				componentsStrings[i] = components[i].GetType().Name;
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

			EditorGUILayout.Space();

			if (componentIndex > -1 && components.Count > 0)
			{
				monitorComponent.component = components[componentIndex];
			}
			else
			{
				monitorComponent.component = null;
			}

			if(monitorComponent.component != null)
			{
				EditorGUILayout.LabelField("Fields");

				EditorGUILayout.BeginHorizontal ("Box");
				EditorGUILayout.BeginVertical();

				for(int j = 0; j < monitorComponent.monitorInputFields.Count; j++)
				{
					var field = monitorComponent.monitorInputFields[j];

					EditorGUILayout.BeginHorizontal();
		
					GUILayout.Label(field.FieldName + " : " + TypeInspectors.GetReadableName(field.fieldTypeName));
					Color newColor = EditorGUILayout.ColorField(field.color, GUILayout.Width(40));
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

				Type instanceType = monitorComponent.component.GetType();

				int level = 0;

				while (instanceType != null && !inspectors.IsSampleType(instanceType))
				{
					TypeInspectors.ITypeInspector inspector = inspectors.GetTypeInspector(instanceType);
					string[] fieldNameStrings = inspector.FieldNameStrings;
					TypeInspectors.Field[] fields = inspector.Fields;

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
					field.color = colors[monitorComponent.monitorInputFields.Count % colors.Length];
					
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
	            EditorUtility.SetDirty (target);
	    }
	}
}