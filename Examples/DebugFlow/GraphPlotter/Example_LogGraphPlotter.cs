using Extenity;
using UnityEngine;

namespace ExtenityExamples.UnityEditorToolbox.GraphPlotting
{

	public class Example_LogGraphPlotter : MonoBehaviour
	{
		public float LogInterval = 1f;
		public string[] Logs;

		protected void Start()
		{
			InvokeRepeating("WriteLogs", LogInterval, LogInterval);
		}

		public void WriteLogs()
		{
			foreach (var log in Logs)
			{
				Log.Info(log);
			}
		}
	}

}
