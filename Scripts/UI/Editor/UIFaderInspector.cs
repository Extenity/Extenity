using Extenity.GameObjectToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(UIFader))]
	public class UIFaderInspector : ExtenityEditorBase<UIFader>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;

			AutoAssignLinksIfNeeded();
		}

		protected override void OnDisableDerived()
		{
		}

		private void AutoAssignLinksIfNeeded()
		{
			var canvasGroupNeeds = Me.CanvasGroup == null;
			var canvasNeeds = Me.Canvas == null;

			if (canvasGroupNeeds || canvasNeeds)
			{
				CanvasGroup foundCanvasGroup = null;
				Canvas foundCanvas = null;
				if (canvasGroupNeeds)
					foundCanvasGroup = Me.GetComponent<CanvasGroup>();
				if (canvasNeeds)
					foundCanvas = Me.GetComponent<Canvas>();
				if (foundCanvasGroup || foundCanvas)
				{
					Undo.RecordObject(Me, "Auto assign UI Fader links");
					if (canvasGroupNeeds)
						Me.CanvasGroup = foundCanvasGroup;
					if (canvasNeeds)
						Me.Canvas = foundCanvas;
					Undo.FlushUndoRecordObjects();
				}
			}
		}

		private readonly GUIContent CachedLabel1 = new GUIContent("Use Initial Alpha As Fade In Alpha");
		private readonly GUIContent CachedLabel2 = new GUIContent("Use Initial Alpha As Fade Out Alpha");
		private readonly GUIContent Cached_FadeIn = new GUIContent("Fade In");
		private readonly GUIContent Cached_FadeOut = new GUIContent("Fade Out");
		private readonly GUIContent Cached_Add = new GUIContent("Add");
		private readonly GUILayoutOption[] Cached_AddButtonLayout = { GUILayout.Width(40) };

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.PropertyField(GetProperty("CanvasGroup"));
			if (Me.CanvasGroup == null)
			{
				if (GUILayout.Button(Cached_Add, Cached_AddButtonLayout))
				{
					Me.CanvasGroup = Undo.AddComponent<CanvasGroup>(Me.gameObject);
					Me.CanvasGroup.MoveComponentAbove(Me);
				}
				EditorGUILayout.HelpBox("A Canvas Group must be assigned.", MessageType.Warning);
			}
			EditorGUI.BeginDisabledGroup(GetProperty("CanvasGroup").objectReferenceValue == null);
			EditorGUILayout.PropertyField(GetProperty("Canvas"));
			if (Me.Canvas == null)
			{
				if (GUILayout.Button(Cached_Add, Cached_AddButtonLayout))
				{
					Me.Canvas = Undo.AddComponent<Canvas>(Me.gameObject);
					Me.Canvas.MoveComponentAbove(Me.CanvasGroup);
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.PropertyField(GetProperty("InitialState"));
			EditorGUILayout.PropertyField(GetProperty("Interactable"));
			EditorGUILayout.PropertyField(GetProperty("BlocksRaycasts"));

			EditorGUILayout.PropertyField(GetProperty("GetFadeInConfigurationFromInitialValue"), CachedLabel1);
			if (!Me.GetFadeInConfigurationFromInitialValue)
			{
				EditorGUILayout.PropertyField(GetProperty("FadeInAlpha"));
			}
			EditorGUILayout.PropertyField(GetProperty("GetFadeOutConfigurationFromInitialValue"), CachedLabel2);
			if (!Me.GetFadeOutConfigurationFromInitialValue)
			{
				EditorGUILayout.PropertyField(GetProperty("FadeOutAlpha"));
			}

			EditorGUILayout.PropertyField(GetProperty("FadeInDuration"));
			EditorGUILayout.PropertyField(GetProperty("FadeOutDuration"));
			EditorGUILayout.PropertyField(GetProperty("FadeInDelay"));
			EditorGUILayout.PropertyField(GetProperty("FadeOutDelay"));

			EditorGUILayout.PropertyField(GetProperty("DEBUG_ShowFadeMessages"));

			GUILayout.Space(30f);

			var fadeButtonsDisabled = !EditorApplication.isPlaying;
			if (fadeButtonsDisabled)
			{
				EditorGUILayout.HelpBox("Press play to test.", MessageType.Info);
			}
			fadeButtonsDisabled |= Me.CanvasGroup == null;
			GUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(fadeButtonsDisabled);
			if (GUILayout.Button(Cached_FadeIn, BigButtonHeight))
			{
				Me.FadeIn();
			}
			if (GUILayout.Button(Cached_FadeOut, BigButtonHeight))
			{
				Me.FadeOut();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
		}
	}

}
