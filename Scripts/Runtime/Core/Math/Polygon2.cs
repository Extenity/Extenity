using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public struct Polygon2
	{
		public IList<Vector2> Points;
		public Bounds2 Bounds;

		public Polygon2(IList<Vector2> polygon, Bounds2 bounds)
		{
			if (polygon == null || polygon.Count < 2)
			{
				throw new Exception("Invalid polygon points.");
			}

			Points = polygon;
			Bounds = bounds;
		}

		public static Polygon2 CreateXY(IList<Vector2> polygon)
		{
			return new Polygon2(polygon, polygon.CalculateBounds());
		}

		public bool IsPointInsidePolygon(Vector2 point)
		{
			if (!Bounds.Contains(point))
			{
				return false;
			}
			return Points.IsPointInsidePolygon(point);
		}
	}

}
