using UnityEngine;
using System.Collections.Generic;

namespace Extenity.Coloring
{

	public class GradientColorScale : ColorScale
	{
		#region Configuration

		public readonly List<ColorStop> ColorStops;

		#endregion

		#region Initialization

		public GradientColorScale()
			: base()
		{
			ColorStops = new List<ColorStop>();
		}

		public GradientColorScale(float minimumValue, float maximumValue, IList<ColorStop> colorStops)
			: base(minimumValue, maximumValue)
		{
			ColorStops = new List<ColorStop>(colorStops.Count);

			for (int i = 0; i < colorStops.Count; i++)
			{
				ColorStops.AddSorted(colorStops[i]);
			}
		}

		#endregion

		#region Add / Remove Color Stop

		public void AddColorStop(ColorStop colorStop)
		{
			ColorStops.AddSorted(colorStop);
		}

		public bool RemoveColorStop(float position)
		{
			for (int i = 0; i < ColorStops.Count; i++)
			{
				if (ColorStops[i].Position.IsAlmostEqual(position))
				{
					ColorStops.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Get Color

		public override Color32 GetNormalizedColor32(float normalizedScalePoint)
		{
			if (ColorStops.IsNullOrEmpty())
				return new Color32(0, 0, 0, 0);

			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			return ColorStops.GetColor32(normalizedScalePoint);
		}

		#endregion
	}

}
