using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
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

			EditorGUILayoutTools.DrawHeader("Tools");
			GUILayout.BeginHorizontal();
			if (GUILayoutTools.Button("Add Child Animators", true, BigButtonHeight))
			{
				AddChildAnimations();
			}
			GUILayout.EndHorizontal();

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

			GUILayout.Space(10f);
		}

		private void AddChildAnimations()
		{
			var childAnimations = Me.transform.GetComponentsInChildren<UISimpleAnimation>();
			foreach (var childAnimation in childAnimations)
			{
				if (Me.Animations.All(entry => entry.Animation != childAnimation)) // Prevent adding same animation more than once
				{
					Undo.RecordObject(Me, "Add Child Animation");
					Me.Animations.Add(new UISimpleAnimationOrchestrator.Entry(childAnimation, false), out Me.Animations);
				}
			}
		}

		private void UndoRecordAllAnimatedObjects(string name)
		{
			foreach (var animation in Me.Animations)
			{
				if (animation.Animation && animation.Animation.AnimatedTransform)
				{
					Undo.RecordObject(animation.Animation.AnimatedTransform, name);
				}
			}
		}
	}

}
