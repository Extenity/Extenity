// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System.Collections;

namespace MonitorComponents 
{
	[AddComponentMenu("Monitor Components/Monitor Debug.Log")]
	[ExecuteInEditMode]
	public class MonitorDebugLog : MonoBehaviour 
	{
		public string filterPrefix;
		private Monitor monitor;

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
				LogCallbackDispatcher.Instance.Add(LogCallback);
			}
		}
		
		public void UpdateMonitors()
		{
			bool componentIsActive = enabled && gameObject.activeInHierarchy;

			if (componentIsActive)
			{
				if (monitor == null)
				{
					monitor = new Monitor("Debug.Log");
					monitor.GameObject = gameObject;
				}

				if(filterPrefix != string.Empty)
				{
					monitor.Name = "Debug.Log (prefix = '"+ filterPrefix + "')";
				}
			}
			else
			{
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
				
			monitor.MoveForward(Time.time);
		}

		void LogCallback(string logString, string stackTrace, LogType type) 
		{
			if (filterPrefix != string.Empty)
			{
				if (!logString.StartsWith(filterPrefix))
				{
					return;
				}
			}

			MonitorEvent monitorEvent = new MonitorEvent();
			monitorEvent.time = Time.time;
			monitorEvent.text = logString;

			monitor.Add(monitorEvent);
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