using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_DecayedCircularList : ExtenityTestBase
	{
		#region Remove Tailing on edge conditions

		[Test]
		public void RemoveTailingIfDecayed_IgnoresEmptyList()
		{
			var list = CreateList(0);
			list.DecayCondition = item => item.ID == -1000;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 0);
		}

		[Test]
		public void RemoveAllTailingIfDecayed_IgnoresEmptyList()
		{
			var list = CreateList(0);
			list.DecayCondition = item => item.ID == -1000;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 0);
		}

		[Test]
		public void RemoveTailingIfDecayed_ThrowsIfDecayConditionIsNotSet()
		{
			var list = CreateList(0);
			Assert.Throws(typeof(Exception), () => { list.RemoveTailingIfDecayed(); });
		}

		[Test]
		public void RemoveAllTailingIfDecayed_ThrowsIfDecayConditionIsNotSet()
		{
			var list = CreateList(0);
			Assert.Throws(typeof(Exception), () => { list.RemoveAllTailingIfDecayed(); });
		}

		[Test]
		public void RemoveTailingIfDecayed_OnlyChecksForTailingItem()
		{
			var list = CreateList(10);
			list.DecayCondition = item => item.ID == 1;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 9);
		}

		[Test]
		public void RemoveAllTailingIfDecayed_OnlyChecksForTailingItem()
		{
			var list = CreateList(10);
			list.DecayCondition = item => item.ID == 1;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 9);
		}

		#endregion

		#region Remove Tailing on arranged and non-arranged lists

		[Test]
		public void RemoveTailingIfDecayed_ArrangedList()
		{
			var list = CreateList(10);

			list.DecayCondition = item => item.ID == -1000;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 9);

			list.DecayCondition = item => item.ID == 0;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 9);
			Expect(list.TailingItem.ID == 1);
			Expect(list.HeadingItem.ID == 9);

			list.RemoveTailingIfDecayed();
			Expect(list.Count == 9);
			Expect(list.TailingItem.ID == 1);
			Expect(list.HeadingItem.ID == 9);

			list.DecayCondition = item => item.ID <= 2;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 8);
			Expect(list.TailingItem.ID == 2);
			Expect(list.HeadingItem.ID == 9);

			list.RemoveTailingIfDecayed();
			Expect(list.Count == 7);
			Expect(list.TailingItem.ID == 3);
			Expect(list.HeadingItem.ID == 9);

			list.RemoveTailingIfDecayed();
			Expect(list.Count == 7);
			Expect(list.TailingItem.ID == 3);
			Expect(list.HeadingItem.ID == 9);
		}

		[Test]
		public void RemoveTailingIfDecayed_NonArrangedList()
		{
			var list = CreateNonArrangedList(10, 3, 3);

			list.DecayCondition = item => item.ID == -1000;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 3);
			Expect(list.HeadingItem.ID == 12);

			list.DecayCondition = item => item.ID == 0;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 3);
			Expect(list.HeadingItem.ID == 12);

			list.DecayCondition = item => item.ID == 2;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 3);
			Expect(list.HeadingItem.ID == 12);

			list.DecayCondition = item => item.ID == 3;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 9);
			Expect(list.TailingItem.ID == 4);
			Expect(list.HeadingItem.ID == 12);

			list.DecayCondition = item => item.ID <= 5;
			list.RemoveTailingIfDecayed();
			Expect(list.Count == 8);
			Expect(list.TailingItem.ID == 5);
			Expect(list.HeadingItem.ID == 12);

			list.RemoveTailingIfDecayed();
			Expect(list.Count == 7);
			Expect(list.TailingItem.ID == 6);
			Expect(list.HeadingItem.ID == 12);

			list.RemoveTailingIfDecayed();
			Expect(list.Count == 7);
			Expect(list.TailingItem.ID == 6);
			Expect(list.HeadingItem.ID == 12);
		}

		#endregion

		#region Pop Tailing on arranged and non-arranged lists

		[Test]
		public void PopTailingIfDecayed_ArrangedList()
		{
			var list = CreateList(10);
			list.DecayCondition = item => item.ID <= 3;
			int i = 0;
			DecayedCircularListTestItem decayed;
			do
			{
				decayed = list.PopTailingIfDecayed();
				if (decayed != null)
					Expect(decayed.ID == i++);
			}
			while (decayed != null);
			Expect(i == 4);
		}

		[Test]
		public void PopTailingIfDecayed_NonArrangedList()
		{
			var list = CreateNonArrangedList(10, 3, 3);
			list.DecayCondition = item => item.ID <= 6;
			int i = 3;
			DecayedCircularListTestItem decayed;
			do
			{
				decayed = list.PopTailingIfDecayed();
				if (decayed != null)
					Expect(decayed.ID == i++);
			}
			while (decayed != null);
			Expect(i == 7);
		}

		#endregion

		#region Remove All Tailing on arranged and non-arranged lists

		[Test]
		public void RemoveAllTailingIfDecayed_ArrangedList()
		{
			var list = CreateList(10);

			list.DecayCondition = item => item.ID == -1000;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 0);
			Expect(list.HeadingItem.ID == 9);

			list.DecayCondition = item => item.ID <= 0;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 9);
			Expect(list.TailingItem.ID == 1);
			Expect(list.HeadingItem.ID == 9);

			list.DecayCondition = item => item.ID <= 3;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 6);
			Expect(list.TailingItem.ID == 4);
			Expect(list.HeadingItem.ID == 9);
		}

		[Test]
		public void RemoveAllTailingIfDecayed_NonArrangedList()
		{
			var list = CreateNonArrangedList(10, 3, 3);

			list.DecayCondition = item => item.ID == -1000;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 10);
			Expect(list.TailingItem.ID == 3);
			Expect(list.HeadingItem.ID == 12);

			list.DecayCondition = item => item.ID <= 3;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 9);
			Expect(list.TailingItem.ID == 4);
			Expect(list.HeadingItem.ID == 12);

			list.DecayCondition = item => item.ID <= 6;
			list.RemoveAllTailingIfDecayed();
			Expect(list.Count == 6);
			Expect(list.TailingItem.ID == 7);
			Expect(list.HeadingItem.ID == 12);
		}

		#endregion

		#region Pop All Tailing on arranged and non-arranged lists

		[Test]
		public void PopAllTailingIfDecayed_ArrangedList()
		{
			var list = CreateList(10);
			list.DecayCondition = item => item.ID <= 3;
			using var _ = New.List<DecayedCircularListTestItem>(out var decayedItems);
			list.PopAllTailingIfDecayed(decayedItems);
			int i = 0;
			foreach (var decayedItem in decayedItems)
			{
				Expect(decayedItem.ID == i++);
			}
			Expect(i == 4);
		}

		[Test]
		public void PopAllTailingIfDecayed_NonArrangedList()
		{
			var list = CreateNonArrangedList(10, 3, 3);
			list.DecayCondition = item => item.ID <= 6;
			using var _ = New.List<DecayedCircularListTestItem>(out var decayedItems);
			list.PopAllTailingIfDecayed(decayedItems);
			int i = 3;
			foreach (var decayedItem in decayedItems)
			{
				Expect(decayedItem.ID == i++);
			}
			Expect(i == 7);
		}

		#endregion

		#region Tools

		private class DecayedCircularListTestItem
		{
			public DecayedCircularListTestItem()
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

		private static DecayedCircularList<DecayedCircularListTestItem> CreateList(int itemCount)
		{
			DecayedCircularListTestItem.ResetIDAssigner();
			var list = new DecayedCircularList<DecayedCircularListTestItem>(itemCount);
			for (int i = 0; i < itemCount; i++)
			{
				list.Add(new DecayedCircularListTestItem());
			}
			return list;
		}

		private static DecayedCircularList<DecayedCircularListTestItem> CreateNonArrangedList(int initialCount, int removeTailingCount, int addLaterCount)
		{
			DecayedCircularListTestItem.ResetIDAssigner();
			var list = new DecayedCircularList<DecayedCircularListTestItem>(initialCount);
			for (int i = 0; i < initialCount; i++)
			{
				list.Add(new DecayedCircularListTestItem());
			}
			for (int i = 0; i < removeTailingCount; i++)
			{
				list.RemoveTailing();
			}
			for (int i = 0; i < addLaterCount; i++)
			{
				list.Add(new DecayedCircularListTestItem());
			}
			return list;
		}

		#endregion
	}

}
