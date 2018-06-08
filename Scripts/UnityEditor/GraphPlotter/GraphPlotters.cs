using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public static class GraphPlotters
	{
		public static readonly List<Monitor> All = new List<Monitor>();

		public static void Register(Monitor plotter)
		{
			if (plotter == null)
				throw new ArgumentNullException(nameof(plotter));
			if (All.Contains(plotter))
				throw new Exception($"Graph plotter '{plotter.Name}' was already registered.");

			All.Add(plotter);
		}

		public static void Deregister(Monitor plotter)
		{
			if (plotter == null)
				throw new ArgumentNullException(nameof(plotter));
			if (!All.Contains(plotter))
				throw new Exception($"Graph plotter '{plotter.Name}' was not registered.");

			All.Remove(plotter);
		}

		public static bool IsAnyGraphForObjectExists(GameObject go)
		{
			for (var i = 0; i < All.Count; i++)
			{
				if (All[i].GameObject == go)
					return true;
			}
			return false;
		}
	}

}