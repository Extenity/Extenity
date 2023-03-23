#if UNITY

using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.PhysicsToolbox
{

	public class RigidbodySleepyStart : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			if (AutoSleepOnStart)
			{
				SleepAllRigidbodies();
			}
			if (AutoDestroyOnStart)
			{
				Destroy(gameObject);
			}
		}

		#endregion

		#region Gather

		public GameObject[] FilterOutGameObjects;

		public void GatherAllRigidbodiesInScene(ActiveCheck activeCheck, bool logFilteredOutRigidbodies)
		{
			var rigidbodies = gameObject.scene.FindObjectsOfType<Rigidbody>(activeCheck);
			for (int i = 0; i < rigidbodies.Count; i++)
			{
				for (int iFilter = 0; iFilter < FilterOutGameObjects.Length; iFilter++)
				{
					if (rigidbodies[i].transform.IsChildOf(FilterOutGameObjects[iFilter].transform))
					{
						if (logFilteredOutRigidbodies)
						{
							Log.VerboseWithContext(rigidbodies[i], "Filtered: " + rigidbodies[i].FullGameObjectName());
						}
						rigidbodies.RemoveAt(i);
						i--;
					}
				}
			}
			Rigidbodies = rigidbodies.ToArray();
		}

		#endregion

		#region Sleep

		public bool AutoSleepOnStart = true;
		public bool AutoDestroyOnStart = true;

		public void SleepAllRigidbodies()
		{
			for (var i = 0; i < Rigidbodies.Length; i++)
			{
				Rigidbodies[i].Sleep();
			}
		}

		#endregion

		#region Rigidbodies

		public Rigidbody[] Rigidbodies;

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(RigidbodySleepyStart));

		#endregion
	}

}

#endif
