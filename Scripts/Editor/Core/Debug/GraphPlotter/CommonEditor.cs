using Extenity.IMGUIToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
{

	public class CommonEditor
	{
		public static void DrawAxisRangeConfiguration(Object undoObject, Graph graph, ref ValueAxisRangeConfiguration range)
		{
			var newSizing = (ValueAxisSizing)EditorGUILayout.EnumPopup("Axis Sizing", range.Sizing);

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

			if (newSizing == ValueAxisSizing.Adaptive ||
			    newSizing == ValueAxisSizing.ZeroBasedAdaptive)
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
				newMin = EditorGUI.FloatField(new Rect(position.x + labelWidth, position.y, inputFieldWidth, height), range.Min);
				GUI.Label(new Rect(position.x + halfWidth, position.y, labelWidth, height), "Max:");
				newMax = EditorGUI.FloatField(new Rect(position.x + halfWidth + labelWidth, position.y, inputFieldWidth, height), range.Max);
				EditorGUILayout.EndHorizontal();

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