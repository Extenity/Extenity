using Extenity.GameObjectToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

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

		private void MakeSureCanvasGroupIsNotInteractableToo()
		{
			if (Me.CanvasGroup)
			{
				if (!Me.Interactable)
				{
					Me.CanvasGroup.interactable = false;
				}
				if (!Me.BlocksRaycasts)
				{
					Me.CanvasGroup.blocksRaycasts = false;
				}
			}
		}

		private void AutoAssignLinksIfNeeded()
		{
			var canvasGroupNeeds = Me.CanvasGroup == null;
			var canvasNeeds = Me.Canvas == null;
			var animationOrchestratorNeeds = Me.TriggeredAnimationOrchestrator == null;

			if (canvasGroupNeeds || canvasNeeds || animationOrchestratorNeeds)
			{
				CanvasGroup foundCanvasGroup = null;
				Canvas foundCanvas = null;
				UISimpleAnimationOrchestrator foundAnimationOrchestrator = null;
				if (canvasGroupNeeds)
					foundCanvasGroup = Me.GetComponent<CanvasGroup>();
				if (canvasNeeds)
					foundCanvas = Me.GetComponent<Canvas>();
				if (animationOrchestratorNeeds)
					foundAnimationOrchestrator = Me.GetComponent<UISimpleAnimationOrchestrator>();
				if (foundCanvasGroup || foundCanvas || foundAnimationOrchestrator)
				{
					Undo.RecordObject(Me, "Auto assign UI Fader links");
					if (canvasGroupNeeds)
						Me.CanvasGroup = foundCanvasGroup;
					if (canvasNeeds)
						Me.Canvas = foundCanvas;
					if (animationOrchestratorNeeds)
						Me.TriggeredAnimationOrchestrator = foundAnimationOrchestrator;
					Undo.FlushUndoRecordObjects();
				}
			}
		}

		private readonly GUIContent CachedLabel1 = new GUIContent("Use Initial Alpha As Fade In Alpha");
		private readonly GUIContent CachedLabel2 = new GUIContent("Use Initial Alpha As Fade Out Alpha");
		private readonly GUIContent Cached_FadeIn = new GUIContent("Fade In");
		private readonly GUIContent Cached_FadeOut = new GUIContent("Fade Out");
		private readonly GUIContent Cached_Add = new GUIContent("Add");
		private readonly GUIContent Cached_Remove = new GUIContent("Remove");
		private readonly GUILayoutOption[] Cached_AddButtonLayout = { GUILayout.Width(60), GUILayout.Height(17) };
		private readonly GUILayoutOption[] Cached_RemoveButtonLayout = { GUILayout.Width(60), GUILayout.Height(17) };

		protected override void OnAfterDefaultInspectorGUI()
		{
			// CanvasGroup
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(GetProperty("CanvasGroup"));
			if (Me.CanvasGroup == null)
			{
				if (GUILayout.Button(Cached_Add, Cached_AddButtonLayout))
				{
					Me.CanvasGroup = Undo.AddComponent<CanvasGroup>(Me.gameObject);
					Me.CanvasGroup.MoveComponentAbove(Me);
				}
			}
			GUILayout.EndHorizontal();
			if (Me.CanvasGroup == null)
			{
				EditorGUILayout.HelpBox("A Canvas Group must be assigned.", MessageType.Warning);
			}

			// Canvas
			EditorGUI.BeginDisabledGroup(!Me.CanvasGroup);
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(GetProperty("Canvas"));
			if (Me.Canvas == null)
			{
				if (GUILayout.Button(Cached_Add, Cached_AddButtonLayout))
				{
					Me.Canvas = Undo.AddComponent<Canvas>(Me.gameObject);
					Me.Canvas.MoveComponentAbove(Me.CanvasGroup);
				}
			}
			GUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.PropertyField(GetProperty("InitialState"));
			EditorGUILayout.PropertyField(GetProperty("Interactable"));
			MakeSureCanvasGroupIsNotInteractableToo();

			// GraphicRaycaster
			if (Me.Canvas)
			{
				var graphicRaycaster = Me.transform.GetComponent<GraphicRaycaster>();
				if (Me.Interactable && !graphicRaycaster)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.HelpBox("A Graphic Raycaster will be needed for interactable canvasses.", MessageType.Warning);
					if (GUILayout.Button(Cached_Add, Cached_AddButtonLayout))
					{
						var component = Undo.AddComponent<GraphicRaycaster>(Me.gameObject);
						component.MoveComponentBelow(Me.Canvas);
					}
					GUILayout.EndHorizontal();
				}
				else if (!Me.Interactable && graphicRaycaster)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.HelpBox("Graphic Raycaster will NOT be needed for interactable canvasses.", MessageType.Warning);
					if (GUILayout.Button(Cached_Remove, Cached_RemoveButtonLayout))
					{
						Undo.DestroyObjectImmediate(graphicRaycaster);
					}
					GUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.PropertyField(GetProperty("BlocksRaycasts"));

			// TriggeredAnimationOrchestrator
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(GetProperty("TriggeredAnimationOrchestrator"));
			if (Me.TriggeredAnimationOrchestrator == null)
			{
				if (GUILayout.Button(Cached_Add, Cached_AddButtonLayout))
				{
					Me.TriggeredAnimationOrchestrator = Undo.AddComponent<UISimpleAnimationOrchestrator>(Me.gameObject);
					Me.TriggeredAnimationOrchestrator.MoveComponentBelow(Me);
				}
			}
			GUILayout.EndHorizontal();

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

			GUILayout.Space(10f);
		}
	}

}
