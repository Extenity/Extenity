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

		public ValueAxisRangeConfiguration VolumeRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f);

		public Monitor monitor_volume;
		private Channel channel_volume;
		// -----------------------------------------------------
		// Input - Pitch
		// -----------------------------------------------------
		public bool showPitch = false;

		public ValueAxisRangeConfiguration PitchRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, 0f, 2f);

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
					monitor_volume.SetRangeConfiguration(VolumeRange);
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

				monitor_pitch.SetRangeConfiguration(PitchRange);

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
					monitor_isPlaying.SetRangeConfiguration(new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f));
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

		protected void LateUpdate()
		{
			if (SampleTime == SampleTime.LateUpdate)
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

			var time = Time.time;
			var frame = Time.frameCount;

			if (showVolume)
			{
				VolumeRange.CopyFrom(monitor_volume.Range);
				channel_volume.Sample(audioSource.volume, time, frame);
			}

			if (showPitch)
			{
				PitchRange.CopyFrom(monitor_pitch.Range);
				channel_pitch.Sample(audioSource.pitch, time, frame);
			}

			if (showIsPlaying)
			{
				channel_isPlaying.Sample(audioSource.isPlaying ? 1f : 0f, time, frame);
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