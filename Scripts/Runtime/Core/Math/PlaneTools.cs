using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.MathToolbox
{

	public static class PlaneTools
	{
		#region Plane

		public static float3 ProjectPointOnPlane(float3 planeNormal, float3 planePoint, float3 point)
		{
			var distance = -dot(normalize(planeNormal), point - planePoint);
			return point + planeNormal * distance;
		}

#if UNITY
		public static bool IsAllPointsOnPlane(this IList<float3> points, float3 planeNormal, float tolerance = 0.0001f)
		{
			var plane = new Plane(planeNormal, points[0]);
			for (int i = 1; i < points.Count; i++)
			{
				var distance = plane.GetDistanceToPoint(points[i]);
				if (!distance.IsZero(tolerance))
					return false;
			}
			return true;
		}

		public static bool Linecast(this Plane plane, float3 lineStart, float3 lineEnd)
		{
			return !plane.SameSide(lineStart, lineEnd);
		}

		public static bool Linecast(this Plane plane, float3 lineStart, float3 lineEnd, out float3 intersection)
		{
			var distanceToPoint1 = plane.GetDistanceToPoint(lineStart);
			var distanceToPoint2 = plane.GetDistanceToPoint(lineEnd);
			var notIntersected = distanceToPoint1 > 0.0f && distanceToPoint2 > 0.0f ||
			                     distanceToPoint1 <= 0.0f && distanceToPoint2 <= 0.0f;
			if (notIntersected)
			{
				intersection = float3Tools.NaN;
				return false;
			}

			var totalDistance = distanceToPoint1 - distanceToPoint2;
			var ratio = distanceToPoint1 / totalDistance;
			intersection = lineStart + (lineEnd - lineStart) * ratio;
			return true;
		}

		public static bool LinecastWithProximity(this Plane plane, float3 lineStart, float3 lineEnd, float3 proximityCheckingPoint, float proximityCheckingRadius)
		{
			var distanceToPoint1 = plane.GetDistanceToPoint(lineStart);
			var distanceToPoint2 = plane.GetDistanceToPoint(lineEnd);
			var notIntersected = distanceToPoint1 > 0.0f && distanceToPoint2 > 0.0f ||
			                     distanceToPoint1 <= 0.0f && distanceToPoint2 <= 0.0f;
			if (notIntersected)
			{
				return false;
			}

			var totalDistance = distanceToPoint1 - distanceToPoint2;
			var ratio = distanceToPoint1 / totalDistance;
			var intersection = lineStart + (lineEnd - lineStart) * ratio;

			var distanceSqr = lengthsq(proximityCheckingPoint - intersection);
			return distanceSqr < proximityCheckingRadius * proximityCheckingRadius;
		}
#endif

		#endregion
	}

}
