#if UNITY

using UnityEngine;

namespace Extenity.DataToolbox
{

	public abstract class ExtenityScriptableObject<TScriptableObject> : ScriptableObject, ISerializationCallbackReceiver
		where TScriptableObject : ExtenityScriptableObject<TScriptableObject>
	{
		#region Version

		[HideInInspector]
		public string Version;

		protected abstract string LatestVersion { get; }

		#endregion

		#region Singleton

		private static TScriptableObject _Instance;

		public static bool GetInstance(out TScriptableObject instance, string loadAtPath)
		{
#if UNITY_EDITOR
			// Check if the file name is still the one that the system requires
			if (_Instance)
			{
				if (!_Instance.IsFileNameValid())
				{
					_Instance = null;
				}
			}
#endif

			if (_Instance == null)
			{
				_Instance = Load(loadAtPath);
			}

			instance = _Instance;
			return instance != null;
		}

		public virtual bool IsFileNameValid()
		{
			return true;
		}

		#endregion

		#region Save / Load

		public static TScriptableObject Create(string path)
		{
			var instance = CreateInstance<TScriptableObject>();
#if UNITY_EDITOR
			// Create asset
			path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
			UnityEditor.AssetDatabase.CreateAsset(instance, path);
			UnityEditor.AssetDatabase.SaveAssets();
#endif
			return instance;
		}

		private static TScriptableObject Load(string path)
		{
			var instance = Resources.Load<TScriptableObject>(path);
			if (instance == null)
			{
				return null;
			}

			var latestVersion = instance.LatestVersion;
			if (instance.Version != latestVersion)
			{
				instance.ApplyMigration(latestVersion);
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(instance);
#endif
			}

			return instance;
		}

		#endregion

		#region Backwards Compatibility / Migration

		protected virtual void ApplyMigration(string targetVersion) { }

		#endregion

		#region Serialization

		protected virtual void OnBeforeSerializeDerived() { }
		protected virtual void OnAfterDeserializeDerived() { }

		public void OnBeforeSerialize()
		{
			// Apply default version.
			if (string.IsNullOrEmpty(Version))
			{
				Version = LatestVersion;
			}

			OnBeforeSerializeDerived();

			var latestVersion = LatestVersion;
			if (Version != latestVersion)
			{
				ApplyMigration(latestVersion);
			}
		}

		public void OnAfterDeserialize()
		{
			OnAfterDeserializeDerived();
		}

		#endregion
	}

}

#endif
