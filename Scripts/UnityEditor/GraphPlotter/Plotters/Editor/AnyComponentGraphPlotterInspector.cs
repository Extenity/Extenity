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
		private int SelectedComponentIndex = -1;
		private List<string> NewChannelSelectionLevels = new List<string>();


		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			var components = Me.GetComponents<Component>();

			// Find selected component index
			if (Me.Component != null)
			{
				SelectedComponentIndex = -1;

				for (int i = 0; i < components.Length; i++)
				{
					if (components[i] == Me.Component)
					{
						SelectedComponentIndex = i;
						break;
					}
				}
			}

			// Create popup names list
			var componentPopupNames = new string[components.Length];
			for (int i = 0; i < components.Length; i++)
			{
				componentPopupNames[i] = i + ". " + components[i].GetType().Name;
			}

			EditorGUILayout.Space();

			// Component selection
			SelectedComponentIndex = EditorGUILayout.Popup("Component", SelectedComponentIndex, componentPopupNames);

			// Axis range configuration
			CommonEditor.DrawAxisRangeConfiguration(Me, Me.Graph, ref Me.Range);

			// Sample Time
			var newSampleTime = (SampleTime)EditorGUILayout.EnumPopup("Sample time", Me.SampleTime);
			if (newSampleTime != Me.SampleTime)
			{
				Undo.RecordObject(target, "Change sample time");
				Me.SampleTime = newSampleTime;
			}

			EditorGUILayout.Space();

			if (SelectedComponentIndex > -1 && components.Length > 0)
			{
				Me.Component = components[SelectedComponentIndex];
			}
			else
			{
				Me.Component = null;
			}

			if (Me.Component != null)
			{
				EditorGUILayout.LabelField("Channels");

				EditorGUILayout.BeginHorizontal("Box");
				EditorGUILayout.BeginVertical();

				for (int i = 0; i < Me.ChannelFields.Count; i++)
				{
					var field = Me.ChannelFields[i];

					EditorGUILayout.BeginHorizontal();

					var newColor = EditorGUILayout.ColorField(field.Color, GUILayout.Width(40));
					if (newColor != field.Color)
					{
						Undo.RecordObject(Me, "Change channel color");
						field.Color = newColor;
					}

					GUILayout.Label(field.FieldName);

					EditorGUILayout.Space();

					if (GUILayout.Button("x", GUILayout.Width(20)))
					{
						Undo.RecordObject(Me, "Remove channel");
						Me.ChannelFields.RemoveAt(i);
						break;
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();
				}

				EditorGUILayout.Space();

				var instanceType = Me.Component.GetType();

				var level = 0;

				while (instanceType != null && !TypeInspectors.Instance.IsKnownType(instanceType))
				{
					var inspector = TypeInspectors.Instance.GetTypeInspector(instanceType);
					var fieldNameStrings = inspector.FieldNameStrings;
					var fields = inspector.Fields;

					var selectedIndex = -1;

					if (level < NewChannelSelectionLevels.Count)
					{
						var previouslySelectedFieldName = NewChannelSelectionLevels[level];
						selectedIndex = Array.FindIndex(fields, field => (field.name == previouslySelectedFieldName));
					}

					selectedIndex = EditorGUILayout.Popup(level == 0 ? "New channel" : "...", selectedIndex, fieldNameStrings);
					if (selectedIndex > -1)
					{
						var fieldName = fields[selectedIndex].name;
						if (level < NewChannelSelectionLevels.Count)
						{
							NewChannelSelectionLevels[level] = fieldName;
						}
						else
						{
							NewChannelSelectionLevels.Add(fieldName);
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
						Field = NewChannelSelectionLevels.ToArray(),
						Color = PlotColors.AllColors[Me.ChannelFields.Count % PlotColors.AllColors.Length]
					};

					Undo.RecordObject(Me, "New channel");
					Me.ChannelFields.Add(field);

					NewChannelSelectionLevels.RemoveAt(NewChannelSelectionLevels.Count - 1);
				}
				else
				{
					if (level > 1)
					{
						EditorGUILayout.HelpBox("Using many levels of reflection may have significant impact on performance.", MessageType.Warning);
					}

				}

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateGraph();
		}
	}

}