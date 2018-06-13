using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Debug.Log Calls")]
	[ExecuteInEditMode]
	public class DebugLogGraphPlotter : MonoBehaviour
	{
		public StringFilter Filter;
		private Graph Graph;

		#region Initialization

		protected void Start()
		{
			UpdateGraph();

			if (Application.isPlaying)
			{
				Application.logMessageReceived -= LogCallback;
				Application.logMessageReceived += LogCallback;
			}
		}

		protected void OnEnable()
		{
			UpdateGraph();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			Application.logMessageReceived -= LogCallback;
			Graph.SafeClose(ref Graph);
		}

		protected void OnDisable()
		{
			UpdateGraph();
		}

		#endregion

		#region Update

		protected void Update()
		{
			if (!Application.isPlaying)
				return;

			Graph.SetTimeCursor(Time.time);
		}

		#endregion

		public void UpdateGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			Graph.SetupGraph(componentIsActive, ref Graph, "Debug.Log", gameObject, null);
		}

		private void LogCallback(string logString, string stackTrace, LogType type)
		{
			if (!Filter.IsMatching(logString))
			{
				return;
			}

			var entry = new TagEntry(Time.time, logString);

			Graph.Add(entry);
		}
	}

}