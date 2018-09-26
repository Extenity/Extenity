#if BeyondAudioUsesWwiseAudio

using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.BeyondAudio.Effects.Editor
{

	[CustomEditor(typeof(MotorSound))]
	public class MotorSoundInspector : ExtenityEditorBase<MotorSound>
	{
		private bool IsDisplayingClipDetails;

		protected override void OnEnableDerived()
		{
			AutoRepaintInspectorPeriod = 0.05f;
			IsAutoRepaintInspectorEnabled = true;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnBeforeDefaultInspectorGUI()
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayoutTools.ProgressBar("Load", Me.Load);
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(10f);
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}

#endif
