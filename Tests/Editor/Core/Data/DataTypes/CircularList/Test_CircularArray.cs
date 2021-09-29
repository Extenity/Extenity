using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_CircularArray : ExtenityTestBase
	{
		#region Construct

		[Test]
		public void ConstructWithZeroOrNegativeCapacity()
		{
			CircularArrayTestItem.ResetIDAssigner();
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { new CircularArray<CircularArrayTestItem>(0); });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { new CircularArray<CircularArrayTestItem>(-1); });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { new CircularArray<CircularArrayTestItem>(-100); });
		}

		[Test]
		public void ConstructWithCapacity()
		{
			CircularArrayTestItem.ResetIDAssigner();
			var array = new CircularArray<CircularArrayTestItem>(1);
			Expect(array.Count == 0);
			Expect(array.Capacity == 1);
			Expect(array.IsCapacityFilled == false);
			array = new CircularArray<CircularArrayTestItem>(100);
			Expect(array.Count == 0);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
		}

		[Test]
		public void ConstructWithCollection()
		{
			CircularArrayTestItem.ResetIDAssigner();
			// Create with empty array
			var copiedArray = New.List<CircularArrayTestItem>();
			Assert.Throws(typeof(Exception), () => { new CircularArray<CircularArrayTestItem>(copiedArray); });

			// Create with filled array
			for (int i = 0; i < 10; i++)
			{
				copiedArray.Add(new CircularArrayTestItem());
			}
			var array = new CircularArray<CircularArrayTestItem>(copiedArray);
			Expect(array.Count == copiedArray.Count);
			Expect(array.Capacity == copiedArray.Count);
			Expect(array.IsCapacityFilled == true);

			// Create with null array
			copiedArray = null;
			Assert.Throws(typeof(ArgumentNullException), () => { new CircularArray<CircularArrayTestItem>(copiedArray); });
		}

		#endregion

		#region Count and Capacity

		[Test]
		public void CountAndCapacityWorksAsItemsAddedToClearList()
		{
			var array = CreateEmptyArray(1);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 1);
			Expect(array.Capacity == 1);
			Expect(array.IsCapacityFilled == true);
			for (int i = 0; i < 1000; i++)
			{
				array.Add(new CircularArrayTestItem());
				Expect(array.Count == 1);
				Expect(array.Capacity == 1);
				Expect(array.IsCapacityFilled == true);
			}

			array = CreateEmptyArray(2);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 1);
			Expect(array.Capacity == 2);
			Expect(array.IsCapacityFilled == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 2);
			Expect(array.Capacity == 2);
			Expect(array.IsCapacityFilled == true);
			for (int i = 0; i < 1000; i++)
			{
				array.Add(new CircularArrayTestItem());
				Expect(array.Count == 2);
				Expect(array.Capacity == 2);
				Expect(array.IsCapacityFilled == true);
			}

			array = CreateEmptyArray(3);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 1);
			Expect(array.Capacity == 3);
			Expect(array.IsCapacityFilled == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 2);
			Expect(array.Capacity == 3);
			Expect(array.IsCapacityFilled == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 3);
			Expect(array.Capacity == 3);
			Expect(array.IsCapacityFilled == true);
			for (int i = 0; i < 1000; i++)
			{
				array.Add(new CircularArrayTestItem());
				Expect(array.Count == 3);
				Expect(array.Capacity == 3);
				Expect(array.IsCapacityFilled == true);
			}
		}

		[Test]
		public void CountAndCapacityWorksAfterRemovingAllItems()
		{
			// Count decreases while Capacity stays the same.
			var array = CreateAndFillArray(100);
			Expect(array.Count == 100);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == true);
			array.Clear();
			Expect(array.Count == 0);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			Assert.Throws(typeof(Exception), () => { array.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { array.RemoveHeading(); });
		}

		[Test]
		public void CountAndCapacityWorksAfterRemovingItemsFromTail()
		{
			// Count decreases while Capacity stays the same.
			var array = CreateAndFillArray(100);
			Expect(array.Count == 100);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == true);
			array.RemoveTailing();
			Expect(array.Count == 99);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveTailing();
			Expect(array.Count == 98);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveTailing();
			Expect(array.Count == 97);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			for (int i = 0; i < 50; i++)
			{
				array.RemoveTailing();
			}
			Expect(array.Count == 47);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			for (int i = 0; i < 45; i++)
			{
				array.RemoveTailing();
			}
			Expect(array.Count == 2);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveTailing();
			Expect(array.Count == 1);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveTailing();
			Expect(array.Count == 0);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			Assert.Throws(typeof(Exception), () => { array.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { array.RemoveHeading(); });
		}

		[Test]
		public void CountAndCapacityWorksAfterRemovingItemsFromHead()
		{
			// Count decreases while Capacity stays the same.
			var array = CreateAndFillArray(100);
			Expect(array.Count == 100);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == true);
			array.RemoveHeading();
			Expect(array.Count == 99);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveHeading();
			Expect(array.Count == 98);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveHeading();
			Expect(array.Count == 97);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			for (int i = 0; i < 50; i++)
			{
				array.RemoveHeading();
			}
			Expect(array.Count == 47);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			for (int i = 0; i < 45; i++)
			{
				array.RemoveHeading();
			}
			Expect(array.Count == 2);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveHeading();
			Expect(array.Count == 1);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			array.RemoveHeading();
			Expect(array.Count == 0);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			Assert.Throws(typeof(Exception), () => { array.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { array.RemoveHeading(); });
		}

		[Test]
		public void CountAndCapacityWorksForNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 20, 19);
			Expect(array.Count == 99);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == false);
			Expect(array.IsArranged == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 100);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == true);
			Expect(array.IsArranged == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.Count == 100);
			Expect(array.Capacity == 100);
			Expect(array.IsCapacityFilled == true);
			Expect(array.IsArranged == false);
		}

		#endregion

		#region Foreach

		[Test]
		public void ForeachDoesNotIterate_WithZeroItems_InCleanList()
		{
			var array = CreateEmptyArray(100);
			var count = 0;
			foreach (var item in array)
				count++;
			Expect(count == 0);
		}

		[Test]
		public void ForeachDoesNotIterate_WithZeroItems_InFilledAndClearedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 100, 0);
			var count = 0;
			foreach (var item in array)
				count++;
			Expect(count == 0);
		}

		[Test]
		public void ForeachIteratesOnce_WithOneItem_InCleanList()
		{
			var array = CreateAndFillArray(100, 1);
			var count = 0;
			foreach (var item in array)
			{
				Expect(0 == item.ID);
				count++;
			}
			Expect(count == 1);

			array = CreateAndFillArray(1, 1);
			count = 0;
			foreach (var item in array)
			{
				Expect(0 == item.ID);
				count++;
			}
			Expect(count == 1);
		}

		[Test]
		public void ForeachIteratesOnce_WithOneItem_InFilledAndClearedArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 99, 0);
			Expect(array.Count == 1);
			Expect(array.CyclicTailIndex == 99);
			var count = 0;
			foreach (var item in array)
			{
				Expect(99 == item.ID);
				count++;
			}
			Expect(count == 1);
		}

		[Test]
		public void ForeachIteratesOnce_WithOneItem_InFilledAndClearedNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 50, 50, 99);
			Expect(array.Count == 1);
			Expect(array.CyclicTailIndex == 49);
			var count = 0;
			foreach (var item in array)
			{
				Expect(149 == item.ID);
				count++;
			}
			Expect(count == 1);
		}

		[Test]
		public void ForeachIteratesTwice_WithTwoItems_InCleanArrangedList()
		{
			var array = CreateAndFillArray(100, 2);
			var i = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
			}
			Expect(i == 2);

			array = CreateAndFillArray(2, 2);
			i = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
			}
			Expect(i == 2);
		}

		[Test]
		public void ForeachIteratesTwice_WithTwoItems_InCleanNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(2, 1, 1);
			var i = 1;
			var count = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
				count++;
			}
			Expect(i == 3);
			Expect(count == 2);
		}

		[Test]
		public void ForeachIteratesTwice_WithTwoItems_InFilledAndClearedArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 98, 0);
			Expect(array.Count == 2);
			Expect(array.CyclicTailIndex == 98);
			var i = 98;
			var count = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
				count++;
			}
			Expect(i == 100);
			Expect(count == 2);
		}

		[Test]
		public void ForeachIteratesTwice_WithTwoItems_InFilledAndClearedNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 50, 50, 98);
			Expect(array.Count == 2);
			Expect(array.CyclicTailIndex == 48);
			var i = 148;
			var count = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
				count++;
			}
			Expect(i == 150);
			Expect(count == 2);
		}

		[Test]
		public void ForeachIteratesHundredTimes_WithHundredItems_InCleanArrangedList()
		{
			var array = CreateAndFillArray(100);
			var i = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
			}
			Expect(i == 100);
		}

		[Test]
		public void ForeachIteratesHundredTimes_WithHundredItems_InCleanNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 30, 30);
			var i = 30;
			var count = 0;
			foreach (var item in array)
			{
				Expect(i == item.ID);
				i++;
				count++;
			}
			Expect(i == 130);
			Expect(count == 100);
		}

		[Test]
		public void ForeachWorksTheSameOnSequentialCalls_InArrangedList()
		{
			var array = CreateAndFillArray(100);
			for (int iForeach = 0; iForeach < 5; iForeach++)
			{
				var i = 0;
				foreach (var item in array)
				{
					Expect(i == item.ID);
					i++;
				}
				Expect(i == 100);
			}
		}

		[Test]
		public void ForeachWorksTheSameOnSequentialCalls_InNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 20, 20);
			for (int iForeach = 0; iForeach < 5; iForeach++)
			{
				var i = 20;
				var count = 0;
				foreach (var item in array)
				{
					Expect(i == item.ID);
					i++;
					count++;
				}
				Expect(i == 120);
				Expect(count == 100);
			}
		}

		#endregion

		#region Tailing Item and Heading Item

		[Test]
		public void TailingItemThrowsIfListIsEmpty()
		{
			var array = CreateEmptyArray(5);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = array.TailingItem; });
		}

		[Test]
		public void HeadingItemThrowsIfListIsEmpty()
		{
			var array = CreateEmptyArray(5);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = array.HeadingItem; });
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithOneItem()
		{
			var array = CreateAndFillArray(1);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 0);
			Expect(array.HeadingItem == array.TailingItem);
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithTwoItems()
		{
			var array = CreateAndFillArray(2);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 1);
			Expect(array.HeadingItem != array.TailingItem);
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithThreeItems()
		{
			var array = CreateAndFillArray(3);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 2);
			Expect(array.HeadingItem != array.TailingItem);
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithHundredItems()
		{
			var array = CreateAndFillArray(100);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 99);
			Expect(array.HeadingItem != array.TailingItem);
		}

		#endregion

		#region Remove Tailing and Remove Heading

		[Test]
		public void RemoveTailingWorks_WithOneItem()
		{
			var array = CreateAndFillArray(1);
			array.RemoveTailing();
			Expect(array.Count == 0);
			Expect(array.CyclicTailIndex == -1);
			Expect(array.CyclicHeadIndex == -1);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = array.TailingItem; });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = array.HeadingItem; });
		}

		[Test]
		public void RemoveTailingWorks_WithTwoItems()
		{
			var array = CreateAndFillArray(2);
			Expect(array.Count == 2);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 1);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 1);
			array.RemoveTailing();
			Expect(array.Count == 1);
			Expect(array.TailingItem == array.HeadingItem);
			Expect(array.TailingItem.ID == 1);
			Expect(array.HeadingItem.ID == 1);
			Expect(array.CyclicTailIndex == 1);
			Expect(array.CyclicHeadIndex == 1);
		}

		[Test]
		public void RemoveTailingWorks_WithHundredItems()
		{
			var array = CreateAndFillArray(100);
			Expect(array.Count == 100);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 99);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 99);
			array.RemoveTailing();
			Expect(array.Count == 99);
			Expect(array.TailingItem.ID == 1);
			Expect(array.HeadingItem.ID == 99);
			Expect(array.CyclicTailIndex == 1);
			Expect(array.CyclicHeadIndex == 99);
		}

		[Test]
		public void RemoveHeadingWorks_WithOneItem()
		{
			var array = CreateAndFillArray(1);
			array.RemoveHeading();
			Expect(array.Count == 0);
			Expect(array.CyclicTailIndex == -1);
			Expect(array.CyclicHeadIndex == -1);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = array.TailingItem; });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = array.HeadingItem; });
		}

		[Test]
		public void RemoveHeadingWorks_WithTwoItems()
		{
			var array = CreateAndFillArray(2);
			Expect(array.Count == 2);
			Expect(array.TailingItem != array.HeadingItem);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 1);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 1);
			array.RemoveHeading();
			Expect(array.Count == 1);
			Expect(array.TailingItem == array.HeadingItem);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 0);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 0);
		}

		[Test]
		public void RemoveHeadingWorks_WithHundredItems()
		{
			var array = CreateAndFillArray(100);
			Expect(array.Count == 100);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 99);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 99);
			array.RemoveHeading();
			Expect(array.Count == 99);
			Expect(array.TailingItem.ID == 0);
			Expect(array.HeadingItem.ID == 98);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 98);
		}

		#endregion

		#region Remove Tailing and Add Item

		[Test]
		public void RemoveTailing_ThenAddNewItem_WhenCapacityAllows()
		{
			var array = CreateAndFillArray(11, 10);
			array.RemoveTailing();
			var item = new CircularArrayTestItem();
			array.Add(item);
			Expect(array.BufferIndexOf(item) == 10);
			Expect(array.CyclicTailIndex == 1);
			Expect(array.CyclicHeadIndex == 10);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item);
			Expect(array.TailingItem.ID == 1);
			Expect(array.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveTailing_ThenAddNewItem_WhenCapacityExceeds()
		{
			var array = CreateAndFillArray(10, 10);
			array.RemoveTailing();
			var item = new CircularArrayTestItem();
			array.Add(item);
			Expect(array.BufferIndexOf(item) == 0);
			Expect(array.CyclicTailIndex == 1);
			Expect(array.CyclicHeadIndex == 0);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item);
			Expect(array.TailingItem.ID == 1);
			Expect(array.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoTailing_ThenAddTwoNewItems_WhenCapacityAllows()
		{
			var array = CreateAndFillArray(12, 10);
			array.RemoveTailing();
			array.RemoveTailing();
			var item1 = new CircularArrayTestItem();
			array.Add(item1);
			Expect(array.IsCapacityFilled == false);
			var item2 = new CircularArrayTestItem();
			array.Add(item2);
			Expect(array.BufferIndexOf(item1) == 10);
			Expect(array.BufferIndexOf(item2) == 11);
			Expect(array.CyclicTailIndex == 2);
			Expect(array.CyclicHeadIndex == 11);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item2);
			Expect(array.TailingItem.ID == 2);
			Expect(array.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveTwoTailing_ThenAddTwoNewItems_WhenCapacityExceeds()
		{
			var array = CreateAndFillArray(10, 10);
			array.RemoveTailing();
			array.RemoveTailing();
			var item1 = new CircularArrayTestItem();
			array.Add(item1);
			Expect(array.IsCapacityFilled == false);
			var item2 = new CircularArrayTestItem();
			array.Add(item2);
			Expect(array.BufferIndexOf(item1) == 0);
			Expect(array.BufferIndexOf(item2) == 1);
			Expect(array.CyclicTailIndex == 2);
			Expect(array.CyclicHeadIndex == 1);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item2);
			Expect(array.TailingItem.ID == 2);
			Expect(array.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoTailing_ThenAddThreeNewItems_WhenCapacityExceedsForOnlyOne()
		{
			var array = CreateAndFillArray(11, 10);
			array.RemoveTailing();
			array.RemoveTailing();
			var item1 = new CircularArrayTestItem();
			array.Add(item1);
			Expect(array.IsCapacityFilled == false);
			var item2 = new CircularArrayTestItem();
			array.Add(item2);
			Expect(array.BufferIndexOf(item1) == 10);
			Expect(array.BufferIndexOf(item2) == 0);
			Expect(array.CyclicTailIndex == 2);
			Expect(array.CyclicHeadIndex == 0);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item2);
			Expect(array.TailingItem.ID == 2);
			Expect(array.IsCapacityFilled == false);
			// Third item
			var item3 = new CircularArrayTestItem();
			array.Add(item3);
			Expect(array.BufferIndexOf(item1) == 10);
			Expect(array.BufferIndexOf(item2) == 0);
			Expect(array.BufferIndexOf(item3) == 1);
			Expect(array.CyclicTailIndex == 2);
			Expect(array.CyclicHeadIndex == 1);
			Expect(array.Count == 11);
			Expect(array.HeadingItem == item3);
			Expect(array.TailingItem.ID == 2);
			Expect(array.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveAllTailing_ThenAddNewItems_ShouldRearrange()
		{
			var array = CreateAndFillNonArrangedArray(10, 3, 3);
			Expect(array.IsArranged == false);
			while (array.Count > 0)
			{
				array.RemoveTailing();
			}
			Expect(array.IsEmpty == true);
			Expect(array.IsEmptyOrArranged == true);
			Expect(array.IsArranged == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.IsEmpty == false);
			Expect(array.IsEmptyOrArranged == true);
			Expect(array.IsArranged == true);
			array.Add(new CircularArrayTestItem());
			Expect(array.IsEmpty == false);
			Expect(array.IsEmptyOrArranged == true);
			Expect(array.IsArranged == true);
		}

		#endregion

		#region Remove Heading and Add Item

		[Test]
		public void RemoveHeading_ThenAddNewItem_WhenCapacityAllows()
		{
			var array = CreateAndFillArray(11, 10);
			array.RemoveHeading();
			var item = new CircularArrayTestItem();
			array.Add(item);
			Expect(array.BufferIndexOf(item) == 9);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 9);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item);
			Expect(array.TailingItem.ID == 0);
			Expect(array.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveHeading_ThenAddNewItem_WhenCapacityWasPreviouslyExceeding()
		{
			var array = CreateAndFillArray(10, 10);
			array.RemoveHeading();
			var item = new CircularArrayTestItem();
			array.Add(item);
			Expect(array.BufferIndexOf(item) == 9);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 9);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item);
			Expect(array.TailingItem.ID == 0);
			Expect(array.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoHeadings_ThenAddTwoNewItems_WhenCapacityAllows()
		{
			var array = CreateAndFillArray(12, 10);
			array.RemoveHeading();
			array.RemoveHeading();
			var item1 = new CircularArrayTestItem();
			array.Add(item1);
			Expect(array.IsCapacityFilled == false);
			var item2 = new CircularArrayTestItem();
			array.Add(item2);
			Expect(array.BufferIndexOf(item1) == 8);
			Expect(array.BufferIndexOf(item2) == 9);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 9);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item2);
			Expect(array.TailingItem.ID == 0);
			Expect(array.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveTwoHeadings_ThenAddTwoNewItems_WhenCapacityWasPreviouslyExceeding()
		{
			var array = CreateAndFillArray(10, 10);
			array.RemoveHeading();
			array.RemoveHeading();
			var item1 = new CircularArrayTestItem();
			array.Add(item1);
			Expect(array.IsCapacityFilled == false);
			var item2 = new CircularArrayTestItem();
			array.Add(item2);
			Expect(array.BufferIndexOf(item1) == 8);
			Expect(array.BufferIndexOf(item2) == 9);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 9);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item2);
			Expect(array.TailingItem.ID == 0);
			Expect(array.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoHeadings_ThenAddThreeNewItems_WhenCapacityWasPreviouslyExceedingForOnlyOne()
		{
			var array = CreateAndFillArray(11, 10);
			array.RemoveHeading();
			array.RemoveHeading();
			var item1 = new CircularArrayTestItem();
			array.Add(item1);
			Expect(array.IsCapacityFilled == false);
			var item2 = new CircularArrayTestItem();
			array.Add(item2);
			Expect(array.BufferIndexOf(item1) == 8);
			Expect(array.BufferIndexOf(item2) == 9);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 9);
			Expect(array.Count == 10);
			Expect(array.HeadingItem == item2);
			Expect(array.TailingItem.ID == 0);
			Expect(array.IsCapacityFilled == false);
			// Third item
			var item3 = new CircularArrayTestItem();
			array.Add(item3);
			Expect(array.BufferIndexOf(item1) == 8);
			Expect(array.BufferIndexOf(item2) == 9);
			Expect(array.BufferIndexOf(item3) == 10);
			Expect(array.CyclicTailIndex == 0);
			Expect(array.CyclicHeadIndex == 10);
			Expect(array.Count == 11);
			Expect(array.HeadingItem == item3);
			Expect(array.TailingItem.ID == 0);
			Expect(array.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveAllHeadings_ThenAddNewItems_ShouldRearrange()
		{
			var array = CreateAndFillNonArrangedArray(10, 3, 3);
			Expect(array.IsArranged == false);
			while (array.Count > 0)
			{
				array.RemoveHeading();
			}
			Expect(array.IsEmpty == true);
			Expect(array.IsEmptyOrArranged == true);
			Expect(array.IsArranged == false);
			array.Add(new CircularArrayTestItem());
			Expect(array.IsEmpty == false);
			Expect(array.IsEmptyOrArranged == true);
			Expect(array.IsArranged == true);
			array.Add(new CircularArrayTestItem());
			Expect(array.IsEmpty == false);
			Expect(array.IsEmptyOrArranged == true);
			Expect(array.IsArranged == true);
		}

		#endregion

		#region Pop Tailing

		[Test]
		public void PopTailing_WithArrangedList()
		{
			var array = CreateAndFillArray(100);
			var i = 0;
			while (array.Count > 0)
			{
				var item = array.PopTailing();
				Expect(item.ID == i);
				i++;
			}
		}

		[Test]
		public void PopTailing_WithNonArrangedList()
		{
			var array = CreateAndFillNonArrangedArray(100, 20, 20);
			var i = 20;
			while (array.Count > 0)
			{
				var item = array.PopTailing();
				Expect(item.ID == i);
				i++;
			}
		}

		#endregion

		#region Tools

		private class CircularArrayTestItem
		{
			public CircularArrayTestItem()
			{
				ID = ++LastGivenID;
			}

			public static int LastGivenID = -1;
			public readonly int ID;

			//public int SomeImportantParameter;
			//public string SomeNotSoImportantParameter;

			public static void ResetIDAssigner()
			{
				LastGivenID = -1;
			}
		}

		private static CircularArray<CircularArrayTestItem> CreateEmptyArray(int capacity)
		{
			CircularArrayTestItem.ResetIDAssigner();
			return new CircularArray<CircularArrayTestItem>(capacity);
		}

		private static CircularArray<CircularArrayTestItem> CreateAndFillArray(int capacity)
		{
			CircularArrayTestItem.ResetIDAssigner();
			var array = new CircularArray<CircularArrayTestItem>(capacity);
			for (int i = 0; i < capacity; i++)
			{
				array.Add(new CircularArrayTestItem());
			}
			return array;
		}

		private static CircularArray<CircularArrayTestItem> CreateAndFillArray(int capacity, int itemCount)
		{
			CircularArrayTestItem.ResetIDAssigner();
			var array = new CircularArray<CircularArrayTestItem>(capacity);
			for (int i = 0; i < itemCount; i++)
			{
				array.Add(new CircularArrayTestItem());
			}
			return array;
		}

		private static CircularArray<CircularArrayTestItem> CreateAndFillNonArrangedArray(int capacity, int removeTailingCount, int addLaterCount, int secondBatchOfRemoveTailingCount = 0)
		{
			CircularArrayTestItem.ResetIDAssigner();
			var array = new CircularArray<CircularArrayTestItem>(capacity);
			for (int i = 0; i < capacity; i++)
			{
				array.Add(new CircularArrayTestItem());
			}
			for (int i = 0; i < removeTailingCount; i++)
			{
				array.RemoveTailing();
			}
			for (int i = 0; i < addLaterCount; i++)
			{
				array.Add(new CircularArrayTestItem());
			}
			for (int i = 0; i < secondBatchOfRemoveTailingCount; i++)
			{
				array.RemoveTailing();
			}
			return array;
		}

		#endregion
	}

}
