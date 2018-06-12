using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Transform")]
	[ExecuteInEditMode]
	public class TransformGraphPlotter : MonoBehaviour
	{
		public enum Space { Local, World };
		public enum ScaleSpace { Local, Lossy };

		public Transform Transform;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool PlotPosition = false;
		public bool PlotPositionX = true;
		public bool PlotPositionY = true;
		public bool PlotPositionZ = true;
		public Space PositionSpace = Space.World;
		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph PositionGraph;
		public Channel PositionChannelX;
		public Channel PositionChannelY;
		public Channel PositionChannelZ;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool PlotRotation = false;
		public bool PlotRotationX = true;
		public bool PlotRotationY = true;
		public bool PlotRotationZ = true;
		public Space RotationSpace = Space.World;
		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 360f);
		public Graph RotationGraph;
		public Channel RotationChannelX;
		public Channel RotationChannelY;
		public Channel RotationChannelZ;
		// -----------------------------------------------------
		// Input - Scale
		// -----------------------------------------------------
		public bool PlotScale = false;
		public bool PlotScaleX = true;
		public bool PlotScaleY = true;
		public bool PlotScaleZ = true;
		public ScaleSpace scaleSpace = ScaleSpace.Local; // TODO: Rename
		public ValueAxisRangeConfiguration ScaleRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph ScaleGraph;
		public Channel ScaleChannelX;
		public Channel ScaleChannelY;
		public Channel ScaleChannelZ;
		// -----------------------------------------------------

		protected void Start()
		{
			UpdateGraph();
		}

		public void UpdateGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			UpdatePositionGraph(componentIsActive);
			UpdateRotationGraph(componentIsActive);
			UpdateScaleGraph(componentIsActive);
		}

		private void UpdatePositionGraph(bool componentIsActive)
		{
			// position
			if (PlotPosition && componentIsActive)
			{
				if (PositionGraph == null)
				{
					PositionGraph = new Graph("", gameObject);
				}

				PositionGraph.Title = "Position (" + (PositionSpace == Space.World ? "world" : "local") + ")";
				PositionGraph.SetRangeConfiguration(PositionRange);
			}
			else
			{
				if (PositionGraph != null)
				{
					PositionGraph.Close();
					PositionGraph = null;
				}
			}

			// position x
			if (PlotPosition && PlotPositionX && componentIsActive)
			{
				if (PositionChannelX == null)
				{
					PositionChannelX = new Channel(PositionGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (PositionChannelX != null)
				{
					PositionChannelX.Close();
					PositionChannelX = null;
				}
			}

			// position y
			if (PlotPosition && PlotPositionY && componentIsActive)
			{
				if (PositionChannelY == null)
				{
					PositionChannelY = new Channel(PositionGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (PositionChannelY != null)
				{
					PositionChannelY.Close();
					PositionChannelY = null;
				}
			}

			// position z
			if (PlotPosition && PlotPositionZ && componentIsActive)
			{
				if (PositionChannelZ == null)
				{
					PositionChannelZ = new Channel(PositionGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (PositionChannelZ != null)
				{
					PositionChannelZ.Close();
					PositionChannelZ = null;
				}
			}
		}

		private void UpdateRotationGraph(bool componentIsActive)
		{
			// rotation
			if (PlotRotation && componentIsActive)
			{
				if (RotationGraph == null)
				{
					RotationGraph = new Graph("", gameObject);
				}

				RotationGraph.Title = "Rotation (" + (RotationSpace == Space.World ? "world" : "local") + ")";
				RotationGraph.SetRangeConfiguration(RotationRange);
			}
			else
			{
				if (RotationGraph != null)
				{
					RotationGraph.Close();
					RotationGraph = null;
				}
			}

			// rotation x
			if (PlotRotation && PlotRotationX && componentIsActive)
			{
				if (RotationChannelX == null)
				{
					RotationChannelX = new Channel(RotationGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (RotationChannelX != null)
				{
					RotationChannelX.Close();
					RotationChannelX = null;
				}
			}

			// rotation y
			if (PlotRotation && PlotRotationY && componentIsActive)
			{
				if (RotationChannelY == null)
				{
					RotationChannelY = new Channel(RotationGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (RotationChannelY != null)
				{
					RotationChannelY.Close();
					RotationChannelY = null;
				}
			}

			// rotation z
			if (PlotRotation && PlotRotationZ && componentIsActive)
			{
				if (RotationChannelZ == null)
				{
					RotationChannelZ = new Channel(RotationGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (RotationChannelZ != null)
				{
					RotationChannelZ.Close();
					RotationChannelZ = null;
				}
			}
		}

		private void UpdateScaleGraph(bool componentIsActive)
		{
			// scale
			if (PlotScale && componentIsActive)
			{
				if (ScaleGraph == null)
				{
					ScaleGraph = new Graph("", gameObject);
				}

				ScaleGraph.Title = "Scale (" + (scaleSpace == ScaleSpace.Local ? "local" : "lossy") + ")";
				ScaleGraph.SetRangeConfiguration(ScaleRange);
			}
			else
			{
				if (ScaleGraph != null)
				{
					ScaleGraph.Close();
					ScaleGraph = null;
				}
			}

			// scale x
			if (PlotScale && PlotScaleX && componentIsActive)
			{
				if (ScaleChannelX == null)
				{
					ScaleChannelX = new Channel(ScaleGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (ScaleChannelX != null)
				{
					ScaleChannelX.Close();
					ScaleChannelX = null;
				}
			}

			// scale y
			if (PlotScale && PlotScaleY && componentIsActive)
			{
				if (ScaleChannelY == null)
				{
					ScaleChannelY = new Channel(ScaleGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (ScaleChannelY != null)
				{
					ScaleChannelY.Close();
					ScaleChannelY = null;
				}
			}

			// scale z
			if (PlotScale && PlotScaleZ && componentIsActive)
			{
				if (ScaleChannelZ == null)
				{
					ScaleChannelZ = new Channel(ScaleGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (ScaleChannelZ != null)
				{
					ScaleChannelZ.Close();
					ScaleChannelZ = null;
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

			if (PlotPosition)
			{
				var position = PositionSpace == Space.Local ? Transform.localPosition : Transform.position;

				PositionRange.CopyFrom(PositionGraph.Range);

				if (PlotPositionX)
				{
					PositionChannelX.Sample(position.x, time, frame);
				}

				if (PlotPositionY)
				{
					PositionChannelY.Sample(position.y, time, frame);
				}

				if (PlotPositionZ)
				{
					PositionChannelZ.Sample(position.z, time, frame);
				}
			}

			if (PlotRotation)
			{
				var euler = (RotationSpace == Space.Local ? Transform.localRotation : Transform.rotation).eulerAngles;

				RotationRange.CopyFrom(RotationGraph.Range);

				if (PlotRotationX)
				{
					RotationChannelX.Sample(euler.x, time, frame);
				}

				if (PlotRotationY)
				{
					RotationChannelY.Sample(euler.y, time, frame);
				}

				if (PlotRotationZ)
				{
					RotationChannelZ.Sample(euler.z, time, frame);
				}
			}

			if (PlotScale)
			{
				var scale = scaleSpace == ScaleSpace.Local ? Transform.localScale : Transform.lossyScale;

				ScaleRange.CopyFrom(ScaleGraph.Range);

				if (PlotScaleX)
				{
					ScaleChannelX.Sample(scale.x, time, frame);
				}

				if (PlotScaleY)
				{
					ScaleChannelY.Sample(scale.y, time, frame);
				}

				if (PlotScaleZ)
				{
					ScaleChannelZ.Sample(scale.z, time, frame);
				}
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

		private void RemoveGraph()
		{
			if (PositionGraph != null)
			{
				PositionGraph.Close();
				PositionGraph = null;
			}

			if (RotationGraph != null)
			{
				RotationGraph.Close();
				RotationGraph = null;
			}

			if (ScaleGraph != null)
			{
				ScaleGraph.Close();
				ScaleGraph = null;
			}
		}
	}

}