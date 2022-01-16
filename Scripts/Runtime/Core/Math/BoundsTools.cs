#if UNITY

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class BoundsTools
	{
		public static readonly Bounds NaN = new Bounds(Vector3Tools.NaN, Vector3Tools.NaN);

		#region Fix and Reset Bounds

		public static void Fix(this Bounds bounds)
		{
			var min = bounds.min;
			var max = bounds.max;

			bool switchX = min.x > max.x;
			bool switchY = min.y > max.y;
			bool switchZ = min.z > max.z;

			if (switchX)
			{
				if (switchY)
				{
					if (switchZ)
					{
						bounds.SetMinMax(
							float3(max.x, max.y, max.z),
							float3(min.x, min.y, min.z));
					}
					else
					{
						bounds.SetMinMax(
							float3(max.x, max.y, min.z),
							float3(min.x, min.y, max.z));
					}
				}
				else
				{
					if (switchZ)
					{
						bounds.SetMinMax(
							float3(max.x, min.y, max.z),
							float3(min.x, max.y, min.z));
					}
					else
					{
						bounds.SetMinMax(
							float3(max.x, min.y, min.z),
							float3(min.x, max.y, max.z));
					}
				}
			}
			else
			{
				if (switchY)
				{
					if (switchZ)
					{
						bounds.SetMinMax(
							float3(min.x, max.y, max.z),
							float3(max.x, min.y, min.z));
					}
					else
					{
						bounds.SetMinMax(
							float3(min.x, max.y, min.z),
							float3(max.x, min.y, max.z));
					}
				}
				else
				{
					if (switchZ)
					{
						bounds.SetMinMax(
							float3(min.x, min.y, max.z),
							float3(max.x, max.y, min.z));
					}
					else
					{
						bounds.SetMinMax(
							float3(min.x, min.y, min.z),
							float3(max.x, max.y, max.z));
					}
				}
			}
		}

		public static void Reset(this Bounds bounds)
		{
			bounds.SetMinMax(
				float3Tools.MaxValue,
				float3Tools.MinValue);
		}

		#endregion

		#region Calculate Bounds Of Points

		public static Bounds2 CalculateBounds(this IList<float2> points)
		{
			var minX = points[0].x;
			var maxX = points[0].x;
			var minY = points[0].y;
			var maxY = points[0].y;
			for (int i = 1; i < points.Count; i++)
			{
				var q = points[i];
				minX = min(q.x, minX);
				maxX = max(q.x, maxX);
				minY = min(q.y, minY);
				maxY = max(q.y, maxY);
			}
			var bounds = new Bounds2();
			bounds.SetMinMax(float2(minX, minY), float2(maxX, maxY));
			return bounds;
		}

		public static Bounds2 CalculateBoundsXY(this IList<float3> points)
		{
			var minX = points[0].x;
			var maxX = points[0].x;
			var minY = points[0].y;
			var maxY = points[0].y;
			for (int i = 1; i < points.Count; i++)
			{
				var q = points[i];
				minX = min(q.x, minX);
				maxX = max(q.x, maxX);
				minY = min(q.y, minY);
				maxY = max(q.y, maxY);
			}
			var bounds = new Bounds2();
			bounds.SetMinMax(float2(minX, minY), float2(maxX, maxY));
			return bounds;
		}

		public static Bounds2 CalculateBoundsXZ(this IList<float3> points)
		{
			var minX = points[0].x;
			var maxX = points[0].x;
			var minY = points[0].z;
			var maxY = points[0].z;
			for (int i = 1; i < points.Count; i++)
			{
				var q = points[i];
				minX = min(q.x, minX);
				maxX = max(q.x, maxX);
				minY = min(q.z, minY);
				maxY = max(q.z, maxY);
			}
			var bounds = new Bounds2();
			bounds.SetMinMax(float2(minX, minY), float2(maxX, maxY));
			return bounds;
		}

		#endregion

		#region Bounds

#if UNITY
		// Source: http://answers.unity3d.com/questions/361275/cant-convert-bounds-from-world-coordinates-to-loca.html
		public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
		{
			var center = transform.TransformPoint(localBounds.center);

			// transform the local extents' axes
			var extents = localBounds.extents;
			var axisX = transform.TransformVector(extents.x, 0, 0);
			var axisY = transform.TransformVector(0, extents.y, 0);
			var axisZ = transform.TransformVector(0, 0, extents.z);

			// sum their absolute value to get the world extents
			extents.x = abs(axisX.x) + abs(axisY.x) + abs(axisZ.x);
			extents.y = abs(axisX.y) + abs(axisY.y) + abs(axisZ.y);
			extents.z = abs(axisX.z) + abs(axisY.z) + abs(axisZ.z);

			return new Bounds { center = center, extents = extents };
		}

		public static Bounds TransformBounds(this Transform transform, Bounds localBounds, Transform relativeTo)
		{
			var center = relativeTo.InverseTransformPoint(transform.TransformPoint(localBounds.center));

			// transform the local extents' axes
			var extents = localBounds.extents;
			var axisX = relativeTo.InverseTransformVector(transform.TransformVector(extents.x, 0, 0));
			var axisY = relativeTo.InverseTransformVector(transform.TransformVector(0, extents.y, 0));
			var axisZ = relativeTo.InverseTransformVector(transform.TransformVector(0, 0, extents.z));

			// sum their absolute value to get the world extents
			extents.x = abs(axisX.x) + abs(axisY.x) + abs(axisZ.x);
			extents.y = abs(axisX.y) + abs(axisY.y) + abs(axisZ.y);
			extents.z = abs(axisX.z) + abs(axisY.z) + abs(axisZ.z);

			return new Bounds { center = center, extents = extents };
		}
#endif

		#endregion
	}

}

#endif
