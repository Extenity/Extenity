using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class DatabaseFile
	{
		private char separator = '=';
		private string filename;

		private Dictionary<string, string> database = new Dictionary<string, string>();


		public DatabaseFile(string filename)
		{
			this.filename = filename;
		}

		#region File Operations

		public bool ReadFromFile()
		{
			FileStream fileStream = null;

			try
			{
				fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
				if (!fileStream.CanRead)
					return false;
				TextReader reader = new StreamReader(fileStream);

				string line;
				while ((line = reader.ReadLine()) != null)
				{
					line = line.Trim();

					//lString = AES.Decrypt(lString);

					if (!string.IsNullOrEmpty(line))
					{
						string[] pair = line.Split(new char[] { separator }, 2, StringSplitOptions.None);

						if (pair.Length != 2)
						{
							Debug.LogError("Invalid database file! Line: " + line);
						}
						else
						{
							database[pair[0].Trim()] = pair[1].Trim();
						}
					}
				}
			}
			catch (System.Exception)
			{
				return false;
			}

			fileStream.Close();
			return true;
		}

		public bool WriteToFile()
		{
			FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			if (!fileStream.CanWrite)
				return false;
			TextWriter writer = new StreamWriter(fileStream);

			foreach (KeyValuePair<string, string> keyValue in database)
			{
				string lineToWrite = keyValue.Key + separator + keyValue.Value;

				//lLineToWrite = AES.Encrypt(lineToWrite);

				writer.WriteLine(lineToWrite);
			}

			writer.Close();
			fileStream.Close();
			return true;
		}

		#endregion

		#region Database Commands

		public bool ContainsKey(string key)
		{
			return database.ContainsKey(key);
		}

		public string GetValue(string key)
		{
			return database[key];
		}

		public Dictionary<string, string> GetValueList(IList<string> keyList)
		{
			var keyValueList = new Dictionary<string, string>(keyList.Count);

			for (int i = 0; i < keyList.Count; i++)
			{
				string key = keyList[i];
				keyValueList.Add(key, GetValue(key));
			}

			return keyValueList;
		}

		public Dictionary<string, string> GetValueList()
		{
			return new Dictionary<string, string>(database); // Copy dictionary
		}

		public List<string> GetKeyList()
		{
			return database.Keys.ToList();
		}

		public void Remove(string key)
		{
			if (database.ContainsKey(key))
			{
				database.Remove(key);
			}
		}

		public void Remove(IList<string> keyList)
		{
			for (int i = 0; i < keyList.Count; i++)
			{
				Remove(keyList[i]);
			}
		}

		public void SetValue(string key, string value)
		{
			database[key] = value;
		}

		public void SetValue(KeyValuePair<string, string> keyValue)
		{
			SetValue(keyValue.Key, keyValue.Value);
		}

		public void SetValueList(Dictionary<string, string> keyValueList)
		{
			foreach (KeyValuePair<string, string> pair in keyValueList)
			{
				SetValue(pair);
			}
		}

		#endregion
	}

}
