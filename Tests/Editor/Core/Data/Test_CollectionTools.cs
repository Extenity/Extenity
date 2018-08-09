using System;
using Extenity.DataToolbox;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_CollectionTools : AssertionHelper
	{
		#region Array / List / Collection / Enumerable

		[Test]
		public static void ArrayInsert()
		{
			int[] array = null;
			Assert.Throws<NullReferenceException>(() => array.Insert(0));

			array = new int[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => array.Insert(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => array.Insert(1));
			array = array.Insert(0);
			Assert.True(array.Length == 1);
			Assert.True(array[0] == 0);

			array = new int[] { 42 };
			array = array.Insert(0);
			Assert.True(array.Length == 2);
			Assert.True(array[0] == 0);
			Assert.True(array[1] == 42);

			array = new int[] { 42 };
			array = array.Insert(1);
			Assert.True(array.Length == 2);
			Assert.True(array[0] == 42);
			Assert.True(array[1] == 0);

			array = new int[] { 21, 42 };
			array = array.Insert(0);
			Assert.True(array.Length == 3);
			Assert.True(array[0] == 0);
			Assert.True(array[1] == 21);
			Assert.True(array[2] == 42);

			array = new int[] { 21, 42 };
			array = array.Insert(1);
			Assert.True(array.Length == 3);
			Assert.True(array[0] == 21);
			Assert.True(array[1] == 0);
			Assert.True(array[2] == 42);

			array = new int[] { 21, 42 };
			array = array.Insert(2);
			Assert.True(array.Length == 3);
			Assert.True(array[0] == 21);
			Assert.True(array[1] == 42);
			Assert.True(array[2] == 0);
		}

		[Test]
		public static void ArrayInsertWithObject()
		{
			int[] array = null;
			Assert.Throws<NullReferenceException>(() => array.Insert(0, 84));

			array = new int[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => array.Insert(-1, 84));
			Assert.Throws<ArgumentOutOfRangeException>(() => array.Insert(1, 84));
			array = array.Insert(0, 84);
			Assert.True(array.Length == 1);
			Assert.True(array[0] == 84);

			array = new int[] { 42 };
			array = array.Insert(0, 84);
			Assert.True(array.Length == 2);
			Assert.True(array[0] == 84);
			Assert.True(array[1] == 42);

			array = new int[] { 42 };
			array = array.Insert(1, 84);
			Assert.True(array.Length == 2);
			Assert.True(array[0] == 42);
			Assert.True(array[1] == 84);

			array = new int[] { 21, 42 };
			array = array.Insert(0, 84);
			Assert.True(array.Length == 3);
			Assert.True(array[0] == 84);
			Assert.True(array[1] == 21);
			Assert.True(array[2] == 42);

			array = new int[] { 21, 42 };
			array = array.Insert(1, 84);
			Assert.True(array.Length == 3);
			Assert.True(array[0] == 21);
			Assert.True(array[1] == 84);
			Assert.True(array[2] == 42);

			array = new int[] { 21, 42 };
			array = array.Insert(2, 84);
			Assert.True(array.Length == 3);
			Assert.True(array[0] == 21);
			Assert.True(array[1] == 42);
			Assert.True(array[2] == 84);
		}

		#endregion
	}

}
