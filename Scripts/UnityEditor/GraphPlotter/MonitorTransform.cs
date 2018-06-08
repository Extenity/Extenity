// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System.Collections;

namespace MonitorComponents 
{
	[AddComponentMenu("Monitor Components/Monitor Transform")]
	[ExecuteInEditMode]
	public class MonitorTransform : MonoBehaviour 
	{
		public enum Space { Local, World };
		public enum ScaleSpace { Local, Lossy };

		// position
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;
		public bool showPosition_z = true;
		public Space positionSpace = Space.World;
		public ValueAxisMode positionMode = ValueAxisMode.Adaptive;
		public float positionMin = float.PositiveInfinity;
		public float positionMax = float.NegativeInfinity;

		public Monitor monitor_position;
		public MonitorInput monitorInput_position_x;
		public MonitorInput monitorInput_position_y;
		public MonitorInput monitorInput_position_z;

		// rotation
		public bool showRotation = false;
		public bool showRotation_x = true; 
		public bool showRotation_y = true;
		public bool showRotation_z = true;
		public Space rotationSpace = Space.World;
		public ValueAxisMode rotationMode = ValueAxisMode.Fixed;
		public float rotationMin = 0f;
		public float rotationMax = 360f;

		public Monitor monitor_rotation;
		public MonitorInput monitorInput_rotation_x; 
		public MonitorInput monitorInput_rotation_y;
		public MonitorInput monitorInput_rotation_z;
		
		// scale
		public bool showScale = false;
		public ScaleSpace scaleSpace = ScaleSpace.Local;
		public bool showScale_x = true;
		public bool showScale_y = true;
		public bool showScale_z = true;
		public ValueAxisMode scaleMode = ValueAxisMode.Adaptive;
		public float scaleMin = float.PositiveInfinity;
		public float scaleMax = float.NegativeInfinity;

		public Monitor monitor_scale;
		public MonitorInput monitorInput_scale_x;
		public MonitorInput monitorInput_scale_y;
		public MonitorInput monitorInput_scale_z;

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
				if (monitor_position == null)
				{
					monitor_position = new Monitor("");
					monitor_position.GameObject = gameObject;
				}

				monitor_position.Name = "Position (" + (positionSpace == Space.World ? "world" : "local") + ")";
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
					monitor_rotation = new Monitor("");
					monitor_rotation.GameObject = gameObject;
				}

				monitor_rotation.Name = "Rotation (" + (rotationSpace == Space.World ? "world" : "local") + ")";
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

		private void UpdateScaleMonitor(bool componentIsActive)
		{
			// scale
			if (showScale && componentIsActive)
			{
				if (monitor_scale == null)
				{
					monitor_scale = new Monitor("");
					monitor_scale.GameObject = gameObject;
				}

				monitor_scale.Name = "Scale (" + (scaleSpace == ScaleSpace.Local ? "local" : "lossy") + ")";
				monitor_scale.Mode = scaleMode;
				monitor_scale.Min = scaleMin;
				monitor_scale.Max = scaleMax;
			}
			else
			{
				if (monitor_scale != null)
				{
					monitor_scale.Close();
					monitor_scale = null;
				}
			}

			// scale x
			if (showScale && showScale_x && componentIsActive)
			{
				if (monitorInput_scale_x == null)
				{
					monitorInput_scale_x = new MonitorInput(monitor_scale, "x", Colors.red);
				}
			}
			else
			{
				if (monitorInput_scale_x != null)
				{
					monitorInput_scale_x.Close();
					monitorInput_scale_x = null;
				}
			}

			// scale y
			if (showScale && showScale_y && componentIsActive)
			{
				if (monitorInput_scale_y == null)
				{
					monitorInput_scale_y = new MonitorInput(monitor_scale, "y", Colors.green);
				}
			}
			else
			{
				if (monitorInput_scale_y != null)
				{
					monitorInput_scale_y.Close();
					monitorInput_scale_y = null;
				}
			}

			// scale z
			if (showScale && showScale_z && componentIsActive)
			{
				if (monitorInput_scale_z == null)
				{
					monitorInput_scale_z = new MonitorInput(monitor_scale, "z", Colors.blue);
				}
			}
			else
			{
				if (monitorInput_scale_z != null)
				{
					monitorInput_scale_z.Close();
					monitorInput_scale_z = null;
				}
			}
		}


		public void Update()
		{
			if (!Application.isPlaying)
				return;

			if (showPosition)
			{
				Vector3 position = positionSpace == Space.Local ? transform.localPosition : transform.position;

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
				Vector3 rotation = (rotationSpace == Space.Local ? transform.localRotation : transform.rotation).eulerAngles;

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

			if (showScale)
			{
				Vector3 scale = scaleSpace == ScaleSpace.Local ? transform.localScale : transform.lossyScale;

				scaleMin = monitor_scale.Min;
				scaleMax = monitor_scale.Max;

				if (showScale_x)
				{
					monitorInput_scale_x.Sample(scale.x);
				}

				if (showScale_y)
				{
					monitorInput_scale_y.Sample(scale.y);
				}

				if (showScale_z)
				{
					monitorInput_scale_z.Sample(scale.z);
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

		private void RemoveMonitors()
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

			if(monitor_scale != null)
			{
				monitor_scale.Close();
				monitor_scale = null;
			}
		}
	}
}