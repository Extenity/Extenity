using UnityEngine;

namespace Extenity.Particle
{

	public class AutoDestroyParticle : MonoBehaviour
	{
		protected void Start()
		{
			Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
		}
	}

}
