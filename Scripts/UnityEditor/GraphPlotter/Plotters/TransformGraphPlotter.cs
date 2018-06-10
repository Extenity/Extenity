using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Transform")]
	[ExecuteInEditMode]
	public class TransformGraphPlotter : MonoBehaviour
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
		public Channel channel_position_x;
		public Channel channel_position_y;
		public Channel channel_position_z;

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
		public Channel channel_rotation_x;
		public Channel channel_rotation_y;
		public Channel channel_rotation_z;

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
		public Channel channel_scale_x;
		public Channel channel_scale_y;
		public Channel channel_scale_z;

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
					monitor_position = new Monitor("", gameObject);
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
					monitor_rotation = new Monitor("", gameObject);
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

		private void UpdateScaleMonitor(bool componentIsActive)
		{
			// scale
			if (showScale && componentIsActive)
			{
				if (monitor_scale == null)
				{
					monitor_scale = new Monitor("", gameObject);
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
				if (channel_scale_x == null)
				{
					channel_scale_x = new Channel(monitor_scale, "x", PlotColors.Red);
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
					channel_scale_y = new Channel(monitor_scale, "y", PlotColors.Green);
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
					channel_scale_z = new Channel(monitor_scale, "z", PlotColors.Blue);
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


		public void FixedUpdate()
		{
			if (!Application.isPlaying)
				return;

			if (showPosition)
			{
				var position = positionSpace == Space.Local ? transform.localPosition : transform.position;

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
				var euler = (rotationSpace == Space.Local ? transform.localRotation : transform.rotation).eulerAngles;

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

			if (showScale)
			{
				var scale = scaleSpace == ScaleSpace.Local ? transform.localScale : transform.lossyScale;

				scaleMin = monitor_scale.Min;
				scaleMax = monitor_scale.Max;

				if (showScale_x)
				{
					channel_scale_x.Sample(scale.x);
				}

				if (showScale_y)
				{
					channel_scale_y.Sample(scale.y);
				}

				if (showScale_z)
				{
					channel_scale_z.Sample(scale.z);
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

			if (monitor_scale != null)
			{
				monitor_scale.Close();
				monitor_scale = null;
			}
		}
	}

}