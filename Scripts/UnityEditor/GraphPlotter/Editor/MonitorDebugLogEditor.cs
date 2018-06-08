using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(MonitorDebugLog))]
	public class MonitorDebugLogEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			MonitorDebugLog monitorDebugLog = target as MonitorDebugLog;

			EditorGUILayout.Space();

			string newFilterPrefix = EditorGUILayout.TextField("Filter prefix", monitorDebugLog.filterPrefix);
			if (newFilterPrefix != monitorDebugLog.filterPrefix)
			{
				Undo.RecordObject(target, "Change filter prefix");
				monitorDebugLog.filterPrefix = newFilterPrefix;
			}

			Utils.OpenButton(monitorDebugLog.gameObject);

			monitorDebugLog.UpdateMonitors();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

}
