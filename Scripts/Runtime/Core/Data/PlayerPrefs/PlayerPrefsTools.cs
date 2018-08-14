using System;
using Extenity.ApplicationToolbox;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public enum PathHashPostfix
	{
		No = 0,
		Yes = 1,
		OnlyInEditor = 2,
	}

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

		#region Path Hash Postfix

		private static string _PathHash;
		public static string PathHash
		{
			get
			{
				if (string.IsNullOrEmpty(_PathHash))
				{
					_PathHash = "-" + ApplicationTools.PathHash;
				}
				return _PathHash;
			}
		}

		#endregion

		#region Generate Key

		public static string GenerateKey(string key, PathHashPostfix appendPathHashToKey)
		{
			switch (appendPathHashToKey)
			{
				case PathHashPostfix.No:
					return key;
				case PathHashPostfix.Yes:
					return key + PathHash;
				case PathHashPostfix.OnlyInEditor:
					if (Application.isEditor)
						return key + PathHash;
					else
						return key;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion
	}

}
