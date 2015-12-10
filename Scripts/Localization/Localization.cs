using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Extenity.Logging;
using Logger = Extenity.Logging.Logger;

namespace Extenity
{

	public class LanguageInfo
	{
		public string Name;
		public string EnglishName;
		public string Code;
	}

	public static class Localization
	{
		#region Initialization

		static Localization()
		{
			AllLanguageTexts = new Dictionary<string, Dictionary<string, string>>();
		}

		#endregion

		#region Configuration

		public static readonly char LanguageSeparator = '_';

		#endregion

		#region Languages

		public static string CurrentLanguage { get; private set; }

		public delegate void LanguageChangeDelegate(string newLanguage);
		public static event LanguageChangeDelegate OnLanguageChanged;

		public static int LanguageCount { get { return AllLanguageTexts.Count; } }
		public static List<LanguageInfo> LanguageInfos = new List<LanguageInfo>();

		public static void AddLanguage(string name, string englishName, string code)
		{
			if (GetLanguageTexts(name) != null)
			{
				Logger.LogError("Tried to add an already added language '" + name + "'.");
				return;
			}
			else
			{
				Logger.Log(string.Format("Adding language {0}, {1} ({2})", englishName, name, code));
			}

			AllLanguageTexts.Add(code, new Dictionary<string, string>(100));
			LanguageInfos.Add(new LanguageInfo { Name = name, EnglishName = englishName, Code = code });
		}

		public static void SetCurrentLanguage(string languageName)
		{
			Logger.Log("Changing language to '" + languageName + "'");

			CurrentLanguage = null;
			CurrentLanguageTexts = null;

			Dictionary<string, string> texts;
			if (!AllLanguageTexts.TryGetValue(languageName, out texts))
			{
				throw new Exception("Language '" + languageName + "' is not found.");
			}

			CurrentLanguage = languageName;
			CurrentLanguageTexts = texts;

			if (OnLanguageChanged != null)
			{
				OnLanguageChanged(languageName);
			}
		}

		#endregion

		#region Data

		public static Dictionary<string, string> CurrentLanguageTexts { get; private set; }
		public static Dictionary<string, Dictionary<string, string>> AllLanguageTexts { get; private set; }

		public static bool IsCurrentLanguageAssigned
		{
			get { return CurrentLanguageTexts != null; }
		}

		public static Dictionary<string, string> GetLanguageTexts(string languageCode)
		{
			Dictionary<string, string> texts;
			if (AllLanguageTexts.TryGetValue(languageCode, out texts))
			{
				return texts;
			}
			return null;
		}

		public static string GetText(string key)
		{
			if (!IsCurrentLanguageAssigned)
			{
				Logger.LogError("Tried to get localized text for '" + key + "' but current language was not assigned.");
				return "";
			}

			string value;
			if (CurrentLanguageTexts.TryGetValue(key, out value))
			{
				return value;
			}
			else
			{
				Logger.LogError("Localization key '" + key + "' is not found in language '" + CurrentLanguage + "'.");
				return "";
			}
		}

		public static string GetText(string key, string languageName)
		{
			var languageTexts = GetLanguageTexts(languageName);

			if (languageTexts == null)
			{
				Logger.LogError("Language '" + languageName + "' does not exist.");
				return "";
			}

			string value;
			if (languageTexts.TryGetValue(key, out value))
			{
				return value;
			}
			else
			{
				Logger.LogError("Localization key '" + key + "' is not found in language '" + CurrentLanguage + "'.");
				return "";
			}
		}

		#endregion

		#region Create String

		public static void AddLanguageEntries(string key, params KeyValuePair<string, string>[] languageAndTextPairs)
		{
			for (int i = 0; i < languageAndTextPairs.Length; i++)
			{
				AddLanguageEntry(key, languageAndTextPairs[i]);
			}
		}

		public static void AddLanguageEntry(string key, KeyValuePair<string, string> languageAndTextPair)
		{
			var languageCode = languageAndTextPair.Key;
			var text = languageAndTextPair.Value;

			var languageTexts = GetLanguageTexts(languageCode);
			if (languageTexts == null)
			{
				Logger.LogError("Tried to create text for an undefined language '" + languageCode + "'.");
			}
			else
			{
				if (languageTexts.ContainsKey(key))
				{
					Logger.LogError("Tried to create an already created text for language '" + languageCode + "' with key '" + key + "'.");
				}
				else
				{
					languageTexts.Add(key, text);
				}
			}
		}

		#endregion

		#region Consistency

		public static void CheckConsistency()
		{
			var allKeys = new List<string>(1000);

			// Collect all keys
			foreach (var language in AllLanguageTexts)
			{
				foreach (var entry in language.Value)
				{
					if (!allKeys.Contains(entry.Key))
					{
						allKeys.Add(entry.Key);
					}
				}
			}

			// Compare each language to all keys list
			foreach (var language in AllLanguageTexts)
			{
				var languageKeys = language.Value;

				foreach (var allKeysEntry in allKeys)
				{
					if (!languageKeys.ContainsKey(allKeysEntry))
					{
						Logger.LogError(string.Format("Language '{0}' does not have an entry for '{1}'", language.Key, allKeysEntry));
					}
				}
			}
		}

		#endregion

		#region Unique Dummy Key Creator

		public static int LastGivenUniqueDummyKey = 0;

		public static string CreateUniqueDummyKey()
		{
			return "##UniqueDummyKey-" + (++LastGivenUniqueDummyKey) + "##";
		}

		#endregion
	}

}
