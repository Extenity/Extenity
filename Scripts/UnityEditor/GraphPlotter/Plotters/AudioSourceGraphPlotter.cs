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
		public bool PlotVolume = false;
		public ValueAxisRangeConfiguration VolumeRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f);
		public Graph VolumeGraph;
		private Channel VolumeChannel;
		// -----------------------------------------------------
		// Input - Pitch
		// -----------------------------------------------------
		public bool PlotPitch = false;
		public ValueAxisRangeConfiguration PitchRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, 0f, 2f);
		public Graph PitchGraph;
		private Channel PitchChannel;
		// -----------------------------------------------------
		// Input - IsPlaying
		// -----------------------------------------------------
		public bool PlotIsPlaying = false;
		public Graph IsPlayingGraph;
		private Channel IsPlayingChannel;
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
			if (PlotVolume && componentIsActive)
			{
				if (VolumeGraph == null)
				{
					VolumeGraph = new Graph("Volume", gameObject);
					VolumeGraph.SetRangeConfiguration(VolumeRange);
				}

				if (VolumeChannel == null)
				{
					VolumeChannel = new Channel(VolumeGraph, "volume", PlotColors.Red);
				}
			}
			else
			{
				if (VolumeGraph != null)
				{
					VolumeGraph.Close();
					VolumeGraph = null;
				}

				if (VolumeChannel != null)
				{
					VolumeChannel.Close();
					VolumeChannel = null;
				}
			}
		}

		private void UpdatePitchGraph(bool componentIsActive)
		{
			if (PlotPitch && componentIsActive)
			{
				if (PitchGraph == null)
				{
					PitchGraph = new Graph("Pitch", gameObject);
				}

				PitchGraph.SetRangeConfiguration(PitchRange);

				if (PitchChannel == null)
				{
					PitchChannel = new Channel(PitchGraph, "pitch", PlotColors.Green);
				}
			}
			else
			{
				if (PitchGraph != null)
				{
					PitchGraph.Close();
					PitchGraph = null;
				}

				if (PitchChannel != null)
				{
					PitchChannel.Close();
					PitchChannel = null;
				}
			}
		}

		private void UpdateIsPlayingGraph(bool componentIsActive)
		{
			if (PlotIsPlaying && componentIsActive)
			{
				if (IsPlayingGraph == null)
				{
					IsPlayingGraph = new Graph("Is playing", gameObject);
					IsPlayingGraph.SetRangeConfiguration(new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f));
				}

				if (IsPlayingChannel == null)
				{
					IsPlayingChannel = new Channel(IsPlayingGraph, "isPlaying", PlotColors.Red);
				}
			}
			else
			{
				if (IsPlayingGraph != null)
				{
					IsPlayingGraph.Close();
					IsPlayingGraph = null;
				}

				if (IsPlayingChannel != null)
				{
					IsPlayingChannel.Close();
					IsPlayingChannel = null;
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

			if (PlotVolume)
			{
				VolumeRange.CopyFrom(VolumeGraph.Range);
				VolumeChannel.Sample(AudioSource.volume, time, frame);
			}

			if (PlotPitch)
			{
				PitchRange.CopyFrom(PitchGraph.Range);
				PitchChannel.Sample(AudioSource.pitch, time, frame);
			}

			if (PlotIsPlaying)
			{
				IsPlayingChannel.Sample(AudioSource.isPlaying ? 1f : 0f, time, frame);
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