using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Extenity.DataToolbox;

namespace Extenity.CryptoToolbox
{

	public static class HashTools
	{
		#region Result Caching

		private class CachedResult
		{
			//public string FilePath; No need to keep this information
			public DateTime CreationTimeUtc;
			public DateTime LastWriteTimeUtc;
			public long FileSize;
			public string Hash;

			public CachedResult(FileInfo fileInfo, string hash)
			{
				CreationTimeUtc = fileInfo.CreationTimeUtc;
				LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
				FileSize = fileInfo.Length;
				Hash = hash;
			}

			public void Set(FileInfo fileInfo, string hash)
			{
				CreationTimeUtc = fileInfo.CreationTimeUtc;
				LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
				FileSize = fileInfo.Length;
				Hash = hash;
			}

			public bool CheckEquality(FileInfo fileInfo)
			{
				return
					CreationTimeUtc == fileInfo.CreationTimeUtc &&
					LastWriteTimeUtc == fileInfo.LastWriteTimeUtc &&
					FileSize == fileInfo.Length;
			}
		}

		#endregion

		#region Comparison

		public static bool IsEqual(string hash1, string hash2)
		{
			return string.Equals(hash1, hash2, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		#region MD5

		private static readonly Dictionary<string, CachedResult> CachedResultsMD5 = new Dictionary<string, CachedResult>();

		public static string CalculateFileMD5(this string filePath)
		{
			var fileInfo = new FileInfo(filePath);

			// Use cache if already calculated.
			CachedResult cachedResult;
			if (CachedResultsMD5.TryGetValue(filePath, out cachedResult))
			{
				if (cachedResult.CheckEquality(fileInfo))
					return cachedResult.Hash;
			}

			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var hash = stream.CalculateMD5();
				if (cachedResult != null)
					cachedResult.Set(fileInfo, hash);
				else
					CachedResultsMD5.Add(filePath, new CachedResult(fileInfo, hash));
				return hash;
			}
		}

		public static string CalculateMD5(this Stream stream)
		{
			using (var hashAlgorithm = MD5.Create())
			{
				var checksum = hashAlgorithm.ComputeHash(stream);
				return checksum.ToHexStringCombined(true);
			}
		}

		public static bool CalculateAndCompareFileMD5(this string filePath, string otherHash)
		{
			var calculatedHash = filePath.CalculateFileMD5(); // Case does not matter.
			return IsEqual(calculatedHash, otherHash);
		}

		public static bool CalculateAndCompareMD5(this Stream stream, string otherHash)
		{
			var calculatedHash = stream.CalculateMD5(); // Case does not matter.
			return IsEqual(calculatedHash, otherHash);
		}

		#endregion

		#region SHA1

		private static readonly Dictionary<string, CachedResult> CachedResultsSHA1 = new Dictionary<string, CachedResult>();

		public static string CalculateFileSHA1(this string filePath)
		{
			var fileInfo = new FileInfo(filePath);

			// Use cache if already calculated.
			CachedResult cachedResult;
			if (CachedResultsSHA1.TryGetValue(filePath, out cachedResult))
			{
				if (cachedResult.CheckEquality(fileInfo))
				{
					return cachedResult.Hash;
				}
			}

			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var hash = stream.CalculateSHA1();
				if (cachedResult != null)
					cachedResult.Set(fileInfo, hash);
				else
					CachedResultsSHA1.Add(filePath, new CachedResult(fileInfo, hash));
				return hash;
			}
		}

		public static string CalculateSHA1(this Stream stream)
		{
			using (var hashAlgorithm = new SHA1Managed())
			{
				var checksum = hashAlgorithm.ComputeHash(stream);
				return checksum.ToHexStringCombined(true);
			}
		}

		public static bool CalculateAndCompareFileSHA1(this string filePath, string otherHash)
		{
			var calculatedHash = filePath.CalculateFileSHA1(); // Case does not matter.
			return IsEqual(calculatedHash, otherHash);
		}

		public static bool CalculateAndCompareSHA1(this Stream stream, string otherHash)
		{
			var calculatedHash = stream.CalculateSHA1(); // Case does not matter.
			return IsEqual(calculatedHash, otherHash);
		}

		#endregion

		#region SHA256

		private static readonly Dictionary<string, CachedResult> CachedResultsSHA256 = new Dictionary<string, CachedResult>();

		public static string CalculateFileSHA256(this string filePath)
		{
			var fileInfo = new FileInfo(filePath);

			// Use cache if already calculated.
			CachedResult cachedResult;
			if (CachedResultsSHA256.TryGetValue(filePath, out cachedResult))
			{
				if (cachedResult.CheckEquality(fileInfo))
					return cachedResult.Hash;
			}

			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var hash = stream.CalculateSHA256();
				if (cachedResult != null)
					cachedResult.Set(fileInfo, hash);
				else
					CachedResultsSHA256.Add(filePath, new CachedResult(fileInfo, hash));
				return hash;
			}
		}

		public static string CalculateSHA256(this Stream stream)
		{
			using (var hashAlgorithm = new SHA256Managed())
			{
				var checksum = hashAlgorithm.ComputeHash(stream);
				return checksum.ToHexStringCombined(true);
			}
		}

		public static bool CalculateAndCompareFileSHA256(this string filePath, string otherHash)
		{
			var calculatedHash = filePath.CalculateFileSHA256(); // Case does not matter.
			return IsEqual(calculatedHash, otherHash);
		}

		public static bool CalculateAndCompareSHA256(this Stream stream, string otherHash)
		{
			var calculatedHash = stream.CalculateSHA256(); // Case does not matter.
			return IsEqual(calculatedHash, otherHash);
		}

		#endregion
	}

}
