using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Debug.Log Calls")]
	[ExecuteInEditMode]
	public class DebugLogGraphPlotter : MonoBehaviour
	{
		public string filterPrefix;
		private Graph Graph;

		protected void Start()
		{
			UpdateGraph();

			if (Application.isPlaying)
			{
				Application.logMessageReceived -= LogCallback;
				Application.logMessageReceived += LogCallback;
			}
		}

		public void UpdateGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			if (componentIsActive)
			{
				if (Graph == null)
				{
					Graph = new Graph("Debug.Log", gameObject);
				}

				if (filterPrefix != string.Empty)
				{
					Graph.Title = "Debug.Log (prefix = '" + filterPrefix + "')";
				}
			}
			else
			{
				if (Graph != null)
				{
					Graph.Close();
					Graph = null;
				}
			}
		}

		protected void Update()
		{
			if (!Application.isPlaying)
				return;

			Graph.SetTimeCursor(Time.time);
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

			Graph.Add(entry);
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
			Application.logMessageReceived -= LogCallback;
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