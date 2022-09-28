#if ExtenityKernel

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Extenity.FileSystemToolbox;

namespace Extenity.KernelToolbox
{

	/// <summary>
	/// Content-Addressable Storage (CAS) that stores data blobs by their hashes. Provide a data blob as byte array to
	/// Storage.Save method to save this blob into a file, which also calculates and returns its hash to caller. Then
	/// the blob can be accessible by calling Storage.Load and providing hash of the blob.
	/// </summary>
	public class Storage
	{
		#region Initialization

		public bool IsInitialized { get; private set; }

		public void Initialize(string persistentPath,
		                       string storageDirectoryName = "Storage",
		                       int maxAllowedInProgressFileDuration = 24 * 3) // 3 days
		{
			if (IsInitialized)
				throw new Exception($"{nameof(Storage)} was already initialized.");

			StoragePath = DirectoryTools.CreateSubDirectory(persistentPath, storageDirectoryName);
			StorageInProgressPath = DirectoryTools.CreateSubDirectory(StoragePath, ".inprogress");

			RemoveOldFiles_NOT_IMPLEMENTED_YET(StorageInProgressPath, maxAllowedInProgressFileDuration);

			IsInitialized = true;
		}

		public void EnsureInitialized()
		{
			if (!IsInitialized)
			{
				throw new Exception($"{nameof(Storage)} was not initialized yet.");
			}
		}

		#endregion

		#region Deinitialization

		public void Deinitialize()
		{
			StoragePath = null;
			StorageInProgressPath = null;

			IsInitialized = false;
		}

		#endregion

		#region Paths

		public string StoragePath { get; private set; }
		public string StorageInProgressPath { get; private set; }

		private string GetObjectPath(string basePath, OID id)
		{
			return Path.Combine(basePath, id.ToPathString());
		}

		private string GetUniqueObjectPath(string basePath, OID id)
		{
			var path = GetObjectPath(basePath, id);
			var counter = 1;
			while (File.Exists(path))
			{
				counter++;
				path = GetObjectPath(basePath, id) + $"-{counter}";

				if (counter > 50000) // Hard brakes.
				{
					throw new Exception("Reached unique path generation limitation.");
				}
			}
			return path;
		}

		#endregion

		#region Hash

		private readonly StringBuilder StringBuilder = new StringBuilder(80); // TODO OPTIMIZATION: Adjust the capacity correctly.

		public string CalculateHash(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				throw new ArgumentNullException(nameof(bytes));

			string hash;
			using (var sha256Hash = SHA256.Create()) // TODO OPTIMIZATION: See if this can be reusable.
			{
				var hashBytes = sha256Hash.ComputeHash(bytes);
				StringBuilder.Clear();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					StringBuilder.Append(hashBytes[i].ToString("X2")); // TODO OPTIMIZATION: Prevent memory allocation. See StringTools.ToStringClampedBetween0To99 and implement a similar tool for byte to hex conversion.
				}
				hash = StringBuilder.ToString();
			}

			if (string.IsNullOrWhiteSpace(hash))
			{
				throw new Exception("Failed to create hash.");
			}

			return hash;
		}

		#endregion

		#region Cleanup

		// TODO OPTIMIZE: Check and remove files in a thread.
		public void RemoveOldFiles_NOT_IMPLEMENTED_YET(string directoryPath, int allowedHours)
		{
			// TODO: Implement me some time in not so distant future!
		}

		#endregion

		#region Save / Load

		// TODO OPTIMIZATION: Read file in a thread. Make it async and allow multiple files to be read simultaneously, which especially boosts Windows file read performance.
		// TODO OPTIMIZATION: Find a way to calculate the hash while loading the file.
		// TODO OPTIMIZATION: Measure allocations and find ways to reduce them. Use reusable memory streams, etc.

		/// <summary>
		/// Loads the data from repository via its OID. Ensures its content hash matches the OID hash to
		/// prevent loading corrupt files.
		/// </summary>
		public byte[] Load(OID id)
		{
			EnsureInitialized();

			if (id.IsEmpty)
			{
				return OID.EmptyData;
			}

			byte[] bytes;
			try
			{
				bytes = File.ReadAllBytes(GetObjectPath(StoragePath, id));
			}
			catch (FileNotFoundException)
			{
				throw new DataNotFoundException(id);
			}

			var hash = CalculateHash(bytes);

			if (!id.Hash.Equals(hash, StringComparison.Ordinal))
			{
				throw new CorruptDataException(id);
			}

			return bytes;
		}

		/// <summary>
		/// Saves the data into repository. Then that data can be loaded via its OID at any point in time. Also
		/// ensures the data is safely saved by checking up in many ways.
		/// </summary>
		public OID Save(byte[] bytes)
		{
			EnsureInitialized();

			if (bytes == null || bytes.Length == 0)
				return OID.Empty;

			var hash = CalculateHash(bytes);
			var id = new OID(hash);
			var path = GetObjectPath(StoragePath, id);

			// See if there is an already existing file at destination path.
			if (File.Exists(path))
			{
				// Wow, there actually is a file with our hash. See if the content of the file is the same. We expect
				// it to be the same. Otherwise, we assume the file is corrupt and overwrite it with the new content.
				var existingBytes = File.ReadAllBytes(path);

				if (bytes.SequenceEqual(existingBytes)) // TODO OPTIMIZATION: See if there is a faster way to compare bytes
				{
					// It's all good. The file was already there with the same content. So no need to write the data.
					return id;
				}
				else
				{
					// It's not looking good. There is a file with the same name but different content. It's either
					// the hash function failed so bad that a collision happened, or more likely the file got corrupt
					// for some reason. So overwrite the file with new content and lets hope everything works out okay.

					Log.CriticalError($"Possible hash collision or file corruption detected for hash {hash} with files of sizes {existingBytes.Length} and {bytes.Length}.");
				}
			}

			// Find an in-progress path that is not in use.
			var inProgressPath = GetUniqueObjectPath(StorageInProgressPath, id);

			// Save into 'inprogress' folder first. Then move it to 'cache' folder. That way, we prevent leaving corrupt
			// files around that are cut in the middle of saving progress.
			File.WriteAllBytes(inProgressPath, bytes);

			// Try to load the file that is just being written.
			var writtenBytes = File.ReadAllBytes(inProgressPath);

			// Compare if if was successfully written.
			if (!bytes.SequenceEqual(writtenBytes)) // TODO OPTIMIZATION: See if there is a quicker way to compare bytes
			{
				// TODO: Detailed logging.
				throw new Exception("File save crosscheck failed.");
			}

			// Move the file to its destination.
			File.Move(inProgressPath, path);

			return id;
		}

		#endregion
	}

}

#endif
