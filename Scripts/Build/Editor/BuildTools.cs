using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Collections;

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
}
