using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Extenity.CompressionToolbox;

namespace Extenity.CryptoToolbox
{

	/// <summary>
	/// https://stackoverflow.com/questions/165808/simple-insecure-two-way-obfuscation-for-c-sharp
	/// Based on https://msdn.microsoft.com/en-us/library/system.security.cryptography.rijndaelmanaged(v=vs.110).aspx
	/// Uses UTF8 Encoding
	///  http://security.stackexchange.com/a/90850
	/// </summary>
	public class SimpleTwoWayEncryptorAES : IDisposable
	{
		private RijndaelManaged Rijn;

		/// <summary>
		/// Initialize the algorithm with key, block size, key size, padding mode and cipher mode to be known.
		/// </summary>
		/// <param name="key">ASCII key to be used for encryption or decryption</param>
		/// <param name="blockSize">block size to use for AES algorithm. 128, 192 or 256 bits</param>
		/// <param name="keySize">key length to use for AES algorithm. 128, 192, or 256 bits</param>
		/// <param name="paddingMode"></param>
		/// <param name="cipherMode"></param>
		public SimpleTwoWayEncryptorAES(string key, int blockSize, int keySize, PaddingMode paddingMode, CipherMode cipherMode)
		{
			Rijn = new RijndaelManaged();
			Rijn.Key = Encoding.UTF8.GetBytes(key);
			Rijn.BlockSize = blockSize;
			Rijn.KeySize = keySize;
			Rijn.Padding = paddingMode;
			Rijn.Mode = cipherMode;
		}

		/// <summary>
		/// Initialize the algorithm just with key.
		/// Defaults for RijndaelManaged class: 
		///   Block Size: 256 bits (32 bytes)
		///   Key Size: 128 bits (16 bytes)
		///   Padding Mode: PKCS7
		///   Cipher Mode: CBC
		/// </summary>
		/// <param name="key">ASCII key to be used for encryption or decryption</param>
		public SimpleTwoWayEncryptorAES(string key)
		{
			Rijn = new RijndaelManaged();
			Rijn.Key = Encoding.UTF8.GetBytes(key);
		}

		public void Dispose()
		{
			//if (rijn != null)
			//	rijn.Dispose();
			Rijn = null;
		}

		/// <summary>
		/// Based on https://msdn.microsoft.com/en-us/library/system.security.cryptography.rijndaelmanaged(v=vs.110).aspx
		/// Encrypt a string using RijndaelManaged encryptor.
		/// </summary>
		/// <param name="plainText">string to be encrypted</param>
		/// <param name="IV">initialization vector to be used by crypto algorithm</param>
		public byte[] Encrypt(string plainText, byte[] IV)
		{
			if (Rijn == null)
				throw new Exception("Provider is not initialized");
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException(nameof(plainText));
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException(nameof(IV));
			byte[] encrypted;

			using (var encryptor = Rijn.CreateEncryptor(Rijn.Key, IV))
			{
				using (var memoryStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						using (var streamWriter = new StreamWriter(cryptoStream))
						{
							streamWriter.Write(plainText);
						}
						encrypted = memoryStream.ToArray();
					}
				}
			}

			return encrypted;
		}

		/// <summary>
		/// Hex can be used in URLs easily. But the data size is significantly bigger than the Base64 solution.
		/// </summary>
		public static string EncryptHexWithIV(string plainText, string key, CompressionMethod compressionMethod = CompressionMethod.NoCompression, int compressionLevel = -1)
		{
			var ivString = GetUniqueString(16);
			using (var aes = new SimpleTwoWayEncryptorAES(key))
			{
				var iv = Encoding.UTF8.GetBytes(ivString);

				// get encrypted bytes (IV bytes prepended to cipher bytes)
				var encryptedBytes = aes.Encrypt(plainText, iv);
				var encryptedBytesWithIV = iv.Concat(encryptedBytes).ToArray();

				encryptedBytesWithIV = encryptedBytesWithIV.CompressBytes(compressionMethod, compressionLevel);

				// get hex string to send with url
				// this hex has both IV and ciphertext
				return ByteArrayToString(encryptedBytesWithIV);
			}
		}

		public static string EncryptBase64WithIV(string plainText, string key, CompressionMethod compressionMethod = CompressionMethod.NoCompression, int compressionLevel = -1)
		{
			var ivString = GetUniqueString(16);
			using (var aes = new SimpleTwoWayEncryptorAES(key))
			{
				var iv = Encoding.UTF8.GetBytes(ivString);

				// get encrypted bytes (IV bytes prepended to cipher bytes)
				var encryptedBytes = aes.Encrypt(plainText, iv);
				var encryptedBytesWithIV = iv.Concat(encryptedBytes).ToArray();

				encryptedBytesWithIV = encryptedBytesWithIV.CompressBytes(compressionMethod, compressionLevel);

				// get base64 string, which has both IV and ciphertext
				return Convert.ToBase64String(encryptedBytesWithIV);
			}
		}

		/// <summary>
		/// Based on https://msdn.microsoft.com/en-us/library/system.security.cryptography.rijndaelmanaged(v=vs.110).aspx
		/// </summary>
		/// <param name="cipherText">bytes to be decrypted back to plaintext</param>
		/// <param name="IV">initialization vector used to encrypt the bytes</param>
		public string Decrypt(byte[] cipherText, byte[] IV)
		{
			if (Rijn == null)
				throw new Exception("Provider is not initialized");
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException(nameof(cipherText));
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException(nameof(IV));

			string plainText;

			using (var decryptor = Rijn.CreateDecryptor(Rijn.Key, IV))
			{
				using (var memoryStream = new MemoryStream(cipherText))
				{
					using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
					{
						using (var streamReader = new StreamReader(cryptoStream))
						{
							plainText = streamReader.ReadToEnd();
						}
					}
				}
			}

			return plainText;
		}

		public static string DecryptHexWithIV(string hex, string key, CompressionMethod compressionMethod = CompressionMethod.NoCompression)
		{
			using (var aes = new SimpleTwoWayEncryptorAES(key))
			{
				var encryptedBytesWithIV = StringToByteArray(hex);
				encryptedBytesWithIV = encryptedBytesWithIV.DecompressBytes(compressionMethod);

				var iv = encryptedBytesWithIV.Take(16).ToArray();
				var cipher = encryptedBytesWithIV.Skip(16).ToArray();

				return aes.Decrypt(cipher, iv);
			}
		}

		public static string DecryptBase64WithIV(string base64, string key, CompressionMethod compressionMethod = CompressionMethod.NoCompression)
		{
			using (var aes = new SimpleTwoWayEncryptorAES(key))
			{
				var encryptedBytesWithIV = Convert.FromBase64String(base64);
				encryptedBytesWithIV = encryptedBytesWithIV.DecompressBytes(compressionMethod);

				var iv = encryptedBytesWithIV.Take(16).ToArray();
				var cipher = encryptedBytesWithIV.Skip(16).ToArray();

				return aes.Decrypt(cipher, iv);
			}
		}

		/// <summary>
		/// Generates a unique encryption vector using RijndaelManaged.GenerateIV() method
		/// </summary>
		private byte[] GenerateEncryptionVector()
		{
			if (Rijn == null)
				throw new Exception("Provider is not initialized");

			Rijn.GenerateIV();
			return Rijn.IV;
		}

		/// <summary>
		/// Based on https://stackoverflow.com/a/1344255
		/// Generate a unique string given number of bytes required.
		/// This string can be used as IV. IV byte size should be equal to cipher-block byte size. 
		/// Allows seeing IV in plaintext so it can be passed along a url or some message.
		/// </summary>
		/// <param name="numBytes"></param>
		private static string GetUniqueString(int numBytes)
		{
			var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
			var data = new byte[numBytes];

			//using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
			//{
			//	crypto.GetBytes(data);
			//}
			var crypto = new RNGCryptoServiceProvider();
			crypto.GetBytes(data);

			var result = new StringBuilder(numBytes);
			foreach (var b in data)
			{
				result.Append(chars[b % (chars.Length)]);
			}
			return result.ToString();
		}

		private static byte[] StringToByteArray(string hex)
		{
			var numberChars = hex.Length;
			var bytes = new byte[numberChars / 2];
			for (int i = 0; i < numberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}

		private static string ByteArrayToString(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "");
		}
	}

}
