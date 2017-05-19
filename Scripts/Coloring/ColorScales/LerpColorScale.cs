using UnityEngine;
using Extenity.DataToolbox;

namespace Extenity.ColoringToolbox
{

	public class LerpColorScale : ColorScale
	{
		#region Configuration

		public Color32 MinimumColor { get; set; }
		public Color32 MaximumColor { get; set; }

		#endregion

		#region Get Color

		public override Color32 GetNormalizedColor32(float normalizedScalePoint)
		{
			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			return ColorTools.FastLerp(MinimumColor, MaximumColor, normalizedScalePoint);
		}

		#endregion
	}

}
