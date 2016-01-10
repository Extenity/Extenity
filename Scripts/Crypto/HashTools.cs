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
						return Encoding.Default.GetString(md5.ComputeHash(stream));
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
