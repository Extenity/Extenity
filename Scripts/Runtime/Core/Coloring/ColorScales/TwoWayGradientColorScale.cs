#if UNITY

using UnityEngine;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.MathToolbox;

namespace Extenity.ColoringToolbox
{

	public class TwoWayGradientColorScale : TwoWayColorScale
	{
		#region Configuration

		public readonly List<ColorStop> PositiveColorStops;
		public readonly List<ColorStop> NegativeColorStops;

		#endregion

		#region Initialization

		public TwoWayGradientColorScale()
			: base()
		{
			PositiveColorStops = new List<ColorStop>();
			NegativeColorStops = new List<ColorStop>();
		}

		public TwoWayGradientColorScale(float minimumValue, float maximumValue, IList<ColorStop> positiveColorStops, IList<ColorStop> negativeColorStops)
			: base(minimumValue, maximumValue)
		{
			PositiveColorStops = new List<ColorStop>(positiveColorStops.Count);
			NegativeColorStops = new List<ColorStop>(negativeColorStops.Count);

			for (int i = 0; i < positiveColorStops.Count; i++)
			{
				PositiveColorStops.AddSorted(positiveColorStops[i]);
			}
			for (int i = 0; i < negativeColorStops.Count; i++)
			{
				NegativeColorStops.AddSorted(negativeColorStops[i]);
			}
		}

		#endregion

		#region Add / Remove Color Stop

		public void AddPositiveColorStop(ColorStop colorStop)
		{
			PositiveColorStops.AddSorted(colorStop);
		}

		public void AddNegativeColorStop(ColorStop colorStop)
		{
			NegativeColorStops.AddSorted(colorStop);
		}

		public bool RemovePositiveColorStop(float position)
		{
			for (int i = 0; i < PositiveColorStops.Count; i++)
			{
				if (PositiveColorStops[i].Position.IsAlmostEqual(position))
				{
					PositiveColorStops.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public bool RemoveNegativeColorStop(float position)
		{
			for (int i = 0; i < NegativeColorStops.Count; i++)
			{
				if (NegativeColorStops[i].Position.IsAlmostEqual(position))
				{
					NegativeColorStops.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Get Color

		public override Color32 GetPositiveNormalizedColor32(float normalizedScalePoint)
		{
			if (PositiveColorStops.IsNullOrEmpty())
				return new Color32(0, 0, 0, 0);

			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			return PositiveColorStops.GetColor32(normalizedScalePoint);
		}

		public override Color32 GetNegativeNormalizedColor32(float normalizedScalePoint)
		{
			if (NegativeColorStops.IsNullOrEmpty())
				return new Color32(0, 0, 0, 0);

			if (normalizedScalePoint < 0f)
				normalizedScalePoint = 0f;
			else if (normalizedScalePoint > 1f)
				normalizedScalePoint = 1f;

			return NegativeColorStops.GetColor32(normalizedScalePoint);
		}

		#endregion
	}

}

#endif
