using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadioGroup : MonoBehaviour
{
	#region Initialization

	protected void Start()
	{
		InitializeRadioButtons();
	}

	#endregion

	#region Deinitialization

	protected void OnDestroy()
	{
	}

	#endregion

	#region Update

	protected void Update()
	{
	}

	#endregion

	private List<Toggle> Buttons;
	public Toggle DefaultButton;
	public Toggle SelectedButton;
	private bool SuspendToggleChangedEvents;

	[Serializable]
	public class RadioGroupSelectionEvent : UnityEvent<Toggle> { }
    public RadioGroupSelectionEvent OnButtonSelected = new RadioGroupSelectionEvent();

	private void InitializeRadioButtons()
	{
		Buttons = transform.GetComponentsInChildren<Toggle>().ToList();

		foreach (var button in Buttons)
		{
			var cachedButton = button;
			button.isOn = false;
			button.onValueChanged.AddListener((toggleValue) => OnToggleChanged(cachedButton, toggleValue));
		}

		SelectButton(DefaultButton);
	}

	private void OnToggleChanged(Toggle toggle, bool toggleValue)
	{
		if (SuspendToggleChangedEvents)
			return;
		SuspendToggleChangedEvents = true;

		if (toggleValue)
		{
			SelectButton(toggle);
			OnButtonSelected.Invoke(SelectedButton);
		}
		else if (SelectedButton == toggle)
		{
			toggle.isOn = true;
		}

		SuspendToggleChangedEvents = false;
	}

	private void SelectButton(Toggle toggle)
	{
		for (int i = 0; i < Buttons.Count; i++)
		{
			var button = Buttons[i];
			var enable = button == toggle;
			if (enable)
			{
				SelectedButton = button;
			}
			button.isOn = enable;
		}
	}
}
