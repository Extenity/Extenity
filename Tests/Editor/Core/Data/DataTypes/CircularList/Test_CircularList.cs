using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_CircularList : ExtenityTestBase
	{
		#region Construct

		[Test]
		public void ConstructWithoutParameters()
		{
			CircularListTestItem.ResetIDAssigner();
			var list = new CircularList<CircularListTestItem>();
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void ConstructWithCapacity()
		{
			CircularListTestItem.ResetIDAssigner();
			var list = new CircularList<CircularListTestItem>(0);
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);
			list = new CircularList<CircularListTestItem>(1);
			Expect(list.Count == 0);
			Expect(list.Capacity == 1);
			Expect(list.IsCapacityFilled == false);
			list = new CircularList<CircularListTestItem>(100);
			Expect(list.Count == 0);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { list = new CircularList<CircularListTestItem>(-1); });
		}

		[Test]
		public void ConstructWithCollection()
		{
			CircularListTestItem.ResetIDAssigner();
			// Create with empty list
			var copiedList = new List<CircularListTestItem>();
			var list = new CircularList<CircularListTestItem>(copiedList);
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);

			// Create with filled list
			for (int i = 0; i < 10; i++)
			{
				copiedList.Add(new CircularListTestItem());
			}
			list = new CircularList<CircularListTestItem>(copiedList);
			Expect(list.Count == copiedList.Count);
			Expect(list.Capacity == copiedList.Count);
			Expect(list.IsCapacityFilled == true);

			// Create with null list
			copiedList = null;
			Assert.Throws(typeof(ArgumentNullException), () => { list = new CircularList<CircularListTestItem>(copiedList); });
		}

		#endregion

		#region Count and Capacity

		[Test]
		public void CountAndCapacityWorksAsItemsAddedToClearList()
		{
			var list = CreateList(0);
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);
			list.Add(new CircularListTestItem());
			Expect(list.Count == 1);
			Expect(list.Capacity == CircularList<CircularListTestItem>.DefaultCapacity);
			Expect(list.IsCapacityFilled == false);
			list.Add(new CircularListTestItem());
			Expect(list.Count == 2);
			Expect(list.Capacity == CircularList<CircularListTestItem>.DefaultCapacity);
			Expect(list.IsCapacityFilled == false);
			for (int i = 0; i < 50; i++)
			{
				list.Add(new CircularListTestItem());
			}
			Expect(list.Count == 52);
			Expect(list.Capacity == 64);
			Expect(list.IsCapacityFilled == false);
			for (int i = 0; i < 1000; i++)
			{
				list.Add(new CircularListTestItem());
			}
			Expect(list.Count == 1052);
			Expect(list.Capacity == 2048);
			Expect(list.IsCapacityFilled == false);
		}

		[Test]
		public void CountAndCapacityWorksAfterRemovingAllItems()
		{
			// Count decreases while Capacity stays the same.
			var list = CreateList(100);
			Expect(list.Count == 100);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == true);
			list.Clear();
			Expect(list.Count == 0);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { list.RemoveHeading(); });
			list.TrimExcess();
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { list.RemoveHeading(); });
		}

		[Test]
		public void CountAndCapacityWorksAfterRemovingItemsFromTail()
		{
			// Count decreases while Capacity stays the same.
			var list = CreateList(100);
			Expect(list.Count == 100);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == true);
			list.RemoveTailing();
			Expect(list.Count == 99);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveTailing();
			Expect(list.Count == 98);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveTailing();
			Expect(list.Count == 97);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			for (int i = 0; i < 50; i++)
			{
				list.RemoveTailing();
			}
			Expect(list.Count == 47);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			for (int i = 0; i < 45; i++)
			{
				list.RemoveTailing();
			}
			Expect(list.Count == 2);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveTailing();
			Expect(list.Count == 1);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveTailing();
			Expect(list.Count == 0);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { list.RemoveHeading(); });
			list.TrimExcess();
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { list.RemoveHeading(); });
		}

		[Test]
		public void CountAndCapacityWorksAfterRemovingItemsFromHead()
		{
			// Count decreases while Capacity stays the same.
			var list = CreateList(100);
			Expect(list.Count == 100);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == true);
			list.RemoveHeading();
			Expect(list.Count == 99);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveHeading();
			Expect(list.Count == 98);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveHeading();
			Expect(list.Count == 97);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			for (int i = 0; i < 50; i++)
			{
				list.RemoveHeading();
			}
			Expect(list.Count == 47);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			for (int i = 0; i < 45; i++)
			{
				list.RemoveHeading();
			}
			Expect(list.Count == 2);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveHeading();
			Expect(list.Count == 1);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			list.RemoveHeading();
			Expect(list.Count == 0);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { list.RemoveHeading(); });
			list.TrimExcess();
			Expect(list.Count == 0);
			Expect(list.Capacity == 0);
			Expect(list.IsCapacityFilled == true);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailing(); });
			Assert.Throws(typeof(Exception), () => { list.RemoveHeading(); });
		}

		[Test]
		public void CountAndCapacityWorksForNonArrangedList()
		{
			var list = CreateNonArrangedList(100, 20, 19);
			Expect(list.Count == 99);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == false);
			Expect(list.IsArranged == false);
			list.Add(new CircularListTestItem());
			Expect(list.Count == 100);
			Expect(list.Capacity == 100);
			Expect(list.IsCapacityFilled == true);
			Expect(list.IsArranged == false);
			list.Add(new CircularListTestItem());
			Expect(list.Count == 101);
			Expect(list.Capacity == 200);
			Expect(list.IsCapacityFilled == false);
			Expect(list.IsArranged == true);
			for (int i = 0; i < 98; i++)
			{
				list.Add(new CircularListTestItem());
			}
			Expect(list.Count == 199);
			Expect(list.Capacity == 200);
			Expect(list.IsCapacityFilled == false);
			Expect(list.IsArranged == true);
			list.Add(new CircularListTestItem());
			Expect(list.Count == 200);
			Expect(list.Capacity == 200);
			Expect(list.IsCapacityFilled == true);
			Expect(list.IsArranged == true);
			list.Add(new CircularListTestItem());
			Expect(list.Count == 201);
			Expect(list.Capacity == 400);
			Expect(list.IsCapacityFilled == false);
			Expect(list.IsArranged == true);
		}

		[Test]
		public void ChangingCapacityRearrangesTheItems()
		{
			// Decrease capacity
			var list = CreateNonArrangedList(100, 20, 19);
			Expect(list.Capacity == 100);
			Expect(list.IsArranged == false);
			list.Capacity = 99;
			Expect(list.Capacity == 99);
			Expect(list.IsArranged == true);
			// Increase capacity
			list = CreateNonArrangedList(100, 20, 19);
			Expect(list.Capacity == 100);
			Expect(list.IsArranged == false);
			list.Capacity = 150;
			Expect(list.Capacity == 150);
			Expect(list.IsArranged == true);
		}

		#endregion

		#region Foreach

		[Test]
		public void ForeachDoesNotIterate_WithZeroItems_InCleanList()
		{
			var list = CreateList(0);
			var count = 0;
			foreach (var item in list)
				count++;
			Expect(count == 0);
		}

		[Test]
		public void ForeachDoesNotIterate_WithZeroItems_InFilledAndClearedList()
		{
			var list = CreateNonArrangedList(100, 100, 0);
			var count = 0;
			foreach (var item in list)
				count++;
			Expect(count == 0);
		}

		[Test]
		public void ForeachIteratesOnce_WithOneItem_InCleanList()
		{
			var list = CreateList(1);
			var count = 0;
			foreach (var item in list)
			{
				Expect(0 == item.ID);
				count++;
			}
			Expect(count == 1);
		}

		[Test]
		public void ForeachIteratesOnce_WithOneItem_InFilledAndClearedArrangedList()
		{
			var list = CreateNonArrangedList(100, 99, 0);
			Expect(list.Count == 1);
			Expect(list.CyclicTailIndex == 99);
			var count = 0;
			foreach (var item in list)
			{
				Expect(99 == item.ID);
				count++;
			}
			Expect(count == 1);
		}

		[Test]
		public void ForeachIteratesOnce_WithOneItem_InFilledAndClearedNonArrangedList()
		{
			var list = CreateNonArrangedList(100, 50, 50, 99);
			Expect(list.Count == 1);
			Expect(list.CyclicTailIndex == 49);
			var count = 0;
			foreach (var item in list)
			{
				Expect(149 == item.ID);
				count++;
			}
			Expect(count == 1);
		}

		[Test]
		public void ForeachIteratesTwice_WithTwoItems_InCleanArrangedList()
		{
			var list = CreateList(2);
			var i = 0;
			foreach (var item in list)
			{
				Expect(i == item.ID);
				i++;
			}
			Expect(i == 2);
		}

		[Test]
		public void ForeachIteratesTwice_WithTwoItems_InCleanNonArrangedList()
		{
			var list = CreateNonArrangedList(2, 1, 1);
			var i = 1;
			var count = 0;
			foreach (var item in list)
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
			var list = CreateNonArrangedList(100, 98, 0);
			Expect(list.Count == 2);
			Expect(list.CyclicTailIndex == 98);
			var i = 98;
			var count = 0;
			foreach (var item in list)
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
			var list = CreateNonArrangedList(100, 50, 50, 98);
			Expect(list.Count == 2);
			Expect(list.CyclicTailIndex == 48);
			var i = 148;
			var count = 0;
			foreach (var item in list)
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
			var list = CreateList(100);
			var i = 0;
			foreach (var item in list)
			{
				Expect(i == item.ID);
				i++;
			}
			Expect(i == 100);
		}

		[Test]
		public void ForeachIteratesHundredTimes_WithHundredItems_InCleanNonArrangedList()
		{
			var list = CreateNonArrangedList(100, 30, 30);
			var i = 30;
			var count = 0;
			foreach (var item in list)
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
			var list = CreateList(100);
			for (int iForeach = 0; iForeach < 5; iForeach++)
			{
				var i = 0;
				foreach (var item in list)
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
			var list = CreateNonArrangedList(100, 20, 20);
			for (int iForeach = 0; iForeach < 5; iForeach++)
			{
				var i = 20;
				var count = 0;
				foreach (var item in list)
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
			var list = CreateList(0);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = list.TailingItem; });
		}

		[Test]
		public void HeadingItemThrowsIfListIsEmpty()
		{
			var list = CreateList(0);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = list.HeadingItem; });
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithOneItem()
		{
			var list = CreateList(1);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 0);
			Expect(list.HeadingItem == list.TailingItem);
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithTwoItems()
		{
			var list = CreateList(2);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 1);
			Expect(list.HeadingItem != list.TailingItem);
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithThreeItems()
		{
			var list = CreateList(3);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 2);
			Expect(list.HeadingItem != list.TailingItem);
		}

		[Test]
		public void TailingAndHeadingItemWorks_WithHundredItems()
		{
			var list = CreateList(100);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 99);
			Expect(list.HeadingItem != list.TailingItem);
		}

		#endregion

		#region Remove Tailing and Remove Heading

		[Test]
		public void RemoveTailingWorks_WithOneItem()
		{
			var list = CreateList(1);
			list.RemoveTailing();
			Expect(list.Count == 0);
			Expect(list.CyclicTailIndex == -1);
			Expect(list.CyclicHeadIndex == -1);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = list.TailingItem; });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = list.HeadingItem; });
		}

		[Test]
		public void RemoveTailingWorks_WithTwoItems()
		{
			var list = CreateList(2);
			Expect(list.Count == 2);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 1);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 1);
			list.RemoveTailing();
			Expect(list.Count == 1);
			Expect(list.TailingItem == list.HeadingItem);
			Expect(list.TailingItem.ID == 1);
			Expect(list.HeadingItem.ID == 1);
			Expect(list.CyclicTailIndex == 1);
			Expect(list.CyclicHeadIndex == 1);
		}

		[Test]
		public void RemoveTailingWorks_WithHundredItems()
		{
			var list = CreateList(100);
			Expect(list.Count == 100);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 99);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 99);
			list.RemoveTailing();
			Expect(list.Count == 99);
			Expect(list.TailingItem.ID == 1);
			Expect(list.HeadingItem.ID == 99);
			Expect(list.CyclicTailIndex == 1);
			Expect(list.CyclicHeadIndex == 99);
		}

		[Test]
		public void RemoveHeadingWorks_WithOneItem()
		{
			var list = CreateList(1);
			list.RemoveHeading();
			Expect(list.Count == 0);
			Expect(list.CyclicTailIndex == -1);
			Expect(list.CyclicHeadIndex == -1);
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = list.TailingItem; });
			Assert.Throws(typeof(ArgumentOutOfRangeException), () => { var item = list.HeadingItem; });
		}

		[Test]
		public void RemoveHeadingWorks_WithTwoItems()
		{
			var list = CreateList(2);
			Expect(list.Count == 2);
			Expect(list.TailingItem != list.HeadingItem);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 1);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 1);
			list.RemoveHeading();
			Expect(list.Count == 1);
			Expect(list.TailingItem == list.HeadingItem);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 0);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 0);
		}

		[Test]
		public void RemoveHeadingWorks_WithHundredItems()
		{
			var list = CreateList(100);
			Expect(list.Count == 100);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 99);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 99);
			list.RemoveHeading();
			Expect(list.Count == 99);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 98);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 98);
		}

		#endregion

		#region Remove Tailing and Add Item

		[Test]
		public void RemoveTailing_ThenAddNewItem_WhenCapacityAllows()
		{
			var list = CreateList(10);
			list.Capacity = 11;
			list.RemoveTailing();
			var item = new CircularListTestItem();
			list.Add(item);
			Expect(list.BufferIndexOf(item) == 10);
			Expect(list.CyclicTailIndex == 1);
			Expect(list.CyclicHeadIndex == 10);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item);
			Expect(list.TailingItem.ID == 1);
			Expect(list.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveTailing_ThenAddNewItem_WhenCapacityExceeds()
		{
			var list = CreateList(10);
			list.RemoveTailing();
			var item = new CircularListTestItem();
			list.Add(item);
			Expect(list.BufferIndexOf(item) == 0);
			Expect(list.CyclicTailIndex == 1);
			Expect(list.CyclicHeadIndex == 0);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item);
			Expect(list.TailingItem.ID == 1);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoTailing_ThenAddTwoNewItems_WhenCapacityAllows()
		{
			var list = CreateList(10);
			list.Capacity = 12;
			list.RemoveTailing();
			list.RemoveTailing();
			var item1 = new CircularListTestItem();
			list.Add(item1);
			Expect(list.IsCapacityFilled == false);
			var item2 = new CircularListTestItem();
			list.Add(item2);
			Expect(list.BufferIndexOf(item1) == 10);
			Expect(list.BufferIndexOf(item2) == 11);
			Expect(list.CyclicTailIndex == 2);
			Expect(list.CyclicHeadIndex == 11);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item2);
			Expect(list.TailingItem.ID == 2);
			Expect(list.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveTwoTailing_ThenAddTwoNewItems_WhenCapacityExceeds()
		{
			var list = CreateList(10);
			list.RemoveTailing();
			list.RemoveTailing();
			var item1 = new CircularListTestItem();
			list.Add(item1);
			Expect(list.IsCapacityFilled == false);
			var item2 = new CircularListTestItem();
			list.Add(item2);
			Expect(list.BufferIndexOf(item1) == 0);
			Expect(list.BufferIndexOf(item2) == 1);
			Expect(list.CyclicTailIndex == 2);
			Expect(list.CyclicHeadIndex == 1);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item2);
			Expect(list.TailingItem.ID == 2);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoTailing_ThenAddThreeNewItems_WhenCapacityExceedsForOnlyOne()
		{
			var list = CreateList(10);
			list.Capacity = 11;
			list.RemoveTailing();
			list.RemoveTailing();
			var item1 = new CircularListTestItem();
			list.Add(item1);
			Expect(list.IsCapacityFilled == false);
			var item2 = new CircularListTestItem();
			list.Add(item2);
			Expect(list.BufferIndexOf(item1) == 10);
			Expect(list.BufferIndexOf(item2) == 0);
			Expect(list.CyclicTailIndex == 2);
			Expect(list.CyclicHeadIndex == 0);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item2);
			Expect(list.TailingItem.ID == 2);
			Expect(list.IsCapacityFilled == false);
			// Third item
			var item3 = new CircularListTestItem();
			list.Add(item3);
			Expect(list.BufferIndexOf(item1) == 10);
			Expect(list.BufferIndexOf(item2) == 0);
			Expect(list.BufferIndexOf(item3) == 1);
			Expect(list.CyclicTailIndex == 2);
			Expect(list.CyclicHeadIndex == 1);
			Expect(list.Count == 11);
			Expect(list.HeadingItem == item3);
			Expect(list.TailingItem.ID == 2);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveAllTailing_ThenAddNewItems_ShouldRearrange()
		{
			var list = CreateNonArrangedList(10, 3, 3);
			Expect(list.IsArranged == false);
			while (list.Count > 0)
			{
				list.RemoveTailing();
			}
			Expect(list.IsEmpty == true);
			Expect(list.IsEmptyOrArranged == true);
			Expect(list.IsArranged == false);
			list.Add(new CircularListTestItem());
			Expect(list.IsEmpty == false);
			Expect(list.IsEmptyOrArranged == true);
			Expect(list.IsArranged == true);
			list.Add(new CircularListTestItem());
			Expect(list.IsEmpty == false);
			Expect(list.IsEmptyOrArranged == true);
			Expect(list.IsArranged == true);
		}

		#endregion

		#region Remove Heading and Add Item

		[Test]
		public void RemoveHeading_ThenAddNewItem_WhenCapacityAllows()
		{
			var list = CreateList(10);
			list.Capacity = 11;
			list.RemoveHeading();
			var item = new CircularListTestItem();
			list.Add(item);
			Expect(list.BufferIndexOf(item) == 9);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 9);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item);
			Expect(list.TailingItem.ID == 0);
			Expect(list.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveHeading_ThenAddNewItem_WhenCapacityWasPreviouslyExceeding()
		{
			var list = CreateList(10);
			list.RemoveHeading();
			var item = new CircularListTestItem();
			list.Add(item);
			Expect(list.BufferIndexOf(item) == 9);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 9);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item);
			Expect(list.TailingItem.ID == 0);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoHeadings_ThenAddTwoNewItems_WhenCapacityAllows()
		{
			var list = CreateList(10);
			list.Capacity = 12;
			list.RemoveHeading();
			list.RemoveHeading();
			var item1 = new CircularListTestItem();
			list.Add(item1);
			Expect(list.IsCapacityFilled == false);
			var item2 = new CircularListTestItem();
			list.Add(item2);
			Expect(list.BufferIndexOf(item1) == 8);
			Expect(list.BufferIndexOf(item2) == 9);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 9);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item2);
			Expect(list.TailingItem.ID == 0);
			Expect(list.IsCapacityFilled == false);
		}

		[Test]
		public void RemoveTwoHeadings_ThenAddTwoNewItems_WhenCapacityWasPreviouslyExceeding()
		{
			var list = CreateList(10);
			list.RemoveHeading();
			list.RemoveHeading();
			var item1 = new CircularListTestItem();
			list.Add(item1);
			Expect(list.IsCapacityFilled == false);
			var item2 = new CircularListTestItem();
			list.Add(item2);
			Expect(list.BufferIndexOf(item1) == 8);
			Expect(list.BufferIndexOf(item2) == 9);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 9);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item2);
			Expect(list.TailingItem.ID == 0);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveTwoHeadings_ThenAddThreeNewItems_WhenCapacityWasPreviouslyExceedingForOnlyOne()
		{
			var list = CreateList(10);
			list.Capacity = 11;
			list.RemoveHeading();
			list.RemoveHeading();
			var item1 = new CircularListTestItem();
			list.Add(item1);
			Expect(list.IsCapacityFilled == false);
			var item2 = new CircularListTestItem();
			list.Add(item2);
			Expect(list.BufferIndexOf(item1) == 8);
			Expect(list.BufferIndexOf(item2) == 9);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 9);
			Expect(list.Count == 10);
			Expect(list.HeadingItem == item2);
			Expect(list.TailingItem.ID == 0);
			Expect(list.IsCapacityFilled == false);
			// Third item
			var item3 = new CircularListTestItem();
			list.Add(item3);
			Expect(list.BufferIndexOf(item1) == 8);
			Expect(list.BufferIndexOf(item2) == 9);
			Expect(list.BufferIndexOf(item3) == 10);
			Expect(list.CyclicTailIndex == 0);
			Expect(list.CyclicHeadIndex == 10);
			Expect(list.Count == 11);
			Expect(list.HeadingItem == item3);
			Expect(list.TailingItem.ID == 0);
			Expect(list.IsCapacityFilled == true);
		}

		[Test]
		public void RemoveAllHeadings_ThenAddNewItems_ShouldRearrange()
		{
			var list = CreateNonArrangedList(10, 3, 3);
			Expect(list.IsArranged == false);
			while (list.Count > 0)
			{
				list.RemoveHeading();
			}
			Expect(list.IsEmpty == true);
			Expect(list.IsEmptyOrArranged == true);
			Expect(list.IsArranged == false);
			list.Add(new CircularListTestItem());
			Expect(list.IsEmpty == false);
			Expect(list.IsEmptyOrArranged == true);
			Expect(list.IsArranged == true);
			list.Add(new CircularListTestItem());
			Expect(list.IsEmpty == false);
			Expect(list.IsEmptyOrArranged == true);
			Expect(list.IsArranged == true);
		}

		#endregion

		#region Pop Tailing

		[Test]
		public void PopTailing_WithArrangedList()
		{
			var list = CreateList(100);
			var i = 0;
			while (list.Count > 0)
			{
				var item = list.PopTailing();
				Expect(item.ID == i);
				i++;
			}
		}

		[Test]
		public void PopTailing_WithNonArrangedList()
		{
			var list = CreateNonArrangedList(100, 20, 20);
			var i = 20;
			while (list.Count > 0)
			{
				var item = list.PopTailing();
				Expect(item.ID == i);
				i++;
			}
		}

		#endregion

		#region Tools

		private class CircularListTestItem
		{
			public CircularListTestItem()
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

		private static CircularList<CircularListTestItem> CreateList(int itemCount)
		{
			CircularListTestItem.ResetIDAssigner();
			var list = new CircularList<CircularListTestItem>(itemCount);
			for (int i = 0; i < itemCount; i++)
			{
				list.Add(new CircularListTestItem());
			}
			return list;
		}

		private static CircularList<CircularListTestItem> CreateNonArrangedList(int initialCount, int removeTailingCount, int addLaterCount, int secondBatchOfRemoveTailingCount = 0)
		{
			CircularListTestItem.ResetIDAssigner();
			var list = new CircularList<CircularListTestItem>(initialCount);
			for (int i = 0; i < initialCount; i++)
			{
				list.Add(new CircularListTestItem());
			}
			for (int i = 0; i < removeTailingCount; i++)
			{
				list.RemoveTailing();
			}
			for (int i = 0; i < addLaterCount; i++)
			{
				list.Add(new CircularListTestItem());
			}
			for (int i = 0; i < secondBatchOfRemoveTailingCount; i++)
			{
				list.RemoveTailing();
			}
			return list;
		}

		#endregion
	}

}
