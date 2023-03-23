#if ExtenityAudio

using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
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
		public VerticalRange VolumeRange = VerticalRange.Fixed(0f, 1f);
		public Graph VolumeGraph;
		private Channel VolumeChannel;
		// -----------------------------------------------------
		// Input - Pitch
		// -----------------------------------------------------
		public bool PlotPitch = false;
		public VerticalRange PitchRange = VerticalRange.Expansive(0f, 2f);
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
			Graph.SetupGraphWithSingleChannel(PlotIsPlaying && componentIsActive, ref IsPlayingGraph, "Is playing", gameObject, VerticalRange.Fixed(0f, 1f), ref IsPlayingChannel, "isPlaying", PlotColors.Red);
		}

		#endregion

		#region Sample

		public void Sample()
		{
			if (!Application.isPlaying)
				return;

			if (!AudioSource)
			{
				Log.WarningWithContext(this, nameof(AudioSourceGraphPlotter) + " requires " + nameof(AudioSource) + " component.");
				return;
			}

			var time = Time.time;

			if (PlotVolume)
			{
				VolumeRange = VolumeGraph.Range;
				VolumeChannel.Sample(AudioSource.volume, time);
			}

			if (PlotPitch)
			{
				PitchRange = PitchGraph.Range;
				PitchChannel.Sample(AudioSource.pitch, time);
			}

			if (PlotIsPlaying)
			{
				IsPlayingChannel.Sample(AudioSource.isPlaying ? 1f : 0f, time);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(AudioSourceGraphPlotter));

		#endregion
	}

}

#endif
