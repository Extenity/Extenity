#if UNITY_5_3_OR_NEWER

using Extenity.DebugToolbox.GraphPlotting;
using Extenity.ProfilingToolbox;

namespace Extenity.FlowToolbox
{

	public static class FPSAnalyzer
	{
		#region State

		public static bool IsEnabled { get; private set; }
		public static TickAnalyzer Analyzer { get; private set; }

		#endregion

		#region Enable / Disable

		public static void Enable()
		{
			if (IsEnabled)
				return;

			IsEnabled = true;

			Analyzer = new TickAnalyzer(
				new TickPlotter("FPS", VerticalRange.ZeroBasedAdaptive()),
				Loop.Time,
				TickAnalyzer.HistorySizeFor(60, 5));

			Loop.RegisterTime(OnTimeUpdate);
		}

		public static void Disable()
		{
			if (!IsEnabled)
				return;

			IsEnabled = false;

			Loop.DeregisterTime(OnTimeUpdate);

			Analyzer = null;
		}

		#endregion

		#region Update

		private static void OnTimeUpdate()
		{
			Analyzer.Tick(Loop.Time);
		}

		#endregion
	}

}

#endif
