using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public static class CollectionTools
{
	#region Array / List / Collection / Enumerable

	public static bool IsEqual<T>(this IEnumerable<T> source, IEnumerable<T> other)
	{
		return new CollectionComparer<T>().Equals(source, other);
	}

	public static bool IsNullOrEmpty<T>(this T[] source)
	{
		return source == null || source.Length == 0;
	}

	public static bool IsNullOrEmpty<T>(this ICollection<T> source)
	{
		return source == null || source.Count == 0;
	}

	public static bool IsNotNullAndEmpty<T>(this T[] source)
	{
		return source != null && source.Length != 0;
	}

	public static bool IsNotNullAndEmpty<T>(this ICollection<T> source)
	{
		return source != null && source.Count != 0;
	}

	public static List<T> Clone<T>(this List<T> source)
	{
		return source.GetRange(0, source.Count);
	}

	public static void Combine<T>(this ICollection<T> thisList, IEnumerable<T> otherList)
	{
		if (otherList == null)
			throw new ArgumentNullException("otherList");

		foreach (T otherListItem in otherList)
		{
			if (!thisList.Contains(otherListItem))
			{
				thisList.Add(otherListItem);
			}
		}
	}

	public static void AddSorted<T>(this List<T> thisList, T item) where T : IComparable<T>
	{
		if (thisList.Count == 0)
		{
			thisList.Add(item);
			return;
		}
		if (thisList[thisList.Count - 1].CompareTo(item) <= 0)
		{
			thisList.Add(item);
			return;
		}
		if (thisList[0].CompareTo(item) >= 0)
		{
			thisList.Insert(0, item);
			return;
		}
		int index = thisList.BinarySearch(item);
		if (index < 0)
			index = ~index;
		thisList.Insert(index, item);
	}

	public static void RemoveAtStart<T>(this List<T> list, int count)
	{
		list.RemoveRange(0, count);
	}

	public static void RemoveAtEnd<T>(this List<T> list, int count)
	{
		list.RemoveRange(list.Count - count, count);
	}

	public static void AddCapacity<T>(ref List<T> list, int additionalCapacity)
	{
		if (list == null)
		{
			list = new List<T>(additionalCapacity);
		}
		else
		{
			list.Capacity += additionalCapacity;
		}
	}

	#region Duplicates

	public static IEnumerable<int> DuplicatesIndexed<T>(this IEnumerable<T> source)
	{
		var itemsSeen = new HashSet<T>();
		int index = 0;

		foreach (T item in source)
		{
			if (!itemsSeen.Add(item))
			{
				yield return index;
			}
			index++;
		}
	}

	public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source)
	{
		var itemsSeen = new HashSet<T>();
		var itemsYielded = new HashSet<T>();

		foreach (T item in source)
		{
			if (!itemsSeen.Add(item))
			{
				if (itemsYielded.Add(item))
				{
					yield return item;
				}
			}
		}
	}

	public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source, IEqualityComparer<T> equaltyComparer)
	{
		var itemsSeen = new HashSet<T>();
		var itemsYielded = new HashSet<T>();

		foreach (T item in source)
		{
			if (itemsSeen.Contains(item, equaltyComparer))
			{
				if (!itemsYielded.Contains(item, equaltyComparer))
				{
					itemsYielded.Add(item);
					yield return item;
				}
			}
			else
			{
				itemsSeen.Add(item);
			}
		}
	}

	#endregion

	#region EqualizeTo

	/// <summary>
	/// Equalizes items in "this list" by comparing each item in "other list". First it removes items what's in "this list" but not in "other list". Then adds items what's in "other list" but not in "this list".
	/// </summary>
	/// <typeparam name="T">Item type of lists</typeparam>
	/// <param name="thisList">"This list" is the modified list that's going to be equalized to "other list" items.</param>
	/// <param name="otherList">"Other list" is used only for comparison to "this list".</param>
	/// <param name="onAdd">Called when an item in "other list" is added to "this list".</param>
	/// <param name="onRemove">Called when an item in "this list" is removed. First parameter "T" is the item that's going to be removed from list. Second parameter "int" is the list index of the item.</param>
	/// <returns>True if anything in "this list" changed. False otherwise.</returns>
	public static bool EqualizeTo<T>(this IList<T> thisList, IList<T> otherList,
		Action<T> onAdd = null,
		Action<T, int> onRemove = null)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");

		bool isAnythingChanged = false;

		// Remove this list item if other list does not contain it.
		for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
		{
			bool found = false;
			var thisListItem = thisList[iThisList];

			for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
			{
				if (thisListItem.Equals(otherList[iOtherList]))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				thisList.RemoveAt(iThisList);
				if (onRemove != null)
					onRemove(thisListItem, iThisList);
				isAnythingChanged = true;
			}
		}

		// Add other list item to this list if this list does not contain it.
		for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
		{
			bool found = false;
			var otherListItem = otherList[iOtherList];

			for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
			{
				if (otherListItem.Equals(thisList[iThisList]))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				thisList.Add(otherListItem);
				if (onAdd != null)
					onAdd(otherListItem);
				isAnythingChanged = true;
			}
		}

		return isAnythingChanged;
	}

	/// <summary>
	/// Equalizes items in "this list" by comparing each item in "other list". First it removes items what's in "this list" but not in "other list". Then adds items what's in "other list" but not in "this list".
	/// </summary>
	/// <typeparam name="T1">Item type of "this list"</typeparam>
	/// <typeparam name="T2">Item type of "other list"</typeparam>
	/// <param name="thisList">"This list" is the modified list that's going to be equalized to "other list" items.</param>
	/// <param name="otherList">"Other list" is used only for comparison to "this list".</param>
	/// <param name="comparer">Allows user to define an equalty comparer between T1 and T2. This is how two different type lists find a common ground.</param>
	/// <param name="creator">Called when an item in "other list" does not appear to be in "this list". User should create an item of type T1 and add it to "this list" manually.</param>
	/// <param name="destroyer">This is optional. Called when an item in "this list" does not appear to be in "other list". User should remove the item from "this list" manually. Other actions like disposing etc. also can be made within this call. First parameter "T1" is the item that's going to be removed from list. Second parameter "int" is the list index of the item.</param>
	/// <returns>True if anything in "this list" changed. False otherwise.</returns>
	public static bool EqualizeTo<T1, T2>(this IList thisList, IList otherList,
		IEqualityComparer<T1, T2> comparer,
		Action<T2> creator,
		Action<T1, int> destroyer = null)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");

		bool isAnythingChanged = false;

		// Remove this list item if other list does not contain it.
		for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
		{
			bool found = false;
			var thisListItem = thisList[iThisList];

			for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
			{
				if (comparer.Equals((T1)thisListItem, (T2)otherList[iOtherList]))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				if (destroyer != null)
				{
					destroyer((T1)thisListItem, iThisList);
					//thisList.RemoveAt(iThisList); // Destroyer should do the remove.
				}
				else
				{
					thisList.RemoveAt(iThisList);
				}
				isAnythingChanged = true;
			}
		}

		// Add other list item to this list if this list does not contain it.
		for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
		{
			bool found = false;
			var otherListItem = otherList[iOtherList];

			for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
			{
				if (comparer.Equals((T1)thisList[iThisList], (T2)otherListItem))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				creator((T2)otherListItem);
				//thisList.Add(creator((T2)otherListItem)); // Creator should do the add.
				isAnythingChanged = true;
			}
		}

		return isAnythingChanged;
	}

	/// <summary>
	/// Equalizes items in "this list" by comparing each item in "other list". First it removes items what's in "this list" but not in "other list". Then adds items what's in "other list" but not in "this list".
	/// </summary>
	/// <typeparam name="T1">Item type of "this list"</typeparam>
	/// <typeparam name="T2">Item type of "other list"</typeparam>
	/// <param name="thisList">"This list" is the modified list that's going to be equalized to "other list" items.</param>
	/// <param name="otherList">"Other list" is used only for comparison to "this list".</param>
	/// <param name="comparer">Allows user to define an equalty comparer between T1 and T2. This is how two different type lists find a common ground.</param>
	/// <param name="creator">Called when an item in "other list" does not appear to be in "this list". User should create an item of type T1 and add it to "this list" manually.</param>
	/// <param name="destroyer">This is optional. Called when an item in "this list" does not appear to be in "other list". User should remove the item from "this list" manually. Other actions like disposing etc. also can be made within this call. First parameter "T1" is the item that's going to be removed from list. Second parameter "int" is the list index of the item.</param>
	/// <returns>True if anything in "this list" changed. False otherwise.</returns>
	public static bool EqualizeTo<T1, T2>(this IList<T1> thisList, IList<T2> otherList,
		IEqualityComparer<T1, T2> comparer,
		Action<T2> creator,
		Action<T1, int> destroyer = null)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");

		bool isAnythingChanged = false;

		// Remove this list item if other list does not contain it.
		for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
		{
			bool found = false;
			var thisListItem = thisList[iThisList];

			for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
			{
				if (comparer.Equals(thisListItem, otherList[iOtherList]))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				if (destroyer != null)
				{
					destroyer(thisListItem, iThisList);
					//thisList.RemoveAt(iThisList); // Destroyer should do the remove.
				}
				else
				{
					thisList.RemoveAt(iThisList);
				}
				isAnythingChanged = true;
			}
		}

		// Add other list item to this list if this list does not contain it.
		for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
		{
			bool found = false;
			var otherListItem = otherList[iOtherList];

			for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
			{
				if (comparer.Equals(thisList[iThisList], otherListItem))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				creator(otherListItem);
				//thisList.Add(creator((T2)otherListItem)); // Creator should do the add.
				isAnythingChanged = true;
			}
		}

		return isAnythingChanged;
	}

	#endregion

	#region ContentEquals

	public static bool ContentEquals<T1, T2>(this IList<T1> thisList, IList<T2> otherList,
		IEqualityComparer<T1, T2> comparer)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");

		if (thisList.Count != otherList.Count)
			return false;
		if (thisList.Count == 0)
			return true;

		for (int iThisList = 0; iThisList < thisList.Count; iThisList++)
		{
			bool found = false;
			var thisListItem = thisList[iThisList];

			for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
			{
				if (comparer.Equals(thisListItem, otherList[iOtherList]))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				return false;
			}
		}

		return true;
	}

	#endregion

	#endregion

	#region Dictionary

	public static Dictionary<string, string> CreateDictionaryFromStringList(this ICollection<string> list, char keyValueSeparator = '=')
	{
		if (list == null)
			throw new ArgumentNullException("list");

		var dictionary = new Dictionary<string, string>(list.Count);

		foreach (var item in list)
		{
			var separatorIndex = item.IndexOf(keyValueSeparator);
			if (separatorIndex < 0)
				throw new Exception("No separator in list item");

			var key = item.Substring(0, separatorIndex);

			if (string.IsNullOrEmpty(key))
				throw new Exception("Key is empty");

			var value = item.Substring(separatorIndex + 1, item.Length - separatorIndex - 1);
			dictionary.Add(key, value);
		}

		return dictionary;
	}

	public static bool HasSameKeys<TKey, TValue>(this Dictionary<TKey, TValue> thisObj, Dictionary<TKey, TValue> otherObj)
	{
		if (thisObj.Count != otherObj.Count)
			return false;

		foreach (TKey key in thisObj.Keys)
		{
			if (!otherObj.ContainsKey(key))
				return false;
		}

		return true;
		//return dictionary1.OrderBy(kvp => kvp.Key).SequenceEqual(dictionary2.OrderBy(kvp => kvp.Key));
	}

	public static bool HasSameKeys<TKey, TValue>(this List<TKey> thisObj, Dictionary<TKey, TValue> dictionary)
	{
		if (thisObj.Count != dictionary.Count)
			return false;

		foreach (TKey key in dictionary.Keys)
		{
			if (!thisObj.Contains(key))
				return false;
		}

		return true;
	}

	public static bool HasSameValues<TKey, TValue>(this List<TValue> thisObj, Dictionary<TKey, TValue> dictionary)
	{
		if (thisObj.Count != dictionary.Count)
			return false;

		foreach (TValue value in dictionary.Values)
		{
			if (!thisObj.Contains(value))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Returns the value associated with the specified key if there
	/// already is one, or inserts a new value for the specified key and
	/// returns that.
	/// </summary>
	/// <typeparam name="TKey">Type of key</typeparam>
	/// <typeparam name="TValue">Type of value, which must either have
	/// a public parameterless constructor or be a value type</typeparam>
	/// <param name="dictionary">Dictionary to access</param>
	/// <param name="key">Key to lookup</param>
	/// <returns>Existing value in the dictionary, or new one inserted</returns>
	public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
												   TKey key)
		where TValue : new()
	{
		TValue ret;
		if (!dictionary.TryGetValue(key, out ret))
		{
			ret = new TValue();
			dictionary[key] = ret;
		}
		return ret;
	}

	/// <summary>
	/// Returns the value associated with the specified key if there already
	/// is one, or calls the specified delegate to create a new value which is
	/// stored and returned.
	/// </summary>
	/// <typeparam name="TKey">Type of key</typeparam>
	/// <typeparam name="TValue">Type of value</typeparam>
	/// <param name="dictionary">Dictionary to access</param>
	/// <param name="key">Key to lookup</param>
	/// <param name="valueProvider">Delegate to provide new value if required</param>
	/// <returns>Existing value in the dictionary, or new one inserted</returns>
	public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
												   TKey key,
												   Func<TValue> valueProvider)
	{
		TValue ret;
		if (!dictionary.TryGetValue(key, out ret))
		{
			ret = valueProvider();
			dictionary[key] = ret;
		}
		return ret;
	}

	/// <summary>
	/// Returns the value associated with the specified key if there
	/// already is one, or inserts the default value and returns it.
	/// </summary>
	/// <typeparam name="TKey">Type of key</typeparam>
	/// <typeparam name="TValue">Type of value</typeparam>
	/// <param name="dictionary">Dictionary to access</param>
	/// <param name="key">Key to lookup</param>
	/// <param name="defaultValue">Value to use when key is missing</param>
	/// <returns>Existing value in the dictionary, or new one inserted</returns>
	public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
												   TKey key,
												   TValue defaultValue)
	{
		TValue ret;
		if (!dictionary.TryGetValue(key, out ret))
		{
			ret = defaultValue;
			dictionary[key] = ret;
		}
		return ret;
	}

	/// <summary>
	/// Returns the value associated with the specified key if there
	/// already is one, or returns the default value without inserting it.
	/// </summary>
	/// <typeparam name="TKey">Type of key</typeparam>
	/// <typeparam name="TValue">Type of value</typeparam>
	/// <param name="dictionary">Dictionary to access</param>
	/// <param name="key">Key to lookup</param>
	/// <param name="defaultValue">Value to use when key is missing</param>
	/// <returns>Existing value in the dictionary, or new one inserted</returns>
	public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
												   TKey key,
												   TValue defaultValue)
	{
		TValue ret;
		if (!dictionary.TryGetValue(key, out ret))
		{
			return defaultValue;
		}
		return ret;
	}

	#endregion

	#region Search Pattern - Byte Array

	private static readonly int[] EmptyIntArray = new int[0];

	public static int Locate(this byte[] self, byte[] candidate, int searchStartIndex = 0, int searchEndIndex = 0)
	{
		if (IsEmptyLocate(self, candidate))
			return -1;
		if (searchStartIndex >= self.Length)
			return -1;
		if (searchEndIndex > self.Length)
			searchEndIndex = self.Length;

		for (int i = searchStartIndex; i < searchEndIndex; i++)
		{
			if (!IsMatch(self, i, candidate))
				continue;

			return i;
		}

		return -1;
	}

	public static int[] LocateMultiple(this byte[] self, byte[] candidate, int searchStartIndex = 0, int searchEndIndex = 0)
	{
		if (IsEmptyLocate(self, candidate))
			return EmptyIntArray;
		if (searchStartIndex >= self.Length)
			return EmptyIntArray;
		if (searchEndIndex > self.Length)
			searchEndIndex = self.Length;

		var list = new List<int>();

		for (int i = searchStartIndex; i < searchEndIndex; i++)
		{
			if (!IsMatch(self, i, candidate))
				continue;

			list.Add(i);
		}

		return list.Count == 0 ? EmptyIntArray : list.ToArray();
	}

	private static bool IsMatch(byte[] array, int position, byte[] candidate)
	{
		if (candidate.Length > (array.Length - position))
			return false;

		for (int i = 0; i < candidate.Length; i++)
			if (array[position + i] != candidate[i])
				return false;

		return true;
	}

	private static bool IsEmptyLocate(byte[] array, byte[] candidate)
	{
		return array == null
			   || candidate == null
			   || array.Length == 0
			   || candidate.Length == 0
			   || candidate.Length > array.Length;
	}

	#endregion
}

public class CollectionComparer<T> : IEqualityComparer<IEnumerable<T>>
{
	public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
	{
		if ((first == null) != (second == null))
			return false;

		if (!object.ReferenceEquals(first, second) && (first != null))
		{
			if (first.Count() != second.Count())
				return false;

			if ((first.Count() != 0) && HaveMismatchedElement(first, second))
				return false;
		}

		return true;
	}

	private static bool HaveMismatchedElement(IEnumerable<T> first, IEnumerable<T> second)
	{
		int firstCount;
		int secondCount;

		var firstElementCounts = GetElementCounts(first, out firstCount);
		var secondElementCounts = GetElementCounts(second, out secondCount);

		if (firstCount != secondCount)
			return true;

		foreach (var kvp in firstElementCounts)
		{
			firstCount = kvp.Value;
			secondElementCounts.TryGetValue(kvp.Key, out secondCount);

			if (firstCount != secondCount)
				return true;
		}

		return false;
	}

	private static Dictionary<T, int> GetElementCounts(IEnumerable<T> enumerable, out int nullCount)
	{
		var dictionary = new Dictionary<T, int>();
		nullCount = 0;

		foreach (T element in enumerable)
		{
			if (element == null)
			{
				nullCount++;
			}
			else
			{
				int num;
				dictionary.TryGetValue(element, out num);
				num++;
				dictionary[element] = num;
			}
		}

		return dictionary;
	}

	public int GetHashCode(IEnumerable<T> enumerable)
	{
		int hash = 17;

		foreach (T val in enumerable.OrderBy(x => x))
			hash = hash * 23 + val.GetHashCode();

		return hash;
	}
}

public class GenericComparer<T> : IEqualityComparer<T> where T : class
{
	private Func<T, object> Expression { get; set; }

	public GenericComparer(Func<T, object> expression)
	{
		Expression = expression;
	}

	public bool Equals(T x, T y)
	{
		var value1 = Expression.Invoke(x);
		var value2 = Expression.Invoke(y);
		return value1 != null && value1.Equals(value2);
	}

	public int GetHashCode(T obj)
	{
		return obj.GetHashCode();
	}
}

public class GenericComparer<T1, T2> : IEqualityComparer<T1, T2>
{
	private Func<T1, object> Expression1 { get; set; }
	private Func<T2, object> Expression2 { get; set; }

	public GenericComparer(Func<T1, object> expression1, Func<T2, object> expression2)
	{
		Expression1 = expression1;
		Expression2 = expression2;
	}

	public bool Equals(T1 x, T2 y)
	{
		var value1 = Expression1.Invoke(x);
		var value2 = Expression2.Invoke(y);
		return value1 != null && value1.Equals(value2);
	}
}
