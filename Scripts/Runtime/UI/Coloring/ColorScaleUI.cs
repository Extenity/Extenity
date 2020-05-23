using UnityEngine;
using DG.Tweening;
using Extenity.ColoringToolbox;
using Extenity.MathToolbox;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class ColorScaleUI : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			InitializeScaleText();
			DelayedRefresh();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			DeinitializeScaleText();
		}

		#endregion

		#region Texture

		public ColorScaleTexture ColorScaleTexture;

		#endregion

		#region Color Scale

		public IColorScale ColorScale;

		#endregion

		#region Scale Text

		public string ScaleNormalTextFormat = "{0:0.0}";
		public string ScaleCustomTextFormat = "> {0:0.0}";
		public Text ScaleTextSample;
		private Text ScaleMaxText;
		private Text ScaleMinText;
		private Text ScaleZeroText;
		private Text CustomLabelText;
		private float MaxPositionY;
		private float MinPositionY;
		//private float CustomLabelPositionY;

		private void InitializeScaleText()
		{
			var parent = ScaleTextSample.rectTransform.parent;
			ScaleMaxText = CreateLabel(ScaleTextSample, parent);
			ScaleMinText = CreateLabel(ScaleTextSample, parent);
			ScaleZeroText = CreateLabel(ScaleTextSample, parent);
			CustomLabelText = CreateLabel(ScaleTextSample, parent);

			//var imageRect = ColorScaleTexture.UIImage.rectTransform.rect;
			//MaxPositionY = imageRect.height;
			//MinPositionY = 0f;

			//Vector3 position;
			//position = Vector3.zero;
			//position.y = MaxPositionY;
			//ScaleMaxText.rectTransform.localPosition = position;

			//position = Vector3.zero;
			//position.y = MinPositionY;
			//ScaleMinText.rectTransform.localPosition = position;


			ScaleTextSample.gameObject.SetActive(false);
		}

		private void DeinitializeScaleText()
		{
			Destroy(ScaleMaxText);
			Destroy(ScaleMinText);
			Destroy(ScaleZeroText);
			Destroy(CustomLabelText);
		}

		private static Text CreateLabel(Text prefab, Transform parent)
		{
			var text = Instantiate(prefab.gameObject).GetComponent<Text>();
			text.rectTransform.SetParent(parent, true);
			text.rectTransform.localScale = Vector3.one;
			return text;
		}

		private void UpdateTexts()
		{
			if (ColorScale == null)
				return;

			var colorScaleMin = ColorScale.MinimumValue;
			var colorScaleMax = ColorScale.MaximumValue;
			var zeroActive = ColorScale.MaximumValue > 0f && ColorScale.MinimumValue < 0f;

			var imageRect = ColorScaleTexture.UIImage.rectTransform.rect;
			MaxPositionY = imageRect.height;
			MinPositionY = 0f;

			// Custom label
			InternalUpdateLabelPosition(CustomLabelText, CustomLabelValue, IsCustomLabelActive, colorScaleMin, colorScaleMax, MinPositionY, MaxPositionY);
			InternalUpdateLabelText(CustomLabelText, CustomLabelValue, ScaleCustomTextFormat, false);

			// Min label
			InternalUpdateLabelPosition(ScaleMinText, colorScaleMin, true, colorScaleMin, colorScaleMax, MinPositionY, MaxPositionY);
			InternalUpdateLabelText(ScaleMinText, colorScaleMin, ScaleNormalTextFormat, true);

			// Max label
			InternalUpdateLabelPosition(ScaleMaxText, colorScaleMax, true, colorScaleMin, colorScaleMax, MinPositionY, MaxPositionY);
			InternalUpdateLabelText(ScaleMaxText, colorScaleMax, ScaleNormalTextFormat, true);

			// Zero label
			InternalUpdateLabelPosition(ScaleZeroText, 0f, zeroActive, colorScaleMin, colorScaleMax, MinPositionY, MaxPositionY);
			InternalUpdateLabelText(ScaleZeroText, 0f, ScaleNormalTextFormat, true);
		}

		private void InternalUpdateLabelPosition(Text labelText, float labelValue, bool isActive, float colorScaleMin, float colorScaleMax, float minPositionY, float maxPositionY)
		{
			if (labelText != null)
			{
				if (isActive)
				{
					var position = Vector3.zero;
					if (colorScaleMin.IsAlmostEqual(colorScaleMax))
					{
						colorScaleMax += 0.0001f;
					}
					position.y = labelValue.Remap(colorScaleMin, colorScaleMax, minPositionY, maxPositionY);
					//labelText.rectTransform.localPosition = position;
					labelText.rectTransform.DOLocalMove(position, 0.15f);
				}
				else
				{
					labelText.rectTransform.localPosition = new Vector3(500000f, 500000f, 500000f);
				}

				labelText.gameObject.SetActive(isActive);
			}
		}

		private void InternalUpdateLabelText(Text labelText, float labelValue, string labelValueFormat, bool checkOverlapWithCustomLabel = true)
		{
			if (labelText != null)
			{
				var alpha = 1f;
				if (checkOverlapWithCustomLabel)
				{
					var distance = labelText.rectTransform.localPosition.y.Distance(CustomLabelText.rectTransform.localPosition.y);
					if (distance < 15f)
					{
						alpha = 0.3f;
					}
				}
				labelText.enabled = true;
				var color = labelText.color;
				color.a = alpha;
				labelText.color = color;
				labelText.text = string.Format(labelValueFormat, labelValue);
			}
		}

		#endregion

		#region Scale Text - Custom Label

		public bool IsCustomLabelActive { get; private set; }
		public float CustomLabelValue { get; private set; }

		public void SetCustomLabel(float value, bool autoEnable)
		{
			CustomLabelValue = value;

			if (autoEnable)
			{
				IsCustomLabelActive = true;
			}

			UpdateTexts();
		}

		public void EnableCustomLabel(bool enable)
		{
			IsCustomLabelActive = enable;

			UpdateTexts();
		}

		#endregion

		#region Refresh

		public void Refresh()
		{
			ColorScaleTexture.ColorScale = ColorScale;
			ColorScaleTexture.RefreshTexture();
			UpdateTexts();
		}

		private void DelayedRefresh()
		{
			Invoke("Refresh", 0.1f);
		}

		#endregion
	}

}
