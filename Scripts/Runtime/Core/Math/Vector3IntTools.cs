using System.IO;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class Vector3IntTools
	{
		public static readonly Vector3Int zero = new Vector3Int(0, 0, 0);
		public static readonly Vector3Int one = new Vector3Int(1, 1, 1);
		public static readonly Vector3Int minValue = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		public static readonly Vector3Int maxValue = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

		public static readonly Vector3Int up = new Vector3Int(0, 1, 0);
		public static readonly Vector3Int down = new Vector3Int(0, -1, 0);
		public static readonly Vector3Int right = new Vector3Int(1, 0, 0);
		public static readonly Vector3Int left = new Vector3Int(-1, 0, 0);
		public static readonly Vector3Int forward = new Vector3Int(0, 0, 1);
		public static readonly Vector3Int back = new Vector3Int(0, 0, -1);

		public static readonly Vector3Int positiveX = new Vector3Int(1, 0, 0);
		public static readonly Vector3Int negativeX = new Vector3Int(-1, 0, 0);
		public static readonly Vector3Int positiveY = new Vector3Int(0, 1, 0);
		public static readonly Vector3Int negativeY = new Vector3Int(0, -1, 0);
		public static readonly Vector3Int positiveZ = new Vector3Int(0, 0, 1);
		public static readonly Vector3Int negativeZ = new Vector3Int(0, 0, -1);

		public static readonly Vector3Int[] directions =
		{
			left, right,
			down, up,
			back, forward,
		};

		#region Magnitude And Distance

		public static float Distance(Vector3Int a, Vector3Int b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static int SqrDistance(Vector3Int a, Vector3Int b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			var dz = b.z - a.z;
			return dx * dx + dy * dy + dz * dz;
		}

		#endregion

		public static Vector3Int Sign(this Vector3Int vector)
		{
			return new Vector3Int(
				vector.x > 0 ? 1 : (vector.x < 0 ? -1 : 0),
				vector.y > 0 ? 1 : (vector.y < 0 ? -1 : 0),
				vector.z > 0 ? 1 : (vector.z < 0 ? -1 : 0));
		}

		public static Vector3Int Abs(this Vector3Int vector)
		{
			return new Vector3Int(
				Mathf.Abs(vector.x),
				Mathf.Abs(vector.y),
				Mathf.Abs(vector.z));
		}

		public static int MinComponent(this Vector3Int vector)
		{
			int xAbs = Mathf.Abs(vector.x);
			int yAbs = Mathf.Abs(vector.y);
			int zAbs = Mathf.Abs(vector.z);

			// TODO: optimize like MaxComponent
			if (xAbs <= yAbs && xAbs <= zAbs) return vector.x;
			if (yAbs <= xAbs && yAbs <= zAbs) return vector.y;
			else return vector.z;
		}

		public static int MaxComponent(this Vector3Int vector)
		{
			int xAbs = Mathf.Abs(vector.x);
			int yAbs = Mathf.Abs(vector.y);
			int zAbs = Mathf.Abs(vector.z);

			if (xAbs > yAbs)
			{
				if (xAbs > zAbs)
				{
					return vector.x;
				}
				else
				{
					return vector.z;
				}
			}
			else
			{
				if (yAbs > zAbs)
				{
					return vector.y;
				}
				else
				{
					return vector.z;
				}
			}
		}

		public static int MultiplyComponents(this Vector3Int vector)
		{
			return vector.x * vector.y * vector.y;
		}

		public static void SwapToMakeLesserAndGreater(ref Vector3Int shouldBeLesser, ref Vector3Int shouldBeGreater)
		{
			if (shouldBeLesser.x > shouldBeGreater.x)
			{
				var tmp = shouldBeLesser.x;
				shouldBeLesser.x = shouldBeGreater.x;
				shouldBeGreater.x = tmp;
			}
			if (shouldBeLesser.y > shouldBeGreater.y)
			{
				var tmp = shouldBeLesser.y;
				shouldBeLesser.y = shouldBeGreater.y;
				shouldBeGreater.y = tmp;
			}
			if (shouldBeLesser.z > shouldBeGreater.z)
			{
				var tmp = shouldBeLesser.z;
				shouldBeLesser.z = shouldBeGreater.z;
				shouldBeGreater.z = tmp;
			}
		}

		public static Vector3Int RotatedY90CW(this Vector3Int vector)
		{
			return new Vector3Int(vector.z, vector.y, -vector.x);
		}

		public static Vector3Int RotatedY90CCW(this Vector3Int vector)
		{
			return new Vector3Int(-vector.z, vector.y, vector.x);
		}

		#region Basic Checks

		public static bool IsUnit(this Vector3Int vector)
		{
			if (vector.x == 0)
			{
				if (vector.y == 0)
				{
					return vector.z == 1 || vector.z == -1;
				}
				if (vector.z == 0)
				{
					return vector.y == 1 || vector.y == -1;
				}
				return false;
			}
			if (vector.y == 0 && vector.z == 0)
			{
				return vector.x == 1 || vector.x == -1;
			}
			return false;
		}

		public static bool IsAllEqual(this Vector3Int vector, int value) { return vector.x == value && vector.y == value && vector.z == value; }
		public static bool IsAnyEqual(this Vector3Int vector, int value) { return vector.x == value || vector.y == value || vector.z == value; }
		public static bool IsZero(this Vector3Int vector) { return IsAllZero(vector); }
		public static bool IsAllZero(this Vector3Int vector) { return vector.x == 0 && vector.y == 0 && vector.z == 0; }
		public static bool IsAnyZero(this Vector3Int vector) { return vector.x == 0 || vector.y == 0 || vector.z == 0; }
		public static bool IsAllBelowZero(this Vector3Int vector) { return vector.x < 0 && vector.y < 0 && vector.z < 0; }
		public static bool IsAnyBelowZero(this Vector3Int vector) { return vector.x < 0 || vector.y < 0 || vector.z < 0; }
		public static bool IsAllAboveZero(this Vector3Int vector) { return vector.x > 0 && vector.y > 0 && vector.z > 0; }
		public static bool IsAnyAboveZero(this Vector3Int vector) { return vector.x > 0 || vector.y > 0 || vector.z > 0; }
		public static bool IsAllBelowOrEqualZero(this Vector3Int vector) { return vector.x <= 0 && vector.y <= 0 && vector.z <= 0; }
		public static bool IsAnyBelowOrEqualZero(this Vector3Int vector) { return vector.x <= 0 || vector.y <= 0 || vector.z <= 0; }
		public static bool IsAllAboveOrEqualZero(this Vector3Int vector) { return vector.x >= 0 && vector.y >= 0 && vector.z >= 0; }
		public static bool IsAnyAboveOrEqualZero(this Vector3Int vector) { return vector.x >= 0 || vector.y >= 0 || vector.z >= 0; }

		public static bool IsAllMininum(this Vector3Int vector) { return vector.x == int.MinValue && vector.y == int.MinValue && vector.z == int.MinValue; }
		public static bool IsAllMaximum(this Vector3Int vector) { return vector.x == int.MaxValue && vector.y == int.MaxValue && vector.z == int.MaxValue; }
		public static bool IsAnyMininum(this Vector3Int vector) { return vector.x == int.MinValue || vector.y == int.MinValue || vector.z == int.MinValue; }
		public static bool IsAnyMaximum(this Vector3Int vector) { return vector.x == int.MaxValue || vector.y == int.MaxValue || vector.z == int.MaxValue; }

		public static bool IsAllMininumOrMaximum(this Vector3Int vector)
		{
			return
				(vector.x == int.MinValue || vector.x == int.MaxValue) &&
				(vector.y == int.MinValue || vector.y == int.MaxValue) &&
				(vector.z == int.MinValue || vector.z == int.MaxValue);
		}

		public static bool IsAnyMininumOrMaximum(this Vector3Int vector)
		{
			return
				(vector.x == int.MinValue || vector.x == int.MaxValue) ||
				(vector.y == int.MinValue || vector.y == int.MaxValue) ||
				(vector.z == int.MinValue || vector.z == int.MaxValue);
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
					if (int.TryParse(split[0], out var x))
					{
						if (int.TryParse(split[1], out var y))
						{
							if (int.TryParse(split[2], out var z))
							{
								result = new Vector3Int(x, y, z);
								return true;
							}
						}
					}
				}
			}
			result = minValue;
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
					if (int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y) && int.TryParse(parts[2], out var z))
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
			if (int.TryParse(parts[0], out var x))
			{
				if (int.TryParse(parts[1], out var y))
				{
					if (int.TryParse(parts[2], out var z))
					{
						value = new Vector3Int(x, y, z);
						return true;
					}
				}
			}
			value = minValue;
			return false;
		}

		#endregion

		#region Serialization - BinaryWriter/BinaryReader

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

		#endregion

		#region Conversion

		public static Vector3 ToVector3(this Vector3Int vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector3Int ToVector3Int(this Vector3 value)
		{
			return new Vector3Int((int)value.x, (int)value.y, (int)value.z);
		}

		#endregion
	}

}
