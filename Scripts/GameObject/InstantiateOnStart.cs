using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class InstantiateOnStart : MonoBehaviour
	{
		public GameObject instantiate;
		public bool destroySelf = true;

		void Start()
		{
			Instantiate(instantiate);

			if (destroySelf)
			{
				DestroyImmediate(gameObject);
			}
		}
	}

}
