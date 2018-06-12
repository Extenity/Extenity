using System;
using System.Collections.Generic;
using UnityEngine;
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

		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Scale
		// -----------------------------------------------------
		public Graph Graph;
		public ValueAxisRangeConfiguration Range = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		// -----------------------------------------------------

		[Serializable]
		public class ChannelField
		{
			[HideInInspector] // Not meant to be shown in raw format
			public string[] Field;
			[HideInInspector] // Not meant to be shown in raw format
			public string FieldTypeName;

			public Color Color = Color.red;

			[NonSerialized]
			public Channel Channel;

			public string FieldName { get { return String.Join(".", Field); } }
		}

		protected void Start()
		{
			UpdateGraph();
		}

		public void UpdateGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			if (component != null && componentIsActive)
			{
				if (Graph == null)
				{
					Graph = new Graph("", gameObject);
				}

				Graph.Title = component.GetType().Name;
				Graph.SetRangeConfiguration(Range);

				foreach (var field in channelFields)
				{
					if (field.Channel == null)
					{
						field.Channel = new Channel(Graph, field.FieldName);
					}

					field.Channel.Color = field.Color;

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

				RemoveGraph();
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

			if (component == null)
				return;

			Range.CopyFrom(Graph.Range);

			var time = Time.time;
			var frame = Time.frameCount;

			foreach (var field in channelFields)
			{
				// ReSharper disable once BuiltInTypeReferenceStyle
				Object instance = component;
				var instanceType = component.GetType();

				for (int level = 0; level < field.Field.Length; level++)
				{
					instance = TypeInspectors.Instance.GetTypeInspector(instanceType).GetValue(instance, field.Field[level]);
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
						field.Channel.Sample((float)instance, time, frame);
					}

					if (instanceType == typeof(double))
					{
						field.Channel.Sample(Convert.ToSingle((double)instance), time, frame);
					}

					if (instanceType == typeof(int))
					{
						field.Channel.Sample((int)instance, time, frame);
					}

					if (instanceType == typeof(bool))
					{
						field.Channel.Sample((bool)instance ? 1f : 0f, time, frame);
					}
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

		private void RemoveGraph()
		{
			if (Graph != null)
			{
				Graph.Close();
				Graph = null;
			}
		}
	}

}