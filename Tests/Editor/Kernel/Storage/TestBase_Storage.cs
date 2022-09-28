#if ExtenityKernel

using System.IO;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.FileSystemToolbox;
using Extenity.KernelToolbox;
using UnityEngine;

// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable HeapView.ClosureAllocation

namespace ExtenityTests.KernelToolbox
{

	public abstract class TestBase_Storage
	{
		#region Test Setup

		protected Storage Storage;

		protected void InitializeStorage()
		{
			var path = Path.Combine(ApplicationTools.UnityProjectPaths.LibraryRelativePath, "StorageForTests");
			// Log.Info("Initializing persistent path to: " + path);

			// Clear the previously created repository.
			try
			{
				DirectoryTools.DeleteWithContent(path);
				Directory.CreateDirectory(path);
			}
			catch
			{
				// ignored
			}

			Storage = new Storage();
			Storage.Initialize(path);
		}

		protected void DeinitializeStorage()
		{
			Storage.Deinitialize();
			Storage = null;
		}

		#endregion

		#region Corrupt Data

		protected void DamageDataFileOfObject(OID oid)
		{
			DamageDataFile(oid.ToPathString());
		}

		protected void DamageDataFileOfData(string data)
		{
			var bytes = Encoding.UTF8.GetBytes(data);
			DamageDataFileOfData(bytes);
		}

		protected void DamageDataFileOfData(byte[] data)
		{
			var hash = Storage.CalculateHash(data);
			DamageDataFile(hash);
		}

		protected void DamageDataFile(string fileName)
		{
			var path = Path.Combine(Storage.StoragePath, fileName);
			File.WriteAllText(path, "DAMAGED FILE FOR TESTING PURPOSES - " + Random.Range(1000000, 9999999));
		}

		#endregion

		#region Delete Data

		protected void DeleteDataFileOfObject(OID oid)
		{
			DeleteDataFile(oid.ToPathString());
		}

		protected void DeleteDataFileOfData(string data)
		{
			var bytes = Encoding.UTF8.GetBytes(data);
			DeleteDataFileOfData(bytes);
		}

		protected void DeleteDataFileOfData(byte[] data)
		{
			var hash = Storage.CalculateHash(data);
			DeleteDataFile(hash);
		}

		protected void DeleteDataFile(string fileName)
		{
			var path = Path.Combine(Storage.StoragePath, fileName);
			FileTools.Delete(path);
		}

		#endregion
	}

}

#endif
