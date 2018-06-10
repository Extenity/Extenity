using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot AudioSource")]
	[ExecuteInEditMode]
	public class AudioSourceGraphPlotter : MonoBehaviour
	{
		// -----------------------------------------------------
		// Input - Volume
		// -----------------------------------------------------
		public bool showVolume = false;

		public Monitor monitor_volume;
		private Channel channel_volume;
		// -----------------------------------------------------
		// Input - Pitch
		// -----------------------------------------------------
		public bool showPitch = false;

		public ValueAxisMode pitchMode = ValueAxisMode.Expansive;
		public float pitchMin = 0f;
		public float pitchMax = 2f;

		public Monitor monitor_pitch;
		private Channel channel_pitch;
		// -----------------------------------------------------
		// Input - IsPlaying
		// -----------------------------------------------------
		public bool showIsPlaying = false;

		public Monitor monitor_isPlaying;
		private Channel channel_isPlaying;
		// -----------------------------------------------------

		private AudioSource audioSource;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		protected void Awake()
		{
			if (Application.isPlaying && !Application.isEditor)
			{
				Destroy(this);
			}
		}

		protected void Start()
		{
			UpdateMonitors();

			if (Application.isPlaying)
			{
				audioSource = GetComponent<AudioSource>();

				if (audioSource == null)
				{
					Debug.LogWarning(nameof(Rigidbody2DGraphPlotter) + " requires " + nameof(AudioSource) + " component.", this);
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
					monitor_volume = new Monitor("Volume", gameObject);
					monitor_volume.Mode = ValueAxisMode.Fixed;
					monitor_volume.Min = 0f;
					monitor_volume.Max = 1f;
				}

				if (channel_volume == null)
				{
					channel_volume = new Channel(monitor_volume, "volume", PlotColors.Red);
				}
			}
			else
			{
				if (monitor_volume != null)
				{
					monitor_volume.Close();
					monitor_volume = null;
				}

				if (channel_volume != null)
				{
					channel_volume.Close();
					channel_volume = null;
				}
			}
		}

		private void UpdatePitchMonitor(bool componentIsActive)
		{
			if (showPitch && componentIsActive)
			{
				if (monitor_pitch == null)
				{
					monitor_pitch = new Monitor("Pitch", gameObject);
				}

				monitor_pitch.Mode = pitchMode;
				monitor_pitch.Min = pitchMin;
				monitor_pitch.Max = pitchMax;

				if (channel_pitch == null)
				{
					channel_pitch = new Channel(monitor_pitch, "pitch", PlotColors.Green);
				}
			}
			else
			{
				if (monitor_pitch != null)
				{
					monitor_pitch.Close();
					monitor_pitch = null;
				}

				if (channel_pitch != null)
				{
					channel_pitch.Close();
					channel_pitch = null;
				}
			}
		}

		private void UpdateIsPlayingMonitor(bool componentIsActive)
		{
			if (showIsPlaying && componentIsActive)
			{
				if (monitor_isPlaying == null)
				{
					monitor_isPlaying = new Monitor("Is playing", gameObject);
					monitor_isPlaying.Mode = ValueAxisMode.Fixed;
					monitor_isPlaying.Min = 0f;
					monitor_isPlaying.Max = 1f;
				}

				if (channel_isPlaying == null)
				{
					channel_isPlaying = new Channel(monitor_isPlaying, "isPlaying", PlotColors.Red);
				}
			}
			else
			{
				if (monitor_isPlaying != null)
				{
					monitor_isPlaying.Close();
					monitor_isPlaying = null;
				}

				if (channel_isPlaying != null)
				{
					channel_isPlaying.Close();
					channel_isPlaying = null;
				}
			}
		}

		protected void Update()
		{
			if (SampleTime == SampleTime.Update)
			{
				Sample();
			}
		}

		protected void FixedUpdate()
		{
			if (SampleTime == SampleTime.FixedUpdate)
			{
				Sample();
			}
		}

		public void Sample()
		{
			if (!Application.isPlaying)
				return;

			if (showVolume)
			{
				channel_volume.Sample(audioSource.volume);
			}

			if (showPitch)
			{
				pitchMin = monitor_pitch.Min;
				pitchMax = monitor_pitch.Max;

				channel_pitch.Sample(audioSource.pitch);
			}

			if (showIsPlaying)
			{
				channel_isPlaying.Sample(audioSource.isPlaying ? 1f : 0f);
			}
		}

		protected void OnEnable()
		{
			UpdateMonitors();
		}

		protected void OnDisable()
		{
			UpdateMonitors();
		}

		protected void OnDestroy()
		{
			RemoveMonitors();
		}

		public void RemoveMonitors()
		{
			if (monitor_volume != null)
			{
				monitor_volume.Close();
				monitor_volume = null;
			}

			if (monitor_pitch != null)
			{
				monitor_pitch.Close();
				monitor_pitch = null;
			}

			if (monitor_isPlaying != null)
			{
				monitor_isPlaying.Close();
				monitor_isPlaying = null;
			}
		}
	}

}