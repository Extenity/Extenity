using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.UnityEditorToolbox.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.SubsystemManagementToolbox
{
	public enum SubsystemType
	{
		Prefab,
		SingletonClass,
	}

	[Serializable]
	public struct SubsystemLevel
	{
		[ListDrawerSettings(AlwaysAddDefaultValue = true, Expanded = true)]
		public SubsystemDefinition[] Subsystems;

		internal void ClearUnusedReferences()
		{
			foreach (var subsystem in Subsystems)
			{
				subsystem.ClearUnusedReferences();
			}
		}
	}

	[Serializable]
	public struct SubsystemDefinition : IConsistencyChecker
	{
		[HorizontalGroup(100f), HideLabel]
		public SubsystemType Type;

		[ShowIf(nameof(Type), SubsystemType.Prefab)]
		[HorizontalGroup, HideLabel]
		[AssetsOnly]
		public GameObject Prefab;

		[ShowIf(nameof(Type), SubsystemType.SingletonClass)]
		[HorizontalGroup, HideLabel]
		[InfoBox("Not implemented yet!", InfoMessageType.Error), ReadOnly]
		public string SingletonType;

		internal void Initialize(bool dontDestroyOnLoad)
		{
			switch (Type)
			{
				case SubsystemType.Prefab:
				{
					var instance = GameObject.Instantiate(Prefab);

					// Remove "(Clone)" from the name and add '_' prefix.
					instance.name = "_" + Prefab.name;

					// Set parent
					// if (parent != null)
					// {
					// 	instance.transform.SetParent(parent);
					// }

					if (dontDestroyOnLoad)
					{
						GameObject.DontDestroyOnLoad(instance);
					}

					return;
				}

				case SubsystemType.SingletonClass:
				{
					throw new NotImplementedException();
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal void ClearUnusedReferences()
		{
			if (Type != SubsystemType.Prefab)
			{
				Prefab = null;
			}

			if (Type != SubsystemType.SingletonClass)
			{
				SingletonType = null;
			}
		}

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
		}

		#endregion
	}

	[HideMonoScript]
	public class SubsystemSettings : ScriptableObject, ISerializationCallbackReceiver
	{
		#region Configuration

		private const string Path = SubsystemConstants.ConfigurationPath;
		private const string CurrentVersion = SubsystemConstants.Version;

		#endregion

		#region Singleton

		private static SubsystemSettings _Instance;

		public static SubsystemSettings Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = LoadOrCreate();
				}

				return _Instance;
			}
		}

		#endregion

		#region Version

		[HideInInspector]
		public string Version = CurrentVersion;

		#endregion

		#region Subsystems

		[TitleGroup("Subsystems", Alignment = TitleAlignments.Centered)]
		[ListDrawerSettings(AlwaysAddDefaultValue = true, Expanded = true)]
		public SubsystemLevel[] SubsystemLevels;

		private void ClearUnusedReferences()
		{
			if (SubsystemLevels != null)
			{
				foreach (var subsystemLevel in SubsystemLevels)
				{
					subsystemLevel.ClearUnusedReferences();
				}
			}
		}

		#endregion

		#region Save / Load

		private static SubsystemSettings CreateNewSettingsFile()
		{
			var settings = CreateInstance<SubsystemSettings>();
			Save(settings);
			return settings;
		}

		internal static void Save(SubsystemSettings settings)
		{
			EditorUtilityTools.SaveUnityAssetFile(Path, settings);
		}

		internal void Save()
		{
			EditorUtilityTools.SaveUnityAssetFile(Path, this);
		}

		private static SubsystemSettings LoadOrCreate()
		{
			SubsystemSettings settings;

			if (!File.Exists(Path))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = EditorUtilityTools.LoadUnityAssetFile<SubsystemSettings>(Path);
			}

			settings.hideFlags = HideFlags.HideAndDontSave;

			if (settings.Version != CurrentVersion)
			{
				ApplyMigration(settings, CurrentVersion);
				Save(settings);
			}

			return settings;
		}

		#endregion

		#region Migration

		private static void ApplyMigration(SubsystemSettings settings, string targetVersion)
		{
			switch (settings.Version)
			{
				// Example
				case "0":
				{
					// Do the migration here.
					// MigrationsToUpdateFromVersion0ToVersion1();

					// Mark the settings with resulting migration.
					settings.Version = "1";
					break;
				}

				default:
					settings.Version = targetVersion;
					return;
			}

			// Apply migration over and over until we reach the target version.
			ApplyMigration(settings, targetVersion);
		}

		#endregion

		#region Serialization

		public void OnBeforeSerialize()
		{
			ClearUnusedReferences();

			if (Version != CurrentVersion)
			{
				ApplyMigration(this, CurrentVersion);
			}
		}

		public void OnAfterDeserialize()
		{
		}

		#endregion
	}
}
