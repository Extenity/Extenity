#if UNITY

#if !DisableUnityPhysics2D

using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Rigidbody2D")]
	[ExecuteInEditMode]
	public class Rigidbody2DGraphPlotter : MonoBehaviour
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
			Graph.SafeClose(ref PositionGraph);
			Graph.SafeClose(ref RotationGraph);
			Graph.SafeClose(ref VelocityGraph);
			Graph.SafeClose(ref AngularVelocityGraph);
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

		public Rigidbody2D Rigidbody2D;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool PlotPosition = false;
		public bool PlotPositionX = true;
		public bool PlotPositionY = true;
		public VerticalRange PositionRange = VerticalRange.Adaptive();
		public Graph PositionGraph;
		private Channel[] PositionChannels;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool PlotRotation = false;
		public bool ClampRotation = true;
		public VerticalRange RotationRange = VerticalRange.Fixed(0f, 360f);
		public Graph RotationGraph;
		private Channel RotationChannel;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool PlotVelocity = false;
		public bool PlotVelocityX = true;
		public bool PlotVelocityY = true;
		public VerticalRange VelocityRange = VerticalRange.Adaptive();
		public Graph VelocityGraph;
		private Channel[] VelocityChannels;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool PlotAngularVelocity = false;
		public VerticalRange AngularVelocityRange = VerticalRange.Adaptive();
		public Graph AngularVelocityGraph;
		private Channel AngularVelocityChannel;
		// -----------------------------------------------------

		public void SetupGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			Graph.SetupGraphWithXYChannels(PlotPosition && componentIsActive, ref PositionGraph, "Position", gameObject, PositionRange, ref PositionChannels, PlotPositionX, PlotPositionY);
			Graph.SetupGraphWithSingleChannel(PlotRotation && componentIsActive, ref RotationGraph, "Rotation", gameObject, RotationRange, ref RotationChannel, "angle", PlotColors.Red);
			Graph.SetupGraphWithXYChannels(PlotVelocity && componentIsActive, ref VelocityGraph, "Velocity", gameObject, VelocityRange, ref VelocityChannels, PlotVelocityX, PlotVelocityY);
			Graph.SetupGraphWithSingleChannel(PlotAngularVelocity && componentIsActive, ref AngularVelocityGraph, "Angular Velocity", gameObject, AngularVelocityRange, ref AngularVelocityChannel, "angular velocity", PlotColors.Red);
		}

		#endregion

		#region Sample

		public void Sample()
		{
			if (!Application.isPlaying)
				return;

			if (Rigidbody2D == null)
			{
				Log.WarningWithContext(this, nameof(Rigidbody2DGraphPlotter) + " requires " + nameof(Rigidbody2D) + " component.");
				return;
			}

			var time = Time.time;

			if (PlotPosition)
			{
				var position = Rigidbody2D.position;

				PositionRange = PositionGraph.Range;

				if (PlotPositionX)
					PositionChannels[0].Sample(position.x, time);
				if (PlotPositionY)
					PositionChannels[1].Sample(position.y, time);
			}

			if (PlotRotation)
			{
				RotationRange = RotationGraph.Range;

				if (PlotRotation)
				{
					var rotation = Rigidbody2D.rotation;
					if (ClampRotation)
					{
						if (rotation > 0)
						{
							rotation = rotation % 360f;
						}
						else
						{
							rotation = rotation - 360f * Mathf.FloorToInt(rotation / 360f);
						}
					}

					RotationChannel.Sample(rotation, time);
				}
			}

			if (PlotVelocity)
			{
				var velocity = Rigidbody2D.velocity;

				VelocityRange = AngularVelocityGraph.Range;

				if (PlotVelocityX)
					VelocityChannels[0].Sample(velocity.x, time);
				if (PlotVelocityY)
					VelocityChannels[1].Sample(velocity.y, time);
			}

			if (PlotAngularVelocity)
			{
				AngularVelocityRange = AngularVelocityGraph.Range;

				if (PlotAngularVelocity)
					AngularVelocityChannel.Sample(Rigidbody2D.angularVelocity, time);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Rigidbody2DGraphPlotter));

		#endregion
	}

}

#endif

#endif
