#if UNITY_5_3_OR_NEWER

using UnityEngine;
using Extenity.DataToolbox;

namespace Extenity.ColoringToolbox
{

	public class TwoWayHueColorScale : TwoWayColorScale
	{
		#region Configuration

		public float PositiveMinimumHue { get; set; }
		public float PositiveMaximumHue { get; set; }
		public float PositiveMinimumSaturation { get; set; }
		public float PositiveMaximumSaturation { get; set; }
		public float PositiveMinimumLightness { get; set; }
		public float PositiveMaximumLightness { get; set; }
		public float NegativeMinimumHue { get; set; }
		public float NegativeMaximumHue { get; set; }
		public float NegativeMinimumSaturation { get; set; }
		public float NegativeMaximumSaturation { get; set; }
		public float NegativeMinimumLightness { get; set; }
		public float NegativeMaximumLightness { get; set; }

		#endregion

		#region Get Color

		public override Color32 GetPositiveNormalizedColor32(float normalizedScalePoint)
		{
			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			var hue = PositiveMinimumHue + (PositiveMaximumHue - PositiveMinimumHue) * normalizedScalePoint;
			var saturation = PositiveMinimumSaturation + (PositiveMaximumSaturation - PositiveMinimumSaturation) * normalizedScalePoint;
			var lightness = PositiveMinimumLightness + (PositiveMaximumLightness - PositiveMinimumLightness) * normalizedScalePoint;

			return ColorTools.HSL2RGBColor32(hue, saturation, lightness);
		}

		public override Color32 GetNegativeNormalizedColor32(float normalizedScalePoint)
		{
			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			var hue = NegativeMinimumHue + (NegativeMaximumHue - NegativeMinimumHue) * normalizedScalePoint;
			var saturation = NegativeMinimumSaturation + (NegativeMaximumSaturation - NegativeMinimumSaturation) * normalizedScalePoint;
			var lightness = NegativeMinimumLightness + (NegativeMaximumLightness - NegativeMinimumLightness) * normalizedScalePoint;

			return ColorTools.HSL2RGBColor32(hue, saturation, lightness);
		}

		#endregion
	}

}

#endif
