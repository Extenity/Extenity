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
		public bool PlotPosition = false;
		public bool PlotPositionX = true;
		public bool PlotPositionT = true;
		public bool PlotPositionZ = true;
		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph PositionGraph;
		private Channel PositionChannelX;
		private Channel PositionChannelY;
		private Channel PositionChannelZ;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool PlotRotation = false;
		public bool PlotRotationX = true;
		public bool PlotRotationY = true;
		public bool PlotRotationZ = true;
		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 360f);
		public Graph RotationGraph;
		private Channel RotationChannelX;
		private Channel RotationChannelY;
		private Channel RotationChannelZ;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool PlotVelocity = false;
		public bool PlotVelocityX = true;
		public bool PlotVelocityY = true;
		public bool PlotVelocityZ = true;
		public ValueAxisRangeConfiguration VelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph VelocityGraph;
		private Channel VelocityChannelX;
		private Channel VelocityChannelY;
		private Channel VelocityChannelZ;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool PlotAngularVelocity = false;
		public bool PlotAngularVelocityX = true;
		public bool PlotAngularVelocityY = true;
		public bool PlotAngularVelocityZ = true;
		public ValueAxisRangeConfiguration AngularVelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		public Graph AngularVelocityGraph;
		private Channel AngularVelocityChannelX;
		private Channel AngularVelocityChannelY;
		private Channel AngularVelocityChannelZ;
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
				Channel.SafeClose(ref PositionChannelX);
			}

			// position y
			if (PlotPosition && PlotPositionT && componentIsActive)
			{
				if (PositionChannelY == null)
				{
					PositionChannelY = new Channel(PositionGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				Channel.SafeClose(ref PositionChannelY);
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
				Channel.SafeClose(ref PositionChannelZ);
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
				Channel.SafeClose(ref RotationChannelX);
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
				Channel.SafeClose(ref RotationChannelY);
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
				Channel.SafeClose(ref RotationChannelZ);
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
				Channel.SafeClose(ref VelocityChannelX);
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
				Channel.SafeClose(ref VelocityChannelY);
			}

			// velocity z
			if (PlotVelocity && PlotVelocityZ && componentIsActive)
			{
				if (VelocityChannelZ == null)
				{
					VelocityChannelZ = new Channel(VelocityGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				Channel.SafeClose(ref VelocityChannelZ);
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
			if (PlotAngularVelocity && PlotAngularVelocityX && componentIsActive)
			{
				if (AngularVelocityChannelX == null)
				{
					AngularVelocityChannelX = new Channel(AngularVelocityGraph, "x", PlotColors.Red);
				}
			}
			else
			{
				Channel.SafeClose(ref AngularVelocityChannelX);
			}

			// angularVelocity y
			if (PlotAngularVelocity && PlotAngularVelocityY && componentIsActive)
			{
				if (AngularVelocityChannelY == null)
				{
					AngularVelocityChannelY = new Channel(AngularVelocityGraph, "y", PlotColors.Green);
				}
			}
			else
			{
				Channel.SafeClose(ref AngularVelocityChannelY);
			}

			// angularVelocity z
			if (PlotAngularVelocity && PlotAngularVelocityZ && componentIsActive)
			{
				if (AngularVelocityChannelZ == null)
				{
					AngularVelocityChannelZ = new Channel(AngularVelocityGraph, "z", PlotColors.Blue);
				}
			}
			else
			{
				Channel.SafeClose(ref AngularVelocityChannelZ);
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

			if (PlotPosition)
			{
				var position = Rigidbody.position;

				PositionRange.CopyFrom(PositionGraph.Range);

				if (PlotPositionX)
				{
					PositionChannelX.Sample(position.x, time, frame);
				}

				if (PlotPositionT)
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
				var euler = Rigidbody.rotation.eulerAngles;

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

			if (PlotVelocity)
			{
				var velocity = Rigidbody.velocity;

				VelocityRange.CopyFrom(VelocityGraph.Range);

				if (PlotVelocityX)
				{
					VelocityChannelX.Sample(velocity.x, time, frame);
				}

				if (PlotVelocityY)
				{
					VelocityChannelY.Sample(velocity.y, time, frame);
				}

				if (PlotVelocityZ)
				{
					VelocityChannelZ.Sample(velocity.z, time, frame);
				}
			}

			if (PlotAngularVelocity)
			{
				var angularVelocity = Rigidbody.angularVelocity;

				AngularVelocityRange.CopyFrom(AngularVelocityGraph.Range);

				if (PlotAngularVelocityX)
				{
					AngularVelocityChannelX.Sample(angularVelocity.x, time, frame);
				}

				if (PlotAngularVelocityY)
				{
					AngularVelocityChannelY.Sample(angularVelocity.y, time, frame);
				}

				if (PlotAngularVelocityZ)
				{
					AngularVelocityChannelZ.Sample(angularVelocity.z, time, frame);
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