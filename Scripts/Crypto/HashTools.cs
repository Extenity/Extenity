using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Extenity.Crypto
{

	public static class HashTools
	{
		#region MD5

		public static string CalculateMD5HashOfFile(string filePath)
		{
			try
			{
				using (var stream = File.OpenRead(filePath))
				{
					using (var md5 = MD5.Create())
					{
						var checksum = md5.ComputeHash(stream);
						return Encoding.Default.GetString(checksum);
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion

		#region SHA256

		public static string CalculateSHA256HashOfFile(string filePath)
		{
			try
			{
				using (FileStream stream = File.OpenRead(filePath))
				{
					using (var sha = new SHA256Managed())
					{
						var checksum = sha.ComputeHash(stream);
						return BitConverter.ToString(checksum).Replace("-", String.Empty);
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion
	}

}
