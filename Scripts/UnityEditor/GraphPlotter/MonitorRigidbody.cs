// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System.Collections;

namespace MonitorComponents 
{
	[AddComponentMenu("Monitor Components/Monitor Rigidbody")]
	[ExecuteInEditMode]
	public class MonitorRigidbody : MonoBehaviour 
	{
		public enum RotationFormat { Euler, Quaternion };
		public enum SampleMode { Update, FixedUpdate };

		public SampleMode sampleMode = SampleMode.FixedUpdate;

		// position
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;
		public bool showPosition_z = true;

		public ValueAxisMode positionMode = ValueAxisMode.Adaptive;
		public float positionMin = float.PositiveInfinity;
		public float positionMax = float.NegativeInfinity;

		public Monitor monitor_position;
		private MonitorInput monitorInput_position_x;
		private MonitorInput monitorInput_position_y;
		private MonitorInput monitorInput_position_z;

		// rotation
		public bool showRotation = false;
		public bool showRotation_x = true; 
		public bool showRotation_y = true;
		public bool showRotation_z = true;

		public ValueAxisMode rotationMode = ValueAxisMode.Fixed;
		public float rotationMin = 0f;
		public float rotationMax = 360f;
		
		public Monitor monitor_rotation;
		private MonitorInput monitorInput_rotation_x; 
		private MonitorInput monitorInput_rotation_y;
		private MonitorInput monitorInput_rotation_z;

		// velocity
		public bool showVelocity = false;
		public bool showVelocity_x = true;
		public bool showVelocity_y = true;
		public bool showVelocity_z = true;

		public ValueAxisMode velocityMode = ValueAxisMode.Adaptive;
		public float velocityMin = float.PositiveInfinity;
		public float velocityMax = float.NegativeInfinity;

		public Monitor monitor_velocity;
		private MonitorInput monitorInput_velocity_x; 
		private MonitorInput monitorInput_velocity_y;
		private MonitorInput monitorInput_velocity_z;

		// angular velocity.
		public bool showAngularVelocity = false;
		public bool showAngularVelocity_x = true; 
		public bool showAngularVelocity_y = true;
		public bool showAngularVelocity_z = true;

		public ValueAxisMode angularVelocityMode = ValueAxisMode.Adaptive;
		public float angularVelocityMin = float.PositiveInfinity;
		public float angularVelocityMax = float.NegativeInfinity;

		public Monitor monitor_angularVelocity;
		private MonitorInput monitorInput_angularVelocity_x; 	
		private MonitorInput monitorInput_angularVelocity_y;
		private MonitorInput monitorInput_angularVelocity_z;

		private bool missingRigidbodyWarning = false;

		private new Rigidbody rigidbody;

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
				rigidbody = GetComponent<Rigidbody>();
				
				if (rigidbody == null)
				{
					Debug.LogWarning("MonitorAudioSource requires an Rigidbody component.", this);
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
					monitor_position = new Monitor("Position");
					monitor_position.GameObject = gameObject;
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
				if (monitorInput_position_x == null)
				{
					monitorInput_position_x = new MonitorInput(monitor_position, "x", Colors.red);
				}
			}
			else
			{
				if (monitorInput_position_x != null)
				{
					monitorInput_position_x.Close();
					monitorInput_position_x = null;
				}
			}

			// position y
			if (showPosition && showPosition_y && componentIsActive)
			{
				if (monitorInput_position_y == null)
				{
					monitorInput_position_y = new MonitorInput(monitor_position, "y", Colors.green);
				}
			}
			else
			{
				if (monitorInput_position_y != null)
				{
					monitorInput_position_y.Close();
					monitorInput_position_y = null;
				}
			}

			// position z
			if (showPosition && showPosition_z && componentIsActive)
			{
				if (monitorInput_position_z == null)
				{
					monitorInput_position_z = new MonitorInput(monitor_position, "z", Colors.blue);
				}
			}
			else
			{
				if (monitorInput_position_z != null)
				{
					monitorInput_position_z.Close();
					monitorInput_position_z = null;
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
					monitor_rotation = new Monitor("Rotation");
					monitor_rotation.GameObject = gameObject;
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
				if (monitorInput_rotation_x == null)
				{
					monitorInput_rotation_x = new MonitorInput(monitor_rotation, "x", Colors.red);
				}
			}
			else
			{
				if (monitorInput_rotation_x != null)
				{
					monitorInput_rotation_x.Close();
					monitorInput_rotation_x = null;
				}
			}

			// rotation y
			if (showRotation && showRotation_y && componentIsActive)
			{
				if (monitorInput_rotation_y == null)
				{
					monitorInput_rotation_y = new MonitorInput(monitor_rotation, "y", Colors.green);
				}
			}
			else
			{
				if (monitorInput_rotation_y != null)
				{
					monitorInput_rotation_y.Close();
					monitorInput_rotation_y = null;
				}
			}

			// rotation z
			if (showRotation && showRotation_z && componentIsActive)
			{
				if (monitorInput_rotation_z == null)
				{
					monitorInput_rotation_z = new MonitorInput(monitor_rotation, "z", Colors.blue);
				}
			}
			else
			{
				if (monitorInput_rotation_z != null)
				{
					monitorInput_rotation_z.Close();
					monitorInput_rotation_z = null;
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
					monitor_velocity = new Monitor("Velocity");
					monitor_velocity.GameObject = gameObject;
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
				if (monitorInput_velocity_x == null)
				{
					monitorInput_velocity_x = new MonitorInput(monitor_velocity, "x", Colors.red);
				}
			}
			else
			{
				if (monitorInput_velocity_x != null)
				{
					monitorInput_velocity_x.Close();
					monitorInput_velocity_x = null;
				}
			}

			// velocity y
			if (showVelocity && showVelocity_y && componentIsActive)
			{
				if (monitorInput_velocity_y == null)
				{
					monitorInput_velocity_y = new MonitorInput(monitor_velocity, "y", Colors.green);
				}
			}
			else
			{
				if (monitorInput_velocity_y != null)
				{
					monitorInput_velocity_y.Close();
					monitorInput_velocity_y = null;
				}
			}

			// velocity z
			if (showVelocity && showVelocity_z && componentIsActive)
			{
				if (monitorInput_velocity_z == null)
				{
					monitorInput_velocity_z = new MonitorInput(monitor_velocity, "z", Colors.blue);
				}
			}
			else
			{
				if (monitorInput_velocity_z != null)
				{
					monitorInput_velocity_z.Close();
					monitorInput_velocity_z = null;
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
					monitor_angularVelocity = new Monitor("Angular Velocity");
					monitor_angularVelocity.GameObject = gameObject;
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
				if (monitorInput_angularVelocity_x == null)
				{
					monitorInput_angularVelocity_x = new MonitorInput(monitor_angularVelocity, "x", Colors.red);
				}
			}
			else
			{
				if (monitorInput_angularVelocity_x != null)
				{
					monitorInput_angularVelocity_x.Close();
					monitorInput_angularVelocity_x = null;
				}
			}

			// angularVelocity y
			if (showAngularVelocity && showAngularVelocity_y && componentIsActive)
			{
				if (monitorInput_angularVelocity_y == null)
				{
					monitorInput_angularVelocity_y = new MonitorInput(monitor_angularVelocity, "y", Colors.green);
				}
			}
			else
			{
				if (monitorInput_angularVelocity_y != null)
				{
					monitorInput_angularVelocity_y.Close();
					monitorInput_angularVelocity_y = null;
				}
			}

			// angularVelocity z
			if (showAngularVelocity && showAngularVelocity_z && componentIsActive)
			{
				if (monitorInput_angularVelocity_z == null)
				{
					monitorInput_angularVelocity_z = new MonitorInput(monitor_angularVelocity, "z", Colors.blue);
				}
			}
			else
			{
				if (monitorInput_angularVelocity_z != null)
				{
					monitorInput_angularVelocity_z.Close();
					monitorInput_angularVelocity_z = null;
				}
			}
		}


		public void FixedUpdate () 
		{
			if (!Application.isPlaying)
				return;
		
			if (sampleMode == SampleMode.FixedUpdate)
			{
				Sample();
			}
		}

		public void Update()
		{
			if (!Application.isPlaying)
				return;
		
			if (sampleMode == SampleMode.Update)
			{
				Sample();
			}	
		}

		private void Sample()
		{
			if (rigidbody == null)
			{
				if (!missingRigidbodyWarning)
				{
					Debug.LogWarning("MonitorRigidbody requires a Rigidbody component.", this);
					missingRigidbodyWarning = true;
				}

				return;
			}

			if (showPosition)
			{
				Vector3 position = rigidbody.position;

				positionMin = monitor_position.Min;
				positionMax = monitor_position.Max;

				if (showPosition_x)
				{
					monitorInput_position_x.Sample(position.x);
				}

				if (showPosition_y)
				{
					monitorInput_position_y.Sample(position.y);
				}

				if (showPosition_z)
				{
					monitorInput_position_z.Sample(position.z);
				}
			}

			if (showRotation)
			{
				Vector3 rotation = rigidbody.rotation.eulerAngles;

				rotationMin = monitor_rotation.Min;
				rotationMax = monitor_rotation.Max;

				if (showRotation_x)
				{
					monitorInput_rotation_x.Sample(rotation.x);
				}

				if (showRotation_y)
				{
					monitorInput_rotation_y.Sample(rotation.y);
				}

				if (showRotation_z)
				{
					monitorInput_rotation_z.Sample(rotation.z);
				}
			}

			if (showVelocity)
			{
				Vector3 velocity = rigidbody.velocity;

				velocityMin = monitor_velocity.Min;
				velocityMax = monitor_velocity.Max;

				if (showVelocity_x)
				{
					monitorInput_velocity_x.Sample(velocity.x);
				}

				if (showVelocity_y)
				{
					monitorInput_velocity_y.Sample(velocity.y);
				}

				if (showVelocity_z)
				{
					monitorInput_velocity_z.Sample(velocity.z);
				}
			}

			if (showAngularVelocity)
			{
				Vector3 angularVelocity = rigidbody.angularVelocity;

				angularVelocityMin = monitor_angularVelocity.Min;
				angularVelocityMax = monitor_angularVelocity.Max;


				if (showAngularVelocity_x)
				{
					monitorInput_angularVelocity_x.Sample(angularVelocity.x);
				}

				if (showAngularVelocity_y)
				{
					monitorInput_angularVelocity_y.Sample(angularVelocity.y);
				}

				if (showAngularVelocity_z)
				{
					monitorInput_angularVelocity_z.Sample(angularVelocity.z);
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
			if(monitor_position != null)
			{
				monitor_position.Close();
				monitor_position = null;
			}

			if(monitor_rotation != null)
			{
				monitor_rotation.Close();
				monitor_rotation = null;
			}

			if(monitor_velocity != null)
			{
				monitor_velocity.Close();
				monitor_velocity = null;
			}

			if(monitor_angularVelocity != null)
			{
				monitor_angularVelocity.Close();
				monitor_angularVelocity = null;
			}
		}
	}
}