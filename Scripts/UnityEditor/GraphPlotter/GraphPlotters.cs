using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public static class GraphPlotters
	{
		public static readonly List<Graph> All = new List<Graph>();

		public static void Register(Graph plotter)
		{
			if (plotter == null)
				throw new ArgumentNullException(nameof(plotter));
			if (All.Contains(plotter))
				throw new Exception($"Graph plotter '{plotter.Title}' was already registered.");

			All.Add(plotter);
		}

		public static void Deregister(Graph plotter)
		{
			if (plotter == null)
				throw new ArgumentNullException(nameof(plotter));
			if (!All.Contains(plotter))
				throw new Exception($"Graph plotter '{plotter.Title}' was not registered.");

			All.Remove(plotter);
		}

		public static bool IsAnyGraphForObjectExists(GameObject go)
		{
			for (var i = 0; i < All.Count; i++)
			{
				if (All[i].Context == go)
					return true;
			}
			return false;
		}
	}

}