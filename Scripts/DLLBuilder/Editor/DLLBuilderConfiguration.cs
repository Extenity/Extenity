using System;
using Extenity.AssetToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public class DLLBuilderConfiguration : ScriptableObject
	{
		#region Configuration - Compiler

		[SerializeField]
		public CompilerConfiguration[] CompilerConfigurations;

		#endregion

		#region Configuration - Distributer

		[SerializeField]
		public DistributerConfiguration Distributer;

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
