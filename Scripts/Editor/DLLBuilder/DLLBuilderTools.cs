using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Extenity.DataToolbox;

namespace Extenity.DLLBuilder
{

	public static class DLLBuilderTools
	{
		public static readonly string CSCPath = @"C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe";
		//public static readonly string CSCPath = @"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\csc.exe";

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
					Log.Info($"Using CSC at path '{path}'.");
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
						Log.Info($"Using CSC at path '{path}'.");
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

			var unityExeDirectoryPath = DLLBuilderConfiguration.GetEnvironmentVariable(Constants.SystemEnvironmentVariables.UnityEditor);
			var unityLibraryPath = Path.Combine(unityExeDirectoryPath, @"Data\Managed");
			var files = Directory.GetFiles(unityLibraryPath, "*.dll");

			for (int i = 0; i < files.Length; i++)
				unityReferences.Add(files[i].FixDirectorySeparatorChars());

			if (unityReferences.Count == 0)
				throw new Exception("Failed to find Unity DLLs in Unity installation path: " + unityLibraryPath);
		}

		public static string GetUnityVersionDefines(string unityVersion)
		{
			var parts = unityVersion.Split('.');
			var regex = new Regex(@"\d+");
			var match = regex.Match(parts[2]);
			var lastPart = match.Success ? match.Value : parts[2];
			return
				"UNITY_" + parts[0] +
				";UNITY_" + parts[0] + "_" + parts[1] +
				";UNITY_" + parts[0] + "_" + parts[1] + "_" + lastPart +
				";UNITY_" + parts[0] + "_" + parts[1] + "_OR_NEWER";
		}
	}

}
