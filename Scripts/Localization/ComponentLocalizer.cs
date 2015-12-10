using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Extenity
{

	public class ComponentLocalizer : MonoBehaviour
	{
		#region Initialization

		private void Awake()
		{
			InitializeText();

			RegisterToLocalization();
		}

		private void Start()
		{
			UpdateText();
		}

		private void OnDestroy()
		{
			DeregisterFromLocalization();
		}

		#endregion

		#region Register

		private void RegisterToLocalization()
		{
			Localization.OnLanguageChanged += OnLanguageChanged;
		}

		private void DeregisterFromLocalization()
		{
			Localization.OnLanguageChanged -= OnLanguageChanged;
		}

		#endregion

		#region Text

		public string Key;
		//public string KeyPrefix;

		#endregion

		#region Component - UI.Text

		private Text Text;

		private void InitializeText()
		{
			Text = GetComponent<Text>();
		}

		#endregion

		#region Update Text

		private void OnLanguageChanged(string newLanguage)
		{
			UpdateText();
		}

		public void UpdateText()
		{
			var text = Localization.GetText(Key);
			//var text = Localization.GetText(KeyPrefix + Key);

			if (Text != null)
			{
				Text.text = text;
			}
		}

		#endregion
	}

}
