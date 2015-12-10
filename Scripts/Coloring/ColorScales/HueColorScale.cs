using UnityEngine;
using Extenity.DataTypes;

namespace Extenity.Coloring
{

	public class HueColorScale : ColorScale
	{
		#region Configuration

		public float MinimumHue { get; set; }
		public float MaximumHue { get; set; }
		public float MinimumSaturation { get; set; }
		public float MaximumSaturation { get; set; }
		public float MinimumLightness { get; set; }
		public float MaximumLightness { get; set; }

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
