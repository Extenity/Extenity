// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using UnityEditor;

namespace MonitorComponents 
{
	public class Utils 
	{
		public static void AxisSettings(Object undoObject, ref ValueAxisMode mode, float inMin, out float outMin, float inMax, out float outMax)
		{
			ValueAxisMode newMode = (ValueAxisMode) EditorGUILayout.EnumPopup("Axis mode ", mode);

			if (newMode != mode)
			{
				Undo.RecordObject(undoObject, "Changed axis mode");
				if (newMode == ValueAxisMode.Fixed &&
					inMin == float.PositiveInfinity &&
					inMax == float.NegativeInfinity)
				{
					inMin = -1f;
					inMax = 1f;
				}

				mode = newMode;
			}

			if (newMode == ValueAxisMode.Adaptive)
			{
				outMin = float.PositiveInfinity;
				outMax = float.NegativeInfinity;	
			}
			else
			{
				outMin = EditorGUILayout.FloatField("Axis min", inMin);
				outMax = EditorGUILayout.FloatField("Axis max", inMax);

				if (outMin != inMin)
				{
					Undo.RecordObject(undoObject, "Changed axis min");
				}

				if (outMax != inMax)
				{
					Undo.RecordObject(undoObject, "Changed axis min");
				}

			}
		}

		public static void OpenButton(GameObject gameObject)
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			if(GUILayout.Button("Open monitors...", GUILayout.Width(110)))
			{
				MonitorsEditorWindow window = (MonitorsEditorWindow) EditorWindow.GetWindow(typeof(MonitorsEditorWindow));
				window.Filter = gameObject;
				window.ShowNotification(new GUIContent("Monitors for " + gameObject.name));
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}
	}
}