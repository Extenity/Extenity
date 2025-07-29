#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class DestroyOnLoad : MonoBehaviour
	{
		public bool DestroyImmediately = false;

		private void Awake()
		{
			if (DestroyImmediately)
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

#endif
