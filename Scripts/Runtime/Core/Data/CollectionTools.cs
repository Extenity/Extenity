using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Generic.Extenity;

namespace Extenity.DataToolbox
{

	public static class CollectionTools
	{
		#region Array / List / Collection / Enumerable

		public static int SafeCount<T>(this ICollection<T> collection)
		{
			if (collection == null)
				return 0;
			return collection.Count;
		}

		public static int SafeLength<T>(this T[] array)
		{
			if (array == null)
				return 0;
			return array.Length;
		}

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

		public static bool IsAnyNonNullItemExists<T>(this IEnumerable<T> collection)
		{
			if (collection == null)
				return false;
			foreach (var item in collection)
			{
				if (item != null)
					return true;
			}
			return false;
		}

		public static bool IsAnyNonNullItemExists<T>(this IList<T> list)
		{
			if (list == null)
				return false;
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
					return true;
			}
			return false;
		}

		public static int RemoveAllNullItems<T>(this IList<T> list)
		{
			if (list == null)
				return 0;
			var count = 0;
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
				{
					list.RemoveAt(i);
					count++;
				}
			}
			return count;
		}

		// See if the list does not contain any items other than specified items.
		public static bool DoesNotContainOtherThan<T>(this List<T> list, params T[] items)
		{
			if (list == null)
				return true;
			if (items == null)
				throw new ArgumentNullException(nameof(items));

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

		public static T[] Combine<T>(this T[] thisArray, T[] appendedArray)
		{
			var result = new T[thisArray.Length + appendedArray.Length];
			Array.Copy(thisArray, result, thisArray.Length);
			Array.Copy(appendedArray, 0, result, thisArray.Length, appendedArray.Length);
			return result;
		}

		public static void Combine<T>(this ICollection<T> thisList, IEnumerable<T> otherList)
		{
			if (otherList == null)
				throw new ArgumentNullException(nameof(otherList));

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

		public static T Dequeue<T>(this List<T> list)
		{
			if (list == null || list.Count == 0)
				return default(T);

			var index = list.Count - 1;
			var item = list[index];
			list.RemoveAt(index);
			return item;
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

		public static void MakeSameSizeAs<T1, T2>(ref T1[] thisList, T2[] otherList)
		{
			if (thisList == null)
			{
				thisList = new T1[otherList != null ? otherList.Length : 0];
			}
			else if (otherList == null)
			{
				if (thisList.Length != 0)
				{
					thisList = new T1[0];
				}
			}
			else if (thisList.Length != otherList.Length)
			{
				Array.Resize(ref thisList, otherList.Length);
			}
		}

		public static bool ResizeIfRequired<T>(ref T[] thisList, int length)
		{
			if (length < 0)
				return false; // Ignored.

			if (thisList == null)
			{
				thisList = new T[length];
				return true;
			}
			else if (thisList.Length != length)
			{
				Array.Resize(ref thisList, length);
				return true;
			}
			return false;
		}

		public static bool ResizeIfRequired<T>(ref T[] thisList, int length, Action<T[], int> processItemToBeRemoved, Action<T[], int> processItemToBeAdded)
		{
			if (length < 0)
				return false; // Ignored.

			if (thisList == null)
			{
				thisList = new T[length];
				if (processItemToBeAdded != null)
				{
					// Array needs to be expanded. Call the callback for added items.
					for (int i = 0; i < length; i++)
					{
						processItemToBeAdded(thisList, i);
					}
				}
				return true;
			}
			else if (thisList.Length != length)
			{
				var currentLength = thisList.Length;
				if (currentLength > length && processItemToBeRemoved != null)
				{
					// Array needs to be sized down. Call the callback for removed items.
					for (int i = length; i < currentLength; i++)
					{
						processItemToBeRemoved(thisList, i);
					}
				}
				Array.Resize(ref thisList, length);
				if (currentLength < length && processItemToBeAdded != null)
				{
					// Array needs to be expanded. Call the callback for added items.
					for (int i = currentLength; i < length; i++)
					{
						processItemToBeAdded(thisList, i);
					}
				}
				return true;
			}
			return false;
		}

		public static bool ExpandIfRequired<T>(ref T[] thisList, int length, bool expandToDoubleSize = false)
		{
			if (length < 0)
				return false; // Ignored.

			if (thisList == null)
			{
				thisList = new T[length];
				return true;
			}
			else if (thisList.Length < length)
			{
				int expandedSize;
				if (expandToDoubleSize)
				{
					expandedSize = thisList.Length * 2;
					if (expandedSize < length)
						expandedSize = length;
				}
				else
				{
					expandedSize = length;
				}
				Array.Resize(ref thisList, expandedSize);
				return true;
			}
			return false;
		}

		public static T[] GetRange<T>(this T[] source, int index, int length)
		{
			var result = new T[length];
			Array.Copy(source, index, result, 0, length);
			return result;
		}

		public static int IndexOf<T>(this T[] source, T value)
		{
			return Array.IndexOf(source, value);
		}

		public static int IndexOf<T>(this T[] source, T value, int startIndex)
		{
			return Array.IndexOf(source, value, startIndex);
		}

		public static int IndexOf<T>(this T[] source, T value, int startIndex, int count)
		{
			return Array.IndexOf(source, value, startIndex, count);
		}

		public static int IndexOf(this string[] source, string value, StringComparison comparisonType)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return source.IndexOf(value, 0, source.Length, comparisonType);
		}

		public static int IndexOf(this string[] source, string value, int startIndex, StringComparison comparisonType)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			return source.IndexOf(value, startIndex, source.Length - startIndex, comparisonType);
		}

		public static int IndexOf(this string[] source, string value, int startIndex, int count, StringComparison comparisonType)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (startIndex < 0 || startIndex > source.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Start index is out of range.");
			if (count < 0 || count > source.Length - startIndex)
				throw new ArgumentOutOfRangeException(nameof(count), count, "Count is out of range.");

			var endIndex = startIndex + count;
			for (int i = startIndex; i < endIndex; i++)
			{
				if (source[i].Equals(value, comparisonType))
					return i;
			}
			return -1;
		}

		public static T[] Remove<T>(this T[] source, T[] removedArray)
		{
			var array = source;
			foreach (var removedItem in removedArray)
			{
				int index;
				while ((index = array.IndexOf(removedItem)) >= 0)
				{
					array = array.RemoveAt(index);
				}
			}
			return array;
		}

		public static T[] Remove<T>(this T[] source, T value)
		{
			var index = source.IndexOf(value);
			if (index < 0)
				return source;
			return source.RemoveAt(index);
		}

		public static T[] RemoveAt<T>(this T[] source, int index)
		{
			if (index < 0 || index >= source.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");
			}

			var result = new T[source.Length - 1];
			if (index > 0)
				Array.Copy(source, 0, result, 0, index);
			if (index < source.Length - 1)
				Array.Copy(source, index + 1, result, index, source.Length - index - 1);
			return result;
		}

		public static T[] Insert<T>(this T[] source, int index)
		{
			if (index < 0 || index > source.Length)
				throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

			var result = new T[source.Length + 1];

			if (source.Length == 0)
				return result;

			// Copy left part
			if (index > 0)
				Array.Copy(source, 0, result, 0, index);
			// Copy right part
			Array.Copy(source, index, result, index + 1, source.Length - index);

			return result;
		}

		public static T[] Insert<T>(this T[] source, int index, T obj)
		{
			if (index < 0 || index > source.Length)
				throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

			var result = new T[source.Length + 1];

			if (source.Length == 0)
			{
				result[0] = obj;
				return result;
			}

			// Copy left part
			if (index > 0)
				Array.Copy(source, 0, result, 0, index);
			// Copy right part
			Array.Copy(source, index, result, index + 1, source.Length - index);

			result[index] = obj;
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
				throw new ArgumentNullException(nameof(thisList));
			if (otherList == null)
				throw new ArgumentNullException(nameof(otherList));
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
				throw new ArgumentNullException(nameof(thisList));
			if (otherList == null)
				throw new ArgumentNullException(nameof(otherList));
			if (Equals(thisList, otherList))
				throw new ArgumentException("thisList and otherList are pointing to the same list");
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			// Detect unchanged items
			if (onUnchanged != null)
			{
				for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
				{
					var thisListItem = thisList[iThisList];
					for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
					{
						var otherListItem = otherList[iOtherList];
						if (comparer.Equals(thisListItem, otherListItem))
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
				throw new ArgumentNullException(nameof(thisList));
			if (otherList == null)
				throw new ArgumentNullException(nameof(otherList));
			if (Equals(thisList, otherList))
				throw new ArgumentException("thisList and otherList are pointing to the same list");
			if (onAdd == null)
				throw new ArgumentNullException(nameof(onAdd));
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));

			// Detect unchanged items
			if (onUnchanged != null)
			{
				for (int iThisList = thisList.Count - 1; iThisList >= 0; iThisList--)
				{
					var thisListItem = thisList[iThisList];
					for (int iOtherList = 0; iOtherList < otherList.Count; iOtherList++)
					{
						var otherListItem = otherList[iOtherList];
						if (comparer.Equals(thisListItem, otherListItem))
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
				throw new ArgumentNullException(nameof(thisList));
			if (otherList == null)
				throw new ArgumentNullException(nameof(otherList));
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

		#region Order

		public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.OrderBy(selector);
			}
			else
			{
				return source.OrderByDescending(selector);
			}
		}

		public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.ThenBy(selector);
			}
			else
			{
				return source.ThenByDescending(selector);
			}
		}

		#endregion

		#region Operation - Zip

		/// <summary>
		/// Takes a sequence of items and a corresponding sequence of bools, and then produces a new
		/// sequence where the bools select which items to take out of the original sequence. This
		/// could be built out of Zip and Where but it is easy to simply write the code out directly.
		/// 
		/// Source: https://gist.github.com/ericlippert/69c9e93b366f8cc5d6ac
		/// </summary>
		public static IEnumerable<T> ZipWhere<T>(this IEnumerable<T> items, IEnumerable<bool> selectors)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (selectors == null)
				throw new ArgumentNullException(nameof(selectors));

			return ZipWhereIterator(items, selectors);
		}

		private static IEnumerable<T> ZipWhereIterator<T>(IEnumerable<T> items, IEnumerable<bool> selectors)
		{
			using (var e1 = items.GetEnumerator())
			using (var e2 = selectors.GetEnumerator())
				while (e1.MoveNext() && e2.MoveNext())
					if (e2.Current)
						yield return e1.Current;
		}

		#endregion

		#region Operation - Combinations

		/// <summary>
		/// Takes a sequence of items and produces all subsequences of that sequence of the given size.
		/// 
		/// Source: https://gist.github.com/ericlippert/69c9e93b366f8cc5d6ac
		/// </summary>
		public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> items, int k)
		{
			if (k < 0)
				throw new InvalidOperationException();
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			return
				from combination in Combinations(items.Count(), k)
				select items.ZipWhere(combination);
		}

		/// <summary>
		/// Takes two numbers n and k where both are positive. Produces all sequences of n bits with
		/// k true bits and n-k false bits.
		/// 
		/// Source: https://gist.github.com/ericlippert/69c9e93b366f8cc5d6ac
		/// </summary>
		public static IEnumerable<ImmutableStack<bool>> Combinations(int n, int k)
		{
			// Base case: if n and k are both zero then the sequence
			// is easy: the sequence of zero bits with zero true bits
			// is the empty sequence.

			if (k == 0 && n == 0)
			{
				yield return ImmutableStack<bool>.Empty;
				yield break;
			}

			// Base case: if n < k then there are no such sequences.
			if (n < k)
				yield break;

			// Otherwise, produce first all the sequences that start with
			// true, and then all the sequences that start with false.

			// At least one of n or k is not zero, and 0 <= k <= n,
			// therefore n is not zero. But k could be.

			if (k > 0)
				foreach (var r in Combinations(n - 1, k - 1))
					yield return r.Push(true);

			foreach (var r in Combinations(n - 1, k))
				yield return r.Push(false);
		}

		#endregion

		#region Register To List Event and Traverse

		public static void RegisterToListEvent<TItem>(this List<TItem> list, bool ignoreIfListIsNull, Action subscriptionAction, Action<TItem> actionForEachItem)
		{
			if (list == null)
			{
				if (!ignoreIfListIsNull)
				{
					throw new ArgumentNullException(nameof(list), "List is null. Could not register to list events.");
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
				throw new ArgumentNullException(nameof(list));

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

		public static void AppendOrCreate<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary,
			TKey key,
			TValue value)
		{
			List<TValue> list;
			if (!dictionary.TryGetValue(key, out list) || list == null)
			{
				list = new List<TValue>();
				dictionary.Add(key, list);
			}
			list.Add(value);
		}

		public static int AddOrIncrement<TKey>(this IDictionary<TKey, int> dictionary,
			TKey key,
			int initialValue = 1)
		{
			int ret;
			if (dictionary.TryGetValue(key, out ret))
			{
				ret++;
				dictionary[key] = ret;
			}
			else
			{
				ret = initialValue;
				dictionary.Add(key, ret);
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
		private readonly Func<T, object> Expression;

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
		private readonly Func<T1, object> Expression1;
		private readonly Func<T2, object> Expression2;

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

}
