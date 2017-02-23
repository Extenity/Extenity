using System;
using System.Text;
using Extenity.DataTypes;
using UnityEngine;
using UnityEditor;

namespace Extenity.EditorUtilities
{

	public class FoolproofActionPopup : EditorWindow
	{
		private string Message;
		private string Key;
		private string OkButton;
		private string CancelButton;
		private Action OnContinue;
		private string EnteredKey;

		private bool NeedsFocus;

		public static void Show(string title, string message, string key, string okButton, string cancelButton, Action onContinue)
		{
			var popup = ScriptableObject.CreateInstance<FoolproofActionPopup>();
			popup.titleContent = new GUIContent(title);
			popup.position = new Rect(100, 100, 400, 250);
			popup.Message = message;
			popup.Key = key;
			popup.OkButton = okButton;
			popup.CancelButton = cancelButton;
			popup.OnContinue += onContinue;
			popup.EnteredKey = "";
			popup.NeedsFocus = true;
			popup.ShowAuxWindow();
		}

		void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(10f);
			{
				GUILayout.BeginVertical();
				GUILayout.Space(20f);
				{
					EditorGUILayout.LabelField(Message, EditorStyles.wordWrappedLabel);

					GUILayout.FlexibleSpace();

					GUILayout.BeginHorizontal();
					GUILayout.Space(80f);
					GUILayout.Label("Passphrase");
					GUILayout.Space(80f);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Space(80f);
					var enterPressed = Event.current.Equals(Event.KeyboardEvent("return"));
					GUI.SetNextControlName("PassphraseField");
					EnteredKey = GUILayout.PasswordField(EnteredKey, '*');
					GUILayout.Space(80f);
					GUILayout.EndHorizontal();

					GUILayout.Space(20f);

					GUILayout.BeginHorizontal();
					if (GUILayout.Button(OkButton, GUILayout.Height(30f)) || enterPressed)
					{
						var sha = new System.Security.Cryptography.SHA512Managed();
						var result = sha.ComputeHash(Encoding.UTF8.GetBytes(EnteredKey)).ToHexStringCombined();
						if (Key.Equals(result, StringComparison.InvariantCultureIgnoreCase))
						{
							Close();
							Repaint();

							if (OnContinue != null)
								OnContinue();
						}
						else
						{
							EnteredKey = "";
						}
					}
					if (GUILayout.Button(CancelButton, GUILayout.Height(30f)))
						Close();
					GUILayout.EndHorizontal();
				}
				GUILayout.Space(10f);
				GUILayout.EndVertical();
			}
			GUILayout.Space(10f);
			GUILayout.EndHorizontal();

			if (NeedsFocus)
			{
				if (Event.current.type == EventType.Layout)
				{
					GUI.FocusControl("PassphraseField");
					NeedsFocus = false;
				}
			}

		}
	}

}
