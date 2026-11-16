#if UNITY_5_3_OR_NEWER
#if ExtenityInstantiator

//#define LogInstantiatorInEditor
//#define LogInstantiatorInBuilds

#define LogInstantiatorInDebugBuilds

#if (UNITY_EDITOR && LogInstantiatorInEditor) || (!UNITY_EDITOR && LogInstantiatorInBuilds) || (!UNITY_EDITOR && DEBUG && LogInstantiatorInDebugBuilds)
#define LoggingEnabled
#else
#undef LoggingEnabled
#endif

using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.GameObjectToolbox
{

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
		public InstantiatorOrder Order = InstantiatorOrder.Order1;

		public Transform Parent = default;

		[FormerlySerializedAs("everlastingPrefabs")]
		public GameObject[] SubsystemPrefabs;

		public GameObject[] VolatilePrefabs;

		[FormerlySerializedAs("nonlastingPrefabs")]
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
