using System;
using System.Collections.Generic;
using Extenity.ApplicationToolbox;
using Extenity.DebugToolbox;
using Extenity.MessagingToolbox;

namespace Extenity.ProfilingToolbox
{

	public static class CodeProfiler
	{
		public static readonly CodeProfilerEntry BaseEntry = new CodeProfilerEntry();
		public static CodeProfilerEntry CurrentEntry = BaseEntry;

		public class EntryEvent : ExtenityEvent<CodeProfilerEntry> { }
		public static readonly EntryEvent OnEntryCreated = new EntryEvent();

		public static void ForeachChildren(CodeProfilerEntry parentEntry, Action<CodeProfilerEntry> onItem)
		{
			var children = parentEntry.Children;
			if (children != null)
			{
				for (int i = 0; i < children.Count; i++)
				{
					onItem(children[i]);
				}
			}
		}

		public static void ForeachChildrenRecursive(CodeProfilerEntry parentEntry, Action<CodeProfilerEntry> onItem)
		{
			var children = parentEntry.Children;
			if (children != null)
			{
				for (int i = 0; i < children.Count; i++)
				{
					onItem(children[i]);
					ForeachChildrenRecursive(children[i], onItem);
				}
			}
		}

		public static void ForeachAllEntries(Action<CodeProfilerEntry> onItem)
		{
			ForeachChildrenRecursive(BaseEntry, onItem);
		}


		public static void BeginSample(int id)
		{
			if (!IsProfilingActive)
				return;

			// Set current entry as the child.
			if (CurrentEntry.GetOrAddChild(id, out CurrentEntry))
			{
				OnEntryCreated.InvokeUnsafe(CurrentEntry);
			}

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

			DebugAssert.IsTrue(CurrentEntry.StartTime > 0.0);
			var duration = now - CurrentEntry.StartTime;
			if (duration < 0.0)
			{
				Log.Warning($"Detected profiler time leap to backwards for '{duration}' seconds.");
				duration = 0.0;
			}
			CurrentEntry.StartTime = 0.0;
			CurrentEntry.LastDuration = duration;
			CurrentEntry.TotalDuration += duration;
			CurrentEntry.TotalCount++;
			CurrentEntry.RunningAverageDuration.Push((float)duration);

			// Set current entry as the parent.
			CurrentEntry = CurrentEntry.Parent;
		}

		#region Activation

		private static bool IsProfilingActive;
		private static int ProfilingActivator;

		public static void RequestActivation()
		{
			ProfilingActivator++;
			IsProfilingActive = ProfilingActivator > 0;
		}

		public static void ReleaseActivation()
		{
			ProfilingActivator--;
			IsProfilingActive = ProfilingActivator > 0;
		}

		#endregion

		#region Register Labels

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
				if (Labels.TryGetValue(id, out var alreadyExistingLabel))
				{
					Log.Fatal($"Overwriting profiling label '{label}' with ID '{id}' which previously was '{alreadyExistingLabel}'.");
				}
			}
			Labels[id] = label;
		}

		public static void DeregisterLabel(int id)
		{
			Labels.Remove(id);
		}

		public static string GetLabelOrID(int id, bool automaticallyRegisterIfNotFound = true)
		{
			if (Labels.TryGetValue(id, out var label))
			{
				return label;
			}
			label = id.ToString();
			if (automaticallyRegisterIfNotFound)
			{
				const bool errorOnOverwrite = false; // We already know the label does not exist.
				RegisterLabel(id, label, errorOnOverwrite);
			}
			return label;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(CodeProfiler));

		#endregion
	}

}
