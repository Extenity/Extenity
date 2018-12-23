using System.Collections.Generic;
using Extenity.DebugFlowTool.GraphPlotting;
using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	public static class QuickTickAnalyzer
	{
		public static readonly Dictionary<string, TickAnalyzer> Analyzers = new Dictionary<string, TickAnalyzer>();

		public static void Tick(string label)
		{
			var now = Time.time;

			if (!Analyzers.TryGetValue(label, out var analyzer))
			{
				analyzer = new TickAnalyzer(
					new TickPlotter(label, ValueAxisRangeConfiguration.CreateZeroBasedAdaptive()),
					now);
				Analyzers.Add(label, analyzer);
			}

			analyzer.Tick(now);
		}
	}

}
