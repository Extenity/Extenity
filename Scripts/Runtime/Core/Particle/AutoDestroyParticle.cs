#if UNITY_5_3_OR_NEWER

#if !DisableUnityParticleSystem

using UnityEngine;

namespace Extenity.ParticleToolbox
{

	public class AutoDestroyParticle : MonoBehaviour
	{
		protected void Start()
		{
			Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
		}
	}

}

#endif

#endif
