#if UNITY

using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class DeparentAllChildrenAtStart : MonoBehaviour
	{
		public bool DoOnAwake = true;
		public bool WorldPositionStays = true;
		public bool DestroyWhenDone = true;

		protected void Awake()
		{
			if (DoOnAwake)
			{
				Deparent();
			}
		}

		protected void Start()
		{
			if (!DoOnAwake)
			{
				Deparent();
			}
		}

		public void Deparent()
		{
			// Deparent while keeping the order of objects.
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				var child = transform.GetChild(0);
				child.SetParent(null, WorldPositionStays);
			}

			if (DestroyWhenDone)
			{
				Destroy(gameObject);
			}
		}
	}

}

#endif
