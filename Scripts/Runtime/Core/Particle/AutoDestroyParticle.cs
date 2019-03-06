#if !DisableParticleSystem

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
