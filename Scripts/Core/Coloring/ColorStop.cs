using System;
using System.Collections.Generic;
using UnityEngine;
using Extenity.DataToolbox;
using Extenity.MathToolbox;

namespace Extenity.ColoringToolbox
{

	#region Color Stop

	public struct ColorStop : IComparable<ColorStop>
	{
		public readonly float Position;
		public readonly float Hue;
		public readonly float Saturation;
		public readonly float Lightness;
		public readonly Color32 Color32;

		public ColorStop(float position, float hue, float saturation, float lightness)
		{
			Position = position;
			Hue = hue;
			Saturation = saturation;
			Lightness = lightness;
			Color32 = ColorTools.HSL2RGB(Hue, Saturation, Lightness);
		}

		public int CompareTo(ColorStop other)
		{
			return Position.CompareTo(other.Position);
		}
	}

	#endregion

	#region Color Stop Extensions

	public static class ColorStopExtensions
	{
		public static int GetColorStopIndex(this IList<ColorStop> colorStops, float position)
		{
			var index = 0;
			while (position > colorStops[index].Position)
			{
				index++;
			}
			return index;
		}

		public static Color32 GetColor32(this IList<ColorStop> colorStops, float position)
		{
			var index = colorStops.GetColorStopIndex(position);

			if (position.IsAlmostEqual(colorStops[index].Position))
			{
				return colorStops[index].Color32;
			}
			if (index == 0)
			{
				//less than the first one
				return colorStops[index].Color32;
			}
			if (index == colorStops.Count - 1 && position > colorStops[colorStops.Count - 1].Position)
			{
				//past the last color
				return colorStops[index].Color32;
			}
			return ColorTools.FastLerp(
				colorStops[index - 1].Color32,
				colorStops[index].Color32,
				(position - colorStops[index - 1].Position) / (colorStops[index].Position - colorStops[index - 1].Position));
		}
	}

	#endregion

}
