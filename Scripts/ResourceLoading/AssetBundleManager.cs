using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Extenity.ResourceLoading
{

	public class AssetBundleManager : MonoBehaviour
	{
		#region Singleton

		/// <summary>
		/// This class is reached via static methods, not via Instance. Instance is used internally.
		/// </summary>
		private AssetBundleManager _Instance;
		protected AssetBundleManager Instance
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

		public void LoadFromFile(string path)
		{
			AssetBundle bundle;
			try
			{
				// Load asset bundle from file
				bundle = AssetBundle.LoadFromFile(path);
				try
				{
					// TODO: register all assets in bundle to ResourceManager
					bundle.GetAllAssetNames().LogList();
					var asset = bundle.LoadAsset(bundle.GetAllAssetNames()[0]);
					//var asset = bundle.mainAsset;
					var go = GameObject.Instantiate(asset);
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
		}

		public void LoadFromMemory()
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
					throw new ArgumentNullException("loadedAssetBundle");

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
				throw new ArgumentNullException("loadedAssetBundle");

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

		internal LoadedAssetBundle()
		{
		}
	}

}
