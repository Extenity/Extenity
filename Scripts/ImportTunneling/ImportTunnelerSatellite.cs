using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Extenity.Applicational;
using UnityEditor;

namespace ImportTunneling
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
		}

		#endregion

		[MenuItem("AAAA/Test")]
		static void TEST()
		{
			var outputPath = "TEST/ImportTunneler/TestFile/SketchUp/can.skp.pck";

			Convert(
				"Assets/Aargh/TEST/ImportTunneler/TestFile/SketchUp/can.skp",
				outputPath,
				BuildTarget.StandaloneWindows64);

			var bundle = AssetBundle.LoadFromFile(outputPath);
			Debug.Log("Bundle : " + bundle);
			try
			{
				bundle.GetAllAssetNames().LogList();
				var asset = bundle.LoadAsset(bundle.GetAllAssetNames()[0]);
				//var asset = bundle.mainAsset;
				var go = GameObject.Instantiate(asset);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				bundle.Unload(false);
			}
		}
	}

}
