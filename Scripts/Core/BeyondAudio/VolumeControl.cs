using System;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Extenity.BeyondAudio
{

	[Serializable]
	public class VolumeControl
	{
		public AudioMixer AudioMixer;
		public string MixerParameterName;
		public bool UsePlayerPrefsForSavingVolume = true;
		public float DefaultVolume = 1.0f;
		public bool DefaultMuted = false;

		public class VolumeEvent : UnityEvent<float> { }
		[NonSerialized]
		public readonly VolumeEvent OnVolumeChanged = new VolumeEvent();

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
				var valueClamped = value.Clamp01();
				if (Volume.IsAlmostEqual(valueClamped))
					return;
				_Volume = valueClamped;
				if (UsePlayerPrefsForSavingVolume)
					PlayerPrefs.SetFloat(MixerParameterNameForVolume, valueClamped);
				ReassignMixerParameter();
			}
		}

		public void SetVolume(float value)
		{
			Volume = value;
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

			//Debug.LogFormat("Reassign mixer parameter for '{0}'. Muted: '{1}'. Volume: '{2}'. Result: '{2}'.", MixerParameterName, IsMuted, Volume.ToString("N2"), resultingVolume.ToString("N2"));

			AudioMixer.SetFloat(MixerParameterName, AudioManager.NormalizedToDbRange(resultingVolume));
			OnVolumeChanged.Invoke(resultingVolume);
		}
	}

}
