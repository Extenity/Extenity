#if BeyondAudioUsesFMOD

using Extenity.DesignPatternsToolbox;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace Extenity.BeyondAudio
{

	public class AudioManager : SingletonUnity<AudioManager>
	{
		#region Initialization

		protected void Awake()
		{
			InitializeSingleton(true);
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Links

		private static ChannelGroup _MasterChannelGroup;
		private static ChannelGroup MasterChannelGroup
		{
			get
			{
				if (_MasterChannelGroup == null)
				{
					RuntimeManager.LowlevelSystem.getMasterChannelGroup(out _MasterChannelGroup);
				}
				return _MasterChannelGroup;
			}
		}

		#endregion

		#region Time Scale

		public static void ChangeTimeScale(float timeScale)
		{
			MasterChannelGroup.setPitch(timeScale);
		}

		#endregion

		#region Play One Shot

		public static void PlayAtPosition(string eventName, Vector3 position, float volume = 1f, float pitch = 1f)
		{
			var ev = FMODUnity.RuntimeManager.CreateInstance("event:/" + eventName);
			ev.set3DAttributes(RuntimeUtils.To3DAttributes(position));
			ev.setPitch(pitch);
			ev.setVolume(volume);
			ev.start();
			ev.release();
		}

		#endregion
	}

}

#endif
