using System;
using UnityEngine;
using Extenity.DebugToolbox;
using System.Collections.Generic;
using Extenity.DataToolbox;

namespace Extenity.ResourceLoadingToolbox
{

	public class AssetBundleManager : MonoBehaviour
	{
		#region Singleton

		/// <summary>
		/// This class is reached via static methods, not via Instance. Instance is used internally.
		/// </summary>
		private static AssetBundleManager _Instance;
		protected static AssetBundleManager Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = FindObjectOfType<AssetBundleManager>();
					if (_Instance == null)
					{
						var go = new GameObject("_AssetBundleManager");
						go.hideFlags = HideFlags.HideAndDontSave;
						DontDestroyOnLoad(go);
						_Instance = go.AddComponent<AssetBundleManager>();
					}
				}
				return _Instance;
			}
		}

		#endregion

		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
#if UNITY_EDITOR

			Debug.Log("################ make sure this is called when pressing Stop in editor");

			// If we are in editor, all asset bundles should be destroyed before pressing Play.
			// If not, don't try to release any resources. They will be destroyed on program
			// termination anyway.
			DestroyAllAssetBundles();

#endif
		}

		#endregion

		#region Loaded Asset Bundles

		public List<LoadedAssetBundle> LoadedAssetBundles = new List<LoadedAssetBundle>();

		#endregion

		#region Load

		public static LoadedAssetBundle LoadFromFile(string path)
		{
			LoadedAssetBundle loadedAssetBundle = null;
			try
			{
				// Load asset bundle from file
				var bundle = AssetBundle.LoadFromFile(path);
				try
				{
					loadedAssetBundle = new LoadedAssetBundle();
					loadedAssetBundle.AssetBundle = bundle;
					loadedAssetBundle.Path = path;
					loadedAssetBundle.AssetNames = bundle.GetAllAssetNames();
					Instance.LoadedAssetBundles.Add(loadedAssetBundle);

					// TODO: register all assets in bundle to ResourceManager
					bundle.GetAllAssetNames().LogList();
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					bundle.Unload(false);
				}
			}
			catch (Exception)
			{
				throw;
			}
			return loadedAssetBundle;
		}

		public static void LoadFromFileAsync()
		{
			throw new NotImplementedException();
		}

		public static void LoadFromMemory()
		{
			throw new NotImplementedException();
		}

		public static void LoadFromMemoryAsync()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Destroy

		public void DestroyAllAssetBundles()
		{
			if (LoadedAssetBundles.IsNullOrEmpty())
				return;

			for (int i = 0; i < LoadedAssetBundles.Count; i++)
			{
				try
				{
					InternalDestroyAssetBundle(LoadedAssetBundles[i]);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
			LoadedAssetBundles.Clear();
		}

		public void DestroyAssetBundle(LoadedAssetBundle loadedAssetBundle)
		{
			try
			{
				if (loadedAssetBundle == null)
					throw new ArgumentNullException(nameof(loadedAssetBundle));

				var result = LoadedAssetBundles.Remove(loadedAssetBundle);
				if (!result)
					throw new Exception("Asset bundle was not loaded");

				InternalDestroyAssetBundle(loadedAssetBundle);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private void InternalDestroyAssetBundle(LoadedAssetBundle loadedAssetBundle)
		{
			if (loadedAssetBundle == null)
				throw new ArgumentNullException(nameof(loadedAssetBundle));

			if (loadedAssetBundle.AssetBundle == null)
				throw new Exception("Asset bundle was not loaded");

			var assetBundle = loadedAssetBundle.AssetBundle; // Cache
			loadedAssetBundle.AssetBundle = null; // Null before calling Destroy
			assetBundle.Unload(false);
			Destroy(assetBundle);
		}

		#endregion
	}

	// TODO: Move
	[Serializable]
	public class LoadedAssetBundle
	{
		public string Path;
		public AssetBundle AssetBundle;
		public string[] AssetNames;

		internal LoadedAssetBundle()
		{
		}
	}

}
