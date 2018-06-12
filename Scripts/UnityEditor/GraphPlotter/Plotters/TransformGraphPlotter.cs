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
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;
		public bool showPosition_z = true;
		public Space positionSpace = Space.World;
		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph PositionGraph;
		public Channel channel_position_x;
		public Channel channel_position_y;
		public Channel channel_position_z;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool showRotation = false;
		public bool showRotation_x = true;
		public bool showRotation_y = true;
		public bool showRotation_z = true;
		public Space rotationSpace = Space.World;
		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 360f);

		public Graph RotationGraph;
		public Channel channel_rotation_x;
		public Channel channel_rotation_y;
		public Channel channel_rotation_z;
		// -----------------------------------------------------
		// Input - Scale
		// -----------------------------------------------------
		public bool showScale = false;
		public bool showScale_x = true;
		public bool showScale_y = true;
		public bool showScale_z = true;
		public ScaleSpace scaleSpace = ScaleSpace.Local;
		public ValueAxisRangeConfiguration ScaleRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph ScaleGraph;
		public Channel channel_scale_x;
		public Channel channel_scale_y;
		public Channel channel_scale_z;
		// -----------------------------------------------------

		protected void Start()
		{
			UpdateMonitors();
		}

		public void UpdateMonitors()
		{
			bool componentIsActive = enabled && gameObject.activeInHierarchy;

			UpdatePositionMonitor(componentIsActive);
			UpdateRotationMonitor(componentIsActive);
			UpdateScaleMonitor(componentIsActive);
		}

		private void UpdatePositionMonitor(bool componentIsActive)
		{
			// position
			if (showPosition && componentIsActive)
			{
				if (PositionGraph == null)
				{
					PositionGraph = new Graph("", gameObject);
				}

				PositionGraph.Title = "Position (" + (positionSpace == Space.World ? "world" : "local") + ")";
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
			if (showPosition && showPosition_x && componentIsActive)
			{
				if (channel_position_x == null)
				{
					channel_position_x = new Channel(PositionGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (channel_position_x != null)
				{
					channel_position_x.Close();
					channel_position_x = null;
				}
			}

			// position y
			if (showPosition && showPosition_y && componentIsActive)
			{
				if (channel_position_y == null)
				{
					channel_position_y = new Channel(PositionGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (channel_position_y != null)
				{
					channel_position_y.Close();
					channel_position_y = null;
				}
			}

			// position z
			if (showPosition && showPosition_z && componentIsActive)
			{
				if (channel_position_z == null)
				{
					channel_position_z = new Channel(PositionGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (channel_position_z != null)
				{
					channel_position_z.Close();
					channel_position_z = null;
				}
			}
		}

		private void UpdateRotationMonitor(bool componentIsActive)
		{
			// rotation
			if (showRotation && componentIsActive)
			{
				if (RotationGraph == null)
				{
					RotationGraph = new Graph("", gameObject);
				}

				RotationGraph.Title = "Rotation (" + (rotationSpace == Space.World ? "world" : "local") + ")";
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
			if (showRotation && showRotation_x && componentIsActive)
			{
				if (channel_rotation_x == null)
				{
					channel_rotation_x = new Channel(RotationGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (channel_rotation_x != null)
				{
					channel_rotation_x.Close();
					channel_rotation_x = null;
				}
			}

			// rotation y
			if (showRotation && showRotation_y && componentIsActive)
			{
				if (channel_rotation_y == null)
				{
					channel_rotation_y = new Channel(RotationGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (channel_rotation_y != null)
				{
					channel_rotation_y.Close();
					channel_rotation_y = null;
				}
			}

			// rotation z
			if (showRotation && showRotation_z && componentIsActive)
			{
				if (channel_rotation_z == null)
				{
					channel_rotation_z = new Channel(RotationGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (channel_rotation_z != null)
				{
					channel_rotation_z.Close();
					channel_rotation_z = null;
				}
			}
		}

		private void UpdateScaleMonitor(bool componentIsActive)
		{
			// scale
			if (showScale && componentIsActive)
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
			if (showScale && showScale_x && componentIsActive)
			{
				if (channel_scale_x == null)
				{
					channel_scale_x = new Channel(ScaleGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (channel_scale_x != null)
				{
					channel_scale_x.Close();
					channel_scale_x = null;
				}
			}

			// scale y
			if (showScale && showScale_y && componentIsActive)
			{
				if (channel_scale_y == null)
				{
					channel_scale_y = new Channel(ScaleGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (channel_scale_y != null)
				{
					channel_scale_y.Close();
					channel_scale_y = null;
				}
			}

			// scale z
			if (showScale && showScale_z && componentIsActive)
			{
				if (channel_scale_z == null)
				{
					channel_scale_z = new Channel(ScaleGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (channel_scale_z != null)
				{
					channel_scale_z.Close();
					channel_scale_z = null;
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

			if (showPosition)
			{
				var position = positionSpace == Space.Local ? Transform.localPosition : Transform.position;

				PositionRange.CopyFrom(PositionGraph.Range);

				if (showPosition_x)
				{
					channel_position_x.Sample(position.x, time, frame);
				}

				if (showPosition_y)
				{
					channel_position_y.Sample(position.y, time, frame);
				}

				if (showPosition_z)
				{
					channel_position_z.Sample(position.z, time, frame);
				}
			}

			if (showRotation)
			{
				var euler = (rotationSpace == Space.Local ? Transform.localRotation : Transform.rotation).eulerAngles;

				RotationRange.CopyFrom(RotationGraph.Range);

				if (showRotation_x)
				{
					channel_rotation_x.Sample(euler.x, time, frame);
				}

				if (showRotation_y)
				{
					channel_rotation_y.Sample(euler.y, time, frame);
				}

				if (showRotation_z)
				{
					channel_rotation_z.Sample(euler.z, time, frame);
				}
			}

			if (showScale)
			{
				var scale = scaleSpace == ScaleSpace.Local ? Transform.localScale : Transform.lossyScale;

				ScaleRange.CopyFrom(ScaleGraph.Range);

				if (showScale_x)
				{
					channel_scale_x.Sample(scale.x, time, frame);
				}

				if (showScale_y)
				{
					channel_scale_y.Sample(scale.y, time, frame);
				}

				if (showScale_z)
				{
					channel_scale_z.Sample(scale.z, time, frame);
				}
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

		private void RemoveMonitors()
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