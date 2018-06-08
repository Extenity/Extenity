using UnityEngine;
using System;
using System.Collections.Generic;
using Object = System.Object;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Any Component")]
	[ExecuteInEditMode]
	public class AnyComponentGraphPlotter : MonoBehaviour
	{
		public Component component;
		public List<ChannelField> channelFields = new List<ChannelField>();
		private List<ChannelField> oldChannelFields = new List<ChannelField>();

		public Monitor monitor;
		public ValueAxisMode mode = ValueAxisMode.Adaptive;

		public enum SampleMode { Update, FixedUpdate }
		public SampleMode sampleMode = SampleMode.FixedUpdate;

		public float min = float.PositiveInfinity;
		public float max = float.NegativeInfinity;

		[Serializable]
		public class ChannelField
		{
			public string[] field;

			// user settings
			public Color color = Color.red;

			// not serialized. 
			public string fieldTypeName;

			// runtime...
			public Channel Channel;

			public string FieldName { get { return String.Join(".", field); } }
		}

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
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			if (component != null && componentIsActive)
			{
				if (monitor == null)
				{
					monitor = new Monitor("", gameObject);
				}

				monitor.Name = component.GetType().Name;
				monitor.Mode = mode;
				monitor.Min = min;
				monitor.Max = max;

				foreach (var field in channelFields)
				{
					if (field.Channel == null)
					{
						field.Channel = new Channel(monitor, String.Join(".", field.field));
					}

					field.Channel.Color = field.color;

					if (oldChannelFields.Contains(field))
					{
						oldChannelFields.Remove(field);
					}
				}

				// destroy all Channel for field that was in the old collection but does not
				// appear in the new collection.
				foreach (var field in oldChannelFields)
				{
					if (field.Channel != null)
					{
						field.Channel.Close();
						field.Channel = null;
					}
				}

				oldChannelFields = new List<ChannelField>(channelFields);
			}
			else
			{
				foreach (var field in channelFields)
				{
					if (field.Channel != null)
					{
						field.Channel.Close();
						field.Channel = null;
					}
				}

				RemoveMonitor();
			}
		}

		public void Update()
		{
			if (!Application.isPlaying)
				return;

			if (sampleMode == SampleMode.Update)
			{
				SampleFields();
			}
		}

		public void FixedUpdate()
		{
			if (!Application.isPlaying)
				return;

			if (sampleMode == SampleMode.FixedUpdate)
			{
				SampleFields();
			}
		}

		private void SampleFields()
		{
			if (component == null)
				return;

			min = monitor.Min;
			max = monitor.Max;

			foreach (var field in channelFields)
			{
				Object instance = component;
				Type instanceType = component.GetType();

				for (int level = 0; level < field.field.Length; level++)
				{
					instance = TypeInspectors.Instance.GetTypeInspector(instanceType).GetValue(instance, field.field[level]);
					if (instance == null)
					{
						break;
					}
					else
					{
						instanceType = instance.GetType();
					}
				}

				if (instance != null)
				{
					if (instanceType == typeof(float))
					{
						field.Channel.Sample((float)instance);
					}

					if (instanceType == typeof(double))
					{
						field.Channel.Sample(Convert.ToSingle((double)instance));
					}

					if (instanceType == typeof(int))
					{
						field.Channel.Sample((int)instance);
					}

					if (instanceType == typeof(bool))
					{
						field.Channel.Sample((bool)instance ? 1f : 0f);
					}
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
			RemoveMonitor();
		}

		private void RemoveMonitor()
		{
			if (monitor != null)
			{
				monitor.Close();
				monitor = null;
			}
		}
	}

}