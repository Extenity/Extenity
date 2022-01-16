using System;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public struct AABB : IEquatable<AABB>
	{
		public const float Epsilon = 0.00001f;

		public float2 Max;
		public float2 Min;

		public AABB(AABB aabb)
		{
			Min = aabb.Min;
			Max = aabb.Max;
		}

		public AABB(float2 min, float2 max)
		{
			Min = min;
			Max = max;
		}

		public AABB(float2 singlePoint)
		{
			Min = singlePoint;
			Max = singlePoint;
		}

		/// <summary>
		/// Gets the width.
		/// </summary>
		/// <Value>The width.</Value>
		public float Width
		{
			get { return Max.x - Min.x; }
		}

		/// <summary>
		/// Gets the height.
		/// </summary>
		/// <Value>The height.</Value>
		public float Height
		{
			get { return Max.y - Min.y; }
		}

		/// <summary>
		/// Gets or sets the center of the AABB
		/// </summary>
		/// <value>The center.</value>
		public float2 Center
		{
			get
			{
				return new float2((Min.x + Max.x) / 2f, (Min.y + Max.y) / 2f);
			}
			set
			{
				var halfWidth = (Max.x - Min.x) / 2f;
				var halfHeight = (Max.y - Min.y) / 2f;
				Min.x = value.x - halfWidth;
				Min.y = value.y - halfHeight;
				Max.x = value.x + halfWidth;
				Max.y = value.y + halfHeight;
			}
		}

		/// <summary>
		/// Gets the vertices of the AABB.
		/// </summary>
		/// <returns>The corners of the AABB</returns>
		public List<float2> Vertices
		{
			get
			{
				return new List<float2>
			{
				Min,
				new float2(Min.x, Max.y),
				Max,
				new float2(Max.x, Min.y)
			};
			}
		}

		/// <summary>
		/// Gets the shortest side.
		/// </summary>
		/// <returns></returns>
		public float ShortestSide
		{
			get
			{
				var width = Max.x - Min.x;
				var height = Max.y - Min.y;
				return width < height ? width : height;
			}
		}

		/// <summary>
		/// Gets the distance to the specified point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns>The distance</returns>
		public float DistanceTo(float2 point)
		{
			var xDistance = Math.Abs(point.x - ((Max.x + Min.x) / 2f)) - (Max.x - Min.x) / 2f;
			var yDistance = Math.Abs(point.y - ((Max.y + Min.y) / 2f)) - (Max.y - Min.y) / 2f;

			if (xDistance > 0 && yDistance > 0)
			{
				return (float)Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
			}
			else
			{
				return Math.Max(xDistance, yDistance);
			}
		}

		/// <summary>
		/// Encapsulates the AABB with specified point.
		/// </summary>
		/// <param name="point">The point.</param>
		public void Encapsulate(float2 point)
		{
			if (point.x < Min.x) Min.x = point.x;
			if (point.x > Max.x) Max.x = point.x;
			if (point.y < Min.y) Min.y = point.y;
			if (point.y > Max.y) Max.y = point.y;
		}

		/// <summary>
		/// Encapsulates the AABB with specified points.
		/// </summary>
		/// <param name="points">The points.</param>
		public void Encapsulate(List<float2> points)
		{
			if (points == null || points.Count == 0)
				return;

			for (int i = 0; i < points.Count; i++)
			{
				var point = points[i];
				if (point.x < Min.x) Min.x = point.x;
				if (point.x > Max.x) Max.x = point.x;
				if (point.y < Min.y) Min.y = point.y;
				if (point.y > Max.y) Max.y = point.y;
			}
		}

		/// <summary>
		/// Encapsulates another AABB.
		/// </summary>
		/// <param name="other">The other AABB.</param>
		public void Encapsulate(AABB other)
		{
			Encapsulate(other.Min);
			Encapsulate(other.Max);
		}

		/// <summary>
		/// Resets the AABB with specified points.
		/// </summary>
		/// <param name="points">The points.</param>
		public void Reset(List<float2> points)
		{
			if (points == null || points.Count == 0)
			{
				Reset();
				return;
			}

			Max = Min = points[0];
			for (int i = 1; i < points.Count; i++)
			{
				var point = points[i];
				if (point.x < Min.x) Min.x = point.x;
				if (point.x > Max.x) Max.x = point.x;
				if (point.y < Min.y) Min.y = point.y;
				if (point.y > Max.y) Max.y = point.y;
			}
		}

		/// <summary>
		/// Resets the AABB with specified points.
		/// </summary>
		/// <param name="points">The points.</param>
		public void ResetXY(List<float3> points)
		{
			if (points == null || points.Count == 0)
			{
				Reset();
				return;
			}

			Max = Min = points[0].xy;
			for (int i = 1; i < points.Count; i++)
			{
				var point = points[i];
				if (point.x < Min.x) Min.x = point.x;
				if (point.x > Max.x) Max.x = point.x;
				if (point.y < Min.y) Min.y = point.y;
				if (point.y > Max.y) Max.y = point.y;
			}
		}

		/// <summary>
		/// Resets the AABB with specified points.
		/// </summary>
		/// <param name="points">The points.</param>
		public void ResetXZ(List<float3> points)
		{
			if (points == null || points.Count == 0)
			{
				Reset();
				return;
			}

			Max = Min = points[0].xz;
			for (int i = 1; i < points.Count; i++)
			{
				var point = points[i];
				if (point.x < Min.x) Min.x = point.x;
				if (point.x > Max.x) Max.x = point.x;
				if (point.z < Min.y) Min.y = point.z;
				if (point.z > Max.y) Max.y = point.z;
			}
		}

		/// <summary>
		/// Resets the AABB to undefined infinite values.
		/// </summary>
		public void Reset()
		{
			Min = new float2(float.PositiveInfinity, float.PositiveInfinity);
			Max = new float2(float.NegativeInfinity, float.NegativeInfinity);
		}

		/// <summary>
		/// Determines whether the AABB contains the specified point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns>
		/// 	<c>true</c> if it contains the specified point; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(float2 point)
		{
			return
				point.x > (Min.x - Epsilon) &&
				point.x < (Max.x + Epsilon) &&
				point.y > (Min.y - Epsilon) &&
				point.y < (Max.y + Epsilon);
		}

		/// <summary>
		/// Check if 2 AABBs intersects
		/// </summary>
		/// <param name="aabb1">The first AABB.</param>
		/// <param name="aabb2">The second AABB</param>
		/// <returns></returns>
		public static bool Intersect(ref AABB aabb1, ref AABB aabb2)
		{
			if (aabb1.Min.x > aabb2.Max.x || aabb2.Min.x > aabb1.Max.x)
			{
				return false;
			}

			if (aabb1.Min.y > aabb2.Max.y || aabb2.Min.y > aabb1.Max.y)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Checks if Max is greater than or equal to Min.
		/// </summary>
		public bool IsValid
		{
			get { return Max.x >= Min.x && Max.y >= Min.y; }
		}

		#region IEquatable Members

		public bool Equals(AABB other)
		{
			return (all(Min == other.Min) && all(Max == other.Max));
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (obj is AABB)
				return Equals((AABB)obj);

			return false;
		}

		public bool Equals(ref AABB other)
		{
			return (all(Min == other.Min) && all(Max == other.Max));
		}

		public override int GetHashCode()
		{
			return (Min.GetHashCode() + Max.GetHashCode());
		}

		public static bool operator ==(AABB a, AABB b)
		{
			return a.Equals(ref b);
		}

		public static bool operator !=(AABB a, AABB b)
		{
			return !a.Equals(ref b);
		}
	}

}
