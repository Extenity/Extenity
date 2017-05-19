using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.DataToolbox;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadioGroup : MonoBehaviour
{
	#region Initialization

	protected void Start()
	{
		InitializeRadioButtons();
		SelectButton(DefaultButton);
	}

	#endregion

	#region Deinitialization

	//protected void OnDestroy()
	//{
	//	DeinitializeRadioButtons();
	//}

	#endregion

	#region Update

	protected void LateUpdate()
	{
		if (ChildrenInvalidated)
		{
			InitializeRadioButtons();
		}
	}

	#endregion

	#region Children

	protected bool ChildrenInvalidated;

	protected void OnTransformChildrenChanged()
	{
		ChildrenInvalidated = true;
	}

	#endregion

	#region Buttons

	private List<Toggle> Buttons;

	private void InitializeRadioButtons()
	{
		var newButtons = transform.GetComponentsInChildren<Toggle>();

		if (Buttons == null)
		{
			Buttons = new List<Toggle>(newButtons.Length);
		}

		Buttons.EqualizeTo(newButtons,
			button =>
			{
				var cachedButton = button;
				button.isOn = false;
				button.onValueChanged.AddListener(toggleValue => OnToggleChanged(cachedButton, toggleValue));
				Buttons.Add(button);
			},
			(toggle, i) =>
			{
				Buttons.RemoveAt(i);
				// TODO: Find a way to use RemoveListener to remove the event registered above.
			}
		);
	}

	//private void DeinitializeRadioButtons()
	//{
	//	if (Buttons == null)
	//		return;

	//	for (int i = 0; i < Buttons.Count; i++)
	//	{
	//		var button = Buttons[i];
	//		button.onValueChanged.RemoveListener();
	//	}

	//	Buttons = null;
	//}

	#endregion

	#region Default Button

	public Toggle DefaultButton = null;

	#endregion

	#region Select Button

	public Toggle SelectedButton { get; private set; }

	public void SelectButton(Toggle toggle)
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

	#endregion

	#region Internal Events - Radio Buttons

	private bool SuspendToggleChangedEvents;

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

	#endregion

	#region Events

	[Serializable]
	public class RadioGroupSelectionEvent : UnityEvent<Toggle> { }
	public RadioGroupSelectionEvent OnButtonSelected = new RadioGroupSelectionEvent();

	#endregion
}
