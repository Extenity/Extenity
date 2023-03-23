using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class RadioGroup : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
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
		[ShowIf(nameof(DefaultSelectionBehaviour), DefaultSelection.SelectSpecifiedButton)]
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
							Log.ErrorWithContext(this, $"Failed to make the default selection. There is no button specified in {nameof(RadioGroup)} of object '{gameObject.FullName()}'.");
						}
					}
					break;
				case DefaultSelection.FindAndSelectTheActiveButton:
					{
						if (RegisteredToggleButtons.IsNullOrEmpty())
						{
							Log.ErrorWithContext(this, $"Failed to make the default selection. There is no registered button in {nameof(RadioGroup)} of object '{gameObject.FullName()}'.");
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
							Log.ErrorWithContext(this, $"Failed to make the default selection. There are '{activeButtonCount}' active buttons while expected only one in {nameof(RadioGroup)} of object '{gameObject.FullName()}'.");
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
				button.SetIsOnWithoutNotify(enable);
			}
		}

		#endregion

		#region Internal Events - Radio Buttons

		private readonly BoolCounter SuspendToggleChangedEvents = new BoolCounter();

		private void OnToggleChanged(Toggle toggle, bool toggleValue)
		{
			if (SuspendToggleChangedEvents.IsTrue)
				return;
			try
			{
				SuspendToggleChangedEvents.Increase();

				if (toggleValue)
				{
					SelectButton(toggle);
					OnButtonSelected.Invoke(SelectedButton);
				}
				else if (SelectedButton == toggle)
				{
					toggle.SetIsOnWithoutNotify(true);
				}
			}
			finally
			{
				SuspendToggleChangedEvents.Decrease();
			}
		}

		#endregion

		#region Events

		[Serializable]
		public class RadioGroupSelectionEvent : UnityEvent<Toggle> { }
		[Header("Events")]
		public RadioGroupSelectionEvent OnButtonSelected = new RadioGroupSelectionEvent();

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void OnValidate()
		{
			if (DefaultSelectionBehaviour != DefaultSelection.SelectSpecifiedButton)
			{
				if (DefaultSelectedButton)
				{
					Log.InfoWithContext(this, $"Clearing '{nameof(DefaultSelectedButton)}' as it is not needed and would leave unused references.");
					DefaultSelectedButton = null;
				}
			}
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(RadioGroup));

		#endregion
	}

}
