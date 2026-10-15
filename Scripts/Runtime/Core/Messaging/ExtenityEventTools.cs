
namespace Extenity.MessagingToolbox
{

	public enum ListenerLifeSpan : byte
	{
		Permanent,
		RemovedAtFirstEmit,
	}

	public static class ExtenityEventTools
	{
		#region Log

		public static bool VerboseLogging = false; // Check this at the location of method call for performance reasons.

		#endregion

		#region Profiling

		/// <summary>
		/// Name of the profiler marker that measures each invocation of a listener method, in the Editor and in
		/// development builds. Listeners of the same method share the marker, whichever event or target they belong to.
		/// Tools that read these timings (e.g. with a ProfilerRecorder) find the marker by this name.
		/// </summary>
		public static string GetProfilerMarkerName(System.Reflection.MethodInfo method)
		{
			var typeName = method.DeclaringType?.Name ?? "Unknown";
			return $"{typeName}.{method.Name}";
		}

		#endregion
	}

}
