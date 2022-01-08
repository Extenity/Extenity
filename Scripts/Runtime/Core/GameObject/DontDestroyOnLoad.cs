#if UNITY

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
