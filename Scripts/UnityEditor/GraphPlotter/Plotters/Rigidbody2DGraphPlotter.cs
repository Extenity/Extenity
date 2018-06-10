using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Rigidbody2D")]
	[ExecuteInEditMode]
	public class Rigidbody2DGraphPlotter : MonoBehaviour
	{
		// position
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;

		public ValueAxisMode positionMode = ValueAxisMode.Adaptive;
		public float positionMin = float.PositiveInfinity;
		public float positionMax = float.NegativeInfinity;

		public Monitor monitor_position;
		private Channel channel_position_x;
		private Channel channel_position_y;

		// rotation
		public bool showRotation = false;
		public bool rotationClamp = true;

		public ValueAxisMode rotationMode = ValueAxisMode.Expansive;
		public float rotationMin = 0f;
		public float rotationMax = 360f;

		public Monitor monitor_rotation;
		private Channel channel_rotation;

		// velocity
		public bool showVelocity = false;
		public bool showVelocity_x = true;
		public bool showVelocity_y = true;

		public ValueAxisMode velocityMode = ValueAxisMode.Adaptive;
		public float velocityMin = float.PositiveInfinity;
		public float velocityMax = float.NegativeInfinity;

		public Monitor monitor_velocity;
		private Channel channel_velocity_x;
		private Channel channel_velocity_y;

		// angular velocity.
		public bool showAngularVelocity = false;

		public ValueAxisMode angularVelocityMode = ValueAxisMode.Adaptive;
		public float angularVelocityMin = float.PositiveInfinity;
		public float angularVelocityMax = float.NegativeInfinity;

		public Monitor monitor_angularVelocity;
		private Channel channel_angularVelocity;

		private bool missingRigidbodyWarning = false;

		private new Rigidbody2D rigidbody2D;

		void Awake()
		{
			if (Application.isPlaying && !Application.isEditor)
			{
				Destroy(this);
			}
		}

		void Start()
		{
			UpdateMonitors();

			if (Application.isPlaying)
			{
				rigidbody2D = GetComponent<Rigidbody2D>();

				if (rigidbody2D == null)
				{
					Debug.LogWarning(nameof(Rigidbody2DGraphPlotter) + " requires " + nameof(Rigidbody2D) + " component.", this);
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

		public void FixedUpdate()
		{
			if (!Application.isPlaying)
				return;

			Sample();
		}

		private void Sample()
		{
			if (rigidbody2D == null)
			{
				if (!missingRigidbodyWarning)
				{
					Debug.LogWarning(nameof(Rigidbody2DGraphPlotter) + " requires " + nameof(Rigidbody2D) + " component.", this);
					missingRigidbodyWarning = true;
				}

				return;
			}

			if (showPosition)
			{
				Vector3 position = rigidbody2D.position;

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
			}

			if (showRotation)
			{
				rotationMin = monitor_rotation.Min;
				rotationMax = monitor_rotation.Max;

				if (showRotation)
				{
					float rotation = rigidbody2D.rotation;
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

					channel_rotation.Sample(rotation);
				}
			}

			if (showVelocity)
			{
				Vector3 velocity = rigidbody2D.velocity;

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
			}

			if (showAngularVelocity)
			{
				angularVelocityMin = monitor_angularVelocity.Min;
				angularVelocityMax = monitor_angularVelocity.Max;


				if (showAngularVelocity)
				{
					channel_angularVelocity.Sample(rigidbody2D.angularVelocity);
				}
			}
		}

		public void OnEnable()
		{
			UpdateMonitors();
		}

		public void OnDisable()
		{
			UpdateMonitors();
		}

		public void OnDestroy()
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