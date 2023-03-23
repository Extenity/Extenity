#if UNITY

#if !DisableUnityPhysics

using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Rigidbody")]
	[ExecuteInEditMode]
	public class RigidbodyGraphPlotter : MonoBehaviour
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

		public Rigidbody Rigidbody;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool PlotPosition = false;
		public bool PlotPositionX = true;
		public bool PlotPositionY = true;
		public bool PlotPositionZ = true;
		public VerticalRange PositionRange = VerticalRange.Adaptive();
		public Graph PositionGraph;
		private Channel[] PositionChannels;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool PlotRotation = false;
		public bool PlotRotationX = true;
		public bool PlotRotationY = true;
		public bool PlotRotationZ = true;
		public VerticalRange RotationRange = VerticalRange.Fixed(0f, 360f);
		public Graph RotationGraph;
		private Channel[] RotationChannels;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool PlotVelocity = false;
		public bool PlotVelocityX = true;
		public bool PlotVelocityY = true;
		public bool PlotVelocityZ = true;
		public VerticalRange VelocityRange = VerticalRange.Adaptive();
		public Graph VelocityGraph;
		private Channel[] VelocityChannels;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool PlotAngularVelocity = false;
		public bool PlotAngularVelocityX = true;
		public bool PlotAngularVelocityY = true;
		public bool PlotAngularVelocityZ = true;
		public VerticalRange AngularVelocityRange = VerticalRange.Adaptive();
		public Graph AngularVelocityGraph;
		private Channel[] AngularVelocityChannels;
		// -----------------------------------------------------

		public void SetupGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			Graph.SetupGraphWithXYZChannels(PlotPosition && componentIsActive, ref PositionGraph, "Position", gameObject, PositionRange, ref PositionChannels, PlotPositionX, PlotPositionY, PlotPositionZ);
			Graph.SetupGraphWithXYZChannels(PlotRotation && componentIsActive, ref RotationGraph, "Rotation", gameObject, RotationRange, ref RotationChannels, PlotRotationX, PlotRotationY, PlotRotationZ);
			Graph.SetupGraphWithXYZChannels(PlotVelocity && componentIsActive, ref VelocityGraph, "Velocity", gameObject, VelocityRange, ref VelocityChannels, PlotVelocityX, PlotVelocityY, PlotVelocityZ);
			Graph.SetupGraphWithXYZChannels(PlotAngularVelocity && componentIsActive, ref AngularVelocityGraph, "Angular Velocity", gameObject, AngularVelocityRange, ref AngularVelocityChannels, PlotAngularVelocityX, PlotAngularVelocityY, PlotAngularVelocityZ);
		}

		#endregion

		#region Sample

		public void Sample()
		{
			if (!Application.isPlaying)
				return;

			if (!Rigidbody)
			{
				Log.WarningWithContext(this, nameof(RigidbodyGraphPlotter) + " requires " + nameof(Rigidbody) + " component.");
				return;
			}

			var time = Time.time;

			if (PlotPosition)
			{
				var position = Rigidbody.position;

				PositionRange = PositionGraph.Range;

				if (PlotPositionX)
					PositionChannels[0].Sample(position.x, time);
				if (PlotPositionY)
					PositionChannels[1].Sample(position.y, time);
				if (PlotPositionZ)
					PositionChannels[2].Sample(position.z, time);
			}

			if (PlotRotation)
			{
				var euler = Rigidbody.rotation.eulerAngles;

				RotationRange = RotationGraph.Range;

				if (PlotRotationX)
					RotationChannels[0].Sample(euler.x, time);
				if (PlotRotationY)
					RotationChannels[1].Sample(euler.y, time);
				if (PlotRotationZ)
					RotationChannels[2].Sample(euler.z, time);
			}

			if (PlotVelocity)
			{
				var velocity = Rigidbody.velocity;

				VelocityRange = VelocityGraph.Range;

				if (PlotVelocityX)
					VelocityChannels[0].Sample(velocity.x, time);
				if (PlotVelocityY)
					VelocityChannels[1].Sample(velocity.y, time);
				if (PlotVelocityZ)
					VelocityChannels[2].Sample(velocity.z, time);
			}

			if (PlotAngularVelocity)
			{
				var angularVelocity = Rigidbody.angularVelocity;

				AngularVelocityRange = AngularVelocityGraph.Range;

				if (PlotAngularVelocityX)
					AngularVelocityChannels[0].Sample(angularVelocity.x, time);
				if (PlotAngularVelocityY)
					AngularVelocityChannels[1].Sample(angularVelocity.y, time);
				if (PlotAngularVelocityZ)
					AngularVelocityChannels[2].Sample(angularVelocity.z, time);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(RigidbodyGraphPlotter));

		#endregion
	}

}

#endif

#endif
