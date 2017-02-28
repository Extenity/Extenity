using System;
using System.IO;
using System.Security.Cryptography;
using Extenity.DataTypes;

namespace Extenity.Crypto
{

	public static class HashTools
	{
		#region Comparison

		public static bool IsEqual(string hash1, string hash2)
		{
			return string.Equals(hash1, hash2, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		#region MD5

		public static string CalculateFileMD5(this string filePath, bool uppercase = true)
		{
			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return stream.CalculateMD5(uppercase);
			}
		}

		public static string CalculateMD5(this Stream stream, bool uppercase = true)
		{
			using (var hashAlgorithm = MD5.Create())
			{
				var checksum = hashAlgorithm.ComputeHash(stream);
				return checksum.ToHexStringCombined(uppercase);
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

		public static string CalculateFileSHA1(this string filePath, bool uppercase = true)
		{
			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return stream.CalculateSHA1(uppercase);
			}
		}

		public static string CalculateSHA1(this Stream stream, bool uppercase = true)
		{
			using (var hashAlgorithm = new SHA1Managed())
			{
				var checksum = hashAlgorithm.ComputeHash(stream);
				return checksum.ToHexStringCombined(uppercase);
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

		public static string CalculateFileSHA256(this string filePath, bool uppercase = true)
		{
			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return stream.CalculateSHA256(uppercase);
			}
		}

		public static string CalculateSHA256(this Stream stream, bool uppercase = true)
		{
			using (var hashAlgorithm = new SHA256Managed())
			{
				var checksum = hashAlgorithm.ComputeHash(stream);
				return checksum.ToHexStringCombined(uppercase);
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
