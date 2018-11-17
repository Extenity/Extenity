using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Extenity.ImportTunnelingToolbox.Editor
{

	public static class ImportTunnelerSatellite
	{
		#region Conversion

		public static void ConvertUsingArgs()
		{
			var sourceAssetPath = CommandLineTools.GetValue("+sourceAssetPath");
			var outputAssetPath = CommandLineTools.GetValue("+outputAssetPath");
			var assetBundlePlatformString = CommandLineTools.GetValue("+assetBundlePlatform");
			if (string.IsNullOrEmpty(sourceAssetPath))
				throw new ArgumentNullException("sourceAssetPath");
			if (string.IsNullOrEmpty(outputAssetPath))
				throw new ArgumentNullException("outputAssetPath");
			if (string.IsNullOrEmpty(assetBundlePlatformString))
				throw new ArgumentNullException("assetBundlePlatform");


			var assetBundlePlatform = EnumTools.ParseSafe<BuildTarget>(assetBundlePlatformString, true);

			Convert(sourceAssetPath, outputAssetPath, assetBundlePlatform);
		}

		public static void Convert(string sourceAssetPath, string outputAssetPath, BuildTarget assetBundlePlatform)
		{
			Debug.LogFormat("Starting asset conversion for '{0}' output as '{1}'.", sourceAssetPath, outputAssetPath);

			var outputFileName = Path.GetFileName(outputAssetPath);
			var outputDirectoryPath = Path.GetDirectoryName(outputAssetPath);
			var importer = AssetImporter.GetAtPath(sourceAssetPath);
			importer.assetBundleName = outputFileName;

			if (!Directory.Exists(outputDirectoryPath))
				Directory.CreateDirectory(outputDirectoryPath);

			var builds = new AssetBundleBuild[]
			{
				new AssetBundleBuild()
				{
					assetBundleName = outputFileName,
					assetNames = new string[] {sourceAssetPath}
				}
			};

			BuildPipeline.BuildAssetBundles(outputDirectoryPath, builds, BuildAssetBundleOptions.UncompressedAssetBundle, assetBundlePlatform);

			Log.Info("Conversion completed");
		}

		#endregion

		#region Tools - CommandLineTools

		private static class CommandLineTools
		{
			#region Initialization

			static CommandLineTools()
			{
				CommandLine = Environment.CommandLine;
				SplitCommandLine = CommandLine.Split(' ');
			}

			#endregion

			#region Data

			public static string CommandLine { get; private set; }
			public static string[] SplitCommandLine { get; private set; }

			#endregion

			#region Get

			public static string GetValue(string key)
			{
				for (int i = 0; i < SplitCommandLine.Length; i++)
				{
					if (SplitCommandLine[i] == key)
					{
						i++;

						if (i < SplitCommandLine.Length)
						{
							return SplitCommandLine[i];
						}
						else
						{
							return null;
						}
					}
				}
				return null;
			}

			#endregion
		}

		#endregion

		#region Tools - EnumTools

		private static class EnumTools
		{
			public static T ParseSafe<T>(string value, bool ignoreCase = false)
			{
				var enumType = typeof(T);

				if (!enumType.IsEnum)
					throw new ArgumentException("Generic type must be an enumeration.", "enumType");

				try
				{
					var result = (T)Enum.Parse(enumType, value, ignoreCase);
					return result;
				}
				catch
				{
				}
				return default(T);
			}
		}

		#endregion
	}

}
