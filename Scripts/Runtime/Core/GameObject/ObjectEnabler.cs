#if UNITY

using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class ObjectEnabler : MonoBehaviour
	{
		public GameObject Object;

		public void EnableObject(bool enable)
		{
			Object.SetActive(enable);
		}

		public void EnableObject()
		{
			Object.SetActive(true);
		}

		public void DisableObject()
		{
			Object.SetActive(false);
		}
	}

}

#endif
