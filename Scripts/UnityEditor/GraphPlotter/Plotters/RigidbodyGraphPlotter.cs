using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Rigidbody")]
	[ExecuteInEditMode]
	public class RigidbodyGraphPlotter : MonoBehaviour
	{
		public Rigidbody Rigidbody;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;
		public bool showPosition_z = true;

		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph PositionGraph;
		private Channel channel_position_x;
		private Channel channel_position_y;
		private Channel channel_position_z;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool showRotation = false;
		public bool showRotation_x = true;
		public bool showRotation_y = true;
		public bool showRotation_z = true;

		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 360f);

		public Graph RotationGraph;
		private Channel channel_rotation_x;
		private Channel channel_rotation_y;
		private Channel channel_rotation_z;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool showVelocity = false;
		public bool showVelocity_x = true;
		public bool showVelocity_y = true;
		public bool showVelocity_z = true;

		public ValueAxisRangeConfiguration VelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph VelocityGraph;
		private Channel channel_velocity_x;
		private Channel channel_velocity_y;
		private Channel channel_velocity_z;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool showAngularVelocity = false;
		public bool showAngularVelocity_x = true;
		public bool showAngularVelocity_y = true;
		public bool showAngularVelocity_z = true;

		public ValueAxisRangeConfiguration AngularVelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph AngularVelocityGraph;
		private Channel channel_angularVelocity_x;
		private Channel channel_angularVelocity_y;
		private Channel channel_angularVelocity_z;
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
			UpdateVelocityGraph(componentIsActive);
			UpdateAngularVelocityGraph(componentIsActive);
		}

		private void UpdatePositionGraph(bool componentIsActive)
		{
			// position
			if (showPosition && componentIsActive)
			{
				if (PositionGraph == null)
				{
					PositionGraph = new Graph("Position", gameObject);
				}

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

		private void UpdateRotationGraph(bool componentIsActive)
		{
			// rotation
			if (showRotation && componentIsActive)
			{
				if (RotationGraph == null)
				{
					RotationGraph = new Graph("Rotation", gameObject);
				}

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

		private void UpdateVelocityGraph(bool componentIsActive)
		{
			// velocity
			if (showVelocity && componentIsActive)
			{
				if (VelocityGraph == null)
				{
					VelocityGraph = new Graph("Velocity", gameObject);
				}

				VelocityGraph.SetRangeConfiguration(VelocityRange);
			}
			else
			{
				if (VelocityGraph != null)
				{
					VelocityGraph.Close();
					VelocityGraph = null;
				}
			}

			// velocity x
			if (showVelocity && showVelocity_x && componentIsActive)
			{
				if (channel_velocity_x == null)
				{
					channel_velocity_x = new Channel(VelocityGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (channel_velocity_x != null)
				{
					channel_velocity_x.Close();
					channel_velocity_x = null;
				}
			}

			// velocity y
			if (showVelocity && showVelocity_y && componentIsActive)
			{
				if (channel_velocity_y == null)
				{
					channel_velocity_y = new Channel(VelocityGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (channel_velocity_y != null)
				{
					channel_velocity_y.Close();
					channel_velocity_y = null;
				}
			}

			// velocity z
			if (showVelocity && showVelocity_z && componentIsActive)
			{
				if (channel_velocity_z == null)
				{
					channel_velocity_z = new Channel(VelocityGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (channel_velocity_z != null)
				{
					channel_velocity_z.Close();
					channel_velocity_z = null;
				}
			}
		}

		private void UpdateAngularVelocityGraph(bool componentIsActive)
		{
			// angularVelocity
			if (showAngularVelocity && componentIsActive)
			{
				if (AngularVelocityGraph == null)
				{
					AngularVelocityGraph = new Graph("Angular Velocity", gameObject);
				}

				AngularVelocityGraph.SetRangeConfiguration(AngularVelocityRange);
			}
			else
			{
				if (AngularVelocityGraph != null)
				{
					AngularVelocityGraph.Close();
					AngularVelocityGraph = null;
				}
			}

			// angularVelocity x
			if (showAngularVelocity && showAngularVelocity_x && componentIsActive)
			{
				if (channel_angularVelocity_x == null)
				{
					channel_angularVelocity_x = new Channel(AngularVelocityGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (channel_angularVelocity_x != null)
				{
					channel_angularVelocity_x.Close();
					channel_angularVelocity_x = null;
				}
			}

			// angularVelocity y
			if (showAngularVelocity && showAngularVelocity_y && componentIsActive)
			{
				if (channel_angularVelocity_y == null)
				{
					channel_angularVelocity_y = new Channel(AngularVelocityGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (channel_angularVelocity_y != null)
				{
					channel_angularVelocity_y.Close();
					channel_angularVelocity_y = null;
				}
			}

			// angularVelocity z
			if (showAngularVelocity && showAngularVelocity_z && componentIsActive)
			{
				if (channel_angularVelocity_z == null)
				{
					channel_angularVelocity_z = new Channel(AngularVelocityGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				if (channel_angularVelocity_z != null)
				{
					channel_angularVelocity_z.Close();
					channel_angularVelocity_z = null;
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

			if (!Rigidbody)
			{
				Debug.LogWarning(nameof(RigidbodyGraphPlotter) + " requires " + nameof(Rigidbody) + " component.", this);
				return;
			}

			var time = Time.time;
			var frame = Time.frameCount;

			if (showPosition)
			{
				var position = Rigidbody.position;

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
				var euler = Rigidbody.rotation.eulerAngles;

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

			if (showVelocity)
			{
				var velocity = Rigidbody.velocity;

				VelocityRange.CopyFrom(VelocityGraph.Range);

				if (showVelocity_x)
				{
					channel_velocity_x.Sample(velocity.x, time, frame);
				}

				if (showVelocity_y)
				{
					channel_velocity_y.Sample(velocity.y, time, frame);
				}

				if (showVelocity_z)
				{
					channel_velocity_z.Sample(velocity.z, time, frame);
				}
			}

			if (showAngularVelocity)
			{
				var angularVelocity = Rigidbody.angularVelocity;

				AngularVelocityRange.CopyFrom(AngularVelocityGraph.Range);

				if (showAngularVelocity_x)
				{
					channel_angularVelocity_x.Sample(angularVelocity.x, time, frame);
				}

				if (showAngularVelocity_y)
				{
					channel_angularVelocity_y.Sample(angularVelocity.y, time, frame);
				}

				if (showAngularVelocity_z)
				{
					channel_angularVelocity_z.Sample(angularVelocity.z, time, frame);
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

		public void RemoveGraph()
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

			if (VelocityGraph != null)
			{
				VelocityGraph.Close();
				VelocityGraph = null;
			}

			if (AngularVelocityGraph != null)
			{
				AngularVelocityGraph.Close();
				AngularVelocityGraph = null;
			}
		}
	}

}