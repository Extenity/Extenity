#if !UNITY_WEBPLAYER
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Extenity.Logging;
using UnityEngine;
using Logger = Extenity.Logging.Logger;

namespace Extenity
{

	public static class LocalizationXmlTools
	{
		public static XElement[] CreateLocalStringXElement(string elementName, LocalString localString)
		{
			var list = new XElement[Localization.LanguageCount];

			for (int i = 0; i < Localization.LanguageInfos.Count; i++)
			{
				var languageCode = Localization.LanguageInfos[i].Code;
				var text = localString.StringInLanguage(languageCode);
				list[i] = new XElement(elementName + Localization.LanguageSeparator + languageCode, new XAttribute("Value", text));
			}

			return list;
		}

		public static LocalString ReadLocalString(this XElement item, string elementName, string keyPrefix, string readNonLocalizedToLanguage = "EN")
		{
			var key = string.IsNullOrEmpty(keyPrefix)
				? elementName
				: keyPrefix + "/" + elementName;

			// Read non-localized string (which does not have separator)
			if (!string.IsNullOrEmpty(readNonLocalizedToLanguage))
			{
				var elementNonLocalized = item.Element(elementName);
				if (elementNonLocalized != null)
				{
					var text = (string)elementNonLocalized.Attribute("Value");
					Localization.AddLanguageEntry(key, new KeyValuePair<string, string>(readNonLocalizedToLanguage, text));
				}
			}

			// Read localized strings
			var elements = item.Descendants().Where(x => x.Name.LocalName.StartsWith(elementName + Localization.LanguageSeparator));
			foreach (var element in elements)
			{
				var fullElementName = element.Name.LocalName;
				if (fullElementName.CountCharacters(Localization.LanguageSeparator) > 1)
				{
					Logger.LogError(string.Format("Language entry '{0}' contains more than 1 separator character.", fullElementName));
					continue;
				}

				var text = (string)element.Attribute("Value");
				var languageName = fullElementName.Split(Localization.LanguageSeparator)[1];
				Localization.AddLanguageEntry(key, new KeyValuePair<string, string>(languageName, text));
			}

			return new LocalString(key);
		}
	}

}

#endif
