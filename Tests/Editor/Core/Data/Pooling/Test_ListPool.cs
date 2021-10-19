using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_ListPool : ExtenityTestBase
	{
		protected override void OnInitialize()
		{
			// Release previous pools from previously ran tests if there were any left.
			ListPoolTools.ReleaseAllListsOfAllTypes();
		}

		protected override void OnDeinitialize()
		{
			// Release pools of last ran test.
			ListPoolTools.ReleaseAllListsOfAllTypes();
		}

		[Test]
		public void PreventUnintentionalUsageOfContainerReferenceAfterReleasingItToThePool()
		{
			var container = New.List<string>();
			Release.List(ref container);
			Assert.IsNull(container);
		}

		[Test]
		public void UseTheSameContainerForConsecutiveInstantiations()
		{
			Assert.AreEqual(ListPool<string>.Pool.Count, 0);

			Release.ListUnsafe(New.List<string>());
			Assert.AreEqual(ListPool<string>.Pool.Count, 1);
			Release.ListUnsafe(New.List<string>());
			Assert.AreEqual(ListPool<string>.Pool.Count, 1);
			Release.ListUnsafe(New.List<string>());
			Assert.AreEqual(ListPool<string>.Pool.Count, 1);
		}

		[Test]
		public void TwoContainersInPool()
		{
			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
			var int1 = New.List<int>();
			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
			var int2 = New.List<int>();
			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
			Release.List(ref int1);
			Assert.AreEqual(ListPool<int>.Pool.Count, 1);
			Release.List(ref int2);
			Assert.AreEqual(ListPool<int>.Pool.Count, 2);
			New.List<int>();
			Assert.AreEqual(ListPool<int>.Pool.Count, 1);
			New.List<int>();
			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
			New.List<int>();
			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
		}

		[Test]
		public void AcceptAnyListThatWasNotInitializedWithPoolingSystem()
		{
			var int1 = new List<int>();
			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
			Release.List(ref int1);
			Assert.AreEqual(ListPool<int>.Pool.Count, 1);
		}

		[Test]
		public void FilledContainerIsClearedUponEnteringIntoThePool()
		{
			var container = new List<int>();
			container.Capacity = 6;
			container.Add(3);
			container.Add(5);
			var instanceForComparison = container;
			Assert.AreEqual(container.Count, 2);
			Release.List(ref container);
			// Ensure the pooled list is the list that we've just created above.
			Assert.AreEqual(ListPool<int>.Pool.Count, 1);
			Assert.AreEqual(ListPool<int>.Pool[0], instanceForComparison);
			Assert.AreEqual(ListPool<int>.Pool[0].Count, 0); // Check if it was really cleared upon entering the pool.
			Assert.AreEqual(ListPool<int>.Pool[0].Capacity, 6);
		}

		[Test]
		public void ReleaseAllTypesOfPools()
		{
			var int1 = New.List<int>();
			var int2 = New.List<int>();
			Release.List(ref int1);
			Release.List(ref int2);
			var string1 = New.List<string>();
			var string2 = New.List<string>();
			var string3 = New.List<string>();
			Release.List(ref string1);
			Release.List(ref string2);
			Release.List(ref string3);
			var float1 = New.List<float>();
			Release.List(ref float1);

			Assert.AreEqual(ListPool<int>.Pool.Count, 2);
			Assert.AreEqual(ListPool<string>.Pool.Count, 3);
			Assert.AreEqual(ListPool<float>.Pool.Count, 1);

			ListPoolTools.ReleaseAllListsOfAllTypes();

			Assert.AreEqual(ListPool<int>.Pool.Count, 0);
			Assert.AreEqual(ListPool<string>.Pool.Count, 0);
			Assert.AreEqual(ListPool<float>.Pool.Count, 0);
		}
	}

}
