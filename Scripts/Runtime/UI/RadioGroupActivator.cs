using System;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.UnityEditorToolbox;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Extenity.UIToolbox
{

	public enum RadioGroupActivatorMode
	{
		ActivateIfAnythingSelected,
		ActivateIfNothingSelected,
		ActivateIfSelectedAnExpectedObject,
	}

	public class RadioGroupActivator : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			RegisterRadioGroupEvents();
		}

		protected void Start()
		{
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
						isActive = RadioGroup && RadioGroup.SelectedButton != null;
					}
					break;
				case RadioGroupActivatorMode.ActivateIfNothingSelected:
					{
						isActive = RadioGroup && RadioGroup.SelectedButton == null;
					}
					break;
				case RadioGroupActivatorMode.ActivateIfSelectedAnExpectedObject:
					{
						if (ExpectedObjects.IsNullOrEmpty())
						{
							Debug.LogError($"No expected object specified for {nameof(RadioGroupActivator)} in object '{gameObject.FullName()}',");
							break;
						}

						if (RadioGroup && RadioGroup.SelectedButton)
						{
							isActive = ExpectedObjects.Contains(RadioGroup.SelectedButton) ||
									   ExpectedObjects.Contains(RadioGroup.SelectedButton.gameObject);
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			SetTargetActivation(isActive);
		}

		#endregion

		#region Radio Group

		[Tooltip("The radio group that it's activities will be checked for. If the configured criteria is met, 'Target' object will be enabled.")]
		public RadioGroup RadioGroup;

		#endregion

		#region Target

		[Tooltip("Object to be activated when criteria is met.")]
		public Object Target;

		private void SetTargetActivation(bool isActive)
		{
			if (Target == null)
			{
				Debug.LogError("Target was not set for radio group activator.");
				return;
			}

			if (Target is GameObject)
			{
				((GameObject)Target).SetActive(isActive);
				if (RebuildLayoutOnActivation)
					LayoutRebuilder.MarkLayoutForRebuild(((GameObject)Target).GetComponent<RectTransform>());
			}
			else if (Target is Button)
			{
				((Button)Target).interactable = isActive;
				if (RebuildLayoutOnActivation)
					LayoutRebuilder.MarkLayoutForRebuild(((Component)Target).GetComponent<RectTransform>());
			}
			else if (Target is Image)
			{
				((Image)Target).enabled = isActive;
				if (RebuildLayoutOnActivation)
					LayoutRebuilder.MarkLayoutForRebuild(((Component)Target).GetComponent<RectTransform>());
			}
			else if (Target is Behaviour)
			{
				((Behaviour)Target).enabled = isActive;
				if (RebuildLayoutOnActivation)
					LayoutRebuilder.MarkLayoutForRebuild(((Component)Target).GetComponent<RectTransform>());
			}
			else
			{
				Debug.LogError("Unrecognized target set for radio group activator.");
			}
		}

		#endregion

		#region Mode

		[Header("Criteria")]
		public RadioGroupActivatorMode Mode = RadioGroupActivatorMode.ActivateIfSelectedAnExpectedObject;

		#endregion

		#region Expected Objects

		[ConditionalHideInInspector("Mode", RadioGroupActivatorMode.ActivateIfSelectedAnExpectedObject, false, HideOrDisable.Hide)]
		public Object[] ExpectedObjects;

		#endregion

		#region Additional Options

		[Header("Additional Options")]
		public bool RebuildLayoutOnActivation = false;

		#endregion

		#region Events

		private void RegisterRadioGroupEvents()
		{
			if (RadioGroup)
			{
				RadioGroup.OnButtonSelected.RemoveListener(OnRadioButtonSelectionChanged); // Just in case.
				RadioGroup.OnButtonSelected.AddListener(OnRadioButtonSelectionChanged);
			}
			else
			{
				Debug.LogError($"{nameof(RadioGroup)} was not specified for {nameof(RadioGroupActivator)} in object '{gameObject.FullName()}'.", this);
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

		#region Validate

		private void OnValidate()
		{
			if (!RadioGroup)
			{
				RadioGroup = GetComponentInParent<RadioGroup>();
			}

			if (Mode != RadioGroupActivatorMode.ActivateIfSelectedAnExpectedObject)
			{
				if (ExpectedObjects.IsNotNullAndEmpty())
				{
					Debug.Log($"Clearing '{nameof(ExpectedObjects)}' as it is not needed and would leave unused references.");
					ExpectedObjects = new Object[0];
				}
			}
		}

		#endregion
	}

}
