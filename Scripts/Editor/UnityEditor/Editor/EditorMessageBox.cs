using System;
using System.Threading.Tasks;
using Extenity.IMGUIToolbox.Editor;
using Extenity.ScreenToolbox;
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
		private UserInputField[] UserInputFields;

		private bool NeedsFocus;

		private Vector2 ScrollPosition;

		private enum FinalizationState
		{
			StillPrompting,
			FinalizedWithOkButton,
			FinalizedWithCancelButtonOrWindowClosed,
		}
		private FinalizationState Finalization;

		public static async Task Show(Vector2Int size, string title, string message, string okButton, string cancelButton, Action onContinue, Action onCancel = null)
		{
			var rect = ScreenTools.GetCenteredRect(size.x, size.y);
			await Show(rect, title, message, okButton, cancelButton, onContinue, onCancel);
		}

		public static async Task Show(Vector2Int size, string title, string message, UserInputField[] userInputFields, string okButton, string cancelButton, Action onContinue, Action onCancel = null)
		{
			var rect = ScreenTools.GetCenteredRect(size.x, size.y);
			await Show(rect, title, message, userInputFields, okButton, cancelButton, onContinue, onCancel);
		}

		public static async Task Show(Rect position, string title, string message, string okButton, string cancelButton, Action onContinue, Action onCancel = null)
		{
			await Show(position, title, message, null, okButton, cancelButton, onContinue, onCancel);
		}

		public static async Task Show(Rect position, string title, string message, UserInputField[] userInputFields, string okButton, string cancelButton, Action onContinue, Action onCancel = null)
		{
			var popup = ScriptableObject.CreateInstance<EditorMessageBox>();
			popup.titleContent = new GUIContent(title);
			popup.position = position;
			popup.Message = message;
			popup.OkButton = okButton;
			popup.CancelButton = cancelButton;
			popup.UserInputFields = userInputFields;
			popup.NeedsFocus = true;
			popup.ShowModalUtility();

			while (true)
			{
				switch (popup.Finalization)
				{
					case FinalizationState.StillPrompting:
						break; // Wait for user to enter input to message box.

					case FinalizationState.FinalizedWithOkButton:
					{
						if (onContinue != null)
						{
							onContinue();
						}

						return;
					}

					case FinalizationState.FinalizedWithCancelButtonOrWindowClosed:
					{
						// If there is no cancel button, act as if the Okay button pressed.
						if (string.IsNullOrEmpty(popup.CancelButton))
						{
							if (onContinue != null)
							{
								onContinue();
							}
						}
						else
						{
							if (onCancel != null)
							{
								onCancel();
							}
						}

						return;
					}

					default:
						throw new ArgumentOutOfRangeException();
				}

				await Task.Delay(1);
			}
		}

		void OnDestroy()
		{
			if (Finalization == FinalizationState.StillPrompting)
			{
				Finalization = FinalizationState.FinalizedWithCancelButtonOrWindowClosed;
			}
		}

		void OnGUI()
		{
			bool okButtonAllowed = true;

			var escapePressed = Event.current.Equals(Event.KeyboardEvent("escape"));
			if (escapePressed)
			{
				Finalization = FinalizationState.FinalizedWithCancelButtonOrWindowClosed;
				Close();
				return;
			}

			var enterPressed = Event.current.Equals(Event.KeyboardEvent("return"));

			GUILayout.BeginHorizontal();
			GUILayout.Space(10f);
			{
				GUILayout.BeginVertical();
				GUILayout.Space(20f);
				{
					ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
					EditorGUILayout.LabelField(Message, EditorStylesTools.RichWordWrappedLabel);
					EditorGUILayout.EndScrollView();

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
							// Turns out, Unity started to support Copy&Paste in TextFields. But keep these codes here
							// for future needs.
							// if (Event.current.type == EventType.KeyUp && Event.current.modifiers == EventModifiers.Control && Event.current.keyCode == KeyCode.V)
							// {
							// 	userInputField.Value += Clipboard.GetClipboardText();
							// 	Repaint();
							// }

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
						Finalization = FinalizationState.FinalizedWithOkButton;
						Close();
					}
					EditorGUI.EndDisabledGroup();
					if (!string.IsNullOrEmpty(CancelButton))
					{
						if (GUILayout.Button(CancelButton, GUILayout.Height(30f)))
						{
							Finalization = FinalizationState.FinalizedWithCancelButtonOrWindowClosed;
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
