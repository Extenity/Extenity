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
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				var child = transform.GetChild(i);
				child.SetParent(null, WorldPositionStays);
			}

			if (DestroyWhenDone)
			{
				Destroy(gameObject);
			}
		}
	}

}
