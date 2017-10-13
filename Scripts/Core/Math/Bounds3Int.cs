using UnityEngine;

namespace Extenity.MathToolbox
{

	public struct Bounds3Int
	{
		public Vector3Int min;
		public Vector3Int max;

		//public Bounds3Int()
		//{
		//    min = Vector3Int.maxValue;
		//    max = Vector3Int.minValue;
		//}

		public static Bounds3Int CreateInvalid()
		{
			return new Bounds3Int(Vector3IntTools.maxValue, Vector3IntTools.minValue);
		}

		public Bounds3Int(Vector3Int min, Vector3Int max)
		{
			this.min = min;
			this.max = max;
		}

		public Bounds3Int(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
		{
			min = new Vector3Int(minX, minY, minZ);
			max = new Vector3Int(maxX, maxY, maxZ);
		}

		public void Reset()
		{
			min = Vector3IntTools.maxValue;
			max = Vector3IntTools.minValue;
		}

		public bool Contains(Vector3Int point)
		{
			return
				point.x >= min.x &&
				point.y >= min.y &&
				point.z >= min.z &&
				point.x <= max.x &&
				point.y <= max.y &&
				point.z <= max.z;
		}

		public void Encapsulate(Bounds3Int bounds)
		{
			Encapsulate(bounds.min);
			Encapsulate(bounds.max);
		}

		public void Encapsulate(Vector3Int point)
		{
			if (min.x > point.x) min.x = point.x;
			if (min.y > point.y) min.y = point.y;
			if (min.z > point.z) min.z = point.z;
			if (max.x < point.x) max.x = point.x;
			if (max.y < point.y) max.y = point.y;
			if (max.z < point.z) max.z = point.z;
		}

		public void Encapsulate(int pointX, int pointY, int pointZ)
		{
			if (min.x > pointX) min.x = pointX;
			if (min.y > pointY) min.y = pointY;
			if (min.z > pointZ) min.z = pointZ;
			if (max.x < pointX) max.x = pointX;
			if (max.y < pointY) max.y = pointY;
			if (max.z < pointZ) max.z = pointZ;
		}

		//public override bool Equals(object other);

		public void Expand(int amount)
		{
			min.x -= amount;
			min.y -= amount;
			min.z -= amount;
			max.x += amount;
			max.y += amount;
			max.z += amount;
		}

		public void Expand(Vector3Int amount)
		{
			min.x -= amount.x;
			min.y -= amount.y;
			min.z -= amount.z;
			max.x += amount.x;
			max.y += amount.y;
			max.z += amount.z;
		}

		//public override int GetHashCode();
		//public bool IntersectRay(Ray ray);
		//public bool IntersectRay(Ray ray, out float distance);
		//public bool Intersects(Bounds bounds);

		public void SetMinMax(Vector3Int min, Vector3Int max)
		{
			this.min = min;
			this.max = max;
		}

		//public float SqrDistance(Vector3 point);
		//public override string ToString();
		//public string ToString(string format);

		public Vector3Int CenterInt
		{
			get
			{
				var tmp = max - min + Vector3Int.one;
				tmp.x /= 2;
				tmp.y /= 2;
				tmp.z /= 2;
				return min + tmp;
			}
		}

		public Vector3 Center
		{
			get { return min.ToVector3() + (max - min + Vector3Int.one).ToVector3() / 2f; }
		}

		public Vector3Int CenterOfNegativeXSurface
		{
			get
			{
				return new Vector3Int(
					min.x,
					min.y + (max.y - min.y + 1) / 2,
					min.z + (max.z - min.z + 1) / 2);
			}
		}

		public Vector3Int CenterOfPositiveXSurface
		{
			get
			{
				return new Vector3Int(
					max.x,
					min.y + (max.y - min.y + 1) / 2,
					min.z + (max.z - min.z + 1) / 2);
			}
		}

		public Vector3Int CenterOfNegativeYSurface
		{
			get
			{
				return new Vector3Int(
					min.x + (max.x - min.x + 1) / 2,
					min.y,
					min.z + (max.z - min.z + 1) / 2);
			}
		}

		public Vector3Int CenterOfPositiveYSurface
		{
			get
			{
				return new Vector3Int(
					min.x + (max.x - min.x + 1) / 2,
					max.y,
					min.z + (max.z - min.z + 1) / 2);
			}
		}

		public Vector3Int CenterOfNegativeZSurface
		{
			get
			{
				return new Vector3Int(
					min.x + (max.x - min.x + 1) / 2,
					min.y + (max.y - min.y + 1) / 2,
					min.z);
			}
		}

		public Vector3Int CenterOfPositiveZSurface
		{
			get
			{
				return new Vector3Int(
					min.x + (max.x - min.x + 1) / 2,
					min.y + (max.y - min.y + 1) / 2,
					max.z);
			}
		}

		public Vector3Int Size
		{
			get { return max - min + Vector3Int.one; }
		}

		public Vector3 SizeVector3
		{
			get { return (max - min + Vector3Int.one).ToVector3(); }
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
			get { return min.x > max.x || min.y > max.y || min.z > max.z; }
		}

		public Bounds ToBounds()
		{
			return new Bounds(Center, SizeVector3);
		}
	}

}
