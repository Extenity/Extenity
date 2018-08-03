using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Extenity.MathToolbox
{

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Bounds2Int
	{
		[SerializeField]
		public Vector2Int min;
		[SerializeField]
		public Vector2Int max;

		//public Bounds2Int()
		//{
		//	min = Vector2Int.maxValue;
		//	max = Vector2Int.minValue;
		//}

		public static Bounds2Int CreateInvalid()
		{
			return new Bounds2Int(
				Vector2IntTools.maxValue,
				Vector2IntTools.minValue);
		}

		public Bounds2Int(Vector2Int min, Vector2Int max)
		{
			this.min = min;
			this.max = max;
		}

		public Bounds2Int(int minX, int minY, int maxX, int maxY)
		{
			min = new Vector2Int(minX, minY);
			max = new Vector2Int(maxX, maxY);
		}

		public void Reset()
		{
			min = Vector2IntTools.maxValue;
			max = Vector2IntTools.minValue;
		}

		public bool Contains(Vector2Int point)
		{
			return
				point.x >= min.x &&
				point.y >= min.y &&
				point.x <= max.x &&
				point.y <= max.y;
		}

		public void Encapsulate(Bounds2Int bounds)
		{
			Encapsulate(bounds.min);
			Encapsulate(bounds.max);
		}

		public void Encapsulate(Vector2Int point)
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

		public void Expand(Vector2Int amount)
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

		public void SetMinMax(Vector2Int min, Vector2Int max)
		{
			this.min = min;
			this.max = max;
		}

		//public float SqrDistance(Vector2 point);
		//public override string ToString();
		//public string ToString(string format);

		public Vector2Int CenterInt
		{
			get
			{
				var tmp = max - min + Vector2Int.one;
				tmp.x /= 2;
				tmp.y /= 2;
				return min + tmp;
			}
		}

		public Vector2 Center
		{
			get { return min.ToVector2() + (max - min + Vector2Int.one).ToVector2() / 2f; }
		}

		public Vector2Int Size
		{
			get { return max - min + Vector2Int.one; }
		}

		public Vector2 SizeVector2
		{
			get { return (max - min + Vector2Int.one).ToVector2(); }
		}

		public float Diagonal
		{
			get { return Size.magnitude; }
		}

		public float HalfDiagonal
		{
			get { return Size.magnitude * 0.5f; }
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
