#if ExtenityKernel

using Extenity.KernelToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.KernelToolbox.Tests
{

	public class Tests_Refs : ExtenityTestBase
	{
		#region Setup

		class TestKernel : KernelBase<TestKernel>
		{
		}

		class TestObject : KernelObject<TestKernel>
		{
		}

		private TestKernel Kernel;

		protected override void OnInitialize()
		{
			Kernel = new TestKernel();
			Kernel.Activate();
		}

		protected override void OnDeinitialize()
		{
			Kernel.Deactivate();
			Kernel = null;
		}

		#endregion

		#region Initialization

		[Test]
		public void CreatingRefs()
		{
			Assert.AreEqual((uint)0, (uint)new Ref<TestObject, TestKernel>(0));
			Assert.AreEqual((uint)1, (uint)new Ref<TestObject, TestKernel>(1));
			Assert.AreEqual((uint)100, (uint)new Ref<TestObject, TestKernel>(100));
			Assert.AreEqual((uint)200, (uint)new Ref<TestObject, TestKernel>(200));
			Assert.AreEqual(uint.MaxValue, (uint)new Ref<TestObject, TestKernel>(uint.MaxValue));
		}

		[Test]
		public void InitializesAsNotSetWithDefaultID()
		{
			Assert.True(new Ref<TestObject, TestKernel>(0).IsNotSet);
			Assert.False(new Ref<TestObject, TestKernel>(0).IsSet);
		}

		[Test]
		public void InitializesAsSetWithValidID()
		{
			Assert.True(new Ref<TestObject, TestKernel>(100).IsSet);
			Assert.False(new Ref<TestObject, TestKernel>(100).IsNotSet);
		}

		#endregion

		#region Referenced Object

		[Test]
		public void GetReferenceToObject()
		{
			var testObject = Kernel.Instantiate<TestObject>();

			// Various ways to get a Ref to a KernelObject
			// var testObjectRef1 = testObject.Ref; // TODO: Hopefully one day we might achieve this. See 112712367.
			var testObjectRef2 = (Ref<TestObject, TestKernel>)testObject;
			var testObjectRef3 = new Ref<TestObject, TestKernel>(testObject.ID);

			// Assert.AreEqual(testObject.ID, testObjectRef1.ReferencedID);
			Assert.AreEqual(testObject.ID, testObjectRef2.ReferencedID);
			Assert.AreEqual(testObject.ID, testObjectRef3.ReferencedID);
			// Assert.AreEqual(typeof(Ref<TestObject, TestKernel>), testObjectRef1.GetType());
			Assert.AreEqual(typeof(Ref<TestObject, TestKernel>), testObjectRef2.GetType());
			Assert.AreEqual(typeof(Ref<TestObject, TestKernel>), testObjectRef3.GetType());
		}

		[Test]
		public void GetReferencedObject()
		{
			var testObject = Kernel.Instantiate<TestObject>();
			var testObjectRef = (Ref<TestObject, TestKernel>)testObject;

			// Get the referenced KernelObject
			var resultingTestObject = testObjectRef.Data;

			Assert.AreEqual(testObject.GetType(), resultingTestObject.GetType());
			Assert.AreEqual(testObject, resultingTestObject);
		}

		#endregion

		#region Equality

		[Test]
		public void RefEquality()
		{
			Assert.True(new Ref<TestObject, TestKernel>(100).Equals(new Ref<TestObject, TestKernel>(100)));
			Assert.True(new Ref<TestObject, TestKernel>(100).Equals((uint)100));
			Assert.True(new Ref<TestObject, TestKernel>(100) == new Ref<TestObject, TestKernel>(100));
			Assert.False(new Ref<TestObject, TestKernel>(100) == new Ref<TestObject, TestKernel>(2222222));

			Assert.False(new Ref<TestObject, TestKernel>(100).Equals(new Ref<TestObject, TestKernel>(2222222)));
			Assert.False(new Ref<TestObject, TestKernel>(100).Equals((uint)2222222));
			Assert.True(new Ref<TestObject, TestKernel>(100) != new Ref<TestObject, TestKernel>(2222222));
			Assert.False(new Ref<TestObject, TestKernel>(100) != new Ref<TestObject, TestKernel>(100));
		}

		#endregion

		#region Comparison

		[Test]
		public void RefComparisonActsAsIntegerComparison()
		{
			Assert.AreEqual(0, new Ref<TestObject, TestKernel>(100).CompareTo(new Ref<TestObject, TestKernel>(100)));

			Assert.AreEqual(1, new Ref<TestObject, TestKernel>(103).CompareTo(new Ref<TestObject, TestKernel>(100)));
			Assert.AreEqual(1, 103.CompareTo(100));

			Assert.AreEqual(-1, new Ref<TestObject, TestKernel>(100).CompareTo(new Ref<TestObject, TestKernel>(103)));
			Assert.AreEqual(-1, 100.CompareTo(103));
		}

		#endregion
	}

}

#endif
