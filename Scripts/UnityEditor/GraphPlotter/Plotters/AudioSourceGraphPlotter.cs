using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot AudioSource")]
	[ExecuteInEditMode]
	public class AudioSourceGraphPlotter : MonoBehaviour
	{
		public AudioSource AudioSource;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Volume
		// -----------------------------------------------------
		public bool showVolume = false;

		public ValueAxisRangeConfiguration VolumeRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f);

		public Graph VolumeGraph;
		private Channel channel_volume;
		// -----------------------------------------------------
		// Input - Pitch
		// -----------------------------------------------------
		public bool showPitch = false;

		public ValueAxisRangeConfiguration PitchRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, 0f, 2f);

		public Graph PitchGraph;
		private Channel channel_pitch;
		// -----------------------------------------------------
		// Input - IsPlaying
		// -----------------------------------------------------
		public bool showIsPlaying = false;

		public Graph IsPlayingGraph;
		private Channel channel_isPlaying;
		// -----------------------------------------------------

		protected void Start()
		{
			UpdateGraph();
		}

		public void UpdateGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			UpdateVolumeGraph(componentIsActive);
			UpdatePitchGraph(componentIsActive);
			UpdateIsPlayingGraph(componentIsActive);
		}

		private void UpdateVolumeGraph(bool componentIsActive)
		{
			if (showVolume && componentIsActive)
			{
				if (VolumeGraph == null)
				{
					VolumeGraph = new Graph("Volume", gameObject);
					VolumeGraph.SetRangeConfiguration(VolumeRange);
				}

				if (channel_volume == null)
				{
					channel_volume = new Channel(VolumeGraph, "volume", PlotColors.Red);
				}
			}
			else
			{
				if (VolumeGraph != null)
				{
					VolumeGraph.Close();
					VolumeGraph = null;
				}

				if (channel_volume != null)
				{
					channel_volume.Close();
					channel_volume = null;
				}
			}
		}

		private void UpdatePitchGraph(bool componentIsActive)
		{
			if (showPitch && componentIsActive)
			{
				if (PitchGraph == null)
				{
					PitchGraph = new Graph("Pitch", gameObject);
				}

				PitchGraph.SetRangeConfiguration(PitchRange);

				if (channel_pitch == null)
				{
					channel_pitch = new Channel(PitchGraph, "pitch", PlotColors.Green);
				}
			}
			else
			{
				if (PitchGraph != null)
				{
					PitchGraph.Close();
					PitchGraph = null;
				}

				if (channel_pitch != null)
				{
					channel_pitch.Close();
					channel_pitch = null;
				}
			}
		}

		private void UpdateIsPlayingGraph(bool componentIsActive)
		{
			if (showIsPlaying && componentIsActive)
			{
				if (IsPlayingGraph == null)
				{
					IsPlayingGraph = new Graph("Is playing", gameObject);
					IsPlayingGraph.SetRangeConfiguration(new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f));
				}

				if (channel_isPlaying == null)
				{
					channel_isPlaying = new Channel(IsPlayingGraph, "isPlaying", PlotColors.Red);
				}
			}
			else
			{
				if (IsPlayingGraph != null)
				{
					IsPlayingGraph.Close();
					IsPlayingGraph = null;
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

			if (!AudioSource)
			{
				Debug.LogWarning(nameof(AudioSourceGraphPlotter) + " requires " + nameof(AudioSource) + " component.", this);
				return;
			}

			var time = Time.time;
			var frame = Time.frameCount;

			if (showVolume)
			{
				VolumeRange.CopyFrom(VolumeGraph.Range);
				channel_volume.Sample(AudioSource.volume, time, frame);
			}

			if (showPitch)
			{
				PitchRange.CopyFrom(PitchGraph.Range);
				channel_pitch.Sample(AudioSource.pitch, time, frame);
			}

			if (showIsPlaying)
			{
				channel_isPlaying.Sample(AudioSource.isPlaying ? 1f : 0f, time, frame);
			}
		}

		protected void OnEnable()
		{
			UpdateGraph();
		}

		protected void OnDisable()
		{
			UpdateGraph();
		}

		protected void OnDestroy()
		{
			RemoveGraph();
		}

		public void RemoveGraph()
		{
			if (VolumeGraph != null)
			{
				VolumeGraph.Close();
				VolumeGraph = null;
			}

			if (PitchGraph != null)
			{
				PitchGraph.Close();
				PitchGraph = null;
			}

			if (IsPlayingGraph != null)
			{
				IsPlayingGraph.Close();
				IsPlayingGraph = null;
			}
		}
	}

}