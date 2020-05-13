using System.Collections.Generic;
using Extenity.DebugToolbox.GraphPlotting;
using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	public static class QuickTickAnalyzer
	{
		public static readonly Dictionary<string, TickAnalyzer> Analyzers = new Dictionary<string, TickAnalyzer>();

		public static void Tick(string label)
		{
			Tick(label, Time.time);
		}

		public static void Tick(string label, float now)
		{
			if (!Analyzers.TryGetValue(label, out var analyzer))
			{
				analyzer = new TickAnalyzer(
					new TickPlotter(label, VerticalRangeConfiguration.CreateZeroBasedAdaptive()),
					now);
				Analyzers.Add(label, analyzer);
			}

			analyzer.Tick(now);
		}
	}

}
