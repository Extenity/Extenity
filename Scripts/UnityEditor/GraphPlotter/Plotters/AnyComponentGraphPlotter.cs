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
		[HideInInspector] // Not meant to be shown in raw format
		public Component Component;
		[HideInInspector] // Not meant to be shown in raw format
		public List<ChannelField> ChannelFields = new List<ChannelField>();
		private readonly List<ChannelField> RemovedChannelFieldTracker = new List<ChannelField>();

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
			var graphIsActive = componentIsActive && Component != null;

			Graph.SetupGraph(graphIsActive, ref Graph, Component?.GetType().Name, gameObject, Range);

			if (graphIsActive)
			{
				foreach (var field in ChannelFields)
				{
					Channel.SetupChannel(true, Graph, ref field.Channel, field.FieldName, field.Color);

					// Remove from tracker list. After that, the list will only contain channels that were previously used but not used anymore.
					if (RemovedChannelFieldTracker.Contains(field))
					{
						RemovedChannelFieldTracker.Remove(field);
					}
				}

				// Close previously created but recently removed channels. Channels in use were removed from tracker list above.
				foreach (var field in RemovedChannelFieldTracker)
				{
					Channel.SafeClose(ref field.Channel);
				}
				RemovedChannelFieldTracker.Clear();

				// Rebuild the tracker list. We will need it next time we call 'UpdateGraph'.
				RemovedChannelFieldTracker.AddRange(ChannelFields);
			}
			else
			{
				// Channels should already be closed by now. But there may be dangling references to them. So do a safe close.
				foreach (var field in ChannelFields)
				{
					Channel.SafeClose(ref field.Channel);
				}
				// Close previously created but recently removed channels.
				foreach (var field in RemovedChannelFieldTracker)
				{
					Channel.SafeClose(ref field.Channel);
				}
				RemovedChannelFieldTracker.Clear();
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

			if (Component == null)
				return;

			Range.CopyFrom(Graph.Range);

			var time = Time.time;
			var frame = Time.frameCount;

			foreach (var field in ChannelFields)
			{
				// ReSharper disable once BuiltInTypeReferenceStyle
				Object instance = Component;
				var instanceType = Component.GetType();

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
			Graph.SafeClose(ref Graph);
		}
	}

}