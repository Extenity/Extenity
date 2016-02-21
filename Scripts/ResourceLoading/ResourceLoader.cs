using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using Extenity.Parallel;

namespace Extenity.ResourceLoading
{

	public abstract class ResourceLoader : MonoBehaviour
	{
		#region Initialization

		public static T Create<T>(bool hotReloadingEnabled = false) where T : ResourceLoader
		{
			return Create<T>(null, hotReloadingEnabled);
		}

		public static T Create<T>(string loadAtPath, bool hotReloadingEnabled = false) where T : ResourceLoader
		{
			var resourceLoader = ResourceLoadersContainer.AddComponent<T>();
			resourceLoader.HotReloadingEnabled = hotReloadingEnabled;
			if (!string.IsNullOrEmpty(loadAtPath))
			{
				resourceLoader.LoadAssetAtPath(loadAtPath);
			}
			return resourceLoader;
		}

		protected virtual void Awake()
		{
			AddToList(this);
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroy()
		{
			RemoveFromList(this);
		}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Resource Loaders Container Game Object

		private static GameObject _ResourceLoadersContainer;
		public static GameObject ResourceLoadersContainer
		{
			get
			{
				if (_ResourceLoadersContainer == null)
				{
					_ResourceLoadersContainer = GameObjectTools.CreateOrGetGameObject("_ResourceLoadersContainer");
					_ResourceLoadersContainer.hideFlags = HideFlags.HideAndDontSave;
				}
				return _ResourceLoadersContainer;
			}
		}

		#endregion

		#region Resource Loaders

		public static List<ResourceLoader> ResourceLoaders { get; private set; }

		private static void AddToList(ResourceLoader resourceLoader)
		{
			if (ResourceLoaders.Contains(resourceLoader))
				throw new Exception("Resource loader was already in list.");

			ResourceLoaders.Add(resourceLoader);
		}

		private static void RemoveFromList(ResourceLoader resourceLoader)
		{
			var result = ResourceLoaders.Remove(resourceLoader);
			if (!result)
				throw new Exception("Resource loader was not in list.");
		}

		#endregion

		#region Resource Path

		[SerializeField]
		private string _ResourcePath;
		public string ResourcePath
		{
			get { return _ResourcePath; }
			private set { _ResourcePath = ResourcePath; }
		}

		#endregion

		#region Load Asset At Path

		protected abstract IEnumerator DoLoadAssetAtPath(CoroutineTask task);

		public IEnumerator LoadAssetAtPath(string path)
		{
			return LoadAssetAtPath(null, path);
		}

		public IEnumerator LoadAssetAtPath(CoroutineTask task, string path)
		{
			ResourcePath = path;
			return DoLoadAssetAtPath(task);
		}

		#endregion

		#region Hot Reloading

		public bool HotReloadingEnabled = false;

		#endregion
	}

}
