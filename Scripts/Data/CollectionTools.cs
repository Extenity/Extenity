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

	public static bool IsAllNullOrEmpty(this string[] source)
	{
		if (source == null)
			return true;
		for (int i = 0; i < source.Length; i++)
		{
			if (!string.IsNullOrEmpty(source[i]))
				return false;
		}
		return true;
	}

	// See if the list does not contain any items other than specified items.
	public static bool DoesNotContainOtherThan<T>(this List<T> list, params T[] items)
	{
		if (list == null)
			return true;
		if (items == null)
			throw new ArgumentNullException("items");

		for (int i = 0; i < list.Count; i++)
		{
			if (!items.Contains(list[i]))
			{
				return false;
			}
		}
		return true;
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

	public static bool AddIfDoesNotContain<T>(this List<T> thisList, T item)
	{
		if (!thisList.Contains(item))
		{
			thisList.Add(item);
			return true;
		}
		return false;
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

	public static T[] GetRange<T>(this T[] source, int index, int length)
	{
		var result = new T[length];
		Array.Copy(source, index, result, 0, length);
		return result;
	}

	public static T[] RemoveAt<T>(this T[] source, int index)
	{
		if (index < 0 || index >= source.Length)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index is out of range.");
		}

		var result = new T[source.Length - 1];
		if (index > 0)
			Array.Copy(source, 0, result, 0, index);
		if (index < source.Length - 1)
			Array.Copy(source, index + 1, result, index, source.Length - index - 1);
		return result;
	}

	public static T[] InsertAt<T>(this T[] source, int index)
	{
		var result = new T[source.Length + 1];

		if (source.Length == 0)
			return result;

		if (index < 0)
			index = 0;
		if (index > source.Length - 1)
			index = source.Length - 1;

		if (index > 0)
			Array.Copy(source, 0, result, 0, index);

		Array.Copy(source, index, result, index + 1, source.Length - index);

		return result;
	}

	public static T[] Swap<T>(this T[] source, int index1, int index2)
	{
		var val = source[index1];
		source[index1] = source[index2];
		source[index2] = val;
		return source;
	}

	public static T[] Add<T>(this T[] source, T item)
	{
		Array.Resize(ref source, source.Length + 1);
		source[source.Length - 1] = item;
		return source;
	}

	public static T[] AddRange<T>(this T[] source, T[] items)
	{
		Array.Resize(ref source, source.Length + items.Length);
		Array.Copy(items, 0, source, source.Length - items.Length, items.Length);
		return source;
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

	/// <summary>
	/// Removes all duplicate items in list. Keeps only the last occurences of duplicate items.
	/// </summary>
	public static void RemoveDuplicates<T>(this List<T> source)
	{
		var hash = new HashSet<T>();
		for (int i = source.Count - 1; i >= 0; i--)
		{
			var item = source[i];
			if (!hash.Add(item))
			{
				source.RemoveAt(i);
			}
		}
	}

	public static T[] RemoveDuplicates<T>(this T[] source)
	{
		var hash = new HashSet<T>();
		var cleanList = new List<T>();
		for (int i = 0; i < source.Length; i++)
		{
			var item = source[i];
			if (hash.Add(item))
			{
				cleanList.Add(item);
			}
		}
		return cleanList.ToArray();
	}

	#endregion

	#region EqualizeTo

	/// <summary>
	/// Equalizes items in "this list" to "other list" by comparing each item. First it detects the items that exist in both lists if "onUnchanged" specified. Then it removes items what's in "this list" but not in "other list". Then adds items what's in "other list" but not in "this list".
	/// </summary>
	/// <typeparam name="T">Item type of lists</typeparam>
	/// <param name="thisList">"This list" is the modified list that's going to be equalized to "other list" items.</param>
	/// <param name="otherList">"Other list" is used only for comparison to "this list" and not modified.</param>
	/// <param name="onAdd">Called when an item in "other list" does not appear to be in "this list". Parameter "T" is the item in "other list" that's going to be added into "this list". It's possible not to specify a method. In this case, the item will be added automatically to "this list". But in case a method is specified, the item must be added at the end of "this list" manually. No other modifications should be made to the list.</param>
	/// <param name="onRemove">Called when an item in "this list" does not appear to be in "other list". First parameter "T" is the item that's going to be removed from "this list". Second parameter "int" is the list index of the item. It's possible not to specify a method. In this case, the item will be removed automatically from "this list". But in case a method is specified, the item must be removed from "this list" manually in this method. Only the item sent to this method should be removed and no other modifications should be made to the list.</param>
	/// <param name="onUnchanged">Called when an item in "this list" appears to be in "other list" too. First parameter "T" is the item in "this list". Second parameter "T" is the item in "other list". It's possible not to specify a method. In this case, no extra calculations will be made for detecting unchanged items. No manual modifications should be made to the list.</param>
	/// <returns>True if anything in "this list" changed. False otherwise.</returns>
	public static bool EqualizeTo<T>(this IList<T> thisList, IList<T> otherList,
		Action<T> onAdd = null,
		Action<T, int> onRemove = null,
		Action<T, T> onUnchanged = null)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");

		// Detect unchanged items
		if (onUnchanged != null)
		{
			for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
			{
				var thisListItem = thisList[iThisList];
				for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
				{
					var otherListItem = otherList[iOtherList];
					if (thisListItem.Equals(otherList[iOtherList]))
					{
						onUnchanged(thisListItem, otherListItem);
						break;
					}
				}
			}
		}

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
				if (onRemove != null)
				{
					onRemove(thisListItem, iThisList);
					//thisList.RemoveAt(iThisList); // User should do the remove in onRemove because onRemove specified.
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
				if (thisList[iThisList].Equals(otherListItem))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				if (onAdd != null)
				{
					onAdd(otherListItem);
					//thisList.Add(otherListItem); // User should do the add in onAdd because onAdd specified.
				}
				else
				{
					thisList.Add(otherListItem);
				}
				isAnythingChanged = true;
			}
		}

		return isAnythingChanged;
	}

	/// <summary>
	/// Equalizes items in "this list" to "other list" by comparing each item. First it detects the items that exist in both lists if "onUnchanged" specified. Then it removes items what's in "this list" but not in "other list". Then adds items what's in "other list" but not in "this list".
	/// </summary>
	/// <typeparam name="T">Item type of lists</typeparam>
	/// <param name="thisList">"This list" is the modified list that's going to be equalized to "other list" items.</param>
	/// <param name="otherList">"Other list" is used only for comparison to "this list" and not modified.</param>
	/// <param name="comparer">Allows user to define a custom equalty comparer.</param>
	/// <param name="onAdd">Called when an item in "other list" does not appear to be in "this list". Parameter "T" is the item in "other list" that's going to be added into "this list". It's possible not to specify a method. In this case, the item will be added automatically to "this list". But in case a method is specified, the item must be added at the end of "this list" manually. No other modifications should be made to the list.</param>
	/// <param name="onRemove">Called when an item in "this list" does not appear to be in "other list". First parameter "T" is the item that's going to be removed from "this list". Second parameter "int" is the list index of the item. It's possible not to specify a method. In this case, the item will be removed automatically from "this list". But in case a method is specified, the item must be removed from "this list" manually in this method. Only the item sent to this method should be removed and no other modifications should be made to the list.</param>
	/// <param name="onUnchanged">Called when an item in "this list" appears to be in "other list" too. First parameter "T" is the item in "this list". Second parameter "T" is the item in "other list". It's possible not to specify a method. In this case, no extra calculations will be made for detecting unchanged items. No manual modifications should be made to the list.</param>
	/// <returns>True if anything in "this list" changed. False otherwise.</returns>
	public static bool EqualizeTo<T>(this IList<T> thisList, IList<T> otherList,
		IEqualityComparer<T> comparer,
		Action<T> onAdd = null,
		Action<T, int> onRemove = null,
		Action<T, T> onUnchanged = null)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");
		if (comparer == null)
			throw new ArgumentNullException("comparer");

		// Detect unchanged items
		if (onUnchanged != null)
		{
			for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
			{
				var thisListItem = thisList[iThisList];
				for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
				{
					var otherListItem = otherList[iOtherList];
					if (thisListItem.Equals(otherList[iOtherList]))
					{
						onUnchanged(thisListItem, otherListItem);
						break;
					}
				}
			}
		}

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
				if (onRemove != null)
				{
					onRemove(thisListItem, iThisList);
					//thisList.RemoveAt(iThisList); // User should do the remove in onRemove because onRemove specified.
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
				if (onAdd != null)
				{
					onAdd(otherListItem);
					//thisList.Add(otherListItem); // User should do the add in onAdd because onAdd specified.
				}
				else
				{
					thisList.Add(otherListItem);
				}
				isAnythingChanged = true;
			}
		}

		return isAnythingChanged;
	}

	/// <summary>
	/// Equalizes items in "this list" to "other list" by comparing each item. First it detects the items that exist in both lists if "onUnchanged" specified. Then it removes items what's in "this list" but not in "other list". Then adds items what's in "other list" but not in "this list".
	/// </summary>
	/// <typeparam name="T1">Item type of "this list"</typeparam>
	/// <typeparam name="T2">Item type of "other list"</typeparam>
	/// <param name="thisList">"This list" is the modified list that's going to be equalized to "other list" items.</param>
	/// <param name="otherList">"Other list" is used only for comparison to "this list" and not modified.</param>
	/// <param name="comparer">Allows user to define a custom equalty comparer between T1 and T2. This is how two different type of lists find a common ground.</param>
	/// <param name="onAdd">Called when an item in "other list" does not appear to be in "this list". Parameter "T2" is the item in "other list" that's going to be converted and added into "this list". The conversion of T2 to T1 can be made in this method and converted item must be added at the end of "this list" manually. No other modifications should be made to the list.</param>
	/// <param name="onRemove">Called when an item in "this list" does not appear to be in "other list". First parameter "T1" is the item that's going to be removed from "this list". Second parameter "int" is the list index of the item. It's possible not to specify a method. In this case, the item will be removed automatically from "this list". But in case a method is specified, the item must be removed from "this list" manually in this method. Only the item sent to this method should be removed and no other modifications should be made to the list.</param>
	/// <param name="onUnchanged">Called when an item in "this list" appears to be in "other list" too. First parameter "T1" is the item in "this list". Second parameter "T2" is the item in "other list". It's possible not to specify a method. In this case, no extra calculations will be made for detecting unchanged items. No manual modifications should be made to the list.</param>
	/// <returns>True if anything in "this list" changed. False otherwise.</returns>
	public static bool EqualizeTo<T1, T2>(this IList<T1> thisList, IList<T2> otherList,
		IEqualityComparer<T1, T2> comparer,
		Action<T2> onAdd,
		Action<T1, int> onRemove = null,
		Action<T1, T2> onUnchanged = null)
	{
		if (thisList == null)
			throw new ArgumentNullException("thisList");
		if (otherList == null)
			throw new ArgumentNullException("otherList");
		if (Equals(thisList, otherList))
			throw new ArgumentException("thisList and otherList are pointing to the same list");
		if (onAdd == null)
			throw new ArgumentNullException("onAdd");
		if (comparer == null)
			throw new ArgumentNullException("comparer");

		// Detect unchanged items
		if (onUnchanged != null)
		{
			for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
			{
				var thisListItem = thisList[iThisList];
				for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
				{
					var otherListItem = otherList[iOtherList];
					if (thisListItem.Equals(otherList[iOtherList]))
					{
						onUnchanged(thisListItem, otherListItem);
						break;
					}
				}
			}
		}

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
				if (onRemove != null)
				{
					onRemove(thisListItem, iThisList);
					//thisList.RemoveAt(iThisList); // User should do the remove in onRemove because onRemove specified.
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
				onAdd(otherListItem);
				//thisList.Add(otherListItem); // User should do the add in onAdd no matter what because we don't know how to do conversion between T1 and T2 and we expect the user to do the conversion in onAdd.
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

	#region Register To List Event and Traverse

	public static void RegisterToListEvent<TItem>(this List<TItem> list, bool ignoreIfListIsNull, Action subscriptionAction, Action<TItem> actionForEachItem)
	{
		if (list == null)
		{
			if (!ignoreIfListIsNull)
			{
				throw new ArgumentNullException("list", "List is null. Could not register to list events.");
			}
		}
		else
		{
			for (int i = 0; i < list.Count; i++)
			{
				actionForEachItem(list[i]);
			}
		}

		subscriptionAction();
	}

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

	#region Helpers

	public static readonly object[] EmptyObjectArray = new object[0];

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
