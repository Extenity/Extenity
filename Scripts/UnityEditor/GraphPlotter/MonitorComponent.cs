// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonitorComponents 
{
	[AddComponentMenu("Monitor Components/Monitor Component")]
	[ExecuteInEditMode]
	public class MonitorComponent : MonoBehaviour 
	{
		public Component component;
		public List<MonitorInputField> monitorInputFields = new List<MonitorInputField>();
		private List<MonitorInputField> oldMonitorInputFields = new List<MonitorInputField>();

		public Monitor monitor;
		public ValueAxisMode mode = ValueAxisMode.Adaptive;

		public float min = float.PositiveInfinity;
		public float max = float.NegativeInfinity;

		[System.Serializable]
		public class MonitorInputField 
		{
			public string[] field;

			// user settings
			public Color color = Color.red;

			// not serialized. 
			public string fieldTypeName;

			// runtime...
			public MonitorInput monitorInput;

			public string FieldName { get { return String.Join(".", field); }}
		}

		void Awake()
		{
			if (Application.isPlaying && !Application.isEditor)
			{
				Destroy(this);
			}
		}

		void Start () 
		{
			UpdateMonitors();
		}

		public void UpdateMonitors()
		{
			bool componentIsActive = enabled && gameObject.activeInHierarchy;

			if (component != null && componentIsActive)
			{
				if (monitor == null)
				{
					monitor = new Monitor("");
					monitor.GameObject = gameObject;
				}

				monitor.Name = component.GetType().Name;
				monitor.Mode = mode;
				monitor.Min = min;
				monitor.Max = max;

				foreach (var field in monitorInputFields)
				{
					if (field.monitorInput == null)
					{
						field.monitorInput = new MonitorInput(monitor, String.Join(".", field.field));
					}

					field.monitorInput.Color = field.color;

					if (oldMonitorInputFields.Contains(field))
					{
						oldMonitorInputFields.Remove(field);
					}
				}

				// destroy all MonitorInput for field that was in the old collection but does not
				// appear in the new collection.
				foreach (var field in oldMonitorInputFields)
				{
					if (field.monitorInput != null)
					{
						field.monitorInput.Close();
						field.monitorInput = null;
					}
				}

				oldMonitorInputFields = new List<MonitorInputField>(monitorInputFields);
			}
			else
			{
				foreach (var field in monitorInputFields)
				{
					if (field.monitorInput != null)
					{
						field.monitorInput.Close();
						field.monitorInput = null;
					}
				}

				if (monitor != null)
				{
					monitor.Close();
					monitor = null;
				}
			}
		}

		public void Update()
		{
			if (!Application.isPlaying)
				return;
		
			if (component != null)
			{
				min = monitor.Min;
				max = monitor.Max;
			
				foreach(var field in monitorInputFields)
				{
					System.Object instance = component;
					Type instanceType = component.GetType();

					for(int level = 0; level < field.field.Length; level++)
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
						if(instanceType == typeof(float))
						{
							field.monitorInput.Sample((float) instance);
						}

						if(instanceType == typeof(double))
						{
							field.monitorInput.Sample(Convert.ToSingle((double) instance));
						}

						if(instanceType == typeof(int))
						{
							field.monitorInput.Sample((int) instance);
						}

						if(instanceType == typeof(bool))
						{
							field.monitorInput.Sample((bool) instance ? 1f : 0f);
						}
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
			if(monitor != null)
			{
				monitor.Close();
				monitor = null;
			}
		}
	}
}