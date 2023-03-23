using System.Collections.Generic;
using Newtonsoft.Json;

namespace Extenity.UnityEditorToolbox.UnityPackageManagement.Editor
{

	public class PackageImage
	{
		public PackageImage.ImageType type;
		public string thumbnailUrl;
		public string url;

		public enum ImageType
		{
			Main,
			Screenshot,
			// ReSharper disable once IdentifierTypo
			Sketchfab,
			Youtube,
		}
	}

	public class PackageLink
	{
		public string name;
		public string url;
	}

	public class FetchedInfo
	{
		public string id;
		public string packageName;
		public string description;
		public string author;
		public string publisherId;
		public string category;
		public string versionString;
		public string versionId;
		public string publishedDate;
		public string displayName;
		public string state;
		public List<string> supportedVersions;
		public List<PackageImage> images;
		public List<PackageLink> links;

		public static FetchedInfo ConvertFromUnityInternal(object unityInternalFetchedInfo, bool logJson)
		{
			var json = JsonConvert.SerializeObject(unityInternalFetchedInfo, Formatting.Indented);
			if (logJson)
			{
				PackageManagerTools.Log.Info("Fetched Info: " + json);
			}
			return JsonConvert.DeserializeObject<FetchedInfo>(json);
		}
	}

}
