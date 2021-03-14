using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public struct Polygon2In3
	{
		public IList<Vector3> Points;
		public Bounds2 Bounds;

		public Polygon2In3(IList<Vector3> polygon, Bounds2 bounds)
		{
			if (polygon == null || polygon.Count < 2)
			{
				throw new Exception("Invalid polygon points.");
			}

			Points = polygon;
			Bounds = bounds;
		}

		public static Polygon2In3 CreateXY(IList<Vector3> polygon)
		{
			return new Polygon2In3(polygon, polygon.CalculateBoundsXY());
		}

		public static Polygon2In3 CreateXZ(IList<Vector3> polygon)
		{
			return new Polygon2In3(polygon, polygon.CalculateBoundsXZ());
		}

		public bool IsPointInsidePolygonXY(Vector2 point)
		{
			if (!Bounds.Contains(point))
			{
				return false;
			}
			return Points.IsPointInsidePolygonXY(point);
		}

		public bool IsPointInsidePolygonXZ(Vector2 point)
		{
			if (!Bounds.Contains(point))
			{
				return false;
			}
			return Points.IsPointInsidePolygonXZ(point);
		}
	}

}
