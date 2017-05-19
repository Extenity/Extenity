using System;
using UnityEngine;
using Extenity.DebugToolbox;
using Extenity.ImportTunneling;
using Extenity.Parallel;

public class ImportTunnelerExample : MonoBehaviour
{
	#region Initialization

	protected void Start()
	{
		Task = CoroutineTask.Create();
		Task.Initialize(ImportTunneler.ConvertToAssetBundle(Task, SourceAssetPath, OutputAssetBundlePath), OnConversionFinished, true);
	}

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

	public ImportTunneler ImportTunneler;

	#endregion

	#region Test Asset

	public string SourceAssetPath;
	public string OutputAssetBundlePath;

	#endregion

	#region Convert

	private CoroutineTask Task;

	private void OnConversionFinished(bool manuallyStopped)
	{
		if (manuallyStopped)
			return;

		Debug.LogFormat("Conversion finished. Loading asset bundle at '{0}'", OutputAssetBundlePath);

		var bundle = AssetBundle.LoadFromFile(OutputAssetBundlePath);
		Debug.Log("Bundle : " + bundle);
		try
		{
			bundle.GetAllAssetNames().LogList();
			var asset = bundle.LoadAsset(bundle.GetAllAssetNames()[0]);
			//var asset = bundle.mainAsset;
			GameObject.Instantiate(asset);
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

	#endregion
}
