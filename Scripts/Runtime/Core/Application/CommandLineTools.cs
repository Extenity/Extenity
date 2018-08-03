using System;

namespace Extenity.ApplicationToolbox
{

	public static class CommandLineTools
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

}
