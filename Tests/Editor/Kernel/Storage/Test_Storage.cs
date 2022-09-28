#if ExtenityKernel

using System;
using System.Text;
using Extenity.KernelToolbox;
using NUnit.Framework;

// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable HeapView.ClosureAllocation

namespace ExtenityTests.KernelToolbox
{

	public class Test_Storage : TestBase_Storage
	{
		#region Test Setup

		[SetUp]
		public void SetupBeforeEachTest()
		{
			InitializeStorage();
		}

		[TearDown]
		public void TearDownAfterEachTest()
		{
			DeinitializeStorage();
		}

		#endregion

		#region Empty Data

		[Test]
		public void ShouldReturnEmptyObjectIDWhenSavingEmptyData()
		{
			{
				var id = Storage.Save(Array.Empty<byte>());
				Assert.AreEqual(OID.Empty, id);
			}
			{
				var id = Storage.Save(null);
				Assert.AreEqual(OID.Empty, id);
			}
		}

		[Test]
		public void ShouldReturnEmptyBytesWhenLoadingEmptyData()
		{
			var bytes = Storage.Load(OID.Empty);
			Assert.AreEqual(Array.Empty<byte>(), bytes);
		}

		#endregion

		#region Save and Load - Simple

		[Test]
		public void SaveAndLoadSampleData()
		{
			var bytes = Encoding.UTF8.GetBytes("ASD 123");
			var id = Storage.Save(bytes);
			var loadedBytes = Storage.Load(id);
			Assert.AreEqual(bytes, loadedBytes);
		}

		#endregion

		#region Save and Load - Multiple

		[Test]
		public void SaveAndLoadMultipleSampleData()
		{
			// Save all data first.
			var bytes1 = Encoding.UTF8.GetBytes("ASD 123");
			var bytes2 = Encoding.UTF8.GetBytes("ASD 123 ZXC 456");
			var bytes3 = Encoding.UTF8.GetBytes("Some content here");
			var bytes4 = Encoding.UTF8.GetBytes("Some other content here\nWhich is multiline");
			var id1 = Storage.Save(bytes1);
			var id2 = Storage.Save(bytes2);
			var id3 = Storage.Save(bytes3);
			var id4 = Storage.Save(bytes4);

			// Load data
			{
				var loadedBytes1 = Storage.Load(id1);
				var loadedBytes2 = Storage.Load(id2);
				var loadedBytes3 = Storage.Load(id3);
				var loadedBytes4 = Storage.Load(id4);
				Assert.AreEqual(bytes1, loadedBytes1);
				Assert.AreEqual(bytes2, loadedBytes2);
				Assert.AreEqual(bytes3, loadedBytes3);
				Assert.AreEqual(bytes4, loadedBytes4);
			}

			// Delete some and load again
			DeleteDataFileOfData(bytes2);
			DeleteDataFileOfData(bytes4);
			{
				var loadedBytes1 = Storage.Load(id1);
				Assert.Throws<DataNotFoundException>(() => Storage.Load(id2));
				var loadedBytes3 = Storage.Load(id3);
				Assert.Throws<DataNotFoundException>(() => Storage.Load(id4));
				Assert.AreEqual(bytes1, loadedBytes1);
				// Assert.AreEqual(bytes2, loadedBytes2);
				Assert.AreEqual(bytes3, loadedBytes3);
				// Assert.AreEqual(bytes4, loadedBytes4);
			}
		}

		#endregion

		#region Save and Load - Missing and Corrupt Data

		[Test]
		public void ShouldFailWhenLoadingMissingData()
		{
			Assert.Throws<DataNotFoundException>(() => Storage.Load(new OID("123ABC456DEF")));
		}

		[Test]
		public void ShouldFailWhenLoadingCorruptData()
		{
			var bytes = Encoding.UTF8.GetBytes("ASD 123");
			var id = Storage.Save(bytes);
			DamageDataFileOfData("ASD 123");
			Assert.Throws<CorruptDataException>(() => Storage.Load(id));
		}

		#endregion
	}

}

#endif
