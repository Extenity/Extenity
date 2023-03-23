using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Extenity.DataToolbox;
using Extenity.ParallelToolbox.Editor;
using Extenity.ReflectionToolbox;
using Debug = UnityEngine.Debug;
using Client = UnityEditor.PackageManager.Client;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Extenity.UnityEditorToolbox.UnityPackageManagement.Editor
{

	public static class PackageManagerTools
	{
		#region Types and Methods

		private static Type _AssetStoreClientInternalType;
		public static Type AssetStoreClientInternalType
		{
			get
			{
				if (_AssetStoreClientInternalType == null)
					_AssetStoreClientInternalType = ReflectionTools.GetTypeInAllAssembliesEnsured(new StringFilter(new StringFilterEntry(StringFilterType.EndsWith, "AssetStoreClientInternal", StringComparison.OrdinalIgnoreCase)));
				return _AssetStoreClientInternalType;
			}
		}

		private static object _AssetStoreClientInternalInstance;
		public static object AssetStoreClientInternalInstance
		{
			get
			{
				if (_AssetStoreClientInternalInstance == null)
				{
					var instanceMethod = AssetStoreClientInternalType.GetMethod("get_instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
					_AssetStoreClientInternalInstance = instanceMethod.Invoke(null, null);
				}
				return _AssetStoreClientInternalInstance;
			}
		}

		private static EventInfo _OnListOperationStartEvent;
		public static EventInfo OnListOperationStartEvent
		{
			get
			{
				if (_OnListOperationStartEvent == null)
					_OnListOperationStartEvent = AssetStoreClientInternalType.GetEvent("onListOperationStart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				return _OnListOperationStartEvent;
			}
		}

		private static EventInfo _OnListOperationFinishEvent;
		public static EventInfo OnListOperationFinishEvent
		{
			get
			{
				if (_OnListOperationFinishEvent == null)
					_OnListOperationFinishEvent = AssetStoreClientInternalType.GetEvent("onListOperationFinish", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				return _OnListOperationFinishEvent;
			}
		}

		#endregion

		public static List<FetchedInfo> GrabAssetStorePackageInfosFromPackageManager(bool logVerbose)
		{
			// This was an idea to get the internal information in another way. Keep it here commented out for future needs.
			// var serializedObject = new SerializedObject((UnityEngine.Object)AssetStoreClientInternalInstance);
			// var property = serializedObject.FindProperty("m_SerializedFetchedInfos");

			var result = new List<FetchedInfo>();
			var fieldInfo = AssetStoreClientInternalType.GetField("m_FetchedInfos", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var fetchedInfoDictionary = (IDictionary)fieldInfo.GetValue(AssetStoreClientInternalInstance);
			var fetchedInfoList = fetchedInfoDictionary.Values;
			foreach (var fetchedInfoInternal in fetchedInfoList)
			{
				// fetchedInfoInternal.LogAllFieldsAndProperties();
				var fetchedInfo = FetchedInfo.ConvertFromUnityInternal(fetchedInfoInternal, logVerbose);
				result.Add(fetchedInfo);
			}
			return result;
		}

		public static void SearchAllViaClient()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoSearchAllViaClient(), null);

			IEnumerator DoSearchAllViaClient()
			{
				var request = Client.SearchAll();
				Log.Info("Searching all packages");
				while (request.IsCompleted == false)
				{
					yield return null;
				}
				Log.Info("Listing packages");
				var packageInfos = request.Result;

				if (request.Error != null)
				{
					Debug.LogError(request.Error.message);
				}
				else
				{
					Log.Info($"Package infos ({packageInfos.Length}):");
					foreach (var packageInfo in packageInfos)
					{
						Log.Info($"{packageInfo.name}");
					}
				}
			}
		}

		public static void ListPackagesViaClient()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DoListPackagesViaClient(), null);

			IEnumerator DoListPackagesViaClient()
			{
				var request = Client.List(false, true);
				Log.Info("Requesting package list");
				while (request.IsCompleted == false)
				{
					yield return null;
				}
				Log.Info("Listing packages");
				var collection = request.Result;

				if (collection.error != null)
				{
					Debug.LogError(collection.error.message);
				}
				else
				{
					foreach (var entry in collection)
					{
						Log.Info($"{entry.displayName}");
					}
				}
			}
		}

		public static void ListPackagesViaGetAll()
		{
			var type = typeof(PackageInfo);
			var method = type.GetMethod("GetAll", BindingFlags.Static | BindingFlags.NonPublic);
			var packageInfos = method.Invoke(null, null) as PackageInfo[];

			Log.Info($"Package infos ({packageInfos.Length}):");
			foreach (var packageInfo in packageInfos)
			{
				Log.Info($"{packageInfo.name}");
			}
		}

		#region Log

		internal static readonly Logger Log = new(nameof(PackageManagerTools));

		#endregion
	}

}
