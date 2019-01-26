using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class MessageBoxDialog : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			InitializeUIElements();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			Close();
		}

		#endregion

		#region Blackout

		// TODO: Replace this with BlackoutUI. But first, move BlackoutUI inside Extenity. Then make it count the Show and Hide requests and act accordingly, i.e. hide only if all the Show requests canceled out with a respective Hide request.
		public GameObject Blackout;

		#endregion

		public TextMeshProUGUI TitleText;
		public TextMeshProUGUI MessageText;
		public TextMeshProUGUI UserInputTitle;
		public InputField UserInputField;
		public Button OkayButton;
		public TextMeshProUGUI OkayButtonLabel;
		public Button CancelButton;
		public TextMeshProUGUI CancelButtonLabel;

		private Action OnClickedOkay;
		private Action<string> OnClickedOkayWithUserInput;
		private Action OnClickedCancel;

		private bool AllowEmptyUserInput;

		private void InitializeUIElements()
		{
			if (UserInputField)
			{
				UserInputField.onValueChanged.AddListener(
					value =>
					{
						RefreshOkayButtonAvailability();
					}
				);
			}

			if (OkayButton)
			{
				OkayButton.onClick.AddListener(() =>
				{
					if (OnClickedOkay != null)
						OnClickedOkay();
					if (OnClickedOkayWithUserInput != null)
						OnClickedOkayWithUserInput(UserInputField.text.Trim());

					Close();
				});
			}

			if (CancelButton)
			{
				CancelButton.onClick.AddListener(() =>
				{
					if (OnClickedCancel != null)
						OnClickedCancel();

					Close();
				});
			}
		}

		private void RefreshOkayButtonAvailability()
		{
			if (UserInputField)
			{
				if (UserInputField.gameObject.activeSelf && !AllowEmptyUserInput)
				{
					OkayButton.interactable = !string.IsNullOrEmpty(UserInputField.text.Trim());
				}
				else
				{
					OkayButton.interactable = true;
				}
			}
			else
			{
				OkayButton.interactable = true;
			}
		}

		/// <summary>
		/// Show the dialog without buttons. If it's modal, the user can't pass the screen and
		/// there is no turning back from this state of the application. The only way will be to
		/// force shutdown.
		/// </summary>
		public void ShowDeadEnd(string title, string message)
		{
			Show(title, message, null, null, null, null);
		}

		public void Show(string title, string message, string okayButtonText, string cancelButtonText = null, Action onClickedOkay = null, Action onClickedCancel = null)
		{
			Debug.Assert(TitleText);
			Debug.Assert(MessageText);

			if (OkayButton)
				OkayButton.gameObject.SetActive(!string.IsNullOrEmpty(okayButtonText));
			if (CancelButton)
				CancelButton.gameObject.SetActive(!string.IsNullOrEmpty(cancelButtonText));
			if (OkayButtonLabel)
				OkayButtonLabel.text = okayButtonText;
			if (CancelButtonLabel)
				CancelButtonLabel.text = cancelButtonText;
			OnClickedOkay = onClickedOkay;
			OnClickedCancel = onClickedCancel;

			TitleText.text = title;
			MessageText.text = message;
			if (UserInputTitle)
				UserInputTitle.gameObject.SetActive(false);
			if (UserInputField)
				UserInputField.gameObject.SetActive(false);

			RefreshOkayButtonAvailability();
			gameObject.SetActive(true);
			Blackout.SetActive(true);
		}

		public void ShowWithUserInput(string title, string message, string okayButtonText, string cancelButtonText, string userInputTitle, string userInputDefaultValue, bool allowEmptyUserInput, Action<string> onClickedOkay, Action onClickedCancel)
		{
			Debug.Assert(TitleText);
			Debug.Assert(MessageText);
			Debug.Assert(UserInputTitle);
			Debug.Assert(UserInputField);
			Debug.Assert(OkayButton);

			OkayButton.gameObject.SetActive(!string.IsNullOrEmpty(okayButtonText));
			if (CancelButton)
				CancelButton.gameObject.SetActive(!string.IsNullOrEmpty(cancelButtonText));
			if (OkayButtonLabel)
				OkayButtonLabel.text = okayButtonText;
			if (CancelButtonLabel)
				CancelButtonLabel.text = cancelButtonText;
			OnClickedOkayWithUserInput = onClickedOkay;
			OnClickedCancel = onClickedCancel;

			TitleText.text = title;
			MessageText.text = message;
			UserInputTitle.gameObject.SetActive(!string.IsNullOrEmpty(userInputTitle));
			UserInputTitle.text = userInputTitle;
			UserInputField.gameObject.SetActive(true);
			UserInputField.text = userInputDefaultValue.Trim();

			AllowEmptyUserInput = allowEmptyUserInput;

			RefreshOkayButtonAvailability();
			gameObject.SetActive(true);
			Blackout.SetActive(true);
		}

		public void Close()
		{
			if (TitleText)
				TitleText.text = "";
			if (MessageText)
				MessageText.text = "";
			if (UserInputTitle)
				UserInputTitle.text = "";
			if (UserInputField)
				UserInputField.text = "";
			OnClickedOkay = null;
			OnClickedOkayWithUserInput = null;
			OnClickedCancel = null;

			gameObject.SetActive(false);
			Blackout.SetActive(false);
		}

		#region Draw Order

		// TODO: Ability to put the dialog in front of every other canvas that's been created.
		// TODO: If possible, tell Unity to draw Sort Order inspector field as disabled for the Canvas component.
		// TODO: Make the Sort Order of Canvas always set to -1000 in editor.

		#endregion
	}

}
