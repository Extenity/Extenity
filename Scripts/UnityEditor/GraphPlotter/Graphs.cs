using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public static class Graphs
	{
		#region All Graphs

		public static readonly List<Graph> All = new List<Graph>();

		#endregion

		#region Register / Deregister

		internal static void Register(Graph graph)
		{
			if (graph == null)
				throw new ArgumentNullException(nameof(graph));
			if (All.Contains(graph))
				throw new Exception($"Graph '{graph.Title}' was already registered.");

			All.Add(graph);
		}

		internal static void Deregister(Graph graph)
		{
			if (graph == null)
				throw new ArgumentNullException(nameof(graph));
			if (!All.Contains(graph))
				throw new Exception($"Graph '{graph.Title}' was not registered.");

			All.Remove(graph);
		}

		#endregion

		#region Queries

		public static bool IsAnyGraphForContextExists(GameObject go)
		{
			for (var i = 0; i < All.Count; i++)
			{
				if (All[i].Context == go)
					return true;
			}
			return false;
		}

		#endregion
	}

}