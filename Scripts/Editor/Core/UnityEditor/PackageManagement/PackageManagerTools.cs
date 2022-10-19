using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.FileSystemToolbox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	#region Package Manifest Entry

	public struct PackageManifestEntry
	{
		public readonly string PackageID;
		public readonly string VersionOrLink;

		public PackageManifestEntry(string packageId, string versionOrLink)
		{
			PackageID = packageId;
			VersionOrLink = versionOrLink;
		}
	}

	#endregion

	public static class PackageManagerTools
	{
		#region Configuration

		public static readonly string PackageJsonFileName = "package.json";
		public static readonly string PackageManifestPath = ApplicationTools.UnityProjectPaths.PackagesRelativePath.AppendFileToPath("manifest.json");
		public static readonly string PackageLockPath = ApplicationTools.UnityProjectPaths.PackagesRelativePath.AppendFileToPath("packages-lock.json");
		public static readonly string ExtenityPackageName = "com.canbaycay.extenity";

		#endregion

		#region Package Manifest Content

		public static List<PackageManifestEntry> GetPackageManifestContent()
		{
			var path = PackageManifestPath;

			// Approach 1: Took 36 ms
			// var jsonText = File.ReadAllText(path);
			// var json = JObject.Parse(jsonText); // 8 ms
			// var dependencies = json.GetValue("dependencies"); // 0 ms
			// var content = dependencies.ToObject<Dictionary<string, string>>(); // 28 ms

			// Approach 2: Took 60 ms
			// JsonConvert.DeserializeObject<>

			// Approach 3: Took 185 ms (the worst idea ever)
			// var jsonText = File.ReadAllText(path);
			// dynamic json = JObject.Parse(jsonText);
			// var dependencies = json["dependencies"];
			// var content = (Dictionary<string, string>)dependencies.ToObject<Dictionary<string, string>>();

			// Approach 4: Took 29 ms
			// Source: https://stackoverflow.com/questions/19438472/json-net-deserialize-a-specific-property
			Dictionary<string, string> content = null;
			using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var streamReader = new StreamReader(fileStream))
			using (var jsonReader = new JsonTextReader(streamReader))
			{
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.PropertyName && (string)jsonReader.Value == "dependencies")
					{
						jsonReader.Read(); // Read once more to get to the value of that property.
						var serializer = new JsonSerializer();
						content = serializer.Deserialize<Dictionary<string, string>>(jsonReader);
					}
				}
			}

			if (content == null)
			{
				return new List<PackageManifestEntry>(0);
			}

			var result = new List<PackageManifestEntry>(content.Count);
			foreach (var item in content)
			{
				result.Add(new PackageManifestEntry(item.Key, item.Value));
			}
			// Log.Info(string.Join("\n", result.Select(x => x.PackageID)));
			return result;
		}

		#endregion

		#region Package Manifest Queries

		/// <summary>
		/// Tells if a package is defined in the manifest. You need to get package manifest content first via
		/// <see cref="GetPackageManifestContent"/>.
		/// </summary>
		public static bool IsPackageDefinedInManifest(this List<PackageManifestEntry> packageManifestContent, string packageID)
		{
			for (var i = 0; i < packageManifestContent.Count; i++)
			{
				if (packageManifestContent[i].PackageID.Equals(packageID, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Development - Measure Package Manifest Read

		/*
		[InitializeOnLoadMethod]
		private static void TEST_ReadPackageManifestContents()
		{
			var stopwatch = new ProfilerStopwatch();
			stopwatch.Start();
			var content = GetPackageManifestContent();
			stopwatch.EndAndLog("Got the package manifest. Took: {0}");
			foreach (var entry in content)
			{
				Log.Info(entry.PackageID + "\t\t" + entry.VersionOrLink);
			}
		}
		*/

		#endregion

		#region Development - Unity's PackageManager Class (Which takes too much time to get manifest contents)

		/*
		public static List<PackageInfo> GetPackageManifestContent()
		{
			var listRequest = Client.List(false, true);

			var maxTries = 5000;
			var tries = 0;

			while (listRequest.Status == StatusCode.InProgress && tries++ < maxTries)
			{
				Thread.Sleep(1);
			}

			if (listRequest.Status == StatusCode.Success)
			{
				var listResult = listRequest.Result;
				if (listResult.error != null)
					throw new Exception(listResult.error.message);
				var list = listResult.ToList();
				return list;
			}

			throw new Exception($"Package manifest query failed. Status: {listRequest.Status}");
		}

		[InitializeOnLoadMethod]
		private static void TEST()
		{
			var stopwatch = new ProfilerStopwatch();
			stopwatch.Start();
			var content = GetPackageManifestContent();
			stopwatch.EndAndLog("Got the package manifest. Took: {0}");
			foreach (var entry in content)
			{
				Log.Info(entry.packageId + "\t\t" + entry.name + "\t\t" + entry.version);
			}

			// EditorApplication.delayCall += () =>
			// {
			// 	var stopwatch = new ProfilerStopwatch();
			// 	stopwatch.Start();
			// 	var listRequest = Client.List(false, true);
			// 	// Thread.Sleep(2000);
			// 	EditorApplication.delayCall += () =>
			// 	{
			// 		stopwatch.EndAndLog("Got the package manifest. Took: {0}");
			// 		Log.Info("listRequest.Status: " + listRequest.Status);
			// 	};
			// };
		}
		*/

		#endregion

		#region Invalidate Package Lock

		/// <summary>
		/// Removes the lock entry of specified package. Then Unity will get the latest revision of that package in next
		/// compilation.
		/// </summary>
		public static bool InvalidatePackageLock(string packageName, bool refreshAssetDatabase)
		{
			var content = File.ReadAllText(PackageLockPath);
			if (string.IsNullOrWhiteSpace(content))
			{
				return false;
			}

			var jObject = (JObject)JsonConvert.DeserializeObject(content);

			jObject["dependencies"]
				.Where(x => ((string)x.Path).IndexOf(packageName, StringComparison.InvariantCulture) >= 0)
				.ToList()
				.ForEach(x => x.Remove());

			var result = jObject.ToString();
			File.WriteAllText(PackageLockPath, result);

			if (refreshAssetDatabase)
			{
				AssetDatabase.Refresh();
			}
			return true;
		}

		#endregion

		#region Get Package Name In Manifest Json

		[Serializable]
		private class NameOnlyPackageManifest
		{
			public string name = null;
		}

		public static string GetPackageNameInManifestJson(string manifestPath)
		{
			var json = File.ReadAllText(manifestPath);
			var manifest = JsonUtility.FromJson<NameOnlyPackageManifest>(json);
			return manifest.name;
		}

		#endregion

		#region Editor Menu

		[MenuItem(ExtenityMenu.Update + "Update Extenity", priority = ExtenityMenu.UpdatePriority)]
		private static void Menu_UpdateExtenity()
		{
			InvalidatePackageLock(ExtenityPackageName, true);
		}

		#endregion
	}

}
