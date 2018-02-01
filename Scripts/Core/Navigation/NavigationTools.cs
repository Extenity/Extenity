using UnityEngine;
using UnityEngine.AI;

namespace Extenity.NavigationToolbox
{

	public static class NavigationTools
	{
		public static void DrawPath(this NavMeshPath path, LineRenderer lineRenderer)
		{
			if (!lineRenderer)
				return;

			if (path.corners.Length < 2)
			{
				lineRenderer.positionCount = 0;
			}
			else
			{
				lineRenderer.positionCount = path.corners.Length;
				lineRenderer.SetPositions(path.corners);
			}
		}

		public static void DrawDebugPath(this NavMeshPath path, Color color, float duration = 0f, bool depthTest = false)
		{
			var corners = path.corners;
			for (int i = 0; i < corners.Length - 1; i++)
				Debug.DrawLine(corners[i], corners[i + 1], color, duration, depthTest);
		}

		#region Length

		//private const int TotalLengthCornersBufferMaximumSize = 100;
		//private static Vector3[] TotalLengthCornersBuffer;

		public static float CalculateTotalLength(this NavMeshPath path)
		{
			// TODO: Optimize. Use GetCornersNonAlloc

			var corners = path.corners;
			if (corners == null || corners.Length < 2)
				return 0f;

			var totalDistance = 0f;
			var previousCorner = corners[0];
			for (int i = 1; i < corners.Length; i++)
			{
				var corner = corners[i];
				totalDistance += (corner - previousCorner).magnitude;
				previousCorner = corner;
			}
			return totalDistance;
		}

		#endregion
	}

}
