using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.UnityEditorToolbox;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class RadioGroup : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			InitializeRadioButtons();
			InitializeDefaultSelection();
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
			if (ChildrenInvalidated && AutoFindChildButtons)
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

		[Header("Buttons")]
		public bool AutoFindChildButtons = true;
		public Toggle[] ToggleButtons;

		private List<Toggle> RegisteredToggleButtons;

		private void InitializeRadioButtons()
		{
			Toggle[] newButtons;
			if (AutoFindChildButtons)
			{
				newButtons = transform.GetComponentsInChildren<Toggle>();
			}
			else
			{
				newButtons = ToggleButtons;
			}

			if (RegisteredToggleButtons == null)
			{
				RegisteredToggleButtons = new List<Toggle>(newButtons.Length);
			}

			RegisteredToggleButtons.EqualizeTo(newButtons,
				button =>
				{
					var cachedButton = button;
					button.onValueChanged.AddListener(toggleValue => OnToggleChanged(cachedButton, toggleValue));
					RegisteredToggleButtons.Add(button);
				},
				(toggle, i) =>
				{
					RegisteredToggleButtons.RemoveAt(i);
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

		#region Default Selection

		public enum DefaultSelection
		{
			Untouched,
			SelectSpecifiedButton,
			FindAndSelectTheActiveButton,
			AllDeselected,
		}

		[Header("Default Selection")]
		public DefaultSelection DefaultSelectionBehaviour = DefaultSelection.SelectSpecifiedButton;
		[ConditionalHideInInspector("DefaultSelectionBehaviour", DefaultSelection.SelectSpecifiedButton, HideOrDisable.Hide)]
		public Toggle DefaultSelectedButton = null;

		private void InitializeDefaultSelection()
		{
			switch (DefaultSelectionBehaviour)
			{
				case DefaultSelection.Untouched:
					break;
				case DefaultSelection.SelectSpecifiedButton:
					{
						if (DefaultSelectedButton)
						{
							SelectButton(DefaultSelectedButton);
						}
						else
						{
							Debug.LogError($"Failed to make the default selection. There is no button specified in {nameof(RadioGroup)} of object '{gameObject.FullName()}'.", this);
						}
					}
					break;
				case DefaultSelection.FindAndSelectTheActiveButton:
					{
						if (RegisteredToggleButtons.IsNullOrEmpty())
						{
							Debug.LogError($"Failed to make the default selection. There is no registered button in {nameof(RadioGroup)} of object '{gameObject.FullName()}'.", this);
							break;
						}

						int activeButtonCount = 0;
						Toggle activeButton = null;
						for (int i = 0; i < RegisteredToggleButtons.Count; i++)
						{
							if (RegisteredToggleButtons[i].isOn)
							{
								activeButton = RegisteredToggleButtons[i];
								activeButtonCount++;
							}
						}

						if (activeButtonCount == 1)
						{
							SelectButton(activeButton);
						}
						else
						{
							Debug.LogError($"Failed to make the default selection. There are '{activeButtonCount}' active buttons while expected only one in {nameof(RadioGroup)} of object '{gameObject.FullName()}'.", this);
						}
					}
					break;
				case DefaultSelection.AllDeselected:
					{
						SelectButton(null);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Select Button

		public Toggle SelectedButton { get; private set; }

		public void SelectButton(Toggle toggle)
		{
			for (int i = 0; i < RegisteredToggleButtons.Count; i++)
			{
				var button = RegisteredToggleButtons[i];
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
		[Header("Events")]
		public RadioGroupSelectionEvent OnButtonSelected = new RadioGroupSelectionEvent();

		#endregion

		#region Editor

		protected void OnValidate()
		{
			if (DefaultSelectionBehaviour != DefaultSelection.SelectSpecifiedButton)
			{
				if (DefaultSelectedButton)
				{
					Debug.Log($"Clearing '{nameof(DefaultSelectedButton)}' as it is not needed and would leave unused references.");
					DefaultSelectedButton = null;
				}
			}
		}

		#endregion
	}

}
