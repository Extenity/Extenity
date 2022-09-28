#if UNITY
#if ExtenityInstantiator

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
using UnityEngine.Serialization;

namespace Extenity.GameObjectToolbox
{

	[TypeInfoBox("Instantiator is the system that initializes subsystems of the application. The basic usage is you create Instantiator prefabs in the project and put them inside scenes. The Instantiators then initialize all subsystems as the first operation before any other scripts.\n\n" +
	             "The initialization happens in Awake of Instantiator. So the Instantiator script should be configured so that it has the highest priority in Script Execution Order, before all other scripts.")]
	[HideMonoScript]
	public class Instantiator : MonoBehaviour
	{
		public enum InstantiatorOrder
		{
			Order1,
			Order2,
			Order3,
			Order4,
			Order5,
		}

		[FormerlySerializedAs("type")]
		[TitleGroup("Configuration", alignment: TitleAlignments.Centered)]
		[BoxGroup("Configuration/Box", false), PropertySpace(20f)]
		[InfoBox("You can have more than one <i>Instantiator</i> in an application to be able to instantiate subsystems when required in different stages of the application.\n\n" +
		         "If there are multiple instantiators trying to be created, these instantiators will be lined up by <i>Order</i> number. Instantiation will be postponed until every other instantiator that has lesser <i>Order</i> number completes their instantiation first.\n\n" +
		         "Note that there can be no gaps. All instantiators should be ordered consecutively.")]
		[DisableIn(PrefabKind.PrefabInstance)]
		public InstantiatorOrder Order = InstantiatorOrder.Order1;

		[BoxGroup("Configuration/Box"), PropertySpace(20f, 20f)]
		[InfoBox("The optional parent object to be set for instantiated prefabs. The instantiated objects will be put at the top level hierarchy of active scene if no parent is provided.\n\n" +
		         "But note that all <i>Subsystem Objects</i> (and maybe some <i>Volatile Objects</i> and <i>Instant Objects</i>) are directly marked with DontDestroyOnLoad, so they are instantly put to the DontDestroyOnLoad scene the moment they are created.")]
		[DisableIn(PrefabKind.PrefabInstance)]
		[SceneObjectsOnly] // Note that this field should be set from scene, unlike other fields.
		public Transform Parent = default;

		[FormerlySerializedAs("everlastingPrefabs")]
		[TitleGroup("Prefabs", alignment: TitleAlignments.Centered)]
		[BoxGroup("Prefabs/Box", false), PropertySpace(20f)]
		[ListDrawerSettings(Expanded = true)]
		[InfoBox("The prefabs in this list will be instantiated only once throughout the life time of the application, no matter how many times the instantiator is created in different scenes.\n\n" +
		         "The singletons of the application, designed in the form of prefabs, should be instantiated this way.\n\n" +
		         "Instantiation order is ensured, so a prefab of <i>The Order 2 Instantiator</i> will always be instantiated after <i>The Order 1 Instantiator</i>.\n\n" +
		         "Instantiated objects will be marked with DontDestroyOnLoad, since they expected to be singletons of the application and they will be created only once.")]
		[DisableIn(PrefabKind.PrefabInstance)]
		public GameObject[] SubsystemPrefabs;

		[BoxGroup("Prefabs/Box"), PropertySpace(20f)]
		[ListDrawerSettings(Expanded = true)]
		[InfoBox("The prefabs in this list will be instantiated whenever the instantiator is created. Consider instantiating the objects this way if they have a lifetime of only one scene.\n\n" +
		         "An example would be defining an Ingame Instantiator with an ingame UI and a score calculator assigned in its <i>Volatile Prefabs</i>. Put the Ingame Instantiator only into game scenes. So the UI and score calculator would be instantiated every time a game scene is launched, and they will be destroyed while changing the scene.\n\n" +
		         "The instantiation order of multiple instantiators is respected just like <i>Subsystem Prefabs</i>. <i>Volatile Prefabs</i> are always instantiated after <i>Subsystem Prefabs</i> of its own Instantiator.")]
		[InfoBox("CAUTION! Not implemented yet! Will only instantiate once and will not instantiate in consecutive scenes.", InfoMessageType.Error)]
		[DisableIn(PrefabKind.PrefabInstance)]
		public GameObject[] VolatilePrefabs;

		[FormerlySerializedAs("nonlastingPrefabs")]
		[BoxGroup("Prefabs/Box"), PropertySpace(20f, 20f)]
		[ListDrawerSettings(Expanded = true)]
		[InfoBox("The prefabs in this list will be instantiated immediately when the Awake of this instantiator gets called.\n\n" +
		         "<i>Instant Prefabs</i> instantiated before <i>Subsystem Prefabs</i> and <i>Volatile Prefabs</i> of this instantiator, but may or may not be instantiated after other previously <i>Ordered</i> instantiators and their prefabs. So the instantiation order for <i>Instant Prefabs</i> is a chaos. Any system that is instantiated via <i>Instant Prefabs</i> should not depend on/try to access other systems.\n\n" +
		         "Note that the instantiation order of multiple instantiators will NOT be respected. So a prefab of <i>The Order 2 Instantiator</i> may get instantiated before a prefab of <i>The Order 1 Instantiator</i>.\n\n" +
		         "You may want to consider using <i>Volatile Prefabs</i> instead, since it is basically the same but also provides <i>Ordered</i> instantiation.\n\n" +
		         "(Previously known as Nonlasting Prefabs)")]
		[DisableIn(PrefabKind.PrefabInstance)]
		public GameObject[] InstantPrefabs;

		#region Initialization

		public bool IsInstantiated
		{
			get { return Instantiated[(int)Order]; }
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitializeForSkippingDomainReload()
		{
			if (Instantiated != null)
			{
				Instantiated = null;
				WillBeInitialized = null;
			}
		}

		private void Awake()
		{
			if (Instantiated == null)
			{
				Instantiated = new bool[1 + (int)InstantiatorOrder.Order5];
				WillBeInitialized = new bool[1 + (int)InstantiatorOrder.Order5];
			}

			// Initialize instant prefabs first. They are not eligible for 'IsInstantiated' checks and will be instantiated whenever the instantiator is created.
			InstantiateInstantPrefabs();

			if (IsInstantiated)
			{
				DestroyImmediate(gameObject);
				return;
			}

			name = "_" + name;

			if (HasAnyPreviousUnawakenInstantiator(Order))
			{
				WillBeInitialized[(int)Order] = true;
			}
			else
			{
				Initialize();
				InitializeMarkedInstantiators();
			}
		}

		private void Initialize()
		{
#if LoggingEnabled
			using (Log.Indent(gameObject, "Initializing instantiator: " + Order))
#endif
			{
				Instantiated[(int)Order] = true;
				WillBeInitialized[(int)Order] = false;

				InstantiateSubsystemPrefabs();
				InstantiateVolatilePrefabs();

				Destroy(gameObject);
			}
		}

		/// <summary>
		/// CAUTION! Use this with extreme care! It will mess up the ordered instantiation of Instantiator system.
		/// Use Volatile and Instant prefabs if they solve the problem.
		/// </summary>
		public static void ResetOrder(InstantiatorOrder order)
		{
			//DebugAssert.IsTrue(instantiated[(int)order]);
			Instantiated[(int)order] = false;
		}

		#endregion

		#region Mark Instantiators For Ordered Initialization

		private static bool[] Instantiated;
		private static bool[] WillBeInitialized;

		private void InitializeMarkedInstantiators()
		{
			for (int iOrder = (int)Order + 1; iOrder < WillBeInitialized.Length; iOrder++)
			{
				if (WillBeInitialized[iOrder])
				{
					var allInstantiators = FindObjectsOfType<Instantiator>();
					for (int iInstantiator = 0; iInstantiator < allInstantiators.Length; iInstantiator++)
					{
						var instantiator = allInstantiators[iInstantiator];
						if ((int)instantiator.Order == iOrder)
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

		private bool HasAnyPreviousUnawakenInstantiator(InstantiatorOrder order)
		{
			for (int i = 0; i < (int)order; i++)
			{
				if (!Instantiated[i])
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Instantiate

		private void InstantiateSubsystemPrefabs()
		{
			for (int i = 0; i < SubsystemPrefabs.Length; i++)
			{
#if LoggingEnabled
				using (Log.Indent(this, $"Instantiating subsystem '{SubsystemPrefabs[i].name}'"))
#endif
				{
					InternalInstantiate(SubsystemPrefabs[i], true);
				}
			}
		}

		private void InstantiateVolatilePrefabs()
		{
			for (int i = 0; i < VolatilePrefabs.Length; i++)
			{
#if LoggingEnabled
				using (Log.Indent(this, $"Instantiating volatile '{VolatilePrefabs[i].name}'"))
#endif
				{
					InternalInstantiate(VolatilePrefabs[i], false);
				}
			}
		}

		private void InstantiateInstantPrefabs()
		{
			for (int i = 0; i < InstantPrefabs.Length; i++)
			{
#if LoggingEnabled
				using (Log.Indent(this, $"Instantiating instant '{InstantPrefabs[i].name}'"))
#endif
				{
					InternalInstantiate(InstantPrefabs[i], false);
				}
			}
		}

		private GameObject InternalInstantiate(GameObject prefab, bool dontDestroyOnLoad)
		{
			if (!prefab) // Just skip if the prefab is missing.
				return null;

			var instance = Instantiate(prefab) as GameObject;

			// Remove "(Clone)" from the name and add '_' prefix.
			instance.name = "_" + prefab.name;

			// Set parent
			if (Parent != null)
			{
				instance.transform.SetParent(Parent);
			}

			if (dontDestroyOnLoad)
			{
				DontDestroyOnLoad(instance);
			}

			return instance;
		}

		#endregion
	}

}

#endif
#endif
