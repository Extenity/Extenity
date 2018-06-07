using Extenity.IMGUIToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox
{

	[CustomEditor(typeof(UISimpleAnimationOrchestrator))]
	public class UISimpleAnimationOrchestratorInspector : ExtenityEditorBase<UISimpleAnimationOrchestrator>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		private static readonly GUIContent ImmediateToggleContent = new GUIContent("Immediate", "Animation will not be played and the object will be placed at the end position immediately if this is selected. Only available in Play mode. Behaves as if it's set to immediate in Edit mode.");

		private bool IsImmediate;

		protected override void OnAfterDefaultInspectorGUI()
		{
			var isPlaying = EditorApplication.isPlaying;

			GUILayout.Space(20f);

			EditorGUILayoutTools.DrawHeader("Animation Controls");
			EditorGUI.BeginDisabledGroup(!isPlaying);
			IsImmediate = GUILayout.Toggle(IsImmediate, ImmediateToggleContent);
			EditorGUI.EndDisabledGroup();
			var immediateOrEditor = IsImmediate || !isPlaying;
			GUILayout.BeginHorizontal();
			if (GUILayoutTools.Button("Animate To A", true, BigButtonHeight))
			{
				if (!isPlaying)
					UndoRecordAllAnimatedObjects("Animate To A");
				Me.AnimateToA(immediateOrEditor);
			}
			if (GUILayoutTools.Button("Animate To B", true, BigButtonHeight))
			{
				if (!isPlaying)
					UndoRecordAllAnimatedObjects("Animate To B");
				Me.AnimateToB(immediateOrEditor);
			}
			GUILayout.EndHorizontal();
		}

		private void UndoRecordAllAnimatedObjects(string name)
		{
			foreach (var animation in Me.Animations)
			{
				if (animation && animation.AnimatedTransform)
				{
					Undo.RecordObject(animation.AnimatedTransform, name);
				}
			}
		}
	}

}
