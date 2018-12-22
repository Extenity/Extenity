using Extenity.ApplicationToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class QuickPlayPreferences
	{
		#region GUI

		private static string KeysAsString;
		private static bool Invalid;

		private static VirtualKeyCode[] KeysToBeAssigned;

		//private static GUIStyle ShortcutKeyFieldStyle;

		[PreferenceItem(nameof(QuickPlay))]
		public static void PreferencesGUI()
		{
			EditorGUILayoutTools.DrawHeader("Target Interface");

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
			var newKeysAsString = EditorGUILayout.DelayedTextField("Shortcut", KeysAsString);
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
				GUILayout.Label("Invalid key");
			}
		}

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
