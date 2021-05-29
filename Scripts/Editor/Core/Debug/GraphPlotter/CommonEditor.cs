using Extenity.IMGUIToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
{

	public class CommonEditor
	{
		public static void DrawVerticalRangeConfiguration(Object undoObject, Graph graph, ref VerticalRange verticalRange)
		{
			var newSizing = (VerticalSizing)EditorGUILayout.EnumPopup("Axis Sizing", verticalRange.Sizing);

			if (newSizing != verticalRange.Sizing)
			{
				Undo.RecordObject(undoObject, "Changed axis sizing");
				if (newSizing == VerticalSizing.Fixed &&
					float.IsPositiveInfinity(verticalRange.Min) &&
					float.IsNegativeInfinity(verticalRange.Max))
				{
					verticalRange.Min = -1f;
					verticalRange.Max = 1f;
				}

				verticalRange.Sizing = newSizing;
			}

			float newMin;
			float newMax;

			if (newSizing == VerticalSizing.Adaptive ||
			    newSizing == VerticalSizing.ZeroBasedAdaptive)
			{
				newMin = float.PositiveInfinity;
				newMax = float.NegativeInfinity;
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Axis Range");
				var position = EditorGUILayout.GetControlRect(false, 20f);
				const float labelWidth = 40f;
				var halfWidth = position.width * 0.5f;
				var inputFieldWidth = Mathf.Max(0f, halfWidth - labelWidth);
				var height = position.height;
				GUI.Label(new Rect(position.x, position.y, labelWidth, height), "Min:");
				newMin = EditorGUI.FloatField(new Rect(position.x + labelWidth, position.y, inputFieldWidth, height), verticalRange.Min);
				GUI.Label(new Rect(position.x + halfWidth, position.y, labelWidth, height), "Max:");
				newMax = EditorGUI.FloatField(new Rect(position.x + halfWidth + labelWidth, position.y, inputFieldWidth, height), verticalRange.Max);
				EditorGUILayout.EndHorizontal();

				if (newMin != verticalRange.Min)
				{
					Undo.RecordObject(undoObject, "Changed axis range minimum");
				}

				if (newMax != verticalRange.Max)
				{
					Undo.RecordObject(undoObject, "Changed axis range maximum");
				}
			}

			if (verticalRange.Min != newMin)
			{
				verticalRange.Min = newMin;

				if (graph != null)
				{
					graph.Range.Min = newMin;
				}
			}

			if (verticalRange.Max != newMax)
			{
				verticalRange.Max = newMax;

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
				window.SetContextFilter(null);
			}
			if (GUILayoutTools.Button("Filtered", Graphs.IsAnyGraphForContextExists(gameObject), GUILayout.ExpandWidth(false)))
			{
				var window = EditorWindow.GetWindow<GraphPlotterWindow>();
				window.RemoveNotification();
				window.SetContextFilter(gameObject);
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
	}

}
