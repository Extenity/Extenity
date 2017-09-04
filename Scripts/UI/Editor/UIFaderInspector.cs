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
		}

		protected override void OnDisableDerived()
		{
		}

		private GUIContent CachedLabel1 = new GUIContent("Use Initial Alpha As Fade In Alpha");
		private GUIContent CachedLabel2 = new GUIContent("Use Initial Alpha As Fade Out Alpha");
		private GUIContent Cached_FadeIn = new GUIContent("Fade In");
		private GUIContent Cached_FadeOut = new GUIContent("Fade Out");

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.PropertyField(GetProperty("CanvasGroup"));
			if (Me.CanvasGroup == null)
			{
				EditorGUILayout.HelpBox("A Canvas Group must be assigned.", MessageType.Warning);
			}
			EditorGUILayout.PropertyField(GetProperty("InitialState"));

			EditorGUILayout.PropertyField(GetProperty("GetFadeInConfigurationFromInitialValue"), CachedLabel1);
			if (Me.GetFadeInConfigurationFromInitialValue)
			{
				EditorGUILayout.PropertyField(GetProperty("FadeInAlpha"));
			}
			EditorGUILayout.PropertyField(GetProperty("GetFadeOutConfigurationFromInitialValue"), CachedLabel2);
			if (Me.GetFadeOutConfigurationFromInitialValue)
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
