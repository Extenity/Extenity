using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Debug.Log Calls")]
	[ExecuteInEditMode]
	public class DebugLogGraphPlotter : MonoBehaviour
	{
		public string filterPrefix;
		private Monitor monitor;

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

			if (Application.isPlaying)
			{
				Application.logMessageReceived -= LogCallback;
				Application.logMessageReceived += LogCallback;
			}
		}

		public void UpdateMonitors()
		{
			bool componentIsActive = enabled && gameObject.activeInHierarchy;

			if (componentIsActive)
			{
				if (monitor == null)
				{
					monitor = new Monitor("Debug.Log", gameObject);
				}

				if (filterPrefix != string.Empty)
				{
					monitor.Name = "Debug.Log (prefix = '" + filterPrefix + "')";
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

		protected void Update()
		{
			if (!Application.isPlaying)
				return;

			monitor.MoveForward(Time.time);
		}

		private void LogCallback(string logString, string stackTrace, LogType type)
		{
			if (filterPrefix != string.Empty)
			{
				if (!logString.StartsWith(filterPrefix))
				{
					return;
				}
			}

			var entry = new TagEntry(Time.time, logString);

			monitor.Add(entry);
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
			Application.logMessageReceived -= LogCallback;
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