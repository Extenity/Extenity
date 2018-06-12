using Extenity.IMGUIToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	public class CommonEditor
	{
		public static void DrawAxisRangeConfiguration(Object undoObject, Graph graph, ref ValueAxisRangeConfiguration range)
		{
			var newSizing = (ValueAxisSizing)EditorGUILayout.EnumPopup("Axis sizing ", range.Sizing);

			if (newSizing != range.Sizing)
			{
				Undo.RecordObject(undoObject, "Changed axis sizing");
				if (newSizing == ValueAxisSizing.Fixed &&
					float.IsPositiveInfinity(range.Min) &&
					float.IsNegativeInfinity(range.Max))
				{
					range.Min = -1f;
					range.Max = 1f;
				}

				range.Sizing = newSizing;
			}

			float newMinimum;
			float newMaximum;

			if (newSizing == ValueAxisSizing.Adaptive)
			{
				newMinimum = float.PositiveInfinity;
				newMaximum = float.NegativeInfinity;
			}
			else
			{
				newMinimum = EditorGUILayout.FloatField("Axis min", range.Min);
				newMaximum = EditorGUILayout.FloatField("Axis max", range.Max);

				if (newMinimum != range.Min)
				{
					Undo.RecordObject(undoObject, "Changed axis range minimum");
				}

				if (newMaximum != range.Max)
				{
					Undo.RecordObject(undoObject, "Changed axis range maximum");
				}
			}

			if (newMinimum != range.Min)
			{
				range.Min = newMinimum;

				if (graph != null)
				{
					graph.Range.Min = newMinimum;
				}
			}

			if (newMaximum != range.Max)
			{
				range.Max = newMaximum;

				if (graph != null)
				{
					graph.Range.Max = newMaximum;
				}
			}
		}

		public static void OpenGraphPlotterButton(GameObject gameObject)
		{
			if (!gameObject)
				return;

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();

			if (GUILayout.Button("Open Graph Plotter", GUILayout.ExpandWidth(false)))
			{
				var window = EditorWindow.GetWindow<MonitorsEditorWindow>();
				window.RemoveNotification();
				window.SetFilter(null);
			}
			if (GUILayoutTools.Button("Filtered", GraphPlotters.IsAnyGraphForObjectExists(gameObject), GUILayout.ExpandWidth(false)))
			{
				var window = EditorWindow.GetWindow<MonitorsEditorWindow>();
				window.RemoveNotification();
				if (window.SetFilter(gameObject))
				{
					window.ShowNotification(new GUIContent($"Filtering only for '{gameObject.name}'"));
				}
				else
				{
					window.ShowNotification(new GUIContent($"No graph to filter for '{gameObject.name}'!"));
				}
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
	}

}