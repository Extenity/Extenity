// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MonitorComponents 
{
	[AddComponentMenu("Monitor Components/Monitor AudioSource")]
	[ExecuteInEditMode]
	public class MonitorAudioSource : MonoBehaviour
	{
		// volume
		public bool showVolume = false;
		
		public Monitor monitor_volume;
		private MonitorInput monitorInput_volume;

		// pitch
		public bool showPitch = false;
		
		public ValueAxisMode pitchMode = ValueAxisMode.Expansive;
		public float pitchMin = 0f;
		public float pitchMax = 2f;

		public Monitor monitor_pitch;
		private MonitorInput monitorInput_pitch;

		// isPlaying
		public bool showIsPlaying = false;

		public Monitor monitor_isPlaying;
		private MonitorInput monitorInput_isPlaying;

		private AudioSource audioSource;

		void Awake()
		{
			if (Application.isPlaying && !Application.isEditor)
			{
				Destroy(this);
			}
		}

		void Start()
		{
			UpdateMonitors();

			if (Application.isPlaying)
			{
				audioSource = GetComponent<AudioSource>();

				if (audioSource == null)
				{
					Debug.LogWarning("MonitorAudioSource requires an AudioSource component.", this);
					enabled = false;
				}
			}
		}

		public void UpdateMonitors()
		{
			bool componentIsActive = enabled && gameObject.activeInHierarchy;

			UpdateVolumeMonitor(componentIsActive);
			UpdatePitchMonitor(componentIsActive);
			UpdateIsPlayingMonitor(componentIsActive);
		}

		private void UpdateVolumeMonitor(bool componentIsActive)
		{
			if (showVolume && componentIsActive)
			{
				if (monitor_volume == null)
				{
					monitor_volume = new Monitor("Volume");
					monitor_volume.GameObject = gameObject;
					monitor_volume.Mode = ValueAxisMode.Fixed;
					monitor_volume.Min = 0f;
					monitor_volume.Max = 1f;
				}

				if (monitorInput_volume == null)
				{
					monitorInput_volume = new MonitorInput(monitor_volume, "volume", Colors.red);
				}
			}
			else
			{
				if (monitor_volume != null)
				{
					monitor_volume.Close();
					monitor_volume = null;
				}

				if (monitorInput_volume != null)
				{
					monitorInput_volume.Close();
					monitorInput_volume = null;
				}
			}
		}

		private void UpdatePitchMonitor(bool componentIsActive)
		{
			if (showPitch && componentIsActive)
			{
				if (monitor_pitch == null)
				{
					monitor_pitch = new Monitor("Pitch");
					monitor_pitch.GameObject = gameObject;
				}

				monitor_pitch.Mode = pitchMode;
				monitor_pitch.Min = pitchMin;
				monitor_pitch.Max = pitchMax;

				if (monitorInput_pitch == null)
				{
					monitorInput_pitch = new MonitorInput(monitor_pitch, "pitch", Colors.green);
				}
			}
			else
			{
				if (monitor_pitch != null)
				{
					monitor_pitch.Close();
					monitor_pitch = null;
				}

				if (monitorInput_pitch != null)
				{
					monitorInput_pitch.Close();
					monitorInput_pitch = null;
				}
			}
		}

		private void UpdateIsPlayingMonitor(bool componentIsActive)
		{
			if (showIsPlaying && componentIsActive)
			{
				if (monitor_isPlaying == null)
				{
					monitor_isPlaying = new Monitor("Is playing");
					monitor_isPlaying.GameObject = gameObject;
					monitor_isPlaying.Mode = ValueAxisMode.Fixed;
					monitor_isPlaying.Min = 0f;
					monitor_isPlaying.Max = 1f;
				}

				if (monitorInput_isPlaying == null)
				{
					monitorInput_isPlaying = new MonitorInput(monitor_isPlaying, "isPlaying", Colors.red);
				}
			}
			else
			{
				if (monitor_isPlaying != null)
				{
					monitor_isPlaying.Close();
					monitor_isPlaying = null;
				}

				if (monitorInput_isPlaying != null)
				{
					monitorInput_isPlaying.Close();
					monitorInput_isPlaying = null;
				}
			}
		}

		public void Update()
		{
			if (!Application.isPlaying)
				return;

			if (showVolume)
			{
				monitorInput_volume.Sample(audioSource.volume);
			}

			if (showPitch)
			{
				pitchMin = monitor_pitch.Min;
				pitchMax = monitor_pitch.Max;

				monitorInput_pitch.Sample(audioSource.pitch);
			}

			if (showIsPlaying)
			{
				monitorInput_isPlaying.Sample(audioSource.isPlaying ? 1f : 0f);
			}
		}

		public void OnEnable()
		{
			UpdateMonitors();
		}	

		public void OnDisable()
		{
			UpdateMonitors();
		}

		public void OnDestroy()
		{
			RemoveMonitors();
		}

		public void RemoveMonitors()
		{
			if(monitor_volume != null)
			{
				monitor_volume.Close();
				monitor_volume = null;
			}

			if(monitor_pitch != null)
			{
				monitor_pitch.Close();
				monitor_pitch = null;
			}

			if(monitor_isPlaying != null)
			{
				monitor_isPlaying.Close();
				monitor_isPlaying = null;
			}
		}
	}
}