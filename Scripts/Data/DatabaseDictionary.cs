using System.Collections.Generic;
using System.Linq;

public class DatabaseDictionary
{
	private Dictionary<string, string> database = new Dictionary<string, string>();


	public DatabaseDictionary()
	{
	}

	#region File Operations

	public void ReadFromFile()
	{

	}

	public void WriteToFile()
	{

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
			WriteToFile();
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
		WriteToFile();
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
