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

			float newMin;
			float newMax;

			if (newSizing == ValueAxisSizing.Adaptive)
			{
				newMin = float.PositiveInfinity;
				newMax = float.NegativeInfinity;
			}
			else
			{
				newMin = EditorGUILayout.FloatField("Axis min", range.Min);
				newMax = EditorGUILayout.FloatField("Axis max", range.Max);

				if (newMin != range.Min)
				{
					Undo.RecordObject(undoObject, "Changed axis range minimum");
				}

				if (newMax != range.Max)
				{
					Undo.RecordObject(undoObject, "Changed axis range maximum");
				}
			}

			if (range.Min != newMin)
			{
				range.Min = newMin;

				if (graph != null)
				{
					graph.Range.Min = newMin;
				}
			}

			if (range.Max != newMax)
			{
				range.Max = newMax;

				if (graph != null)
				{
					graph.Range.Max = newMax;
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
				var window = EditorWindow.GetWindow<GraphPlotterWindow>();
				window.RemoveNotification();
				window.SetFilter(null);
			}
			if (GUILayoutTools.Button("Filtered", Graphs.IsAnyGraphForObjectExists(gameObject), GUILayout.ExpandWidth(false)))
			{
				var window = EditorWindow.GetWindow<GraphPlotterWindow>();
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