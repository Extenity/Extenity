using UnityEngine;
using Logger = Extenity.Logger;

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

		#region Log

		private static readonly Logger Log = new("Example");

		#endregion
	}

}
