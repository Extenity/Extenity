using Extenity.Logging;
using UnityEngine;

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

	[Tooltip("Instantiator types will be instantiated sequentally.")]
	public InstantiatorTypes type = InstantiatorTypes.Type1;
	public GameObject[] everlastingPrefabs;
	public GameObject[] nonlastingPrefabs;


	void Awake()
	{
		if (instantiated == null)
		{
			instantiated = new bool[1 + (int)InstantiatorTypes.Type5];
			willBeInitialized = new bool[1 + (int)InstantiatorTypes.Type5];
		}

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
		using (Logger.Indent(gameObject, "Initializing instantiator: " + type))
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
			EverlastingInstantiate(everlastingPrefabs[i]);
		}
		for (int i = 0; i < nonlastingPrefabs.Length; i++)
		{
			NonlastingInstantiate(nonlastingPrefabs[i]);
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
		//instance.transform.SetParent(ParentTransform, true);
		return instance;
	}
}
