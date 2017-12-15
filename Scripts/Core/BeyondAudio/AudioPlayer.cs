using UnityEngine;

namespace Extenity.BeyondAudio
{

	public static class AudioPlayer
	{
		#region Time Scale

		//private static bool LogShown = false;

		//public static void ChangeTimeScale(float timeScale)
		//{
		//	if (!LogShown)
		//	{
		//		LogShown = true;
		//		Debug.LogError("Not implemented yet!");
		//	}

		//	//AudioManager.ChangeTimeScale(timeScale);
		//}

		//private float LastSetTimeScale = 1f;

		//void Update()
		//{
		//	if (Time.timeScale != LastSetTimeScale)
		//	{
		//		LastSetTimeScale = Time.timeScale;
		//		ChangeTimeScale(LastSetTimeScale);
		//	}
		//}

		#endregion

		#region Play One Shot

		public static AudioSource Play(string eventName, float volume = 1f, float pitch = 1f)
		{
			return AudioManager.Play(eventName, volume, pitch);
		}

		public static AudioSource PlayAtPosition(string eventName, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
		{
			return AudioManager.PlayAtPosition(eventName, position, volume, pitch, spatialBlend);
		}

		public static AudioSource PlayLooped(string eventName, float volume = 1f, float pitch = 1f)
		{
			return AudioManager.PlayLooped(eventName, volume, pitch);
		}

		#endregion
	}

}
