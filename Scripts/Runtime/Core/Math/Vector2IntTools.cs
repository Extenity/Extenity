#if UNITY_5_3_OR_NEWER // TODO-UniversalExtenity: Convert these to Mathematics after importing it into Universal project.

using System.IO;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class Vector2IntTools
	{
		public static int GetIndex(this Vector2Int vector, int width)
		{
			return vector.y * width + vector.x;
		}

		public static int GetIndex(int x, int y, int width)
		{
			return y * width + x;
		}

		public static Vector2Int CreateFromIndex(int index, int width)
		{
			var x = index % width;
			return new Vector2Int(x, (index - x) / width);
		}


		public static readonly Vector2Int zero = new Vector2Int(0, 0);
		public static readonly Vector2Int one = new Vector2Int(1, 1);
		public static readonly Vector2Int minValue = new Vector2Int(int.MinValue, int.MinValue);
		public static readonly Vector2Int maxValue = new Vector2Int(int.MaxValue, int.MaxValue);

		public static readonly Vector2Int up = new Vector2Int(0, 1);
		public static readonly Vector2Int down = new Vector2Int(0, -1);
		public static readonly Vector2Int right = new Vector2Int(1, 0);
		public static readonly Vector2Int left = new Vector2Int(-1, 0);

		public static readonly Vector2Int[] directions =
		{
			left, right,
			down, up,
		};

		#region Magnitude And Distance

		public static float Distance(Vector2Int a, Vector2Int b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return sqrt(dx * dx + dy * dy);
		}

		public static int SqrDistance(Vector2Int a, Vector2Int b)
		{
			var dx = b.x - a.x;
			var dy = b.y - a.y;
			return dx * dx + dy * dy;
		}

		#endregion

		public static Vector2Int Sign(this Vector2Int vector)
		{
			return new Vector2Int(
				vector.x > 0 ? 1 : (vector.x < 0 ? -1 : 0),
				vector.y > 0 ? 1 : (vector.y < 0 ? -1 : 0));
		}

		public static Vector2Int Abs(this Vector2Int vector)
		{
			return new Vector2Int(
				abs(vector.x),
				abs(vector.y));
		}

		public static int MinComponent(this Vector2Int vector)
		{
			int xAbs = abs(vector.x);
			int yAbs = abs(vector.y);

			if (xAbs < yAbs)
				return vector.x;
			else
				return vector.y;
		}

		public static int MaxComponent(this Vector2Int vector)
		{
			int xAbs = abs(vector.x);
			int yAbs = abs(vector.y);

			if (xAbs > yAbs)
				return vector.x;
			else
				return vector.y;
		}

		public static int MultiplyComponents(this Vector2Int vector)
		{
			return vector.x * vector.y;
		}

		public static void SwapToMakeLesserAndGreater(ref Vector2Int shouldBeLesser, ref Vector2Int shouldBeGreater)
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
		}

		#region Basic Checks

		public static bool IsUnit(this Vector2Int vector)
		{
			if (vector.x == 0)
			{
				return vector.y == 1 || vector.y == -1;
			}
			if (vector.y == 0)
			{
				return vector.x == 1 || vector.x == -1;
			}
			return false;
		}

		public static bool IsAllEqual(this Vector2Int vector, int value) { return vector.x == value && vector.y == value; }
		public static bool IsAnyEqual(this Vector2Int vector, int value) { return vector.x == value || vector.y == value; }
		public static bool IsZero(this Vector2Int vector) { return IsAllZero(vector); }
		public static bool IsAllZero(this Vector2Int vector) { return vector.x == 0 && vector.y == 0; }
		public static bool IsAnyZero(this Vector2Int vector) { return vector.x == 0 || vector.y == 0; }
		public static bool IsAllBelowZero(this Vector2Int vector) { return vector.x < 0 && vector.y < 0; }
		public static bool IsAnyBelowZero(this Vector2Int vector) { return vector.x < 0 || vector.y < 0; }
		public static bool IsAllAboveZero(this Vector2Int vector) { return vector.x > 0 && vector.y > 0; }
		public static bool IsAnyAboveZero(this Vector2Int vector) { return vector.x > 0 || vector.y > 0; }
		public static bool IsAllBelowOrEqualZero(this Vector2Int vector) { return vector.x <= 0 && vector.y <= 0; }
		public static bool IsAnyBelowOrEqualZero(this Vector2Int vector) { return vector.x <= 0 || vector.y <= 0; }
		public static bool IsAllAboveOrEqualZero(this Vector2Int vector) { return vector.x >= 0 && vector.y >= 0; }
		public static bool IsAnyAboveOrEqualZero(this Vector2Int vector) { return vector.x >= 0 || vector.y >= 0; }

		public static bool IsAllMinimum(this Vector2Int vector) { return vector.x == int.MinValue && vector.y == int.MinValue; }
		public static bool IsAllMaximum(this Vector2Int vector) { return vector.x == int.MaxValue && vector.y == int.MaxValue; }
		public static bool IsAnyMinimum(this Vector2Int vector) { return vector.x == int.MinValue || vector.y == int.MinValue; }
		public static bool IsAnyMaximum(this Vector2Int vector) { return vector.x == int.MaxValue || vector.y == int.MaxValue; }

		public static bool IsAllMinimumOrMaximum(this Vector2Int vector)
		{
			return
				(vector.x == int.MinValue || vector.x == int.MaxValue) &&
				(vector.y == int.MinValue || vector.y == int.MaxValue);
		}

		public static bool IsAnyMinimumOrMaximum(this Vector2Int vector)
		{
			return
				(vector.x == int.MinValue || vector.x == int.MaxValue) ||
				(vector.y == int.MinValue || vector.y == int.MaxValue);
		}

		#endregion

		#region Serialization

		public static string Serialize(this Vector2Int value, char separator = ' ')
		{
			return value.x.ToString() + separator + value.y.ToString();
		}

		public static bool Deserialize(string valueString, out Vector2Int result, char separator = ' ')
		{
			if (!string.IsNullOrEmpty(valueString))
			{
				var split = valueString.Split(separator);
				if (split.Length == 2)
				{
					if (int.TryParse(split[0], out var x))
					{
						if (int.TryParse(split[1], out var y))
						{
							result = new Vector2Int(x, y);
							return true;
						}
					}
				}
			}
			result = minValue;
			return false;
		}

		public static Vector2Int Parse(string text)
		{
			string[] parts = text.Split(' ');
			return new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
		}

		public static Vector2Int ParseSafe(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string[] parts = text.Split(' ');
				if (parts.Length == 2)
				{
					if (int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y))
					{
						return new Vector2Int(x, y);
					}
				}
			}
			return zero;
		}

		public static bool TryParse(string text, out Vector2Int value)
		{
			string[] parts = text.Split(' ');
			if (int.TryParse(parts[0], out var x))
			{
				if (int.TryParse(parts[1], out var y))
				{
					value = new Vector2Int(x, y);
					return true;
				}
			}
			value = minValue;
			return false;
		}

		#endregion

		#region Serialization - BinaryWriter/BinaryReader

		public static void Write(this BinaryWriter writer, Vector2Int value)
		{
			writer.Write(value.x);
			writer.Write(value.y);
		}

		public static Vector2Int ReadVector2Int(this BinaryReader reader)
		{
			return new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
		}

		#endregion

		#region Conversion

		public static Vector2 ToVector2(this Vector2Int vector)
		{
			return new Vector2(vector.x, vector.y);
		}

		public static Vector2Int ToVector2Int(this Vector2 value)
		{
			return new Vector2Int((int)value.x, (int)value.y);
		}

		#endregion
		
		#region Manipulate Components

		public static void ChangeZerosTo(ref Vector2Int value, int changeTo)
		{
			if (value.x == 0) value.x = changeTo;
			if (value.y == 0) value.y = changeTo;
		}

		// @formatter:off
		public static Vector2Int WithX(this Vector2Int value, int overriddenX) { value.x = overriddenX; return value; }
		public static Vector2Int WithY(this Vector2Int value, int overriddenY) { value.y = overriddenY; return value; }

		public static Vector2Int AddX(this Vector2Int value, int addedX) { value.x += addedX; return value; }
		public static Vector2Int AddY(this Vector2Int value, int addedY) { value.y += addedY; return value; }
		// @formatter:on

		#endregion
	}

}

#endif
