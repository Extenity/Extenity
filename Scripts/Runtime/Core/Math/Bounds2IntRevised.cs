using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	// TODO: This type will be renamed as Bounds2Int in future. But first we need to make sure all projects using the current Bounds2Int should be migrated into the new type. Use a diff tool to see what has changed between the original and the revised types.
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Bounds2IntRevised
	{
		public int2 min;
		public int2 max;

		public static Bounds2Int CreateInvalid()
		{
			return new Bounds2Int(
				int2(int.MaxValue),
				int2(int.MinValue));
		}

		public Bounds2IntRevised(int2 min, int2 max)
		{
			this.min = min;
			this.max = max;
		}

		public Bounds2IntRevised(int minX, int minY, int maxX, int maxY)
		{
			min = new int2(minX, minY);
			max = new int2(maxX, maxY);
		}

		public void Reset()
		{
			min = int2(int.MaxValue);
			max = int2(int.MinValue);
		}

		public bool Contains(int2 point)
		{
			return
				point.x >= min.x &&
				point.y >= min.y &&
				point.x <= max.x &&
				point.y <= max.y;
		}

		public bool Contains(float2 point)
		{
			return
				point.x.FloorToInt() >= min.x &&
				point.y.FloorToInt() >= min.y &&
				point.x.CeilToInt() <= max.x &&
				point.y.CeilToInt() <= max.y;
		}

		public void Encapsulate(Bounds2Int bounds)
		{
			Encapsulate(bounds.min);
			Encapsulate(bounds.max);
		}

		public void Encapsulate(int2 point)
		{
			if (min.x > point.x) min.x = point.x;
			if (min.y > point.y) min.y = point.y;
			if (max.x < point.x) max.x = point.x;
			if (max.y < point.y) max.y = point.y;
		}

		public void Encapsulate(int pointX, int pointY)
		{
			if (min.x > pointX) min.x = pointX;
			if (min.y > pointY) min.y = pointY;
			if (max.x < pointX) max.x = pointX;
			if (max.y < pointY) max.y = pointY;
		}

		//public override bool Equals(object other);

		public void Expand(int amount)
		{
			min.x -= amount;
			min.y -= amount;
			max.x += amount;
			max.y += amount;
		}

		public void Expand(int2 amount)
		{
			min.x -= amount.x;
			min.y -= amount.y;
			max.x += amount.x;
			max.y += amount.y;
		}

		//public override int GetHashCode();
		//public bool IntersectRay(Ray ray);
		//public bool IntersectRay(Ray ray, out float distance);
		//public bool Intersects(Bounds bounds);

		public void SetMinMax(int2 min, int2 max)
		{
			this.min = min;
			this.max = max;
		}

		//public float SqrDistance(Vector2 point);
		//public override string ToString();
		//public string ToString(string format);

		public int2 CenterInt
		{
			get
			{
				return int2(
					(max.x + min.x) / 2,
					(max.y + min.y) / 2);
			}
		}

		public float2 Center
		{
			get
			{
				return float2(
					(max.x + min.x) * 0.5f,
					(max.y + min.y) * 0.5f);
			}
		}

		public int2 Size
		{
			get
			{
				return int2(
					max.x - min.x,
					max.y - min.y);
			}
		}

		public float Diagonal
		{
			get { return length(float2(Size)); }
		}

		public bool IsInvalid
		{
			get { return min.x > max.x || min.y > max.y; }
		}

		//public Bounds2 ToBounds()
		//{
		//	return new Bounds2(Center, SizeVector2);
		//}
	}

}
