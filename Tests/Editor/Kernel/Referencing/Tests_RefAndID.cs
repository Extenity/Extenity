using System;
using Extenity.KernelToolbox;
using NUnit.Framework;

// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable HeapView.ClosureAllocation

namespace ExtenityTests.KernelToolbox.Tests
{

	public class Tests_RefAndID
	{
		#region ID

		[Test]
		public void CreatingRefsAndIDs()
		{
			throw new NotImplementedException();
			// Assert.AreEqual(0, (int)new Ref(0));
			// Assert.AreEqual(1, (int)new Ref(1));
			// Assert.AreEqual(100, (int)new Ref(100));
			// Assert.AreEqual(200, (int)new Ref(200));
			// Assert.AreEqual(int.MaxValue, (int)new Ref(int.MaxValue));
			//
			// Assert.AreEqual(0, (int)new ID(0));
			// Assert.AreEqual(1, (int)new ID(1));
			// Assert.AreEqual(100, (int)new ID(100));
			// Assert.AreEqual(200, (int)new ID(200));
			// Assert.AreEqual(int.MaxValue, (int)new ID(int.MaxValue));
		}

		#endregion

		#region Equality

		[Test]
		public void EqualityWorksBetweenRefsAndIDs()
		{
			throw new NotImplementedException();
			// Assert.True(new ID(100) == new ID(100));
			// Assert.True(new ID(100) == new Ref(100));
			// Assert.True(new Ref(100) == new ID(100));
			// Assert.True(new Ref(100) == new Ref(100));
			//
			// // Let's also check inequality, just to be sure.
			// Assert.False(new ID(100) == new ID(22));
			// Assert.False(new ID(100) == new Ref(22));
			// Assert.False(new Ref(100) == new ID(22));
			// Assert.False(new Ref(100) == new Ref(22));
		}

		#endregion
	}

}
