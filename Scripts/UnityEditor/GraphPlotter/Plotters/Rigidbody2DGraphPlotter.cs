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

		public Monitor monitor_position;
		private Channel channel_position_x;
		private Channel channel_position_y;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool showRotation = false;
		public bool rotationClamp = true;

		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, 0f, 360f);

		public Monitor monitor_rotation;
		private Channel channel_rotation;
		// -----------------------------------------------------
		// Input - Velocity
		// -----------------------------------------------------
		public bool showVelocity = false;
		public bool showVelocity_x = true;
		public bool showVelocity_y = true;

		public ValueAxisRangeConfiguration VelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Monitor monitor_velocity;
		private Channel channel_velocity_x;
		private Channel channel_velocity_y;
		// -----------------------------------------------------
		// Input - Angular Velocity
		// -----------------------------------------------------
		public bool showAngularVelocity = false;

		public ValueAxisRangeConfiguration AngularVelocityRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Monitor monitor_angularVelocity;
		private Channel channel_angularVelocity;
		// -----------------------------------------------------

		protected void Awake()
		{
			if (Application.isPlaying && !Application.isEditor)
			{
				Destroy(this);
			}
		}

		protected void Start()
		{
			UpdateMonitors();
		}

		public void UpdateMonitors()
		{
			bool componentIsActive = enabled && gameObject.activeInHierarchy;

			UpdatePositionMonitor(componentIsActive);
			UpdateRotationMonitor(componentIsActive);
			UpdateVelocityMonitor(componentIsActive);
			UpdateAngularVelocityMonitor(componentIsActive);
		}

		private void UpdatePositionMonitor(bool componentIsActive)
		{
			// position
			if (showPosition && componentIsActive)
			{
				if (monitor_position == null)
				{
					monitor_position = new Monitor("Position", gameObject);
				}

				monitor_position.SetRangeConfiguration(PositionRange);
			}
			else
			{
				if (monitor_position != null)
				{
					monitor_position.Close();
					monitor_position = null;
				}
			}

			// position x
			if (showPosition && showPosition_x && componentIsActive)
			{
				if (channel_position_x == null)
				{
					channel_position_x = new Channel(monitor_position, "x", PlotColors.Red);
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
					channel_position_y = new Channel(monitor_position, "y", PlotColors.Green);
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

		private void UpdateRotationMonitor(bool componentIsActive)
		{
			// rotation
			if (showRotation && componentIsActive)
			{
				if (monitor_rotation == null)
				{
					monitor_rotation = new Monitor("Rotation", gameObject);
				}

				monitor_rotation.SetRangeConfiguration(RotationRange);
			}
			else
			{
				if (monitor_rotation != null)
				{
					monitor_rotation.Close();
					monitor_rotation = null;
				}
			}

			// rotation 
			if (showRotation)
			{
				if (channel_rotation == null)
				{
					channel_rotation = new Channel(monitor_rotation, "angle", PlotColors.Red);
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

		private void UpdateVelocityMonitor(bool componentIsActive)
		{
			// velocity
			if (showVelocity && componentIsActive)
			{
				if (monitor_velocity == null)
				{
					monitor_velocity = new Monitor("Velocity", gameObject);
				}

				monitor_velocity.SetRangeConfiguration(VelocityRange);
			}
			else
			{
				if (monitor_velocity != null)
				{
					monitor_velocity.Close();
					monitor_velocity = null;
				}
			}

			// velocity x
			if (showVelocity && showVelocity_x && componentIsActive)
			{
				if (channel_velocity_x == null)
				{
					channel_velocity_x = new Channel(monitor_velocity, "x", PlotColors.Red);
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
					channel_velocity_y = new Channel(monitor_velocity, "y", PlotColors.Green);
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

		private void UpdateAngularVelocityMonitor(bool componentIsActive)
		{
			// angularVelocity
			if (showAngularVelocity && componentIsActive)
			{
				if (monitor_angularVelocity == null)
				{
					monitor_angularVelocity = new Monitor("Angular Velocity", gameObject);
				}

				monitor_angularVelocity.SetRangeConfiguration(AngularVelocityRange);
			}
			else
			{
				if (monitor_angularVelocity != null)
				{
					monitor_angularVelocity.Close();
					monitor_angularVelocity = null;
				}
			}

			// angularVelocity x
			if (showAngularVelocity && componentIsActive)
			{
				if (channel_angularVelocity == null)
				{
					channel_angularVelocity = new Channel(monitor_angularVelocity, "angular velocity", PlotColors.Red);
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

				PositionRange.CopyFrom(monitor_position.Range);

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
				RotationRange.CopyFrom(monitor_rotation.Range);

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

				VelocityRange.CopyFrom(monitor_angularVelocity.Range);

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
				AngularVelocityRange.CopyFrom(monitor_angularVelocity.Range);

				if (showAngularVelocity)
				{
					channel_angularVelocity.Sample(Rigidbody2D.angularVelocity, time, frame);
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

		public void RemoveMonitors()
		{
			if (monitor_position != null)
			{
				monitor_position.Close();
				monitor_position = null;
			}

			if (monitor_rotation != null)
			{
				monitor_rotation.Close();
				monitor_rotation = null;
			}

			if (monitor_velocity != null)
			{
				monitor_velocity.Close();
				monitor_velocity = null;
			}

			if (monitor_angularVelocity != null)
			{
				monitor_angularVelocity.Close();
				monitor_angularVelocity = null;
			}
		}
	}

}