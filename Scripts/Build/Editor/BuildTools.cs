using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Extenity.Crypto;
using UnityEditor;

public static class BuildTools
{
	#region Process

	public static int RunConsoleCommandAndCaptureOutput(string filePath, string arguments, out string output)
	{
		if (string.IsNullOrEmpty(filePath))
			throw new ArgumentNullException("filePath");

		var process = new Process();

		// Redirect the output stream of the child process.
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.FileName = filePath;
		if (!string.IsNullOrEmpty(arguments))
			process.StartInfo.Arguments = arguments;
		process.Start();

		// Do not wait for the child process to exit before
		// reading to the end of its redirected stream.
		// process.WaitForExit();
		// Read the output stream first and then wait.
		output = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		return process.ExitCode;
	}

	public static int RunCommandLine(string commandLine, out string output, bool terminateWhenDone)
	{
		if (string.IsNullOrEmpty(commandLine))
			throw new ArgumentNullException("commandLine");

		string arguments = (terminateWhenDone ? "/C " : "/K ") + commandLine;
		return RunConsoleCommandAndCaptureOutput("cmd.exe", arguments, out output);
	}

	#endregion

	#region Mercurial

	public static string GetVersionInfoFromMercurialRepository()
	{
		string output;
		int exitCode;
		try
		{
			exitCode = RunConsoleCommandAndCaptureOutput("hg", "id -i -b", out output);
		}
		catch (Exception e)
		{
			throw new Exception("Could not get version from Mercurial repository. Exception: " + e);
		}

		if (exitCode != 0)
		{
			throw new Exception("Could not get version from Mercurial repository. Exit code is " + exitCode + ". Output: '" + output + "'");
		}
		return output.Trim().Trim(new[] { '\r', '\n' });
	}

	#endregion

	#region Get Scene Names From Build Settings

	public static string[] GetSceneNamesFromBuildSettings(List<string> excludingNames = null)
	{
		var list = new List<string>();

		for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			var scene = EditorBuildSettings.scenes[i];

			if (scene.enabled)
			{
				string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
				name = name.Substring(0, name.Length - 6);

				if (excludingNames != null)
				{
					if (excludingNames.Contains(name))
						continue;
				}

				list.Add(name);
			}
		}

		return list.ToArray();
	}

	public static string[] GetScenePathsFromBuildSettings(List<string> excludingPaths = null)
	{
		var list = new List<string>();

		for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			var scene = EditorBuildSettings.scenes[i];

			if (scene.enabled)
			{
				if (excludingPaths != null)
				{
					if (excludingPaths.Contains(scene.path))
						continue;
				}

				list.Add(scene.path);
			}
		}

		return list.ToArray();
	}

	public static string GetScenePathFromBuildSettings(string sceneName, bool onlyIfEnabled)
	{
		if (string.IsNullOrEmpty(sceneName))
			return null;

		for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			var scene = EditorBuildSettings.scenes[i];

			if (!onlyIfEnabled || scene.enabled)
			{
				string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
				name = name.Substring(0, name.Length - 6);

				if (name == sceneName)
				{
					return scene.path;
				}
			}
		}
		return null;
	}

	#endregion

	#region Unity FileID Calculator

	/// <summary>
	/// http://forum.unity3d.com/threads/yaml-fileid-hash-function-for-dll-scripts.252075/
	/// </summary>
	public static int CalculateFileID(Type t)
	{
		string toBeHashed = "s\0\0\0" + t.Namespace + t.Name;

		using (HashAlgorithm hash = new MD4())
		{
			byte[] hashed = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed));

			int result = 0;

			for (int i = 3; i >= 0; --i)
			{
				result <<= 8;
				result |= hashed[i];
			}

			return result;
		}
	}

	#endregion
}
