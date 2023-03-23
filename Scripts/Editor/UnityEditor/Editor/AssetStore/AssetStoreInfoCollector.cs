using System;
using System.Collections;
using System.Reflection;
using Extenity.ParallelToolbox.Editor;
using Extenity.ReflectionToolbox;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Extenity.UnityEditorToolbox.UnityPackageManagement.Editor
{

	public static class AssetStoreInfoCollector
	{
		private const int CountOfFetchedPackages = 1000;

		#region Types and Methods

		private static Type AssetStoreClientInternalType => PackageManagerTools.AssetStoreClientInternalType;
		public static object AssetStoreClientInternalInstance => PackageManagerTools.AssetStoreClientInternalInstance;
		public static EventInfo OnListOperationStartEvent => PackageManagerTools.OnListOperationStartEvent;
		public static EventInfo OnListOperationFinishEvent => PackageManagerTools.OnListOperationFinishEvent;

		#endregion

		#region Menu

		[MenuItem(ExtenityMenu.PackageManagerTools + "Fetch all Asset Store packages", priority = ExtenityMenu.PackageManagerToolsPriority + 1)]
		private static void Menu_FetchAllAssetStorePackages()
		{
			FetchAllAssetStorePackages();
		}

		[MenuItem(ExtenityMenu.PackageManagerTools + "Log Asset Store package infos (Needs to be fetched first)", priority = ExtenityMenu.PackageManagerToolsPriority + 2)]
		private static void Menu_ListAssets()
		{
			var fetchedInfos = PackageManagerTools.GrabAssetStorePackageInfosFromPackageManager(true);
			if (fetchedInfos.Count >= CountOfFetchedPackages - 5)
			{
				Debug.LogError($"Seems like there are lots of packages. This system only supports fetching a maximum of '{CountOfFetchedPackages}' packages at once to prevents Unity servers from working too much. It will only fetch the first batch and fetching multiple batches is not implemented yet. You may need to manually fetch the full asset list in Package Manager and then this system will reach and use that full list.");
			}
		}

		#endregion

		#region Fetch All Asset Store Packages

		private static bool IsGettingAssetStoreList;
		private static EventInfo RegisteredOnListOperationStartEventInfo;
		private static EventInfo RegisteredOnListOperationFinishEventInfo;
		private static Delegate RegisteredOnListOperationStartDelegate;
		private static Delegate RegisteredOnListOperationFinishDelegate;

		public static void FetchAllAssetStorePackages()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoFetchAllAssetStorePackages(), null);
		}

		static IEnumerator DoFetchAllAssetStorePackages()
		{
			if (IsGettingAssetStoreList)
				yield break;
			IsGettingAssetStoreList = true;

			// Package Manager window needs to be opened so Unity may parse the details of fetched packages and populate its internal lists.
			UnityEditor.PackageManager.UI.Window.Open("My Assets");

			Log.Info("Fetching list of all Asset Store packages... Unity may become unresponsive.");

			// Register for onListOperationStart
			{
				RegisteredOnListOperationStartEventInfo = OnListOperationStartEvent;
				RegisteredOnListOperationStartDelegate = Delegate.CreateDelegate(OnListOperationStartEvent.EventHandlerType,
				                                                                 null,
				                                                                 typeof(AssetStoreInfoCollector).GetMethod(nameof(OnListOperationStart), BindingFlags.Static | BindingFlags.NonPublic),
				                                                                 true);
				OnListOperationStartEvent.AddEventHandler(AssetStoreClientInternalInstance, RegisteredOnListOperationStartDelegate);
			}

			AssetStoreClientInternalType.GetMethodAsAction<object /*instance*/, int /*offset*/, int /*limit*/, string /*searchText*/, bool /*fetchDetails*/>("List", out var listMethod);
			listMethod.Invoke(AssetStoreClientInternalInstance, 0, CountOfFetchedPackages, "", true);

			yield return null;
		}

		private static void OnListOperationStart()
		{
			try // We should definitely not throw inside an internal Unity callback.
			{
				if (!IsGettingAssetStoreList)
					return;

				// Deregister from onListOperationStart
				{
					RegisteredOnListOperationStartEventInfo.RemoveEventHandler(AssetStoreClientInternalInstance, RegisteredOnListOperationStartDelegate);
					RegisteredOnListOperationStartEventInfo = null;
					RegisteredOnListOperationStartDelegate = null;
				}

				// Register for onListOperationFinish
				{
					RegisteredOnListOperationFinishEventInfo = OnListOperationFinishEvent;
					RegisteredOnListOperationFinishDelegate = Delegate.CreateDelegate(OnListOperationFinishEvent.EventHandlerType,
					                                                                  null,
					                                                                  typeof(AssetStoreInfoCollector).GetMethod(nameof(OnListOperationFinish), BindingFlags.Static | BindingFlags.NonPublic),
					                                                                  true);
					OnListOperationFinishEvent.AddEventHandler(AssetStoreClientInternalInstance, RegisteredOnListOperationFinishDelegate);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private static void OnListOperationFinish()
		{
			try // We should definitely not throw inside an internal Unity callback.
			{
				if (!IsGettingAssetStoreList)
					return;
				IsGettingAssetStoreList = false;

				Log.Info("Asset list fetched. Note that Unity may still require some more time to fetch all package details. Opening My Assets view in Package Manager and scrolling through the full list manually by hand helps Unity to realize and fetch their info.");

				// Deregister from onListOperationFinish
				{
					RegisteredOnListOperationFinishEventInfo.RemoveEventHandler(AssetStoreClientInternalInstance, RegisteredOnListOperationFinishDelegate);
					RegisteredOnListOperationFinishEventInfo = null;
					RegisteredOnListOperationFinishDelegate = null;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(AssetStoreInfoCollector));

		#endregion
	}

}
