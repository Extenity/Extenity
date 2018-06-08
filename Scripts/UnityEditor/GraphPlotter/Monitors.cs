using System.Collections.Generic;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public class Monitors
	{
		private static Monitors instance = null;

		private List<Monitor> monitors = new List<Monitor>();

		public static Monitors Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Monitors();
				}

				return instance;
			}
		}

		public void Add(Monitor monitor)
		{
			monitors.Add(monitor);
		}

		public void Remove(Monitor monitor)
		{
			monitors.Remove(monitor);
		}

		public List<Monitor> All
		{
			get
			{
				return monitors;
			}
		}
	}

}