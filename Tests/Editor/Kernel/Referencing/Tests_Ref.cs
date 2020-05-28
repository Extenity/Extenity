using Extenity.Kernel;
using NUnit.Framework;

namespace ExtenityTests.Kernel.Tests
{

	public class Tests_Ref
	{
		#region ID

		[Test]
		public void SettingIDInConstructorWorks()
		{
			Assert.AreEqual(0, new Ref(0, true).ID);
			Assert.AreEqual(1, new Ref(1, true).ID);
			Assert.AreEqual(100, new Ref(100, true).ID);
			Assert.AreEqual(200, new Ref(200, true).ID);
			Assert.AreEqual(int.MaxValue, new Ref(int.MaxValue, true).ID);

			Assert.AreEqual(0, new Ref(0, false).ID);
			Assert.AreEqual(1, new Ref(1, false).ID);
			Assert.AreEqual(100, new Ref(100, false).ID);
			Assert.AreEqual(200, new Ref(200, false).ID);
			Assert.AreEqual(int.MaxValue, new Ref(int.MaxValue, false).ID);
		}

		[Test]
		public void GettingIDAlwaysStripsOwnershipInfo()
		{
			Assert.AreEqual(100, new Ref(100, true).ID);
		}

		#endregion

		#region Ownership

		[Test]
		public void SettingOwnershipInConstructorWorks()
		{
			Assert.True(new Ref(100, true).IsOwner);
			Assert.False(new Ref(100, false).IsOwner);
		}

		[Test]
		public void CopyingRefKeepsOwnershipInfo()
		{
			var a = new Ref(100, true);
			var b = new Ref(100, false);
			var copyA = a;
			var copyB = b;
			Assert.True(copyA.IsOwner);
			Assert.False(copyB.IsOwner);
		}

		[Test]
		public void GettingReferenceDoesNotKeepOwnershipInfo()
		{
			Assert.False(new Ref(100, true).Reference.IsOwner);
			Assert.False(new Ref(100, false).Reference.IsOwner);
		}

		#endregion

		#region Equality

		[Test]
		public void EqualityWorksRegardlessOfOwnership()
		{
			Assert.True(new Ref(100, true) == new Ref(100, true));
			Assert.True(new Ref(100, true) == new Ref(100, false));
			Assert.True(new Ref(100, false) == new Ref(100, true));
			Assert.True(new Ref(100, false) == new Ref(100, false));

			// Let's also check inequality, just to be sure.
			Assert.False(new Ref(100, true) == new Ref(22, true));
			Assert.False(new Ref(100, true) == new Ref(22, false));
			Assert.False(new Ref(100, false) == new Ref(22, true));
			Assert.False(new Ref(100, false) == new Ref(22, false));
		}

		#endregion
	}

}
