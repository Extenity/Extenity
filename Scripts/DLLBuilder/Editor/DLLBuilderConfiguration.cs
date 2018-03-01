using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.AssetToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public class DLLBuilderConfiguration : ScriptableObject
	{
		#region Configuration

		public RemoteBuilderConfiguration[] RemoteBuilderConfigurations;
		public CollectorConfiguration[] CollectorConfigurations;
		public CompilerConfiguration[] CompilerConfigurations;
		public PackerConfiguration[] PackerConfigurations;
		public DistributerConfiguration[] DistributerConfigurations;

		/// <summary>
		/// Gives a list of Remote Builder Configurations that are enabled.
		/// See also EnabledAndIgnoreFilteredRemoteBuilderConfigurations.
		///</summary>
		public List<RemoteBuilderConfiguration> EnabledRemoteBuilderConfigurations
		{
			get
			{
				if (RemoteBuilderConfigurations == null)
					throw new NullReferenceException("RemoteBuilderConfigurations list is null");
				return (from configuration in RemoteBuilderConfigurations
						where configuration != null && configuration.Enabled
						select configuration).ToList();
			}
		}

		/// <summary>
		/// Gives a list of Remote Builder Configurations that are enabled. 
		/// Does not add the non-existing project paths if they are marked 
		/// as IgnoreIfNotFound. Note that non-existing paths are added
		/// to the list if they are not marked as IgnoreIfNotFound, which
		/// should be handled manually in further operations.
		/// </summary>
		public List<RemoteBuilderConfiguration> EnabledAndIgnoreFilteredRemoteBuilderConfigurations
		{
			get
			{
				if (RemoteBuilderConfigurations == null)
					throw new NullReferenceException("RemoteBuilderConfigurations list is null");
				return (from configuration in RemoteBuilderConfigurations
						where configuration != null && configuration.Enabled && (Directory.Exists(DLLBuilderConfiguration.InsertEnvironmentVariables(configuration.ProjectPath)) || !configuration.IgnoreIfNotFound)
						select configuration).ToList();
			}
		}

		public List<CollectorConfiguration> EnabledCollectorConfigurations
		{
			get
			{
				if (CollectorConfigurations == null)
					throw new NullReferenceException("CollectorConfigurations list is null");
				return (from configuration in CollectorConfigurations
						where configuration != null && configuration.Enabled
						select configuration).ToList();
			}
		}

		public List<CompilerConfiguration> EnabledCompilerConfigurations
		{
			get
			{
				if (CompilerConfigurations == null)
					throw new NullReferenceException("CompilerConfigurations list is null");
				return (from configuration in CompilerConfigurations
						where configuration != null && configuration.Enabled
						select configuration).ToList();
			}
		}

		public List<PackerConfiguration> EnabledPackerConfigurations
		{
			get
			{
				if (PackerConfigurations == null)
					throw new NullReferenceException("PackerConfigurations list is null");
				return (from configuration in PackerConfigurations
						where configuration != null && configuration.Enabled
						select configuration).ToList();
			}
		}

		public List<DistributerConfiguration> EnabledDistributerConfigurations
		{
			get
			{
				if (DistributerConfigurations == null)
					throw new NullReferenceException("DistributerConfigurations list is null");
				return (from configuration in DistributerConfigurations
						where configuration != null && configuration.Enabled
						select configuration).ToList();
			}
		}

		#endregion

		#region Instance

		private static DLLBuilderConfiguration _Instance;
		public static DLLBuilderConfiguration Instance
		{
			get
			{
				if (_Instance == null)
					LoadOrCreateConfiguration();
				return _Instance;
			}
		}

		#endregion

		#region Load or Create Configuration Asset

		public static void LoadOrCreateConfiguration()
		{
			var filter = "t:" + typeof(DLLBuilderConfiguration).Name;
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			var assetIDs = AssetDatabase.FindAssets(filter);
			if (assetIDs.Length == 0)
			{
				Debug.Log(Constants.DLLBuilderName + " configuration asset does not exist. Creating new one.");
				_Instance = AssetTools.CreateAsset<DLLBuilderConfiguration>(Constants.DefaultConfigurationPath);
			}
			else if (assetIDs.Length > 1)
			{
				throw new Exception("There are more than one " + typeof(DLLBuilderConfiguration).Name + " asset in project. Please make sure only one of them exists. Until then, " + Constants.DLLBuilderName + " won't be able to function.");
			}
			else
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(assetIDs[0]);
				Debug.Log(Constants.DLLBuilderName + " configuration asset found at path: " + assetPath);
				_Instance = AssetDatabase.LoadAssetAtPath<DLLBuilderConfiguration>(assetPath);
			}

			// It's a great time to also load environment variables.
			LoadEnvironmentVariables();
		}

		#endregion

		#region Environment Variables

		public const string EnvironmentVariableStart = "$(";
		public const string EnvironmentVariableEnd = ")";
		public static readonly List<KeyValue<string, string>> EnvironmentVariables = new List<KeyValue<string, string>>();

		public static string InsertEnvironmentVariables(string text)
		{
			string result;
			text.ReplaceBetweenAll(
				EnvironmentVariableStart,
				EnvironmentVariableEnd,
				key =>
				{
					Debug.LogFormat("Replacing '{0}' in text '{1}'.", key, text);

					for (int i = 0; i < EnvironmentVariables.Count; i++)
					{
						if (key == EnvironmentVariables[i].Key)
							return EnvironmentVariables[i].Value;
					}
					throw new Exception(_BuildErrorMessage(key));
				},
				false,
				false,
				out result
			);
			return result;
		}

		public static void CheckEnvironmentVariableConsistency(string text, ref List<ConsistencyError> errors)
		{
			// Do a simulation of replace operation to see if we fail to find any keys. 
			// Note that this also handles recursive replacements.
			List<ConsistencyError> errorList = null;
			string dummy;
			text.ReplaceBetweenAll(EnvironmentVariableStart, EnvironmentVariableEnd,
				key =>
				{
					for (int i = 0; i < EnvironmentVariables.Count; i++)
					{
						if (key == EnvironmentVariables[i].Key)
							return EnvironmentVariables[i].Value;
					}
					if (errorList == null)
						errorList = new List<ConsistencyError>();
					errorList.Add(new ConsistencyError(null, _BuildErrorMessage(key), true));
					return null;
				},
				false,
				false,
				out dummy
			);
			if (errorList != null)
				errors.AddRange(errorList);
		}

		private static string _BuildErrorMessage(string key)
		{
			return string.Format("Key '{0}' is not found in environment variables. You may happen to forgot to create an entry for this key in configuration?", key);
		}

		#endregion

		#region Environment Variables - Save/Load

		private static string EnvironmentVariablesPrefsKey
		{
			get
			{
				return "DLLBuilder.EnvironmentVariables." +
					   ApplicationTools.AsciiCompanyName + "." +
					   ApplicationTools.AsciiProductName;
			}
		}

		public static void LoadEnvironmentVariables()
		{
			EnvironmentVariables.Clear();

			var json = EditorPrefs.GetString(EnvironmentVariablesPrefsKey);
			if (string.IsNullOrEmpty(json))
				return;

			var list = JsonConvert.DeserializeObject<List<KeyValue<string, string>>>(json);
			//var list = JsonUtility.FromJson<List<KeyValue<string, string>>>(json); Unity can't serialize KeyValue
			EnvironmentVariables.AddRange(list);
		}

		public static void SaveEnvironmentVariables()
		{
			var json = JsonConvert.SerializeObject(EnvironmentVariables);
			//var json = EditorJsonUtility.ToJson(EnvironmentVariables); Unity can't serialize KeyValue
			EditorPrefs.SetString(EnvironmentVariablesPrefsKey, json);
		}

		#endregion

		#region Menu

		[MenuItem(Constants.MenuItemPrefix + "Select Configuration Asset", priority = 1005)]
		public static void SelectOrCreateConfigurationAsset()
		{
			Selection.activeObject = Instance;
		}

		#endregion
	}

}
