using System;
using System.Collections.Generic;
using System.Text;

namespace Extenity.ApplicationToolbox
{

	public struct Argument
	{
		public readonly string Key;
		public readonly string Value;
		public readonly string RawValue;

		public Argument(string key) : this()
		{
			Key = key;
		}

		public Argument(string key, string value, string rawValue)
		{
			Key = key;
			Value = value;
			RawValue = rawValue;
		}
	}

	public struct Arguments
	{
		public readonly Argument[] Entries;
		public readonly string RawArguments;

		#region Configuration

		private const string ArgumentPrefix = "-";

		#endregion

		#region Initialization

		public Arguments(Argument[] entries, string rawArguments)
		{
			Entries = entries;
			RawArguments = rawArguments;
		}

		#endregion

		#region Parse

		public static Arguments ParseCommandLine()
		{
			return Parse(Environment.CommandLine, true);
		}

		/// <summary>
		/// C-like argument parser. It's mainly used for parsing command line arguments but the same format can be used
		/// for other configuration needs too.
		///
		/// Example: myprogram.exe -somekey -someotherkey AndItsValue -anotherKey "quoted value with spaces"
		/// </summary>
		/// <remarks>
		/// Don't use C# args directly. It modifies the data unpleasantly and fails to parse quote characters. Instead,
		/// this parser respects the formatting and it also groups key-value pairs together for easier use.
		///
		/// Does not support using multiple command abbreviations in a single key, like Linux 'ls -lh' command, which
		/// 'l' stands for 'list' and 'h' stands for 'human readable file sizes'.
		/// </remarks>
		/// <param name="arguments">
		/// Command line string with arguments. You may use <see cref="Environment.CommandLine"/> to get application
		/// arguments given at launch and feed it here.</param>
		/// <param name="omitFirstEntry"></param>
		public static Arguments Parse(string arguments, bool omitFirstEntry)
		{
			// Source: https://stackoverflow.com/questions/9287812/backslash-and-quote-in-command-line-arguments
			// Modified to convert command line into key-value list.

			var rawArguments = omitFirstEntry
				? null
				: arguments;

			if (string.IsNullOrWhiteSpace(arguments))
			{
				return new Arguments(new Argument[0], null);
			}

			// Having line feeds in input data is not supported. In order to support that, some modifications needed
			// in the code below that uses '\n' as separators, which is not a big deal. But better cover the code with
			// unit test before making those modifications.
			if (arguments.IndexOf('\n') >= 0 || arguments.IndexOf('\r') >= 0)
			{
				throw new Exception("Arguments should not contain line feed characters.");
			}

			var argsBuilder = new StringBuilder(arguments);
			var inQuote = false;

			// Convert the spaces to a newline sign so we can split at newline later on
			// Only convert spaces which are outside the boundaries of quoted text
			for (int i = 0; i < argsBuilder.Length; i++)
			{
				if (argsBuilder[i].Equals('"'))
				{
					inQuote = !inQuote;
				}

				if (argsBuilder[i].Equals(' ') && !inQuote)
				{
					argsBuilder[i] = '\n';
					if (rawArguments == null)
					{
						rawArguments = arguments.Substring(i + 1).Trim();
					}
				}
			}

			// Split to args array
			var args = argsBuilder.ToString().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);

			var startIndex = omitFirstEntry ? 1 : 0; // The first one is the executable path, not an argument. So we skip it.
			var list = new List<Argument>(args.Length);
			for (int i = startIndex; i < args.Length;)
			{
				if (args[i].StartsWith(ArgumentPrefix))
				{
					var key = args[i++];
					if (i >= args.Length)
					{
						list.Add(new Argument(key));
					}
					else
					{
						if (args[i].StartsWith(ArgumentPrefix))
						{
							list.Add(new Argument(key));
						}
						else
						{
							var rawValue = args[i++];
							// Clean the '"' signs from the args as needed.
							var value = ClearQuotes(rawValue);
							list.Add(new Argument(key, value, rawValue));
						}
					}
				}
				else
				{
					throw new Exception($"Unexpected argument '{args[i]}'.");
				}
			}

			return new Arguments(list.ToArray(), rawArguments);
		}

		/// <summary>
		/// Cleans quotes from the arguments.<br/>
		/// All single quotes (") will be removed.<br/>
		/// Every pair of quotes ("") will transform to a single quote.<br/>
		/// </summary>
		/// <param name="stringWithQuotes">A string with quotes.</param>
		/// <returns>The same string if its without quotes, or a clean string if its with quotes.</returns>
		private static string ClearQuotes(string stringWithQuotes)
		{
			int quoteIndex;
			if ((quoteIndex = stringWithQuotes.IndexOf('"')) == -1)
			{
				// String is without quotes..
				return stringWithQuotes;
			}

			// Linear sb scan is faster than string assignment if quote count is 2 or more (=always)
			var sb = new StringBuilder(stringWithQuotes);
			for (int i = quoteIndex; i < sb.Length; i++)
			{
				if (sb[i].Equals('"'))
				{
					// If we are not at the last index and the next one is '"', we need to jump one to preserve one
					if (i != sb.Length - 1 && sb[i + 1].Equals('"'))
					{
						i++;
					}

					// We remove and then set index one backwards.
					// This is because the remove itself is going to shift everything left by 1.
					sb.Remove(i--, 1);
				}
			}

			return sb.ToString();
		}

		#endregion

		#region Getters

		public string GetSingleValueEnsured(string key)
		{
			var found = false;
			var value = string.Empty;
			for (int i = 0; i < Entries.Length; i++)
			{
				if (Entries[i].Key == key)
				{
					if (found)
						throw new Exception($"There are more than one '{key}' in arguments.");
					found = true;
					value = Entries[i].Value;
				}
			}

			if (!found)
				throw new Exception($"There is no '{key}' in arguments.");

			return value;
		}

		#endregion

		#region Log

		public string ToHumanReadableString(string linePrefix = "", string keyValueSeparator = " = ", string lineEnding = "\n")
		{
			var stringBuilder = new StringBuilder();
			for (var i = 0; i < Entries.Length; i++)
			{
				var item = Entries[i];
				stringBuilder.Append(linePrefix);
				stringBuilder.Append(item.Key);
				if (!string.IsNullOrEmpty(item.RawValue))
				{
					stringBuilder.Append(keyValueSeparator);
					stringBuilder.Append(item.RawValue);
				}
				stringBuilder.Append(lineEnding);
			}
			return stringBuilder.ToString(0, Math.Max(0, stringBuilder.Length - lineEnding.Length)); // Omit the last line ending character
		}

		#endregion
	}

}
