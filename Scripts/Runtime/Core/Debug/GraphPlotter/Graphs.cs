#if UNITY_5_3_OR_NEWER

using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
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

		public static Graph GetGraphByTitleAndContext(string title, GameObject context)
		{
			for (var i = 0; i < All.Count; i++)
			{
				if (All[i].Context == context && All[i].Title.Equals(title, StringComparison.Ordinal))
					return All[i];
			}
			return null;
		}

		public static bool IsAnyGraphForContextExists(GameObject context)
		{
			for (var i = 0; i < All.Count; i++)
			{
				if (All[i].Context == context)
					return true;
			}
			return false;
		}

		public static void GatherContextObjects(List<GameObject> contextObjects, bool sortByName)
		{
			foreach (var graph in All)
			{
				if (graph.Context != null)
				{
					if (!contextObjects.Contains(graph.Context))
					{
						contextObjects.Add(graph.Context);
					}
				}
			}

			contextObjects.Sort((a, b) =>
			{
				var comparison = a.name.CompareTo(b.name);
				if (comparison != 0)
					return comparison;
				return a.GetInstanceID().CompareTo(b.GetInstanceID());
			});
		}

		public static void GatherDisplayedContextObjectNames(List<GameObject> contextObjects, ref string[] contextObjectNames)
		{
			const bool sortByName = true; // This is needed. See comments below.
			GatherContextObjects(contextObjects, sortByName);
			var size = contextObjects.Count + 1;
			CollectionTools.ResizeIfRequired(ref contextObjectNames, size);

			contextObjectNames[0] = "All";

			for (int i = 0; i < contextObjects.Count; i++)
			{
				contextObjectNames[i + 1] = contextObjects[i].name;
			}

			// Rename objects that have the same name. Object names should be sorted.
			for (int i = 1; i < contextObjectNames.Length; i++)
			{
				var lastIndexWithSameName = i;
				for (int j = i + 1; j < contextObjectNames.Length; j++)
				{
					if (contextObjectNames[j] == contextObjectNames[i])
					{
						lastIndexWithSameName = j;
					}
					else
					{
						break;
					}
				}
				if (lastIndexWithSameName > i)
				{
					int n = 1;
					for (int j = i; j <= lastIndexWithSameName; j++)
					{
						contextObjectNames[j] = contextObjectNames[j] + "/" + n + "";
						n++;
					}
					i = lastIndexWithSameName + 1;
				}
			}
		}

		#endregion
	}

}

#endif
