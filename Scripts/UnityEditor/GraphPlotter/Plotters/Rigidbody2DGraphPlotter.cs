using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Rigidbody2D")]
	[ExecuteInEditMode]
	public class Rigidbody2DGraphPlotter : MonoBehaviour
	{
		public Rigidbody2D Rigidbody2D;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;

		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph PositionGraph;
		private Channel channel_position_x;
		private Channel channel_position_y;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool showRotation = false;
		public bool rotationClamp = true;

		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, 0f, 360f);

		public Graph RotationGraph;
		private Channel channel_rotation;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool showVelocity = false;
		public bool showVelocity_x = true;
		public bool showVelocity_y = true;

		public ValueAxisRangeConfiguration VelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph VelocityGraph;
		private Channel channel_velocity_x;
		private Channel channel_velocity_y;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool showAngularVelocity = false;

		public ValueAxisRangeConfiguration AngularVelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Graph AngularVelocityGraph;
		private Channel channel_angularVelocity;
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

			// rotation 
			if (showRotation)
			{
				if (channel_rotation == null)
				{
					channel_rotation = new Channel(RotationGraph, "angle", PlotColors.Red);
				}
			}
			else
			{
				if (channel_rotation != null)
				{
					channel_rotation.Close();
					channel_rotation = null;
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
			if (showAngularVelocity && componentIsActive)
			{
				if (channel_angularVelocity == null)
				{
					channel_angularVelocity = new Channel(AngularVelocityGraph, "angular velocity", PlotColors.Red);
				}
			}
			else
			{
				if (channel_angularVelocity != null)
				{
					channel_angularVelocity.Close();
					channel_angularVelocity = null;
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

			if (Rigidbody2D == null)
			{
				Debug.LogWarning(nameof(Rigidbody2DGraphPlotter) + " requires " + nameof(UnityEngine.Rigidbody2D) + " component.", this);
				return;
			}

			var time = Time.time;
			var frame = Time.frameCount;

			if (showPosition)
			{
				var position = Rigidbody2D.position;

				PositionRange.CopyFrom(PositionGraph.Range);

				if (showPosition_x)
				{
					channel_position_x.Sample(position.x, time, frame);
				}

				if (showPosition_y)
				{
					channel_position_y.Sample(position.y, time, frame);
				}
			}

			if (showRotation)
			{
				RotationRange.CopyFrom(RotationGraph.Range);

				if (showRotation)
				{
					var rotation = Rigidbody2D.rotation;
					if (rotationClamp)
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

					channel_rotation.Sample(rotation, time, frame);
				}
			}

			if (showVelocity)
			{
				var velocity = Rigidbody2D.velocity;

				VelocityRange.CopyFrom(AngularVelocityGraph.Range);

				if (showVelocity_x)
				{
					channel_velocity_x.Sample(velocity.x, time, frame);
				}

				if (showVelocity_y)
				{
					channel_velocity_y.Sample(velocity.y, time, frame);
				}
			}

			if (showAngularVelocity)
			{
				AngularVelocityRange.CopyFrom(AngularVelocityGraph.Range);

				if (showAngularVelocity)
				{
					channel_angularVelocity.Sample(Rigidbody2D.angularVelocity, time, frame);
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