#if UNITY_5_3_OR_NEWER

using UnityEngine;
using Extenity.DataToolbox;

namespace Extenity.ColoringToolbox
{

	public class TwoWayLerpColorScale : TwoWayColorScale
	{
		#region Configuration

		public Color32 PositiveMinimumColor { get; set; }
		public Color32 PositiveMaximumColor { get; set; }
		public Color32 NegativeMinimumColor { get; set; }
		public Color32 NegativeMaximumColor { get; set; }

		#endregion

		#region Get Color

		public override Color32 GetPositiveNormalizedColor32(float normalizedScalePoint)
		{
			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			return ColorTools.FastLerp(PositiveMinimumColor, PositiveMaximumColor, normalizedScalePoint);
		}

		public override Color32 GetNegativeNormalizedColor32(float normalizedScalePoint)
		{
			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			return ColorTools.FastLerp(NegativeMinimumColor, NegativeMaximumColor, normalizedScalePoint);
		}

		#endregion
	}

}

#endif
