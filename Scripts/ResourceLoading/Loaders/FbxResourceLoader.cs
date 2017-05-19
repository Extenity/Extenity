using System;
using System.Collections;
using System.IO;
using Extenity.ImportTunnelingToolbox;
using Extenity.Parallel;

namespace Extenity.ResourceLoading
{

	public class FbxResourceLoader : ResourceLoader
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Import Tunneler

		public string ImportTunnelerIntermediateAssetBundlePath = null;

		private ImportTunneler _ImportTunneler;
		public ImportTunneler ImportTunneler
		{
			get
			{
				if (_ImportTunneler == null)
				{
					_ImportTunneler = gameObject.AddComponent<ImportTunneler>();
				}
				return _ImportTunneler;
			}
		}

		#endregion

		#region Load Asset At Path

		protected override IEnumerator DoLoadAssetAtPath(CoroutineTask task)
		{
#if UNITY_STANDALONE_WIN

			var resourcePath = ResourcePath; // Cache to prevent any modifications
			var intermediateAssetBundlePath = ImportTunnelerIntermediateAssetBundlePath; // Cache to prevent any modifications

			var fileExtension = Path.GetExtension(resourcePath);
			if (fileExtension.ToLowerInvariant() != "fbx")
				throw new Exception("Tried to load a file with extension '" + fileExtension + "' as an FBX file.");

			yield return task.StartNested(ImportTunneler.ConvertToAssetBundle(task, resourcePath, intermediateAssetBundlePath));

			var loadedAssetBundle = AssetBundleManager.LoadFromFile(intermediateAssetBundlePath);

			LoadedAsset = loadedAssetBundle.AssetBundle.LoadAsset(loadedAssetBundle.AssetNames[0]);

#else

			throw new NotImplementedException();

#endif
		}

		#endregion
	}

}
