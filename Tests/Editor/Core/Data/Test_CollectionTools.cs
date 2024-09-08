using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_CollectionTools : ExtenityTestBase
	{
		#region Array / List / Collection / Enumerable

		[Test]
		public void Array_Insert()
		{
			int[] array = null;
			Assert.Throws<NullReferenceException>(() => array.Insert(0, 84, out array));

			array = Array.Empty<int>();
			Assert.Throws<ArgumentOutOfRangeException>(() => array.Insert(-1, 84, out array));
			Assert.Throws<ArgumentOutOfRangeException>(() => array.Insert(1, 84, out array));
			array.Insert(0, 84, out array);
			AssertArray(array, 84);

			array = new int[] { 42 };
			array.Insert(0, 84, out array);
			AssertArray(array, 84, 42);

			array = new int[] { 42 };
			array.Insert(1, 84, out array);
			AssertArray(array, 42, 84);

			array = new int[] { 21, 42 };
			array.Insert(0, 84, out array);
			AssertArray(array, 84, 21, 42);

			array = new int[] { 21, 42 };
			array.Insert(1, 84, out array);
			AssertArray(array, 21, 84, 42);

			array = new int[] { 21, 42 };
			array.Insert(2, 84, out array);
			AssertArray(array, 21, 42, 84);
		}

		[Test]
		public void List_AddSorted()
		{
			List<int> list = null;
			Assert.Throws<NullReferenceException>(() => list.AddSorted(123));

			list = new List<int>();

			list.AddSorted(3);
			AssertList(list, 3);

			list.AddSorted(1);
			AssertList(list, 1, 3);

			list.AddSorted(2);
			AssertList(list, 1, 2, 3);

			list.AddSorted(4);
			AssertList(list, 1, 2, 3, 4);

			list.AddSorted(-100);
			AssertList(list, -100, 1, 2, 3, 4);

			list.AddSorted(100);
			AssertList(list, -100, 1, 2, 3, 4, 100);

			list.AddSorted(2);
			AssertList(list, -100, 1, 2, 2, 3, 4, 100);

			list.AddSorted(2);
			AssertList(list, -100, 1, 2, 2, 2, 3, 4, 100);

			list.AddSorted(1);
			AssertList(list, -100, 1, 1, 2, 2, 2, 3, 4, 100);

			list.AddSorted(1);
			AssertList(list, -100, 1, 1, 1, 2, 2, 2, 3, 4, 100);

			list.AddSorted(2);
			AssertList(list, -100, 1, 1, 1, 2, 2, 2, 2, 3, 4, 100);

			list.AddSorted(3);
			AssertList(list, -100, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 100);

			list.AddSorted(-100);
			AssertList(list, -100, -100, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 100);

			list.AddSorted(100);
			AssertList(list, -100, -100, 1, 1, 1, 2, 2, 2, 2, 3, 3, 4, 100, 100);
		}

		#endregion

		#region Utilities

		private static void AssertArray<T>(T[] array, params T[] expected)
		{
			Assert.AreEqual(expected.Length, array.Length);
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], array[i]);
			}
		}

		private static void AssertList<T>(List<T> list, params int[] expected)
		{
			Assert.AreEqual(expected.Length, list.Count);
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], list[i]);
			}
		}

		#endregion
	}

}