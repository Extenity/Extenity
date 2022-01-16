using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Extenity.MathToolbox
{

	public struct Polygon2In3
	{
		public IList<float3> Points;
		public Bounds2 Bounds;

		public Polygon2In3(IList<float3> polygon, Bounds2 bounds)
		{
			if (polygon == null || polygon.Count < 2)
			{
				throw new Exception("Invalid polygon points.");
			}

			Points = polygon;
			Bounds = bounds;
		}

		public static Polygon2In3 CreateXY(IList<float3> polygon)
		{
			return new Polygon2In3(polygon, polygon.CalculateBoundsXY());
		}

		public static Polygon2In3 CreateXZ(IList<float3> polygon)
		{
			return new Polygon2In3(polygon, polygon.CalculateBoundsXZ());
		}

		public bool IsPointInsidePolygonXY(float2 point)
		{
			if (!Bounds.Contains(point))
			{
				return false;
			}
			return Points.IsPointInsidePolygonXY(point);
		}

		public bool IsPointInsidePolygonXZ(float2 point)
		{
			if (!Bounds.Contains(point))
			{
				return false;
			}
			return Points.IsPointInsidePolygonXZ(point);
		}
	}

}
