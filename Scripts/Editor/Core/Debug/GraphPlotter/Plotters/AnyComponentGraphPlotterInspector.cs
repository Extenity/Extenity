using System;
using System.Collections.Generic;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(AnyComponentGraphPlotter))]
	public class AnyComponentGraphPlotterInspector : ExtenityEditorBase<AnyComponentGraphPlotter>
	{
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
			EditorGUILayout.Space();

			// Component link
			EditorGUILayout.PropertyField(GetProperty("Component"));

			// Axis range configuration
			CommonEditor.DrawVerticalRangeConfiguration(Me, Me.Graph, ref Me.Range);

			// Sample Time
			EditorGUILayout.PropertyField(GetProperty("SampleTime"));

			EditorGUILayout.Space();

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

				while (instanceType != null && !TypeInspectors.IsKnownType(instanceType))
				{
					var inspector = TypeInspectors.GetTypeInspector(instanceType);
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
				Me.SetupGraph();
		}
	}

}
