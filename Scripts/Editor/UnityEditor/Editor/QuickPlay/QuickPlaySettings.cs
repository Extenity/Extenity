// QuickPlay shortcuts are not supported outside of Windows environment.

#if ExtenityQuickPlay && UNITY_EDITOR_WIN

using Extenity.ApplicationToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class QuickPlaySettings : SettingsProvider
	{
		#region Configuration

		class Styles
		{
			public static GUIContent TargetInterface = new GUIContent("Target Interface");
			public static GUIContent Shortcut = new GUIContent("Shortcut");
			public static GUIContent InvalidKey = new GUIContent("Invalid key");
		}

		#endregion

		#region Initialization

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			base.OnActivate(searchContext, rootElement);
		}

		private QuickPlaySettings(string path, SettingsScope scope)
			: base(path, scope)
		{
		}

		[SettingsProvider]
		public static SettingsProvider Create()
		{
			var provider = new QuickPlaySettings("Preferences/QuickPlay", SettingsScope.User);

			// Automatically extract all keywords from the Styles.
			provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
			return provider;
		}

		#endregion

		#region Deinitialization

		public override void OnDeactivate()
		{
			base.OnDeactivate();
		}

		#endregion

		#region GUI

		private static string KeysAsString;
		private static bool Invalid;

		private static VirtualKeyCode[] KeysToBeAssigned;

		//private static GUIStyle ShortcutKeyFieldStyle;

		public override void OnTitleBarGUI()
		{
		}

		public override void OnGUI(string searchContext)
		{
			EditorGUILayoutTools.DrawHeader(Styles.TargetInterface);

			//if (ShortcutKeyFieldStyle == null)
			//{
			//	ShortcutKeyFieldStyle = new GUIStyle(EditorStyles.textField);
			//}

			if (string.IsNullOrEmpty(KeysAsString))
			{
				KeysAsString = QuickPlay.ShortcutKeyCombinationToString(QuickPlay.ShortcutKeyCombination);
			}

			// Predefined keys
			{
				GUILayout.BeginHorizontal();
				foreach (var predefinedShortcutKeyCombination in QuickPlay.PredefinedShortcutKeyCombinations)
				{
					var text = QuickPlay.ShortcutKeyCombinationToString(predefinedShortcutKeyCombination);
					if (GUILayout.Button(text))
					{
						AssignKeys(text);
					}
				}
				GUILayout.EndHorizontal();
			}

			//var newKeysAsString = EditorGUILayout.DelayedTextField("Shortcut", KeysAsString, ShortcutKeyFieldStyle);
			var newKeysAsString = EditorGUILayout.DelayedTextField(Styles.Shortcut, KeysAsString);
			if (newKeysAsString != KeysAsString)
			{
				Invalid = false;
				try
				{
					AssignKeys(newKeysAsString);
				}
				catch
				{
					Invalid = true;
				}
			}

			if (Invalid)
			{
				GUI.color = Color.yellow;
				GUILayout.Label(Styles.InvalidKey);
			}
		}

		#endregion

		#region Configure

		private static void AssignKeys(string newKeysAsString)
		{
			KeysToBeAssigned = QuickPlay.ShortcutKeyCombinationFromString(newKeysAsString);
			KeysAsString = newKeysAsString;
			// Delay the settings assignment so it will prevent detecting shortcut keys as pressed while typing.
			EditorApplication.delayCall -= AssignSettingsDelayed;
			EditorApplication.delayCall += AssignSettingsDelayed;
		}

		private static void AssignSettingsDelayed()
		{
			QuickPlay.ShortcutKeyCombination = KeysToBeAssigned;
			KeysToBeAssigned = null;
		}

		#endregion
	}

}

#endif
