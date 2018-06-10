using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Rigidbody")]
	[ExecuteInEditMode]
	public class RigidbodyGraphPlotter : MonoBehaviour
	{
		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;
		public bool showPosition_z = true;

		public ValueAxisMode positionMode = ValueAxisMode.Adaptive;
		public float positionMin = float.PositiveInfinity;
		public float positionMax = float.NegativeInfinity;

		public Monitor monitor_position;
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

		public ValueAxisMode rotationMode = ValueAxisMode.Fixed;
		public float rotationMin = 0f;
		public float rotationMax = 360f;

		public Monitor monitor_rotation;
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

		public ValueAxisMode velocityMode = ValueAxisMode.Adaptive;
		public float velocityMin = float.PositiveInfinity;
		public float velocityMax = float.NegativeInfinity;

		public Monitor monitor_velocity;
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

		public ValueAxisMode angularVelocityMode = ValueAxisMode.Adaptive;
		public float angularVelocityMin = float.PositiveInfinity;
		public float angularVelocityMax = float.NegativeInfinity;

		public Monitor monitor_angularVelocity;
		private Channel channel_angularVelocity_x;
		private Channel channel_angularVelocity_y;
		private Channel channel_angularVelocity_z;
		// -----------------------------------------------------

		public SampleTime SampleTime = SampleTime.FixedUpdate;

		private bool missingRigidbodyWarning = false;

		private new Rigidbody rigidbody;

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

			if (Application.isPlaying)
			{
				rigidbody = GetComponent<Rigidbody>();

				if (rigidbody == null)
				{
					Debug.LogWarning(nameof(RigidbodyGraphPlotter) + " requires " + nameof(Rigidbody) + " component.", this);
					enabled = false;
				}
			}
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

				monitor_position.Mode = positionMode;
				monitor_position.Min = positionMin;
				monitor_position.Max = positionMax;
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

			// position z
			if (showPosition && showPosition_z && componentIsActive)
			{
				if (channel_position_z == null)
				{
					channel_position_z = new Channel(monitor_position, "z", PlotColors.Blue);
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
				if (monitor_rotation == null)
				{
					monitor_rotation = new Monitor("Rotation", gameObject);
				}

				monitor_rotation.Mode = rotationMode;
				monitor_rotation.Min = rotationMin;
				monitor_rotation.Max = rotationMax;
			}
			else
			{
				if (monitor_rotation != null)
				{
					monitor_rotation.Close();
					monitor_rotation = null;
				}
			}

			// rotation x
			if (showRotation && showRotation_x && componentIsActive)
			{
				if (channel_rotation_x == null)
				{
					channel_rotation_x = new Channel(monitor_rotation, "x", PlotColors.Red);
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
					channel_rotation_y = new Channel(monitor_rotation, "y", PlotColors.Green);
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
					channel_rotation_z = new Channel(monitor_rotation, "z", PlotColors.Blue);
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

		private void UpdateVelocityMonitor(bool componentIsActive)
		{
			// velocity
			if (showVelocity && componentIsActive)
			{
				if (monitor_velocity == null)
				{
					monitor_velocity = new Monitor("Velocity", gameObject);
				}

				monitor_velocity.Mode = velocityMode;
				monitor_velocity.Min = velocityMin;
				monitor_velocity.Max = velocityMax;
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

			// velocity z
			if (showVelocity && showVelocity_z && componentIsActive)
			{
				if (channel_velocity_z == null)
				{
					channel_velocity_z = new Channel(monitor_velocity, "z", PlotColors.Blue);
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

		private void UpdateAngularVelocityMonitor(bool componentIsActive)
		{
			// angularVelocity
			if (showAngularVelocity && componentIsActive)
			{
				if (monitor_angularVelocity == null)
				{
					monitor_angularVelocity = new Monitor("Angular Velocity", gameObject);
				}

				monitor_angularVelocity.Mode = angularVelocityMode;
				monitor_angularVelocity.Min = angularVelocityMin;
				monitor_angularVelocity.Max = angularVelocityMax;
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
			if (showAngularVelocity && showAngularVelocity_x && componentIsActive)
			{
				if (channel_angularVelocity_x == null)
				{
					channel_angularVelocity_x = new Channel(monitor_angularVelocity, "x", PlotColors.Red);
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
					channel_angularVelocity_y = new Channel(monitor_angularVelocity, "y", PlotColors.Green);
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
					channel_angularVelocity_z = new Channel(monitor_angularVelocity, "z", PlotColors.Blue);
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

			if (rigidbody == null)
			{
				if (!missingRigidbodyWarning)
				{
					Debug.LogWarning(nameof(RigidbodyGraphPlotter) + " requires " + nameof(Rigidbody) + " component.", this);
					missingRigidbodyWarning = true;
				}

				return;
			}

			if (showPosition)
			{
				var position = rigidbody.position;

				positionMin = monitor_position.Min;
				positionMax = monitor_position.Max;

				if (showPosition_x)
				{
					channel_position_x.Sample(position.x);
				}

				if (showPosition_y)
				{
					channel_position_y.Sample(position.y);
				}

				if (showPosition_z)
				{
					channel_position_z.Sample(position.z);
				}
			}

			if (showRotation)
			{
				var euler = rigidbody.rotation.eulerAngles;

				rotationMin = monitor_rotation.Min;
				rotationMax = monitor_rotation.Max;

				if (showRotation_x)
				{
					channel_rotation_x.Sample(euler.x);
				}

				if (showRotation_y)
				{
					channel_rotation_y.Sample(euler.y);
				}

				if (showRotation_z)
				{
					channel_rotation_z.Sample(euler.z);
				}
			}

			if (showVelocity)
			{
				var velocity = rigidbody.velocity;

				velocityMin = monitor_velocity.Min;
				velocityMax = monitor_velocity.Max;

				if (showVelocity_x)
				{
					channel_velocity_x.Sample(velocity.x);
				}

				if (showVelocity_y)
				{
					channel_velocity_y.Sample(velocity.y);
				}

				if (showVelocity_z)
				{
					channel_velocity_z.Sample(velocity.z);
				}
			}

			if (showAngularVelocity)
			{
				var angularVelocity = rigidbody.angularVelocity;

				angularVelocityMin = monitor_angularVelocity.Min;
				angularVelocityMax = monitor_angularVelocity.Max;


				if (showAngularVelocity_x)
				{
					channel_angularVelocity_x.Sample(angularVelocity.x);
				}

				if (showAngularVelocity_y)
				{
					channel_angularVelocity_y.Sample(angularVelocity.y);
				}

				if (showAngularVelocity_z)
				{
					channel_angularVelocity_z.Sample(angularVelocity.z);
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