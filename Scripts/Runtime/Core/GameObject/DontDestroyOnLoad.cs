#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class DontDestroyOnLoad : MonoBehaviour
	{
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}

}

#endif
