using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct Vector3Int
	{
		public int x;
		public int y;
		public int z;

		public Vector3Int(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static Vector3Int operator -(Vector3Int a)
		{
			a.x = -a.x;
			a.y = -a.y;
			a.z = -a.z;
			return a;
		}

		public static Vector3Int operator -(Vector3Int a, Vector3Int b)
		{
			return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3Int operator +(Vector3Int a, Vector3Int b)
		{
			return new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3 operator *(float d, Vector3Int a)
		{
			return new Vector3(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3Int operator *(int d, Vector3Int a)
		{
			return new Vector3Int(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3 operator *(Vector3Int a, float d)
		{
			return new Vector3(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3Int operator *(Vector3Int a, int d)
		{
			return new Vector3Int(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3Int operator /(Vector3Int a, float d)
		{
			return new Vector3Int((int)(a.x / d), (int)(a.y / d), (int)(a.z / d));
		}

		public static Vector3Int operator /(Vector3Int a, int d)
		{
			return new Vector3Int(a.x / d, a.y / d, a.z / d);
		}

		public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
		}

		public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
		}

		public static bool operator >(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x > rhs.x && lhs.y > rhs.y && lhs.z > rhs.z;
		}

		public static bool operator >=(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x >= rhs.x && lhs.y >= rhs.y && lhs.z >= rhs.z;
		}

		public static bool operator <(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x < rhs.x && lhs.y < rhs.y && lhs.z < rhs.z;
		}

		public static bool operator <=(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x <= rhs.x && lhs.y <= rhs.y && lhs.z <= rhs.z;
		}

		public override bool Equals(object other)
		{
			if (other is Vector3Int)
				return Equals((Vector3Int)other);
			else
				return false;
		}

		public bool Equals(Vector3Int other)
		{
			return x == other.x && y == other.y && z == other.z;
		}

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
		}

		public static Vector3Int zero = new Vector3Int(0, 0, 0);
		public static Vector3Int one = new Vector3Int(1, 1, 1);
		public static Vector3Int minValue = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		public static Vector3Int maxValue = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

		public static Vector3Int up = new Vector3Int(0, 1, 0);
		public static Vector3Int down = new Vector3Int(0, -1, 0);
		public static Vector3Int right = new Vector3Int(1, 0, 0);
		public static Vector3Int left = new Vector3Int(-1, 0, 0);
		public static Vector3Int forward = new Vector3Int(0, 0, 1);
		public static Vector3Int back = new Vector3Int(0, 0, -1);

		public static Vector3Int positiveX = new Vector3Int(1, 0, 0);
		public static Vector3Int negativeX = new Vector3Int(-1, 0, 0);
		public static Vector3Int positiveY = new Vector3Int(0, 1, 0);
		public static Vector3Int negativeY = new Vector3Int(0, -1, 0);
		public static Vector3Int positiveZ = new Vector3Int(0, 0, 1);
		public static Vector3Int negativeZ = new Vector3Int(0, 0, -1);

		public float magnitude
		{
			get { return Mathf.Sqrt(x * x + y * y + z * z); }
		}

		public int sqrMagnitude
		{
			get { return x * x + y * y + z * z; }
		}

		//public Vector3 normalized { get; }

		//public int this[int index] { get; set; }

		public Vector3Int Sign
		{
			get
			{
				return new Vector3Int(
					x > 0 ? 1 : (x < 0 ? -1 : 0),
					y > 0 ? 1 : (y < 0 ? -1 : 0),
					z > 0 ? 1 : (z < 0 ? -1 : 0));
			}
		}

		public Vector3Int Abs
		{
			get
			{
				return new Vector3Int(
					Mathf.Abs(x),
					Mathf.Abs(y),
					Mathf.Abs(z));
			}
		}

		public int MinComponent
		{
			get
			{
				int xAbs = Mathf.Abs(x);
				int yAbs = Mathf.Abs(y);
				int zAbs = Mathf.Abs(z);

				// TODO: optimize like MaxComponent
				if (xAbs <= yAbs && xAbs <= zAbs) return x;
				if (yAbs <= xAbs && yAbs <= zAbs) return y;
				else return z;
			}
		}

		public int MaxComponent
		{
			get
			{
				int xAbs = Mathf.Abs(x);
				int yAbs = Mathf.Abs(y);
				int zAbs = Mathf.Abs(z);

				if (xAbs > yAbs)
				{
					if (xAbs > zAbs)
					{
						return x;
					}
					else
					{
						return z;
					}
				}
				else
				{
					if (yAbs > zAbs)
					{
						return y;
					}
					else
					{
						return z;
					}
				}
			}
		}

		//public static Vector3Int Max(Vector3Int lhs, Vector3Int rhs);
		//public static Vector3Int Min(Vector3Int lhs, Vector3Int rhs);
		//public static float Angle(Vector3Int from, Vector3Int to);
		//public static Vector3 ClampMagnitude(Vector3Int vector, float maxLength);
		//public static float Distance(Vector3Int a, Vector3Int b);
		//public static float Magnitude(Vector3Int a);
		//public static float SqrMagnitude(Vector3Int a);
		//public void Normalize();
		//public static Vector3 Normalize(Vector3Int value);
		//public void Scale(Vector3 scale);
		//public static Vector3 Scale(Vector3Int a, Vector3Int b);

		public Vector3Int RotatedY90CW
		{
			get { return new Vector3Int(z, y, -x); }
		}

		public Vector3Int RotatedY90CCW
		{
			get { return new Vector3Int(-z, y, x); }
		}

		#region Basic Checks

		public bool IsUnit()
		{
			if (x == 0)
			{
				if (y == 0)
				{
					return z == 1 || z == -1;
				}
				if (z == 0)
				{
					return y == 1 || y == -1;
				}
				return false;
			}
			if (y == 0 && z == 0)
			{
				return x == 1 || x == -1;
			}
			return false;
		}

		public bool IsAllEqual(int value) { return x == value && y == value && z == value; }
		public bool IsAnyEqual(int value) { return x == value || y == value || z == value; }
		public bool IsZero() { return IsAllZero(); }
		public bool IsAllZero() { return x == 0 && y == 0 && z == 0; }
		public bool IsAnyZero() { return x == 0 || y == 0 || z == 0; }
		public bool IsAllBelowZero() { return x < 0 && y < 0 && z < 0; }
		public bool IsAnyBelowZero() { return x < 0 || y < 0 || z < 0; }
		public bool IsAllAboveZero() { return x > 0 && y > 0 && z > 0; }
		public bool IsAnyAboveZero() { return x > 0 || y > 0 || z > 0; }
		public bool IsAllBelowOrEqualZero() { return x <= 0 && y <= 0 && z <= 0; }
		public bool IsAnyBelowOrEqualZero() { return x <= 0 || y <= 0 || z <= 0; }
		public bool IsAllAboveOrEqualZero() { return x >= 0 && y >= 0 && z >= 0; }
		public bool IsAnyAboveOrEqualZero() { return x >= 0 || y >= 0 || z >= 0; }

		public bool IsAllMininum() { return x == int.MinValue && y == int.MinValue && z == int.MinValue; }
		public bool IsAllMaximum() { return x == int.MaxValue && y == int.MaxValue && z == int.MaxValue; }
		public bool IsAnyMininum() { return x == int.MinValue || y == int.MinValue || z == int.MinValue; }
		public bool IsAnyMaximum() { return x == int.MaxValue || y == int.MaxValue || z == int.MaxValue; }

		public bool IsAllMininumOrMaximum()
		{
			return
				(x == int.MinValue || x == int.MaxValue) &&
				(y == int.MinValue || y == int.MaxValue) &&
				(z == int.MinValue || z == int.MaxValue);
		}

		public bool IsAnyMininumOrMaximum()
		{
			return
				(x == int.MinValue || x == int.MaxValue) ||
				(y == int.MinValue || y == int.MaxValue) ||
				(z == int.MinValue || z == int.MaxValue);
		}

		#endregion

		#region Serialization

		public static string Serialize(Vector3Int value, char separator = ' ')
		{
			return value.x.ToString() + separator + value.y.ToString() + separator + value.z.ToString();
		}

		public static bool Deserialize(string valueString, out Vector3Int result, char separator = ' ')
		{
			if (!string.IsNullOrEmpty(valueString))
			{
				var split = valueString.Split(separator);
				if (split.Length == 3)
				{
					Vector3Int value;
					if (int.TryParse(split[0], out value.x))
					{
						if (int.TryParse(split[1], out value.y))
						{
							if (int.TryParse(split[2], out value.z))
							{
								result = value;
								return true;
							}
						}
					}
				}
			}
			result.x = 0;
			result.y = 0;
			result.z = 0;
			return false;
		}

		public static Vector3Int Parse(string text)
		{
			string[] parts = text.Split(' ');
			return new Vector3Int(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
		}

		public static Vector3Int ParseSafe(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string[] parts = text.Split(' ');
				if (parts.Length == 3)
				{
					int x;
					int y;
					int z;
					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y) && int.TryParse(parts[2], out z))
					{
						return new Vector3Int(x, y, z);
					}
				}
			}
			return zero;
		}

		public static bool TryParse(string text, out Vector3Int value)
		{
			string[] parts = text.Split(' ');
			if (int.TryParse(parts[0], out value.x))
				if (int.TryParse(parts[1], out value.y))
					if (int.TryParse(parts[2], out value.z))
						return true;
			value = zero;
			return false;
		}

		public override string ToString()
		{
			return x + " " + y + " " + z;
		}

		//public string ToString(string format);

		#endregion

		#region Conversion

		public Vector3 ToVector3()
		{
			return new Vector3(x, y, z);
		}

		#endregion
	}

	public static class Vector3IntExt
	{
		public static void Write(this BinaryWriter writer, Vector3Int value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
			writer.Write(value.z);
		}

		public static Vector3Int ReadVector3Int(this BinaryReader reader)
		{
			return new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		}

		public static Vector3Int ToVector3Int(this Vector3 value)
		{
			return new Vector3Int((int)value.x, (int)value.y, (int)value.z);
		}
	}

}
