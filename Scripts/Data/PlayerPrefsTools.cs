using UnityEngine;

namespace Extenity.DataToolbox
{

	public static class PlayerPrefsTools
	{
		public static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}

		public static bool GetBool(string key, bool defaultValue = default(bool))
		{
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
		}
	}

}
