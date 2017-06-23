using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.AssetToolbox.Editor;
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
						where configuration != null && configuration.Enabled && (Directory.Exists(configuration.ProjectPath) || !configuration.IgnoreIfNotFound)
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
