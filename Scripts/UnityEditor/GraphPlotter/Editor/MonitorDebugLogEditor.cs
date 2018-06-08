// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MonitorComponents 
{
	[CustomEditor(typeof(MonitorDebugLog))]
	public class MonitorDebugLogEditor : Editor
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
	            EditorUtility.SetDirty (target);
		}
	}
}
