using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Transform")]
	[ExecuteInEditMode]
	public class TransformGraphPlotter : MonoBehaviour
	{
		public enum Space { Local, World };
		public enum ScaleSpace { Local, Lossy };

		// -----------------------------------------------------
		// Input - Position
		// -----------------------------------------------------
		public bool showPosition = false;
		public bool showPosition_x = true;
		public bool showPosition_y = true;
		public bool showPosition_z = true;
		public Space positionSpace = Space.World;
		public ValueAxisRangeConfiguration PositionRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Monitor monitor_position;
		public Channel channel_position_x;
		public Channel channel_position_y;
		public Channel channel_position_z;
		// -----------------------------------------------------
		// Input - Rotation
		// -----------------------------------------------------
		public bool showRotation = false;
		public bool showRotation_x = true;
		public bool showRotation_y = true;
		public bool showRotation_z = true;
		public Space rotationSpace = Space.World;
		public ValueAxisRangeConfiguration RotationRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, 0f, 360f);

		public Monitor monitor_rotation;
		public Channel channel_rotation_x;
		public Channel channel_rotation_y;
		public Channel channel_rotation_z;
		// -----------------------------------------------------
		// Input - Scale
		// -----------------------------------------------------
		public bool showScale = false;
		public bool showScale_x = true;
		public bool showScale_y = true;
		public bool showScale_z = true;
		public ScaleSpace scaleSpace = ScaleSpace.Local;
		public ValueAxisRangeConfiguration ScaleRange = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

		public Monitor monitor_scale;
		public Channel channel_scale_x;
		public Channel channel_scale_y;
		public Channel channel_scale_z;
		// -----------------------------------------------------

		public SampleTime SampleTime = SampleTime.FixedUpdate;

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
				monitor_scale.SetRangeConfiguration(ScaleRange);
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

			var time = Time.time;
			var frame = Time.frameCount;

			if (showPosition)
			{
				var position = positionSpace == Space.Local ? transform.localPosition : transform.position;

				PositionRange.CopyFrom(monitor_position.Range);

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
				var euler = (rotationSpace == Space.Local ? transform.localRotation : transform.rotation).eulerAngles;

				RotationRange.CopyFrom(monitor_rotation.Range);

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

			if (showScale)
			{
				var scale = scaleSpace == ScaleSpace.Local ? transform.localScale : transform.lossyScale;

				ScaleRange.CopyFrom(monitor_scale.Range);

				if (showScale_x)
				{
					channel_scale_x.Sample(scale.x, time, frame);
				}

				if (showScale_y)
				{
					channel_scale_y.Sample(scale.y, time, frame);
				}

				if (showScale_z)
				{
					channel_scale_z.Sample(scale.z, time, frame);
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