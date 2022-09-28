#if ExtenityAudio

using System;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Extenity.Audio
{

	[Serializable]
	public class VolumeControl
	{
		public AudioMixer AudioMixer;
		public string MixerParameterName;
		public bool UsePlayerPrefsForSavingVolume = true;
		public float DefaultVolume = 1.0f;
		public bool DefaultMuted = false;
		[NonSerialized]
		public bool LogMixerAndVolumeChanges = false;

		public class VolumeEvent : UnityEvent<float> { }
		public class MuteEvent : UnityEvent<bool> { }
		[NonSerialized]
		public readonly VolumeEvent OnVolumeChanged = new VolumeEvent();
		[NonSerialized]
		public readonly MuteEvent OnMuteChanged = new MuteEvent();

		public void ChangeMixerParameterName(string mixerParameterName)
		{
			MixerParameterName = mixerParameterName;
			_MixerParameterNameForVolume = null;
			_MixerParameterNameForMute = null;
		}

		private string _MixerParameterNameForVolume;
		private string MixerParameterNameForVolume
		{
			get
			{
				if (string.IsNullOrEmpty(_MixerParameterNameForVolume))
					_MixerParameterNameForVolume = MixerParameterName + ".Volume";
				return _MixerParameterNameForVolume;
			}
		}

		private string _MixerParameterNameForMute;
		private string MixerParameterNameForMute
		{
			get
			{
				if (string.IsNullOrEmpty(_MixerParameterNameForMute))
					_MixerParameterNameForMute = MixerParameterName + ".Mute";
				return _MixerParameterNameForMute;
			}
		}

		private bool _IsMutedInitialized;
		private bool _IsMuted;
		public bool IsMuted
		{
			get
			{
				if (!_IsMutedInitialized)
				{
					_IsMuted = UsePlayerPrefsForSavingVolume
						? PlayerPrefsTools.GetBool(MixerParameterNameForMute, DefaultMuted)
						: DefaultMuted;
					_IsMutedInitialized = true;
				}
				return _IsMuted;
			}
			set
			{
				if (IsMuted == value)
					return;
				_IsMuted = value;
				if (UsePlayerPrefsForSavingVolume)
					PlayerPrefsTools.SetBool(MixerParameterNameForMute, value);
				if (LogMixerAndVolumeChanges)
				{
					Log.Info($"Mute set to '{value}' for parameter '{MixerParameterName}'.");
				}
				OnMuteChanged.Invoke(value);
				ReassignMixerParameter();
			}
		}

		private bool _IsVolumeInitialized;
		private float _Volume;
		public float Volume
		{
			get
			{
				if (!_IsVolumeInitialized)
				{
					_Volume = UsePlayerPrefsForSavingVolume
						? PlayerPrefs.GetFloat(MixerParameterNameForVolume, DefaultVolume)
						: DefaultVolume;
					_Volume = _Volume.Clamp01();
					_IsVolumeInitialized = true;
				}
				return _Volume;
			}
			set
			{
				if (IsMuted)
					return; // Ignore changes if muted because UI sliders would try to update volume settings even though it is not the intention.
				var valueClamped = value.Clamp01();
				if (Volume.IsAlmostEqual(valueClamped))
					return;
				_Volume = valueClamped;
				if (UsePlayerPrefsForSavingVolume)
					PlayerPrefs.SetFloat(MixerParameterNameForVolume, valueClamped);
				if (LogMixerAndVolumeChanges)
				{
					Log.Info($"Volume set to '{valueClamped:N2} for parameter '{MixerParameterName}''.");
				}
				ReassignMixerParameter();
			}
		}

		public float MuteAppliedVolume
		{
			get
			{
				if (IsMuted)
					return 0f;
				return Volume;
			}
		}

		public void SetVolume(float value)
		{
			Volume = value;
		}

		public void SetMute(bool isMuted)
		{
			IsMuted = isMuted;
		}

		public void SetMuteInverted(bool isMuted)
		{
			IsMuted = !isMuted;
		}

		public float GetActualVolume()
		{
			float value;
			AudioMixer.GetFloat(MixerParameterName, out value);
			return AudioManager.DbToNormalizedRange(value);
		}

		public void ReassignMixerParameter()
		{
			var resultingVolume = IsMuted ? 0f : Volume;

			if (LogMixerAndVolumeChanges)
			{
				Log.Info($"Reassign mixer parameter for '{MixerParameterName}'. Muted: '{IsMuted}'. Volume: '{Volume:N2}'. Result: '{resultingVolume:N2}'.");
			}

			AudioMixer.SetFloat(MixerParameterName, AudioManager.NormalizedToDbRange(resultingVolume));

			OnVolumeChanged.Invoke(resultingVolume);
		}
	}

}

#endif
