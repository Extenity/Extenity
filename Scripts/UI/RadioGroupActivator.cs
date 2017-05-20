using System;
using UnityEngine;
using UnityEngine.UI;
using Logger = Extenity.DebugToolbox.Logger;
using Object = UnityEngine.Object;

namespace Extenity.UIToolbox
{

	public class RadioGroupActivator : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			RegisterRadioGroupEvents();
			Refresh();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			DeregisterRadioGroupEvents();
		}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Refresh

		private void Refresh()
		{
			var isActive = false;

			switch (Mode)
			{
				case RadioGroupActivatorMode.ActivateIfAnythingSelected:
					{
						isActive = RadioGroup.SelectedButton != null;
					}
					break;
				case RadioGroupActivatorMode.ActivateIfNothingSelected:
					{
						isActive = RadioGroup.SelectedButton == null;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException("Mode");
			}

			SetTargetActivation(isActive);
		}

		#endregion

		#region Radio Group

		public RadioGroup RadioGroup;

		#endregion

		#region Target

		public Object Target;

		private void SetTargetActivation(bool isActive)
		{
			if (Target == null)
			{
				Logger.LogError("Target was not set for radio group activator.");
				return;
			}

			if (Target is GameObject)
			{
				((GameObject)Target).SetActive(isActive);
			}
			else if (Target is Button)
			{
				((Button)Target).interactable = isActive;
			}
			else if (Target is Image)
			{
				((Image)Target).enabled = isActive;
			}
			else if (Target is Behaviour)
			{
				((Behaviour)Target).enabled = isActive;
			}
			else
			{
				Logger.LogError("Unrecognized target set for radio group activator.");
			}
		}

		#endregion

		#region Mode

		public enum RadioGroupActivatorMode
		{
			ActivateIfAnythingSelected,
			ActivateIfNothingSelected,
		}

		public RadioGroupActivatorMode Mode;

		#endregion

		#region Events

		private void RegisterRadioGroupEvents()
		{
			if (RadioGroup)
			{
				RadioGroup.OnButtonSelected.AddListener(OnRadioButtonSelectionChanged);
			}
			else
			{
				Logger.LogError("Radio group was not specified for activator.", this);
			}
		}

		private void DeregisterRadioGroupEvents()
		{
			if (RadioGroup)
			{
				RadioGroup.OnButtonSelected.RemoveListener(OnRadioButtonSelectionChanged);
			}
		}

		private void OnRadioButtonSelectionChanged(Toggle toggle)
		{
			Refresh();
		}

		#endregion
	}

}
