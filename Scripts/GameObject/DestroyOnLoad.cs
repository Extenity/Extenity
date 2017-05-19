using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class DestroyOnLoad : MonoBehaviour
	{
		public bool destroyImmediate = false;

		private void Awake()
		{
			if (destroyImmediate)
			{
				DestroyImmediate(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}

}
