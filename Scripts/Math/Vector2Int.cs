using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;

[Serializable]
public struct Vector2Int
{
	public int x;
	public int y;

	public Vector2Int(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public int GetIndex(int width)
	{
		return y * width + x;
	}

	public static int GetIndex(int x, int y, int width)
	{
		return y * width + x;
	}

	public static Vector2Int CreateFromIndex(int index, int width)
	{
		Vector2Int value;
		value.x = index % width;
		value.y = (index - value.x) / width;
		return value;
	}

	public static Vector2Int operator -(Vector2Int a)
	{
		a.x = -a.x;
		a.y = -a.y;
		return a;
	}
	public static Vector2Int operator -(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x - b.x, a.y - b.y);
	}
	public static Vector2Int operator +(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x + b.x, a.y + b.y);
	}
	public static Vector2 operator *(float d, Vector2Int a)
	{
		return new Vector2(a.x * d, a.y * d);
	}
	public static Vector2Int operator *(int d, Vector2Int a)
	{
		return new Vector2Int(a.x * d, a.y * d);
	}
	public static Vector2 operator *(Vector2Int a, float d)
	{
		return new Vector2(a.x * d, a.y * d);
	}
	public static Vector2Int operator *(Vector2Int a, int d)
	{
		return new Vector2Int(a.x * d, a.y * d);
	}
	public static Vector2Int operator /(Vector2Int a, float d)
	{
		return new Vector2Int((int)(a.x / d), (int)(a.y / d));
	}
	public static Vector2Int operator /(Vector2Int a, int d)
	{
		return new Vector2Int(a.x / d, a.y / d);
	}
	public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}
	public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}
	public static bool operator >(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x > rhs.x && lhs.y > rhs.y;
	}
	public static bool operator >=(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x >= rhs.x && lhs.y >= rhs.y;
	}
	public static bool operator <(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x < rhs.x && lhs.y < rhs.y;
	}
	public static bool operator <=(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x <= rhs.x && lhs.y <= rhs.y;
	}

	public override bool Equals(object other)
	{
		if (other is Vector2Int)
			return Equals((Vector2Int)other);
		else
			return false;
	}

	public bool Equals(Vector2Int other)
	{
		return x == other.x && y == other.y;
	}

	[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode() << 2;
	}

	public static Vector2Int zero = new Vector2Int(0, 0);
	public static Vector2Int one = new Vector2Int(1, 1);
	public static Vector2Int minValue = new Vector2Int(int.MinValue, int.MinValue);
	public static Vector2Int maxValue = new Vector2Int(int.MaxValue, int.MaxValue);

	public static Vector2Int up = new Vector2Int(0, 1);
	public static Vector2Int down = new Vector2Int(0, -1);
	public static Vector2Int right = new Vector2Int(1, 0);
	public static Vector2Int left = new Vector2Int(-1, 0);

	//public int this[int index] { get; set; }

	public float magnitude
	{
		get { return Mathf.Sqrt(x * x + y * y); }
	}

	public float sqrMagnitude
	{
		get { return (x * x + y * y); }
	}

	public float distance(Vector2Int b)
	{
		return (this - b).magnitude;
	}

	public Vector2Int Sign
	{
		get
		{
			return new Vector2Int(
				x > 0 ? 1 : (x < 0 ? -1 : 0),
				y > 0 ? 1 : (y < 0 ? -1 : 0));
		}
	}

	public Vector2Int Abs
	{
		get
		{
			return new Vector2Int(
				Mathf.Abs(x),
				Mathf.Abs(y));
		}
	}

	public int MinComponent
	{
		get
		{
			int xAbs = Mathf.Abs(x);
			int yAbs = Mathf.Abs(y);

			if (xAbs < yAbs)
				return x;
			else
				return y;
		}
	}

	public int MaxComponent
	{
		get
		{
			int xAbs = Mathf.Abs(x);
			int yAbs = Mathf.Abs(y);

			if (xAbs > yAbs)
				return x;
			else
				return y;
		}
	}

	//public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs);
	//public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs);
	//public static float Angle(Vector2Int from, Vector2Int to);
	//public static Vector2 ClampMagnitude(Vector2Int vector, float maxLength);
	//public void Normalize();
	//public static Vector2 Normalize(Vector2Int value);
	//public void Scale(Vector2 scale);
	//public static Vector2 Scale(Vector2Int a, Vector2Int b);

	#region Basic Checks

	public bool IsUnit()
	{
		if (x == 0)
		{
			return y == 1 || y == -1;
		}
		if (y == 0)
		{
			return x == 1 || x == -1;
		}
		return false;
	}

	public bool IsAllEqual(int value) { return x == value && y == value; }
	public bool IsAnyEqual(int value) { return x == value || y == value; }
	public bool IsZero() { return IsAllZero(); }
	public bool IsAllZero() { return x == 0 && y == 0; }
	public bool IsAnyZero() { return x == 0 || y == 0; }
	public bool IsAllBelowZero() { return x < 0 && y < 0; }
	public bool IsAnyBelowZero() { return x < 0 || y < 0; }
	public bool IsAllAboveZero() { return x > 0 && y > 0; }
	public bool IsAnyAboveZero() { return x > 0 || y > 0; }
	public bool IsAllBelowOrEqualZero() { return x <= 0 && y <= 0; }
	public bool IsAnyBelowOrEqualZero() { return x <= 0 || y <= 0; }
	public bool IsAllAboveOrEqualZero() { return x >= 0 && y >= 0; }
	public bool IsAnyAboveOrEqualZero() { return x >= 0 || y >= 0; }

	public bool IsAllMininum() { return x == int.MinValue && y == int.MinValue; }
	public bool IsAllMaximum() { return x == int.MaxValue && y == int.MaxValue; }
	public bool IsAnyMininum() { return x == int.MinValue || y == int.MinValue; }
	public bool IsAnyMaximum() { return x == int.MaxValue || y == int.MaxValue; }

	public bool IsAllMininumOrMaximum()
	{
		return
			(x == int.MinValue || x == int.MaxValue) &&
			(y == int.MinValue || y == int.MaxValue);
	}

	public bool IsAnyMininumOrMaximum()
	{
		return
			(x == int.MinValue || x == int.MaxValue) ||
			(y == int.MinValue || y == int.MaxValue);
	}

	#endregion

	#region Serialization

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
				int x;
				int y;
				if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
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
		if (int.TryParse(parts[0], out value.x))
			if (int.TryParse(parts[1], out value.y))
				return true;
		value = zero;
		return false;
	}

	//public string ToString(string format);

	public override string ToString()
	{
		return x + " " + y;
	}

	#endregion

	#region Conversion

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	#endregion
}

public static class Vector2IntExt
{
	public static void Write(this BinaryWriter writer, Vector2Int value)
	{
		writer.Write(value.x);
		writer.Write(value.y);
	}

	public static Vector2Int ReadVector2Int(this BinaryReader reader)
	{
		return new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
	}

	public static Vector2Int ToVector2Int(this Vector2 value)
	{
		return new Vector2Int((int)value.x, (int)value.y);
	}
}
