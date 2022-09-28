#if ExtenityAudio

using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.Audio.Effects
{

	public class ImpactSound : MonoBehaviour
	{
		[Header("Collision")]
		public LayerMask CollidedObjectLayerFilter = ~0; // Everything
		public string Sound;
		public float ImpulseVolumeFactor = 0.001f;
		public float MinimumVolume = 0.5f;
		public float RequiredTimeToPlayNext = 0.1f;

		private float LastPlayTime = -10f;

		protected void OnCollisionEnter(Collision collision)
		{
			var now = Time.time; // Not sure if this should be unscaled time.
			if (now < LastPlayTime + RequiredTimeToPlayNext)
				return; // Ignore.

			if ((CollidedObjectLayerFilter.value & (1 << collision.gameObject.layer)) > 0)
			{
				var impulse = collision.impulse.magnitude;
				if (impulse < 0.01f)
					return; // Ignore.

				var pureVolume = impulse * ImpulseVolumeFactor;
				var volume = (pureVolume * (1f - MinimumVolume) + MinimumVolume).Clamp01();
				var pitch = 1f;
				var selectorPin = pureVolume.Clamp01();

				//Log.Info($"Playing impact sound. Impulse '{impulse:N2}'\t Volume: '{volume:N2}'\t SelectorPin: '{selectorPin:N2}'\t Sound: '{Sound}'");
				LastPlayTime = now;
				AudioManager.PlayAttached(Sound, selectorPin, collision.transform, Vector3.zero, false, volume, pitch, 1f);
			}
		}
	}

}

#endif
