#if !DisableParticleSystem

using UnityEngine;

namespace Extenity.ParticleToolbox
{

	public static class ParticleTools
	{
		public static void Place(this ParticleSystem[] particleSystems, Vector3 position)
		{
			if (particleSystems == null)
				return;

			for (var i = 0; i < particleSystems.Length; i++)
			{
				var particleSystem = particleSystems[i];
				if (particleSystem)
				{
					particleSystem.transform.position = position;
				}
			}
		}

		public static void PlaceAndPlay(this ParticleSystem[] particleSystems, Vector3 position, bool withChildren = true)
		{
			if (particleSystems == null)
				return;

			for (var i = 0; i < particleSystems.Length; i++)
			{
				var particleSystem = particleSystems[i];
				if (particleSystem)
				{
					particleSystem.transform.position = position;
					particleSystem.Play(withChildren);
				}
			}
		}

		public static void Play(this ParticleSystem[] particleSystems, bool withChildren = true)
		{
			if (particleSystems == null)
				return;

			for (var i = 0; i < particleSystems.Length; i++)
			{
				var particleSystem = particleSystems[i];
				if (particleSystem)
					particleSystem.Play(withChildren);
			}
		}

		public static void Stop(this ParticleSystem[] particleSystems, bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
		{
			if (particleSystems == null)
				return;

			for (var i = 0; i < particleSystems.Length; i++)
			{
				var particleSystem = particleSystems[i];
				if (particleSystem)
					particleSystem.Stop(withChildren, stopBehavior);
			}
		}

		public static void Switch(this ParticleSystem[] particleSystems, bool isOn, bool withChildren = true, ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
		{
			if (particleSystems == null)
				return;

			if (isOn)
			{
				for (var i = 0; i < particleSystems.Length; i++)
				{
					var particleSystem = particleSystems[i];
					if (particleSystem)
						particleSystem.Play(withChildren);
				}
			}
			else
			{
				for (var i = 0; i < particleSystems.Length; i++)
				{
					var particleSystem = particleSystems[i];
					if (particleSystem)
						particleSystem.Stop(withChildren, stopBehavior);
				}
			}
		}
	}

}

#endif
