using System;
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

		public GameObject Blackout;

		#endregion

		public Text TitleText;
		public Text MessageText;
		public Text UserInputTitle;
		public InputField UserInputField;
		public Button OkayButton;
		public Button CancelButton;

		private Action OnClickedOkay;
		private Action<string> OnClickedOkayWithUserInput;
		private Action OnClickedCancel;

		private bool AllowEmptyUserInput;

		private void InitializeUIElements()
		{
			UserInputField.onValueChanged.AddListener(
				value =>
				{
					RefreshOkayButtonAvailability();
				}
			);

			OkayButton.onClick.AddListener(() =>
			{
				if (OnClickedOkay != null)
					OnClickedOkay();
				if (OnClickedOkayWithUserInput != null)
					OnClickedOkayWithUserInput(UserInputField.text.Trim());

				Close();
			});
			CancelButton.onClick.AddListener(() =>
			{
				if (OnClickedCancel != null)
					OnClickedCancel();

				Close();
			});
		}

		private void RefreshOkayButtonAvailability()
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

		public void Show(string title, string message, string okayButtonText, string cancelButtonText = null, Action onClickedOkay = null, Action onClickedCancel = null)
		{
			Debug.Assert(TitleText);
			Debug.Assert(MessageText);

			OkayButton.gameObject.SetActive(!string.IsNullOrEmpty(okayButtonText));
			CancelButton.gameObject.SetActive(!string.IsNullOrEmpty(cancelButtonText));
			OkayButton.GetComponentInChildren<Text>().text = okayButtonText;
			CancelButton.GetComponentInChildren<Text>().text = cancelButtonText;
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

			OkayButton.gameObject.SetActive(!string.IsNullOrEmpty(okayButtonText));
			CancelButton.gameObject.SetActive(!string.IsNullOrEmpty(cancelButtonText));
			OkayButton.GetComponentInChildren<Text>().text = okayButtonText;
			CancelButton.GetComponentInChildren<Text>().text = cancelButtonText;
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
			TitleText.text = "";
			MessageText.text = "";
			UserInputTitle.text = "";
			UserInputField.text = "";
			OnClickedOkay = null;
			OnClickedOkayWithUserInput = null;
			OnClickedCancel = null;

			gameObject.SetActive(false);
			Blackout.SetActive(false);
		}
	}

}
