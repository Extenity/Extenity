using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class InstantiateOnStart : MonoBehaviour
	{
		public GameObject Object;
		public bool DestroySelf = true;

		private void Start()
		{
			Instantiate(Object);

			if (DestroySelf)
			{
				DestroyImmediate(gameObject);
			}
		}
	}

}
