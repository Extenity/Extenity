using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Transform")]
	[ExecuteInEditMode]
	public class TransformGraphPlotter : MonoBehaviour
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
			Graph.SafeClose(ref ScaleGraph);
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

		public Transform Transform;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool PlotPosition = false;
		public bool PlotPositionX = true;
		public bool PlotPositionY = true;
		public bool PlotPositionZ = true;
		public CoordinateSystem PositionSpace = CoordinateSystem.World;
		public VerticalRangeConfiguration PositionRange = VerticalRangeConfiguration.CreateAdaptive();
		public Graph PositionGraph;
		public Channel[] PositionChannels;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool PlotRotation = false;
		public bool PlotRotationX = true;
		public bool PlotRotationY = true;
		public bool PlotRotationZ = true;
		public CoordinateSystem RotationSpace = CoordinateSystem.World;
		public VerticalRangeConfiguration RotationRange = VerticalRangeConfiguration.CreateFixed(0f, 360f);
		public Graph RotationGraph;
		public Channel[] RotationChannels;
		// -----------------------------------------------------
		// Input - Scale
		// -----------------------------------------------------
		public bool PlotScale = false;
		public bool PlotScaleX = true;
		public bool PlotScaleY = true;
		public bool PlotScaleZ = true;
		public ScaleCoordinateSystem ScaleSpace = ScaleCoordinateSystem.Local;
		public VerticalRangeConfiguration ScaleRange = VerticalRangeConfiguration.CreateAdaptive();
		public Graph ScaleGraph;
		public Channel[] ScaleChannels;
		// -----------------------------------------------------

		public void SetupGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			Graph.SetupGraphWithXYZChannels(PlotPosition && componentIsActive, ref PositionGraph, "Position (" + (PositionSpace == CoordinateSystem.World ? "world" : "local") + ")", gameObject, PositionRange, ref PositionChannels, PlotPositionX, PlotPositionY, PlotPositionZ);
			Graph.SetupGraphWithXYZChannels(PlotRotation && componentIsActive, ref RotationGraph, "Rotation (" + (RotationSpace == CoordinateSystem.World ? "world" : "local") + ")", gameObject, RotationRange, ref RotationChannels, PlotRotationX, PlotRotationY, PlotRotationZ);
			Graph.SetupGraphWithXYZChannels(PlotScale && componentIsActive, ref ScaleGraph, "Scale (" + (ScaleSpace == ScaleCoordinateSystem.Local ? "local" : "lossy") + ")", gameObject, ScaleRange, ref ScaleChannels, PlotScaleX, PlotScaleY, PlotScaleZ);
		}

		#endregion

		#region Sample

		public void Sample()
		{
			if (!Application.isPlaying)
				return;

			var time = Time.time;

			if (PlotPosition)
			{
				var position = PositionSpace == CoordinateSystem.Local ? Transform.localPosition : Transform.position;

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
				var euler = (RotationSpace == CoordinateSystem.Local ? Transform.localRotation : Transform.rotation).eulerAngles;

				RotationRange = RotationGraph.Range;

				if (PlotRotationX)
					RotationChannels[0].Sample(euler.x, time);
				if (PlotRotationY)
					RotationChannels[1].Sample(euler.y, time);
				if (PlotRotationZ)
					RotationChannels[2].Sample(euler.z, time);
			}

			if (PlotScale)
			{
				var scale = ScaleSpace == ScaleCoordinateSystem.Local ? Transform.localScale : Transform.lossyScale;

				ScaleRange = ScaleGraph.Range;

				if (PlotScaleX)
					ScaleChannels[0].Sample(scale.x, time);
				if (PlotScaleY)
					ScaleChannels[1].Sample(scale.y, time);
				if (PlotScaleZ)
					ScaleChannels[2].Sample(scale.z, time);
			}
		}

		#endregion
	}

}
