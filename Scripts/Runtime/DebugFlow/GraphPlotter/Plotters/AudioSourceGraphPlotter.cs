using UnityEngine;

namespace Extenity.DebugFlowTool.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot AudioSource")]
	[ExecuteInEditMode]
	public class AudioSourceGraphPlotter : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			SetupGraph();
		}

		protected void OnEnable()
		{
			SetupGraph();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			Graph.SafeClose(ref VolumeGraph);
			Graph.SafeClose(ref PitchGraph);
			Graph.SafeClose(ref IsPlayingGraph);
		}

		protected void OnDisable()
		{
			SetupGraph();
		}

		#endregion

		#region Update

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

		#endregion

		#region Metadata and Configuration

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

		public void SetupGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			Graph.SetupGraphWithSingleChannel(PlotVolume && componentIsActive, ref VolumeGraph, "Volume", gameObject, VolumeRange, ref VolumeChannel, "volume", PlotColors.Red);
			Graph.SetupGraphWithSingleChannel(PlotPitch && componentIsActive, ref PitchGraph, "Pitch", gameObject, PitchRange, ref PitchChannel, "pitch", PlotColors.Green);
			Graph.SetupGraphWithSingleChannel(PlotIsPlaying && componentIsActive, ref IsPlayingGraph, "Is playing", gameObject, new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 1f), ref IsPlayingChannel, "isPlaying", PlotColors.Red);
		}

		#endregion

		#region Sample

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

		#endregion
	}

}