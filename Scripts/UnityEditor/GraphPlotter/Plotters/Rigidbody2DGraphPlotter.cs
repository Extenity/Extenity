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
		public bool PlotPosition = false;
		public bool PlotPositionX = true;
		public bool PlotPositionY = true;
		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph PositionGraph;
		private Channel PositionChannelX;
		private Channel PositionChannelY;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool PlotRotation = false;
		public bool ClampRotation = true;
		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, 0f, 360f);
		public Graph RotationGraph;
		private Channel RotationChannel;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool PlotVelocity = false;
		public bool PlotVelocityX = true;
		public bool PlotVelocityY = true;
		public ValueAxisRangeConfiguration VelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph VelocityGraph;
		private Channel VelocityChannelX;
		private Channel VelocityChannelY;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool PlotAngularVelocity = false;
		public ValueAxisRangeConfiguration AngularVelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph AngularVelocityGraph;
		private Channel AngularVelocityChannel;
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
			if (PlotPosition && componentIsActive)
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
		}

		private void UpdateRotationGraph(bool componentIsActive)
		{
			// rotation
			if (PlotRotation && componentIsActive)
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
			if (PlotRotation)
			{
				if (RotationChannel == null)
				{
					RotationChannel = new Channel(RotationGraph, "angle", PlotColors.Red);
				}
			}
			else
			{
				if (RotationChannel != null)
				{
					RotationChannel.Close();
					RotationChannel = null;
				}
			}
		}

		private void UpdateVelocityGraph(bool componentIsActive)
		{
			// velocity
			if (PlotVelocity && componentIsActive)
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
			if (PlotVelocity && PlotVelocityX && componentIsActive)
			{
				if (VelocityChannelX == null)
				{
					VelocityChannelX = new Channel(VelocityGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				if (VelocityChannelX != null)
				{
					VelocityChannelX.Close();
					VelocityChannelX = null;
				}
			}

			// velocity y
			if (PlotVelocity && PlotVelocityY && componentIsActive)
			{
				if (VelocityChannelY == null)
				{
					VelocityChannelY = new Channel(VelocityGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				if (VelocityChannelY != null)
				{
					VelocityChannelY.Close();
					VelocityChannelY = null;
				}
			}
		}

		private void UpdateAngularVelocityGraph(bool componentIsActive)
		{
			// angularVelocity
			if (PlotAngularVelocity && componentIsActive)
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
			if (PlotAngularVelocity && componentIsActive)
			{
				if (AngularVelocityChannel == null)
				{
					AngularVelocityChannel = new Channel(AngularVelocityGraph, "angular velocity", PlotColors.Red);
				}
			}
			else
			{
				if (AngularVelocityChannel != null)
				{
					AngularVelocityChannel.Close();
					AngularVelocityChannel = null;
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

			if (PlotPosition)
			{
				var position = Rigidbody2D.position;

				PositionRange.CopyFrom(PositionGraph.Range);

				if (PlotPositionX)
				{
					PositionChannelX.Sample(position.x, time, frame);
				}

				if (PlotPositionY)
				{
					PositionChannelY.Sample(position.y, time, frame);
				}
			}

			if (PlotRotation)
			{
				RotationRange.CopyFrom(RotationGraph.Range);

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

					RotationChannel.Sample(rotation, time, frame);
				}
			}

			if (PlotVelocity)
			{
				var velocity = Rigidbody2D.velocity;

				VelocityRange.CopyFrom(AngularVelocityGraph.Range);

				if (PlotVelocityX)
				{
					VelocityChannelX.Sample(velocity.x, time, frame);
				}

				if (PlotVelocityY)
				{
					VelocityChannelY.Sample(velocity.y, time, frame);
				}
			}

			if (PlotAngularVelocity)
			{
				AngularVelocityRange.CopyFrom(AngularVelocityGraph.Range);

				if (PlotAngularVelocity)
				{
					AngularVelocityChannel.Sample(Rigidbody2D.angularVelocity, time, frame);
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