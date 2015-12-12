using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Logger = Extenity.Logging.Logger;

public class RadioGroupGameObjectActivator : MonoBehaviour
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

		gameObject.SetActive(isActive);
	}

	#endregion

	#region Radio Group

	public RadioGroup RadioGroup;

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
