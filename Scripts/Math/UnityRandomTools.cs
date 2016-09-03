using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class UnityRandomTools
{
	public static void RandomizeGenerator()
	{
		Random.InitState((int)(Time.realtimeSinceStartup * 1000f));
	}

	public static int RandomRange(int min, int max)
	{
		return Random.Range(min, max);
	}
	public static int RandomRangeIncludingMax(int min, int max)
	{
		return Random.Range(min, max + 1);
	}
	public static Color RandomColor
	{
		get { return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)); }
	}

	public static float RandomPI
	{
		get { return Mathf.PI * Random.value; }
	}
	public static float RandomHalfPI
	{
		get { return Mathf.PI * 0.5f * Random.value; }
	}
	public static float Random180
	{
		get { return 180f * Random.value; }
	}
	public static float Random360
	{
		get { return 360f * Random.value; }
	}

	public static bool RandomBool
	{
		get { return 0.5f > Random.value; }
	}
	public static bool RandomBoolRatio(float ratio)
	{
		return ratio > Random.value;
	}

	public static float RandomPosNeg
	{
		get { return RandomBool ? -1f : 1f; }
	}

	public static Vector2 RandomVector2(float range)
	{
		return new Vector2(Random.Range(-range, range), Random.Range(-range, range));
	}
	public static Vector3 RandomVector3(float range)
	{
		return new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
	}
	public static Vector2 RandomVector2(float rangeX, float rangeY)
	{
		return new Vector2(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY));
	}
	public static Vector3 RandomVector3(float rangeX, float rangeY, float rangeZ)
	{
		return new Vector3(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY), Random.Range(-rangeZ, rangeZ));
	}
	public static Vector2 RandomVector2(Vector2 range)
	{
		return new Vector2(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y));
	}
	public static Vector3 RandomVector3(Vector3 range)
	{
		return new Vector3(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y), Random.Range(-range.z, range.z));
	}
	public static Vector2 RandomUnitVector2
	{
		get { return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)); }
	}
	public static Vector3 RandomUnitVector3
	{
		get { return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)); }
	}

	#region Collection Operations

	public static int RandomIndexSelection<T>(this T[] list)
	{
		if (list.Length == 0)
			return -1;
		return Random.Range(0, list.Length);
	}
	public static int RandomIndexSelection<T>(this T[] list, System.Random random)
	{
		if (list.Length == 0)
			return -1;
		return random.Next(0, list.Length);
	}
	public static int RandomIndexSelection<T>(this ICollection<T> collection)
	{
		if (collection.Count == 0)
			return -1;
		return Random.Range(0, collection.Count);
	}
	public static int RandomIndexSelection<T>(this ICollection<T> collection, System.Random random)
	{
		if (collection.Count == 0)
			return -1;
		return random.Next(0, collection.Count);
	}
	public static int RandomIndexSelection<T>(this IList<T> list, bool removeFromlist)
	{
		if (list.Count == 0)
			return -1;
		int index = Random.Range(0, list.Count);
		if (removeFromlist)
			list.RemoveAt(index);
		return index;
	}
	public static int RandomIndexSelection<T>(this IList<T> list, bool removeFromlist, System.Random random)
	{
		if (list.Count == 0)
			return -1;
		int index = random.Next(0, list.Count);
		if (removeFromlist)
			list.RemoveAt(index);
		return index;
	}

	public static T RandomSelection<T>(this T[] list)
	{
		if (list.Length == 0)
			return default(T);
		return list[Random.Range(0, list.Length)];
	}
	public static T RandomSelection<T>(this T[] list, System.Random random)
	{
		if (list.Length == 0)
			return default(T);
		return list[random.Next(0, list.Length)];
	}
	public static T RandomSelection<T>(this IList<T> list)
	{
		if (list.Count == 0)
			return default(T);
		return list[Random.Range(0, list.Count)];
	}
	public static T RandomSelection<T>(this IList<T> list, System.Random random)
	{
		if (list.Count == 0)
			return default(T);
		return list[random.Next(0, list.Count)];
	}
	public static T RandomSelection<T>(this IList<T> list, bool removeFromlist)
	{
		if (list.Count == 0)
			return default(T);
		int index = Random.Range(0, list.Count);
		T val = list[index];
		if (removeFromlist)
			list.RemoveAt(index);
		return val;
	}
	public static T RandomSelection<T>(this IList<T> list, bool removeFromlist, System.Random random)
	{
		if (list.Count == 0)
			return default(T);
		int index = random.Next(0, list.Count);
		T val = list[index];
		if (removeFromlist)
			list.RemoveAt(index);
		return val;
	}

	public static void RandomizeOrderFisherYates<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	#endregion

	#region Enums

	public static T RandomSelection<T>()
	{
		Array values = Enum.GetValues(typeof(T));
		return (T)values.GetValue(Random.Range(0, values.Length));
	}

	#endregion
}
