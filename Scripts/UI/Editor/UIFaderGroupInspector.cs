using Extenity.IMGUIToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(UIFaderGroup))]
	public class UIFaderGroupInspector : ExtenityEditorBase<UIFaderGroup>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		private readonly GUILayoutOption[] Cached_FaderButtonLayout = { GUILayoutTools.ExpandWidth, GUILayout.Height(30) };
		private readonly GUILayoutOption[] Cached_FadeButtonLayout = { GUILayout.Width(80), GUILayout.Height(30) };
		private readonly GUIContent Cached_FadeIn = new GUIContent("Fade In");
		private readonly GUIContent Cached_FadeOut = new GUIContent("Fade Out");
		private readonly GUIContent Cached_FadeInAll = new GUIContent("Fade In All");
		private readonly GUIContent Cached_FadeOutAll = new GUIContent("Fade Out All");

		protected override void OnBeforeDefaultInspectorGUI()
		{
			if (Me.Faders != null)
			{
				EditorGUI.BeginDisabledGroup(Me.Faders.Count == 0);
				GUILayout.Space(15f);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(Cached_FadeInAll, BigButtonHeight))
				{
					Me.FadeInAllImmediate();
				}
				if (GUILayout.Button(Cached_FadeOutAll, BigButtonHeight))
				{
					Me.FadeOutAllImmediate();
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(15f);
				EditorGUI.EndDisabledGroup();

				GUILayout.BeginVertical();
				for (var i = 0; i < Me.Faders.Count; i++)
				{
					var fader = Me.Faders[i];
					GUILayout.BeginHorizontal();
					if (GUILayoutTools.Button(fader == null ? "[Not assigned]" : fader.name, fader != null, Cached_FaderButtonLayout))
					{
						Selection.activeGameObject = fader.gameObject;
					}
					EditorGUI.BeginDisabledGroup(fader == null);
					{
						if (GUILayout.Button(Cached_FadeIn, Cached_FadeButtonLayout))
						{
							Me.FadeInImmediate(fader);
						}
						if (GUILayout.Button(Cached_FadeOut, Cached_FadeButtonLayout))
						{
							fader.FadeOutImmediate();
						}
					}
					GUILayout.EndHorizontal();
					EditorGUI.EndDisabledGroup();
				}
				GUILayout.EndVertical();
			}
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
