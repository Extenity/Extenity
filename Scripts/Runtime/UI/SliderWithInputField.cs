using System;
using UnityEngine;
using Extenity.MathToolbox;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class SliderWithInputField : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			InitializeSliderAndInputField();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			DeinitializeSliderAndInputField();
		}

		#endregion

		#region Slider and Input Field

		public Slider Slider;
		public InputField InputField;

		private void InitializeSliderAndInputField()
		{
			if (Slider == null)
				throw new Exception("Slider UI element was not assigned.");
			if (InputField == null)
				throw new Exception("InputField UI element was not assigned.");

			InputField.text = Value.ToString();
			Slider.value = Value;
			InputField.onEndEdit.AddListener(OnInputFieldEndEdit);
			Slider.onValueChanged.AddListener(OnSliderValueChanged);
		}

		private void DeinitializeSliderAndInputField()
		{
			InputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
			Slider.onValueChanged.RemoveListener(OnSliderValueChanged);
		}

		#endregion

		#region Internal Events

		private void OnInputFieldEndEdit(string value)
		{
			if (float.TryParse(value, out var valueFloat))
			{
				if (!Slider.value.IsAlmostEqual(valueFloat))
				{
					Slider.value = valueFloat;
				}
			}
		}

		private void OnSliderValueChanged(float value)
		{
			InputField.text = value.ToString();
		}

		#endregion

		#region Value

		public float Value
		{
			get { return Slider.value; }
			set { Slider.value = value; }
		}

		public float NormalizedValue
		{
			get { return Slider.normalizedValue; }
			set { Slider.normalizedValue = value; }
		}

		public float MaxValue
		{
			get { return Slider.maxValue; }
			set { Slider.maxValue = value; }
		}

		public float MinValue
		{
			get { return Slider.minValue; }
			set { Slider.minValue = value; }
		}

		public bool WholeNumbers
		{
			get { return Slider.wholeNumbers; }
			set { Slider.wholeNumbers = value; }
		}

		#endregion

		#region Label

		public Text LabelText;

		public string Label
		{
			get { return LabelText != null ? LabelText.text : ""; }
			set
			{
				if (LabelText != null)
				{
					LabelText.text = value;
				}
				else
				{
					Log.ErrorWithContext(this, "Label UI element was not assigned.");
				}
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(SliderWithInputField));

		#endregion
	}

}
