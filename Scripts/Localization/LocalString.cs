using System;
using UnityEngine;
using System.Collections;

namespace Extenity
{

	public class LocalString
	{
		#region Initialization

		public LocalString(string key)
		{
			Key = key;
		}

		#endregion

		#region Data

		public string Key { get; private set; }

		public string String
		{
			get { return Localization.GetText(Key); }
		}

		public string StringInLanguage(string languageName)
		{
			return Localization.GetText(Key, languageName);
		}

		#endregion

		#region Conversion

		public static implicit operator string(LocalString localString)
		{
			return localString.String;
		}

		public override string ToString()
		{
			return String;
		}

		#endregion
	}

}
