﻿using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.DataToolbox
{

	public enum CardinalDirection4
	{
		Unspecified,
		East,
		North,
		West,
		South,
	}

	public enum CardinalDirection8
	{
		Unspecified,
		East,
		NorthEast,
		North,
		NorthWest,
		West,
		SouthWest,
		South,
		SouthEast,
	}

	public static class DirectionTools
	{
		#region Names

		public static string ToStringAcronym(this CardinalDirection4 value)
		{
			switch (value)
			{
				case CardinalDirection4.Unspecified:
					return "Unspecified";
				case CardinalDirection4.East:
					return "E";
				case CardinalDirection4.North:
					return "N";
				case CardinalDirection4.West:
					return "W";
				case CardinalDirection4.South:
					return "S";
				default:
					return "Undefined";
			}
		}

		public static string ToStringAcronym(this CardinalDirection8 value)
		{
			switch (value)
			{
				case CardinalDirection8.Unspecified:
					return "Unspecified";
				case CardinalDirection8.East:
					return "E";
				case CardinalDirection8.NorthEast:
					return "NE";
				case CardinalDirection8.North:
					return "N";
				case CardinalDirection8.NorthWest:
					return "NW";
				case CardinalDirection8.West:
					return "W";
				case CardinalDirection8.SouthWest:
					return "SW";
				case CardinalDirection8.South:
					return "S";
				case CardinalDirection8.SouthEast:
					return "SE";
				default:
					return "Undefined";
			}
		}

		#endregion

		#region Vector2/3 To CardinalDirection4/8

		public static CardinalDirection4 ToCardinalDirection4(this float2 vector)
		{
			var angle = atan2(vector.y, vector.x);
			var octant = round(4 * angle / (2 * PI) + 4) % 4;
			return (CardinalDirection4)(octant + 1);
		}

		public static CardinalDirection4 ToCardinalDirection4XZ(this float3 vector)
		{
			var angle = atan2(vector.z, vector.x);
			var octant = round(4 * angle / (2 * PI) + 4) % 4;
			return (CardinalDirection4)(octant + 1);
		}

		public static CardinalDirection8 ToCardinalDirection8(this float2 vector)
		{
			var angle = atan2(vector.y, vector.x);
			var octant = round(8 * angle / (2 * PI) + 8) % 8;
			return (CardinalDirection8)(octant + 1);
		}

		public static CardinalDirection8 ToCardinalDirection8XZ(this float3 vector)
		{
			var angle = atan2(vector.z, vector.x);
			var octant = round(8 * angle / (2 * PI) + 8) % 8;
			return (CardinalDirection8)(octant + 1);
		}

		#endregion
	}

}
