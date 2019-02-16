using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(UISimpleAnimation))]
	public class UISimpleAnimationInspector : ExtenityEditorBase<UISimpleAnimation>
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

			var animatedTransformAvailable = Me.AnimatedTransform;
			var anchorAAvailable = Me.AnchorA;
			var anchorBAvailable = Me.AnchorB;
			var needAnchors = !anchorAAvailable || !anchorBAvailable;

			GUILayout.Space(20f);

			EditorGUILayoutTools.DrawHeader("Tools");
			GUILayout.BeginHorizontal();
			if (GUILayoutTools.Button("Get Or Create Anchors", needAnchors, BigButtonHeight))
			{
				CreateAnchorsIfNeeded(true);
			}
			if (GUILayoutTools.Button("Create Anchors", needAnchors, BigButtonHeight))
			{
				CreateAnchorsIfNeeded(false);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(20f);

			EditorGUILayoutTools.DrawHeader("Animation Controls");
			EditorGUI.BeginDisabledGroup(!isPlaying);
			IsImmediate = GUILayout.Toggle(IsImmediate, ImmediateToggleContent);
			EditorGUI.EndDisabledGroup();
			var immediateOrEditor = IsImmediate || !isPlaying;
			GUILayout.BeginHorizontal();
			if (GUILayoutTools.Button("Animate To A", anchorAAvailable && animatedTransformAvailable, BigButtonHeight))
			{
				if (!isPlaying)
					Undo.RecordObject(Me.AnimatedTransform, "Animate To A");
				Me.AnimateToA(immediateOrEditor);
			}
			if (GUILayoutTools.Button("Animate To B", anchorBAvailable && animatedTransformAvailable, BigButtonHeight))
			{
				if (!isPlaying)
					Undo.RecordObject(Me.AnimatedTransform, "Animate To B");
				Me.AnimateToB(immediateOrEditor);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10f);
		}

		#region Create Anchors

		private void CreateAnchorsIfNeeded(bool firstTryToGetAlreadyExistingAnchorsWithSameName)
		{
			Undo.RecordObject(Me, "Create Animation Anchors");
			var animated = Me.transform.GetComponent<RectTransform>();
			var parent = Me.transform.parent.GetComponent<RectTransform>();
			CreateAnchor(ref Me.AnchorB, parent, animated, "B", firstTryToGetAlreadyExistingAnchorsWithSameName); // B first, for correct sibling order.
			CreateAnchor(ref Me.AnchorA, parent, animated, "A", firstTryToGetAlreadyExistingAnchorsWithSameName);
		}

		private void CreateAnchor(ref RectTransform anchor, RectTransform parent, RectTransform animatedObject, string namePostfix, bool firstTryToGetAlreadyExistingAnchorsWithSameName)
		{
			if (!anchor)
			{
				var anchorName = animatedObject.gameObject.name + "-Anchor" + namePostfix;

				// First, try to find a gameobject with exact name. Maybe the user 
				if (firstTryToGetAlreadyExistingAnchorsWithSameName)
				{
					var alreadyExisting = parent.Find(anchorName);
					if (alreadyExisting)
					{
						anchor = alreadyExisting.GetComponent<RectTransform>();
					}
				}

				// Create new anchor if needed
				if (!anchor)
				{
					var go = new GameObject(anchorName, typeof(RectTransform));
					Undo.RegisterCreatedObjectUndo(go, "Create Animation Anchor Object");
					go.transform.SetParent(parent, false);
					go.transform.SetSiblingIndex(animatedObject.GetSiblingIndex() + 1);
					anchor = go.GetComponent<RectTransform>();
				}

				EditorUtility.CopySerialized(animatedObject, anchor);
			}
		}

		#endregion
	}

}
