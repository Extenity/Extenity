using System;
using UnityEngine;
using Extenity.DataToolbox;

namespace Extenity.ColoringToolbox
{

	[Serializable]
	public class HueColorScale : ColorScale
	{
		#region Configuration

		public float MinimumHue;
		public float MaximumHue;
		public float MinimumSaturation;
		public float MaximumSaturation;
		public float MinimumLightness;
		public float MaximumLightness;

		#endregion

		#region Get Color

		public override Color32 GetNormalizedColor32(float normalizedScalePoint)
		{
			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			var hue = MinimumHue + (MaximumHue - MinimumHue) * normalizedScalePoint;
			var saturation = MinimumSaturation + (MaximumSaturation - MinimumSaturation) * normalizedScalePoint;
			var lightness = MinimumLightness + (MaximumLightness - MinimumLightness) * normalizedScalePoint;

			return ColorTools.HSL2RGB(hue, saturation, lightness);
		}

		#endregion
	}

}
