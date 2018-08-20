using System;
using System.Collections.Generic;
using Extenity.ApplicationToolbox;
using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	public static class CodeProfiler
	{
		public static readonly CodeProfilerEntry BaseEntry = new CodeProfilerEntry();
		public static CodeProfilerEntry CurrentEntry = BaseEntry;

		public static void BeginSample(int id)
		{
			if (!IsProfilingActive)
				return;

			// Set current entry as the child.
			CurrentEntry = CurrentEntry.GetOrAddChild(id);

			//Profiler.BeginSample(title);
			var now = PrecisionTiming.PreciseTime;
			CurrentEntry.StartTime = now;
		}

		public static void EndSample()
		{
			if (!IsProfilingActive)
				return;

			var now = PrecisionTiming.PreciseTime;
			//Profiler.EndSample();

			Debug.Assert(CurrentEntry.StartTime > 0.0);
			var duration = now - CurrentEntry.StartTime;
			if (duration < 0.0)
			{
				Debug.LogWarning($"Detected profiler time leap to backwards for '{duration}' seconds.");
				duration = 0.0;
			}
			CurrentEntry.StartTime = 0.0;
			CurrentEntry.LastDuration = duration;
			CurrentEntry.TotalDuration += duration;
			CurrentEntry.TotalCount++;

			// Set current entry as the parent.
			CurrentEntry = CurrentEntry.Parent;
		}

		#region Activation

		private static bool IsProfilingActive;
		private static int ProfilingActivator;

		public static void RequestAcivation()
		{
			ProfilingActivator++;
			IsProfilingActive = ProfilingActivator > 0;
		}

		public static void ReleaseAcivation()
		{
			ProfilingActivator--;
			IsProfilingActive = ProfilingActivator > 0;
		}

		#endregion

		#region Register Names

		public static readonly Dictionary<int, string> Labels = new Dictionary<int, string>(50);

		public static void RegisterEnumLabels<T>()
		{
			var enumEntries = Enum.GetValues(typeof(T));
			foreach (var enumEntry in enumEntries)
			{
				var id = (int)enumEntry;
				var label = enumEntry.ToString();
				RegisterLabel(id, label, true);
			}
		}

		public static void RegisterLabel(int id, string label, bool errorOnOverwrite)
		{
			if (errorOnOverwrite)
			{
				string alreadyExistingLabel;
				if (Labels.TryGetValue(id, out alreadyExistingLabel))
				{
					Debug.LogError($"Overwriting profiling label '{label}' with ID '{id}' which previously was '{alreadyExistingLabel}'.");
				}
			}
			Labels[id] = label;
		}

		public static void DeregisterLabel(int id)
		{
			Labels.Remove(id);
		}

		#endregion
	}

}
