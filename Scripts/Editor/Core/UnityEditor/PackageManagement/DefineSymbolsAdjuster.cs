using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.ProfilingToolbox;
using Extenity.ProjectToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	#region Some Implementation Details

	public enum DefineSymbolAdjustmentOperation
	{
		Unspecified,

		[Tooltip("Adds the symbol into defines if not already added.")]
		Define,

		[Tooltip("Removes the symbol from defines if added before.")]
		Undefine,

		[Tooltip("Adds the symbol into defines, if the module exists. Removes if the module does not exist.")]
		DefineWithModuleExistence,

		[Tooltip("Removes the symbol from defines, if the module exists. Adds if the module does not exist.")]
		UndefineWithModuleExistence,
	}

	public struct DefineSymbolAdjustmentEntry
	{
		public DefineSymbolAdjustmentOperation Operation;
		public string Module;
		public string Symbol;

		private DefineSymbolAdjustmentEntry(DefineSymbolAdjustmentOperation operation, string module, string symbol)
		{
			Operation = operation;
			Module = module;
			Symbol = symbol;
		}

		public static DefineSymbolAdjustmentEntry Define(string symbol)
		{
			return new DefineSymbolAdjustmentEntry(DefineSymbolAdjustmentOperation.Define, null, symbol);
		}

		public static DefineSymbolAdjustmentEntry Undefine(string symbol)
		{
			return new DefineSymbolAdjustmentEntry(DefineSymbolAdjustmentOperation.Undefine, null, symbol);
		}

		public static DefineSymbolAdjustmentEntry DefineWithModuleExistence(string module, string symbol)
		{
			return new DefineSymbolAdjustmentEntry(DefineSymbolAdjustmentOperation.DefineWithModuleExistence, module, symbol);
		}

		public static DefineSymbolAdjustmentEntry UndefineWithModuleExistence(string module, string symbol)
		{
			return new DefineSymbolAdjustmentEntry(DefineSymbolAdjustmentOperation.UndefineWithModuleExistence, module, symbol);
		}

		#region Equality

		public bool ModuleAndSymbolEquals(DefineSymbolAdjustmentEntry other)
		{
			if ((string.IsNullOrWhiteSpace(Module) && string.IsNullOrWhiteSpace(other.Module)) ||
			    string.Equals(Module, other.Module, StringComparison.OrdinalIgnoreCase))
			{
				if ((string.IsNullOrWhiteSpace(Symbol) && string.IsNullOrWhiteSpace(other.Symbol)) ||
				    string.Equals(Symbol, other.Symbol, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public class ModuleAndSymbolEqualityComparer : IEqualityComparer<DefineSymbolAdjustmentEntry>
		{
			public static readonly ModuleAndSymbolEqualityComparer Instance = new ModuleAndSymbolEqualityComparer();

			public bool Equals(DefineSymbolAdjustmentEntry x, DefineSymbolAdjustmentEntry y)
			{
				return x.ModuleAndSymbolEquals(y);
			}

			public int GetHashCode(DefineSymbolAdjustmentEntry obj)
			{
				return obj.Module.GetHashCode();
			}
		}

		#endregion
	}

	public class DefineSymbolAdjusterAssetPostprocessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (deletedAssets == null)
				return;
			for (int i = 0; i < deletedAssets.Length; i++)
			{
				var path = deletedAssets[i];
				if (path.StartsWith("Package") && path.EndsWith("package.json"))
				{
					// This means a package was removed from manifest. If the removal causes any compilation errors,
					// Unity will not reload assemblies and will not call InitializeOnLoadMethod methods.
					// So AdjustDefineSymbolsForInstalledModules won't be called to allow making necessary
					// adjustments to define symbol configurations that would possibly fix the compilation errors.
					//
					// So we call it here manually. It's not that heavy on processor. so it's safe to call it
					// multiple times.
					DefineSymbolsAdjuster.AdjustDefineSymbolsForInstalledModules();
				}
			}
		}
	}

	#endregion

	public static class DefineSymbolsAdjuster
	{
		public static DefineSymbolAdjustmentEntry[] DefineSymbolAdjustmentConfiguration_ExtenityDefaults => new[]
		{
			DefineSymbolAdjustmentEntry.Define("UNITY"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.audio", "DisableUnityAudio"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.physics", "DisableUnityPhysics"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.physics2d", "DisableUnityPhysics2D"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.cloth", "DisableUnityCloth"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.terrain", "DisableUnityTerrain"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.particlesystem", "DisableUnityParticleSystem"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.modules.ai", "DisableUnityAI"),
			DefineSymbolAdjustmentEntry.UndefineWithModuleExistence("com.unity.timeline", "DisableUnityTimeline"),
		};

		#region User Configuration

		// TODO: Implement user configuration UI. The UI can be placed in Project Settings window. It should also show Extenity defaults as readonly entries that can't be altered by user. BUT they can be overriden by defining a new user entry that exactly matches the Module+Symbol of an existing Extenity default entry, so that it is possible to change the Operation of that default entry. Though the UI should state that it is not a wise idea to do so. Note that GetCombinedDefineSymbolAdjustmentConfiguration is tested and works alright.
		public static DefineSymbolAdjustmentEntry[] DefineSymbolAdjustmentConfiguration_User => Array.Empty<DefineSymbolAdjustmentEntry>();

		#endregion

		#region Combining Configurations

		public static List<DefineSymbolAdjustmentEntry> GetCombinedDefineSymbolAdjustmentConfiguration()
		{
			// Get Extenity Defaults configuration as the starting list. The user configuration will override this.
			var result = DefineSymbolAdjustmentConfiguration_ExtenityDefaults.ToList();

			// Make sure there are no duplicates in Extenity Defaults configuration.
			EnsureNoDuplicates(result);

			var userDefineSymbols = DefineSymbolAdjustmentConfiguration_User.ToList();

			// Make sure there are no duplicates in user configuration.
			EnsureNoDuplicates(userDefineSymbols);

			// Merge user configuration with Extenity Defaults configuration.
			foreach (var userDefineSymbol in userDefineSymbols)
			{
				// Override the one in Extenity Defaults that exactly matches Module+Symbol pair.
				if (!ModifyMatchingModuleAndSymbolEntry(result, userDefineSymbol))
				{
					// Add if nothing to override.
					result.Add(userDefineSymbol);
				}
			}

			// Filter out the unspecified entries.
			for (var iResult = 0; iResult < result.Count; iResult++)
			{
				if (result[iResult].Operation == DefineSymbolAdjustmentOperation.Unspecified)
				{
					result.RemoveAt(iResult);
					iResult--;
				}
			}

			// Make sure there are no duplicates in the result. That is just one extra safety net.
			EnsureNoDuplicates(result);
			return result;
		}

		private static bool ModifyMatchingModuleAndSymbolEntry(List<DefineSymbolAdjustmentEntry> list, DefineSymbolAdjustmentEntry entry)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].ModuleAndSymbolEquals(entry))
				{
					list[i] = entry;
					return true;
				}
			}
			return false;
		}

		private static void EnsureNoDuplicates(List<DefineSymbolAdjustmentEntry> list)
		{
			var duplicatesExist = list.Duplicates(DefineSymbolAdjustmentEntry.ModuleAndSymbolEqualityComparer.Instance).Any();
			if (duplicatesExist)
			{
				throw new Exception($"There are duplicate Module+Symbol entries in {nameof(DefineSymbolsAdjuster)} configuration");
			}
		}

		#endregion

		#region Adjust Define Symbols For Installed Modules

		[InitializeOnLoadMethod]
		public static void AdjustDefineSymbolsForInstalledModules()
		{
			using (new QuickProfilerStopwatch(Log, nameof(DefineSymbolsAdjuster), 1f))
			{
				var packageManifest = PackageManagerTools.GetPackageManifestContent();
				var configuration = GetCombinedDefineSymbolAdjustmentConfiguration();
				var defineSymbolsOfPlatforms = PlayerSettingsTools.GetAllDefineSymbolsOfAllPlatforms();

				var addCount = 0;
				var removeCount = 0;

				foreach (var configurationEntry in configuration)
				{
					bool symbolShouldExist;
					switch (configurationEntry.Operation)
					{
						case DefineSymbolAdjustmentOperation.Define:
							symbolShouldExist = true;
							break;

						case DefineSymbolAdjustmentOperation.Undefine:
							symbolShouldExist = false;
							break;

						case DefineSymbolAdjustmentOperation.DefineWithModuleExistence:
						{
							var moduleExists = packageManifest.IsPackageDefinedInManifest(configurationEntry.Module);
							symbolShouldExist = moduleExists;
						}
							break;

						case DefineSymbolAdjustmentOperation.UndefineWithModuleExistence:
						{
							var moduleExists = packageManifest.IsPackageDefinedInManifest(configurationEntry.Module);
							symbolShouldExist = !moduleExists;
						}
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					foreach (var defineSymbolsOfPlatform in defineSymbolsOfPlatforms.Values)
					{
						if (symbolShouldExist)
						{
							if (defineSymbolsOfPlatform.AddUnique(configurationEntry.Symbol))
							{
								addCount++;
							}
						}
						else
						{
							if (defineSymbolsOfPlatform.Remove(configurationEntry.Symbol))
							{
								removeCount++;
							}
						}
					}
				}

				if (addCount + removeCount > 0)
				{
					Log.Info($"Added '{addCount}' and removed '{removeCount}' define symbol(s) in project configuration.");
					PlayerSettingsTools.SetAllDefineSymbolsOfAllPlatforms(defineSymbolsOfPlatforms);
					AssetDatabase.SaveAssets();
				}
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(DefineSymbolsAdjuster));

		#endregion
	}

}
