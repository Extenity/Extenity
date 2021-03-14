using System.Collections.Generic;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class BoundsTools
	{
		public static Bounds2 CalculateBounds(this IList<Vector2> points)
		{
			var minX = points[0].x;
			var maxX = points[0].x;
			var minY = points[0].y;
			var maxY = points[0].y;
			for (int i = 1; i < points.Count; i++)
			{
				var q = points[i];
				minX = Mathf.Min(q.x, minX);
				maxX = Mathf.Max(q.x, maxX);
				minY = Mathf.Min(q.y, minY);
				maxY = Mathf.Max(q.y, maxY);
			}
			var bounds = new Bounds2();
			bounds.SetMinMax(new Vector2(minX, minY), new Vector2(maxX, maxY));
			return bounds;
		}

		public static Bounds2 CalculateBoundsXY(this IList<Vector3> points)
		{
			var minX = points[0].x;
			var maxX = points[0].x;
			var minY = points[0].y;
			var maxY = points[0].y;
			for (int i = 1; i < points.Count; i++)
			{
				var q = points[i];
				minX = Mathf.Min(q.x, minX);
				maxX = Mathf.Max(q.x, maxX);
				minY = Mathf.Min(q.y, minY);
				maxY = Mathf.Max(q.y, maxY);
			}
			var bounds = new Bounds2();
			bounds.SetMinMax(new Vector2(minX, minY), new Vector2(maxX, maxY));
			return bounds;
		}

		public static Bounds2 CalculateBoundsXZ(this IList<Vector3> points)
		{
			var minX = points[0].x;
			var maxX = points[0].x;
			var minY = points[0].z;
			var maxY = points[0].z;
			for (int i = 1; i < points.Count; i++)
			{
				var q = points[i];
				minX = Mathf.Min(q.x, minX);
				maxX = Mathf.Max(q.x, maxX);
				minY = Mathf.Min(q.z, minY);
				maxY = Mathf.Max(q.z, maxY);
			}
			var bounds = new Bounds2();
			bounds.SetMinMax(new Vector2(minX, minY), new Vector2(maxX, maxY));
			return bounds;
		}
	}

}
