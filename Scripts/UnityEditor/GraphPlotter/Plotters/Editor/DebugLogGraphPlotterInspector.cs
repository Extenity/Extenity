using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(DebugLogGraphPlotter))]
	public class DebugLogGraphPlotterInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var Me = target as DebugLogGraphPlotter;

			EditorGUILayout.Space();

			string newFilterPrefix = EditorGUILayout.TextField("Filter prefix", Me.filterPrefix);
			if (newFilterPrefix != Me.filterPrefix)
			{
				Undo.RecordObject(target, "Change filter prefix");
				Me.filterPrefix = newFilterPrefix;
			}

			Utils.OpenButton(Me.gameObject);

			Me.UpdateMonitors();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

}
