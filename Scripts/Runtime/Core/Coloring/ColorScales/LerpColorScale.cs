#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;
using Extenity.DataToolbox;

namespace Extenity.ColoringToolbox
{

	[Serializable]
	public class LerpColorScale : ColorScale
	{
		#region Configuration

		public Color32 MinimumColor;
		public Color32 MaximumColor;

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

#endif
