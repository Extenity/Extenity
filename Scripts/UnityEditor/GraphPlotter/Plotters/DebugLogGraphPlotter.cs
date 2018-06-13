using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	[AddComponentMenu("Graph Plotter/Plot Debug.Log Calls")]
	[ExecuteInEditMode]
	public class DebugLogGraphPlotter : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			SetupGraph();

			if (Application.isPlaying)
			{
				Application.logMessageReceived -= LogCallback;
				Application.logMessageReceived += LogCallback;
			}
		}

		protected void OnEnable()
		{
			SetupGraph();
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
			SetupGraph();
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

		#region Metadata and Configuration

		// -----------------------------------------------------
		// Input - No value other than Debug log calls
		// -----------------------------------------------------
		public StringFilter Filter;
		public Graph Graph;
		// -----------------------------------------------------

		public void SetupGraph()
		{
			var componentIsActive = enabled && gameObject.activeInHierarchy;

			Graph.SetupGraph(componentIsActive, ref Graph, "Debug.Log", gameObject, null);
		}

		#endregion

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