using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(DebugLogGraphPlotter))]
	public class DebugLogGraphPlotterInspector : ExtenityEditorBase<DebugLogGraphPlotter>
	{
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

			string newFilterPrefix = EditorGUILayout.TextField("Filter prefix", Me.filterPrefix);
			if (newFilterPrefix != Me.filterPrefix)
			{
				Undo.RecordObject(target, "Change filter prefix");
				Me.filterPrefix = newFilterPrefix;
			}

			Utils.OpenButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateMonitors();
		}
	}

}
