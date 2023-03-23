using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using UnityEditor;

namespace Extenity.ProjectToolbox
{

	#region Define Symbol Entry

	[Serializable]
	public struct DefineSymbolEntry
	{
		/// <summary>
		/// The index that tells where this define symbol should be located. 0 means at the beginning. -1 means at the end.
		/// </summary>
		public int Index;
		public string Symbol;

		public bool IsAtTheEnd => Index < 0;
		public bool IsValid => !string.IsNullOrWhiteSpace(Symbol);

		public DefineSymbolEntry(int index, string symbol)
		{
			Index = index;
			Symbol = symbol;
		}

		public DefineSymbolEntry(string symbol)
		{
			Index = -1;
			Symbol = symbol;
		}

		public override string ToString()
		{
			return Symbol;
		}
	}

	#endregion

	public static class PlayerSettingsTools
	{
		#region Get/Set Define Symbols Of All Platforms

		public static Dictionary<BuildTargetGroup, List<string>> GetAllDefineSymbolsOfAllPlatforms()
		{
			var buildTargets = Enum.GetValues(typeof(BuildTarget));
			var result = new Dictionary<BuildTargetGroup, List<string>>(buildTargets.Length * 4); // A capacity of 4 times is the optimized size of dictionary to work fast.

			foreach (BuildTarget target in buildTargets)
			{
				var group = BuildPipeline.GetBuildTargetGroup(target);
				if (group == BuildTargetGroup.Unknown || result.ContainsKey(group))
					continue;

				try
				{
					var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
					                                  .Split(';')
					                                  .Select(d => d.Trim())
					                                  .ToList();

					result.Add(group, defineSymbols);
				}
				catch (Exception exception)
				{
					Log.Error($"Failed to get define symbols for build target group '{group}'. Reason: {exception.Message}");
				}
			}

			return result;
		}

		public static void SetAllDefineSymbolsOfAllPlatforms(Dictionary<BuildTargetGroup, List<string>> defineSymbolsByPlatforms)
		{
			foreach (var defineSymbols in defineSymbolsByPlatforms)
			{
				try
				{
					var joined = string.Join(";", defineSymbols.Value.ToArray());
					PlayerSettings.SetScriptingDefineSymbolsForGroup(defineSymbols.Key, joined);
				}
				catch (Exception exception)
				{
					Log.Error($"Failed to set define symbols for build target group '{defineSymbols.Key}'. Reason: {exception.Message}");
				}
			}
		}

		#endregion

		#region Add/Remove Define Symbols

		public static void AddDefineSymbols(string[] symbols, bool ensureNotAddedBefore, bool saveAssets)
		{
			AddDefineSymbols(symbols.Select(entry => new DefineSymbolEntry(entry)).ToArray(), ensureNotAddedBefore, saveAssets);
		}

		public static void AddDefineSymbols(string[] symbols, BuildTargetGroup targetGroup, bool ensureNotAddedBefore, bool saveAssets)
		{
			AddDefineSymbols(symbols.Select(entry => new DefineSymbolEntry(entry)).ToArray(), targetGroup, ensureNotAddedBefore, saveAssets);
		}

		public static void AddDefineSymbols(DefineSymbolEntry[] symbols, bool ensureNotAddedBefore, bool saveAssets)
		{
			var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			AddDefineSymbols(symbols, activeBuildTargetGroup, ensureNotAddedBefore, saveAssets);
		}

		/// <summary>
		/// Source: https://answers.unity.com/questions/1225189/how-can-i-change-scripting-define-symbols-before-a.html
		/// </summary>
		public static void AddDefineSymbols(DefineSymbolEntry[] symbols, BuildTargetGroup targetGroup, bool ensureNotAddedBefore, bool saveAssets)
		{
			if (symbols == null)
				throw new ArgumentNullException();
			symbols = symbols.Where(entry => entry.IsValid).ToArray();
			if (symbols.Length == 0)
				throw new ArgumentException();
			Log.Info($"Adding {symbols.Length.ToStringWithEnglishPluralPostfix("define symbol")} '{string.Join(", ", symbols)}'.");

			symbols = symbols.OrderBy(entry => entry.Index).ToArray();
			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();

			foreach (var symbol in symbols)
			{
				if (!allDefines.Contains(symbol.Symbol))
				{
					if (symbol.Index < 0 || symbol.Index >= allDefines.Count)
					{
						allDefines.Add(symbol.Symbol);
					}
					else
					{
						allDefines.Insert(symbol.Index, symbol.Symbol);
					}
				}
				else if (ensureNotAddedBefore)
				{
					throw new Exception($"The symbol '{symbol}' was already added before.");
				}
			}

			var newDefinesString = string.Join(";", allDefines);
			if (!definesString.Equals(newDefinesString, StringComparison.Ordinal))
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefinesString);
			}

			// Ensure the symbols added.
			foreach (var symbol in symbols)
			{
				if (!HasDefineSymbol(symbol.Symbol, targetGroup))
				{
					throw new Exception($"Failed to complete Define Symbol Add operation for symbol(s) '{string.Join(", ", symbol)}'.");
				}
			}

			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}
		}

		public static DefineSymbolEntry[] RemoveDefineSymbols(string[] symbols, bool saveAssets)
		{
			var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			return RemoveDefineSymbols(symbols, activeBuildTargetGroup, saveAssets);
		}

		public static DefineSymbolEntry[] RemoveDefineSymbols(string[] symbols, BuildTargetGroup targetGroup, bool saveAssets)
		{
			if (symbols == null)
				throw new ArgumentNullException();
			symbols = symbols.Where(entry => !string.IsNullOrWhiteSpace(entry)).ToArray();
			if (symbols.Length == 0)
				throw new ArgumentException();
			Log.Info($"Removing {symbols.Length.ToStringWithEnglishPluralPostfix("define symbol")} '{string.Join(", ", symbols)}'.");

			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();
			var originalDefines = allDefines.Clone();
			var removedDefines = new List<DefineSymbolEntry>();

			foreach (var symbol in symbols)
			{
				if (allDefines.Remove(symbol))
				{
					removedDefines.Add(new DefineSymbolEntry(originalDefines.IndexOf(symbol), symbol));
				}
			}

			var newDefinesString = string.Join(";", allDefines);
			if (!definesString.Equals(newDefinesString, StringComparison.Ordinal))
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefinesString);
			}

			// Ensure the symbols removed.
			foreach (var symbol in symbols)
			{
				if (HasDefineSymbol(symbol, targetGroup))
				{
					throw new Exception($"Failed to complete Define Symbol Remove operation for symbol(s) '{string.Join(", ", symbol)}'.");
				}
			}

			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}

			return removedDefines.ToArray();
		}

		#endregion

		#region Get Define Symbols

		public static string GetDefineSymbols()
		{
			var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			return GetDefineSymbols(activeBuildTargetGroup);
		}

		public static string GetDefineSymbols(BuildTargetGroup targetGroup)
		{
			return PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
		}

		public static bool HasDefineSymbol(string symbol)
		{
			var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			return HasDefineSymbol(symbol, activeBuildTargetGroup);
		}

		public static bool HasDefineSymbol(string symbol, BuildTargetGroup targetGroup)
		{
			if (string.IsNullOrWhiteSpace(symbol))
				throw new ArgumentNullException(nameof(symbol));

			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();
			var result = allDefines.Contains(symbol);

			// Warn the user if there is that symbol but with different letter cases.
			if (!result)
			{
				if (allDefines.Contains(symbol, StringComparer.OrdinalIgnoreCase))
				{
					Log.Warning($"Checking for define symbol '{symbol}' for build target group '{targetGroup}' which has the symbol but with different letter cases.");
				}
			}

			return result;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(PlayerSettingsTools));

		#endregion
	}

}
