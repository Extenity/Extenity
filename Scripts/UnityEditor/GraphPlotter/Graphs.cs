using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public static class Graphs
	{
		public static readonly List<Graph> All = new List<Graph>();

		public static void Register(Graph graph)
		{
			if (graph == null)
				throw new ArgumentNullException(nameof(graph));
			if (All.Contains(graph))
				throw new Exception($"Graph '{graph.Title}' was already registered.");

			All.Add(graph);
		}

		public static void Deregister(Graph graph)
		{
			if (graph == null)
				throw new ArgumentNullException(nameof(graph));
			if (!All.Contains(graph))
				throw new Exception($"Graph '{graph.Title}' was not registered.");

			All.Remove(graph);
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