#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace Extenity.ColoringToolbox
{

	[Serializable]
	public class SingleColorScale : ColorScale
	{
		#region Configuration

		public Color32 Color;

		#endregion

		#region Initialization

		public SingleColorScale(Color32 color)
			: base()
		{
			Color = color;
		}

		public SingleColorScale(float minimumValue, float maximumValue, Color32 color)
			: base(minimumValue, maximumValue)
		{
			Color = color;
		}

		#endregion

		#region Get Color

		public override Color32 GetNormalizedColor32(float normalizedScalePoint)
		{
			return Color;
		}

		#endregion
	}

}

#endif
