using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Extenity.DebugToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Any Component")]
	[ExecuteInEditMode]
	public class AnyComponentGraphPlotter : MonoBehaviour
	{
		[Serializable]
		public class ChannelField
		{
			[HideInInspector] // Not meant to be shown in raw format
			public string[] Field;

			public Color Color = Color.red;

			[NonSerialized]
			public Channel Channel;

			public string FieldName { get { return String.Join(".", Field); } }
		}

		#region Initialization

		protected void Start()
		{
			SetupGraph();
		}

		protected void OnEnable()
		{
			SetupGraph();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			Graph.SafeClose(ref Graph);
		}

		protected void OnDisable()
		{
			SetupGraph();
		}

		#endregion

		#region Update

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

		#endregion

		#region Metadata and Configuration

		public Component Component;
		public SampleTime SampleTime = SampleTime.FixedUpdate;

		// -----------------------------------------------------
		// Input - Any Value
		// -----------------------------------------------------
		[HideInInspector] // Not meant to be shown in raw format
		public List<ChannelField> ChannelFields = new List<ChannelField>();
		public ValueAxisRangeConfiguration Range = ValueAxisRangeConfiguration.CreateAdaptive();
		public Graph Graph;
		// -----------------------------------------------------

		#endregion

		#region Setup Graph Dynamically

		private readonly List<ChannelField> RemovedChannelFieldTracker = new List<ChannelField>();

		public void SetupGraph()
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

		#endregion

		#region Sample

		public void Sample()
		{
			if (!Application.isPlaying)
				return;

			if (Component == null)
				return;

			Range.CopyFrom(Graph.Range);

			var time = Time.time;

			foreach (var field in ChannelFields)
			{
				// ReSharper disable once BuiltInTypeReferenceStyle
				Object instance = Component;
				var instanceType = Component.GetType();

				for (int level = 0; level < field.Field.Length; level++)
				{
					instance = TypeInspectors.GetTypeInspector(instanceType).GetValue(instance, field.Field[level]);
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
						field.Channel.Sample((float)instance, time);
					}
					else if (instanceType == typeof(double))
					{
						field.Channel.Sample(Convert.ToSingle((double)instance), time);
					}
					else if (instanceType == typeof(int))
					{
						field.Channel.Sample((int)instance, time);
					}
					else if (instanceType == typeof(bool))
					{
						field.Channel.Sample((bool)instance ? 1f : 0f, time);
					}
				}
			}
		}

		#endregion
	}

}
