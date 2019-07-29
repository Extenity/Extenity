//#define LogInstantiatorInEditor
//#define LogInstantiatorInBuilds
#define LogInstantiatorInDebugBuilds

#if (UNITY_EDITOR && LogInstantiatorInEditor) || (!UNITY_EDITOR && LogInstantiatorInBuilds) || (!UNITY_EDITOR && DEBUG && LogInstantiatorInDebugBuilds)
#define LoggingEnabled
#else
#undef LoggingEnabled
#endif

using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class Instantiator : MonoBehaviour
	{
		public enum InstantiatorTypes
		{
			Type1,
			Type2,
			Type3,
			Type4,
			Type5,
		}

		[Header("Configuration")]
		[Tooltip("Instantiator types allows the objects to be instantiated in groups. If there are multiple instantiators in scene, instantiators will be lined up by type number. Instantiation will be postponed until every other type that has lesser type number completes their instantiation first.")]
		[EnableIf(nameof(IsNotInScene))]
		public InstantiatorTypes type = InstantiatorTypes.Type1;
		[Tooltip("The parent object to be set for instantiated objects. This can be unassigned for making the objects instantiated at top level.")]
		[EnableIf(nameof(IsNotInScene))]
		public Transform Parent;

		[Header("Prefabs")]
		[EnableIf(nameof(IsNotInScene))]
		public GameObject[] everlastingPrefabs;
		[EnableIf(nameof(IsNotInScene))]
		public GameObject[] nonlastingPrefabs;

		void Awake()
		{
			if (instantiated == null)
			{
				instantiated = new bool[1 + (int)InstantiatorTypes.Type5];
				willBeInitialized = new bool[1 + (int)InstantiatorTypes.Type5];
			}

			// Initialize nonlasting prefabs first. They are not eligible for 'IsInstantiated' checks and will be instantiated whenever the instantiator created.
			InitializeNonLastingPrefabs();

			if (IsInstantiated)
			{
				DestroyImmediate(gameObject);
				return;
			}

			name = "_" + name;

			if (HasAnyPreviousUnawakenInstantiator(type))
			{
				willBeInitialized[(int)type] = true;
			}
			else
			{
				Initialize();
				InitializeMarkedInstantiators();
			}
		}

		/// <summary>
		/// CAUTION! Use this with care.
		/// </summary>
		public static void ResetType(InstantiatorTypes type)
		{
			//DebugAssert.IsTrue(instantiated[(int)type]);
			instantiated[(int)type] = false;
		}

		private void InitializeMarkedInstantiators()
		{
			for (int iType = (int)type + 1; iType < willBeInitialized.Length; iType++)
			{
				if (willBeInitialized[iType])
				{
					var allInstantiators = FindObjectsOfType<Instantiator>();
					for (int iInstantiator = 0; iInstantiator < allInstantiators.Length; iInstantiator++)
					{
						var instantiator = allInstantiators[iInstantiator];
						if ((int)instantiator.type == iType)
						{
							instantiator.Initialize();
							break;
						}
					}
				}
				else
				{
					break;
				}
			}
		}

		private bool HasAnyPreviousUnawakenInstantiator(InstantiatorTypes type)
		{
			for (int i = 0; i < (int)type; i++)
			{
				if (!instantiated[i])
				{
					return true;
				}
			}
			return false;
		}

		private void Initialize()
		{
#if LoggingEnabled
			using (Log.Indent(gameObject, "Initializing instantiator: " + type))
#endif
			{
				instantiated[(int)type] = true;
				willBeInitialized[(int)type] = false;

				InstantiateLists();

				Destroy(gameObject);
			}
		}

		private static bool[] instantiated;
		private static bool[] willBeInitialized;

		public bool IsInstantiated
		{
			get { return instantiated[(int)type]; }
		}

		private void InstantiateLists()
		{
			for (int i = 0; i < everlastingPrefabs.Length; i++)
			{
#if LoggingEnabled
				using (Log.Indent(this, $"Instantiating everlasting '{everlastingPrefabs[i].name}'"))
#endif
				{
					EverlastingInstantiate(everlastingPrefabs[i]);
				}
			}
		}

		private void InitializeNonLastingPrefabs()
		{
			for (int i = 0; i < nonlastingPrefabs.Length; i++)
			{
#if LoggingEnabled
				using (Log.Indent(this, $"Instantiating nonlasting '{nonlastingPrefabs[i].name}'"))
#endif
				{
					NonlastingInstantiate(nonlastingPrefabs[i]);
				}
			}
		}

		private void EverlastingInstantiate(GameObject prefab)
		{
			var instance = NonlastingInstantiate(prefab);
			DontDestroyOnLoad(instance);
		}

		private GameObject NonlastingInstantiate(GameObject prefab)
		{
			var instance = Instantiate(prefab) as GameObject;

			// Remove "(Clone)" from the name and add '_' prefix.
			instance.name = "_" + prefab.name;

			// Set parent
			if (Parent != null)
			{
				instance.transform.SetParent(Parent);
			}

			return instance;
		}

		private bool IsNotInScene()
		{
			return !gameObject.scene.isLoaded;
		}
	}

}
