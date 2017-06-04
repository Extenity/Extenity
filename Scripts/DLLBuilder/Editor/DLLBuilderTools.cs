using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public static class DLLBuilderTools
	{
		public static string AutoDetectCSCPath()
		{
			string CSCPath = null;
			var dirs = new List<string>(Directory.GetDirectories(@"C:\Program Files (x86)\MSBuild"));
			dirs.Sort((a, b) => b.CompareTo(a));

			int i = 0;
			for (; i < dirs.Count; i++)
			{
				var path = Path.Combine(dirs[i], @"bin\csc.exe");

				if (File.Exists(path))
				{
					CSCPath = path;
					Debug.Log("Using CSC at path \"" + path + "\".");
					break;
				}
			}

			if (i >= dirs.Count)
			{
				dirs.AddRange(Directory.GetDirectories(@"C:\Windows\Microsoft.NET\Framework64"));
				dirs.Sort((a, b) => b.CompareTo(a));

				for (i = 0; i < dirs.Count; i++)
				{
					var path = Path.Combine(dirs[i], @"csc.exe");

					if (File.Exists(path))
					{
						CSCPath = path;
						Debug.Log("Using CSC at path \"" + path + "\".");
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(CSCPath))
			{
				throw new Exception("Failed to find csc.exe.");
			}
			return CSCPath;
		}

		public static void DetectUnityReferences(ref List<string> unityReferences)
		{
			unityReferences = new List<string>();

			var unityExeDirectoryPath = Path.GetDirectoryName(EditorApplication.applicationPath);
			var unityLibraryPath = Path.Combine(unityExeDirectoryPath, @"Data\Managed");
			var files = Directory.GetFiles(unityLibraryPath, "*.dll");

			for (int i = 0; i < files.Length; i++)
				unityReferences.Add(files[i]);

			if (unityReferences.Count == 0)
				throw new Exception("Failed to find Unity DLLs in Unity installation path: " + unityLibraryPath);
		}

		public static bool IsAssetInPath(string assetPath, string sourcePath)
		{
			if (assetPath.Length <= sourcePath.Length)
				return false;

			int i = 0;
			for (int j = 0; i < assetPath.Length && j < sourcePath.Length; i++, j++)
			{
				if (assetPath[i] == sourcePath[j])
					continue;

				if ((assetPath[i] != Path.DirectorySeparatorChar && assetPath[i] != Path.AltDirectorySeparatorChar) ||
				    (sourcePath[j] != Path.DirectorySeparatorChar && sourcePath[j] != Path.AltDirectorySeparatorChar))
				{
					return false;
				}
			}

			return assetPath[i] == Path.DirectorySeparatorChar || assetPath[i] == Path.AltDirectorySeparatorChar;
		}

		public static string GetUnityVersionDefines(string unityVersion)
		{
			string[] parts = unityVersion.Split('.');
			Regex re = new Regex(@"\d+");
			Match m = re.Match(parts[2]);
			if (m.Success == true)
				return "UNITY_" + parts[0] + ";UNITY_" + parts[0] + "_" + parts[1] + ";UNITY_" + parts[0] + "_" + parts[1] + "_" + m.Value;
			return "UNITY_" + parts[0] + ";UNITY_" + parts[0] + "_" + parts[1] + ";UNITY_" + parts[0] + "_" + parts[1] + "_" + parts[2];
		}
	}

}
