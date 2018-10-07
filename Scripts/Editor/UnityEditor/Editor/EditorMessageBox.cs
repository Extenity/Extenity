using System;
using Extenity.ApplicationToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class UserInputField
	{
		public string Header;
		public string Value;
		public bool AllowEmpty;

		public UserInputField(string header, bool allowEmpty = false)
			: this(header, null, allowEmpty)
		{
		}

		public UserInputField(string header, string initialValue, bool allowEmpty = false)
		{
			Header = header;
			Value = initialValue;
			AllowEmpty = allowEmpty;
		}
	}

	public class EditorMessageBox : EditorWindow
	{
		private string Message;
		private string OkButton;
		private string CancelButton;
		private Action OnContinue;
		private Action OnCancel;
		private UserInputField[] UserInputFields;

		private bool NeedsFocus;

		private bool InternalIsCancelled = true;

		public static void Show(Rect position, string title, string message, string okButton, string cancelButton, Action onContinue, Action onCancel = null)
		{
			Show(position, title, message, null, okButton, cancelButton, onContinue, onCancel);
		}

		public static void Show(Rect position, string title, string message, UserInputField[] userInputFields, string okButton, string cancelButton, Action onContinue, Action onCancel = null)
		{
			var popup = ScriptableObject.CreateInstance<EditorMessageBox>();
			popup.titleContent = new GUIContent(title);
			popup.position = position;
			popup.Message = message;
			popup.OkButton = okButton;
			popup.CancelButton = cancelButton;
			popup.OnContinue += onContinue;
			popup.OnCancel += onCancel;
			popup.UserInputFields = userInputFields;
			popup.NeedsFocus = true;
			popup.ShowAuxWindow();
		}

		void OnDestroy()
		{
			if (InternalIsCancelled)
			{
				if (OnCancel != null)
					OnCancel();
			}
		}

		void OnGUI()
		{
			bool okButtonAllowed = true;

			var escapePressed = Event.current.Equals(Event.KeyboardEvent("escape"));
			if (escapePressed)
			{
				Close();

				// If there is no cancel button, act as if the Okay button pressed.
				if (string.IsNullOrEmpty(CancelButton))
				{
					if (OnContinue != null)
						OnContinue();
				}
				return;
			}

			var enterPressed = Event.current.Equals(Event.KeyboardEvent("return"));

			GUILayout.BeginHorizontal();
			GUILayout.Space(10f);
			{
				GUILayout.BeginVertical();
				GUILayout.Space(20f);
				{
					EditorGUILayout.LabelField(Message, EditorStyles.wordWrappedLabel);

					GUILayout.FlexibleSpace();

					if (UserInputFields != null && UserInputFields.Length > 0)
					{
						for (var i = 0; i < UserInputFields.Length; i++)
						{
							var userInputField = UserInputFields[i];
							GUILayout.BeginHorizontal();
							GUILayout.Space(80f);
							GUILayout.Label(userInputField.Header);
							GUILayout.Space(80f);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							GUILayout.Space(80f);

							// CTRL + V
							if (Event.current.type == EventType.KeyUp && Event.current.modifiers == EventModifiers.Control && Event.current.keyCode == KeyCode.V)
							{
								userInputField.Value += Clipboard.GetClipboardText();
								Repaint();
							}

							GUI.SetNextControlName("UserInputField-" + i);
							userInputField.Value = GUILayout.TextField(userInputField.Value);
							GUILayout.Space(80f);
							GUILayout.EndHorizontal();

							GUILayout.Space(20f);

							if (!userInputField.AllowEmpty && string.IsNullOrEmpty(userInputField.Value))
							{
								okButtonAllowed = false;
							}
						}
					}

					GUILayout.BeginHorizontal();
					EditorGUI.BeginDisabledGroup(!okButtonAllowed);
					if (GUILayout.Button(OkButton, GUILayout.Height(30f)) || (enterPressed && okButtonAllowed))
					{
						InternalIsCancelled = false;
						Close();

						if (OnContinue != null)
							OnContinue();
					}
					EditorGUI.EndDisabledGroup();
					if (!string.IsNullOrEmpty(CancelButton))
					{
						if (GUILayout.Button(CancelButton, GUILayout.Height(30f)))
						{
							Close();
						}
					}
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
					if (UserInputFields != null && UserInputFields.Length > 0)
					{
						GUI.FocusControl("UserInputField-0");
					}
					NeedsFocus = false;
				}
			}

		}
	}

}
