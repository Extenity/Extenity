// #define DisableConsistencyCheckerProfiling
// #define DisableConsistencyCheckerDetailedProfiling

#if !DisableConsistencyCheckerProfiling
#define _ProfilingEnabled
#if !DisableConsistencyCheckerDetailedProfiling
#define _DetailedProfilingEnabled
#endif
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Extenity.DataToolbox;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using Exception = System.Exception;
using IDisposable = System.IDisposable;
using Type = System.Type;

#if _ProfilingEnabled
using System.Linq;
using Extenity.ApplicationToolbox;
#if UNITY
using UnityEngine.Pool;
#endif
#endif

// Unlike any other ContextObject definitions, ConsistencyChecker always uses System.Object type in all platforms.
// But lets keep using ContextObject naming conventions like other systems that may use varying Log context types.
// See 11746845.
#if UNITY
using ContextObject = UnityEngine.Object;
#else
using ContextObject = System.Object;
#endif
using ConsistencyContextObject = System.Object;

namespace Extenity.ConsistencyToolbox
{

	public readonly struct InconsistencyEntry
	{
		public readonly string Message;
		public readonly ConsistencyContextObject Target;
		public readonly bool IsError;

		internal InconsistencyEntry(string message, ConsistencyContextObject target, bool isError)
		{
			Target = target;
			Message = message;
			IsError = isError;
		}

		public override string ToString()
		{
			return (IsError ? "Error: " : "Warning: ") + Message + " (" + Target.FullObjectName() + ")";
		}
	}

	public interface IConsistencyChecker
	{
		void CheckConsistency(ConsistencyChecker checker);
	}

	public enum ThrowRule
	{
		NoThrow = 1,
		OnlyOnErrors = 2,
		OnErrorsAndWarnings = 3,
	}

	public class ConsistencyChecker : IDisposable
	{
		#region Data

		private List<InconsistencyEntry> _Inconsistencies;
		public IReadOnlyList<InconsistencyEntry> Inconsistencies => _Inconsistencies;
		public ConsistencyContextObject MainContextObject;
		public ConsistencyContextObject CurrentCallerContextObject;
		public string CurrentPath;

		public int InconsistencyCount => _Inconsistencies != null ? _Inconsistencies.Count : 0;
		public bool HasAnyInconsistencies => _Inconsistencies != null && _Inconsistencies.Count > 0;

		public int ErrorCount
		{
			get
			{
				var total = 0;
				if (_Inconsistencies != null)
				{
					foreach (var inconsistency in _Inconsistencies)
					{
						if (inconsistency.IsError)
						{
							total++;
						}
					}
				}
				return total;
			}
		}

		public int WarningCount
		{
			get
			{
				var total = 0;
				if (_Inconsistencies != null)
				{
					foreach (var inconsistency in _Inconsistencies)
					{
						if (!inconsistency.IsError)
						{
							total++;
						}
					}
				}
				return total;
			}
		}

		public bool HasAnyErrors
		{
			get
			{
				if (_Inconsistencies != null)
				{
					foreach (var inconsistency in _Inconsistencies)
					{
						if (inconsistency.IsError)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool HasAnyWarning
		{
			get
			{
				if (_Inconsistencies != null)
				{
					foreach (var inconsistency in _Inconsistencies)
					{
						if (!inconsistency.IsError)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#endregion

		#region Initialization / Deinitialization

		public ConsistencyChecker(string overriddenTitleName, ConsistencyContextObject mainContextObject, float thresholdDurationToConsiderLogging)
			: this(mainContextObject, thresholdDurationToConsiderLogging)
		{
			OverriddenTitleName = overriddenTitleName;
		}

		public ConsistencyChecker(ConsistencyContextObject mainContextObject, float thresholdDurationToConsiderLogging)
		{
			MainContextObject = mainContextObject;
			IsInstantLoggingEnabled = true;
			InitializeProfiling(thresholdDurationToConsiderLogging);
		}

		private void InitializeEntriesIfRequired()
		{
			if (_Inconsistencies == null)
			{
				_Inconsistencies = New.List<InconsistencyEntry>();
			}
		}

		public void Dispose()
		{
			MainContextObject = default;
			CurrentCallerContextObject = default;

			if (_Inconsistencies != null)
			{
				Release.List(ref _Inconsistencies);
			}

			DeinitializeProfiling();
		}

		#endregion

		#region Add Consistency Entry

		public void AddError(string message, ConsistencyContextObject overrideContext)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(AppendPathToMessage(message), overrideContext, isError: true));
			if (IsInstantLoggingEnabled)
			{
				Log.Error(_Inconsistencies[^1].Message, _Inconsistencies[^1].Target as ContextObject);
			}
		}

		public void AddError(string message)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(AppendPathToMessage(message), CurrentCallerContextObject, isError: true));
			if (IsInstantLoggingEnabled)
			{
				Log.Error(_Inconsistencies[^1].Message, _Inconsistencies[^1].Target as ContextObject);
			}
		}

		public void AddWarning(string message, ConsistencyContextObject overrideContext)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(AppendPathToMessage(message), overrideContext, isError: false));
			if (IsInstantLoggingEnabled)
			{
				Log.Warning(_Inconsistencies[^1].Message, _Inconsistencies[^1].Target as ContextObject);
			}
		}

		public void AddWarning(string message)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(AppendPathToMessage(message), CurrentCallerContextObject, isError: false));
			if (IsInstantLoggingEnabled)
			{
				Log.Warning(_Inconsistencies[^1].Message, _Inconsistencies[^1].Target as ContextObject);
			}
		}

		#endregion

		#region Check Consistency

		public static ConsistencyChecker CheckConsistency(IConsistencyChecker target, float thresholdDurationToConsiderLogging)
		{
			var checker = new ConsistencyChecker(target, thresholdDurationToConsiderLogging);
			checker.IsInstantLoggingEnabled = false;
			checker.ProceedTo(checker.GetContextObjectLogName(target), target);
			checker.Finalize(false, true, ThrowRule.NoThrow);
			return checker;
		}

		public static ConsistencyChecker CheckConsistencyAndLog(IConsistencyChecker target, float thresholdDurationToConsiderLogging)
		{
			var checker = new ConsistencyChecker(target, thresholdDurationToConsiderLogging);
			// checker.IsInstantLoggingEnabled = true; Already set in constructor.
			checker.ProceedTo(checker.GetContextObjectLogName(target), target);
			checker.Finalize(true, true, ThrowRule.NoThrow);
			return checker;
		}

		public static ConsistencyChecker CheckConsistencyAndThrow(IConsistencyChecker target, float thresholdDurationToConsiderLogging, ThrowRule throwRule)
		{
			var checker = new ConsistencyChecker(target, thresholdDurationToConsiderLogging);
			// checker.IsInstantLoggingEnabled = true; Already set in constructor.
			checker.ProceedTo(checker.GetContextObjectLogName(target), target);
			checker.Finalize(true, true, throwRule);
			return checker;
		}

		public void Finalize(bool logTitle, bool logProfilingInfoIfExceedsThreshold, ThrowRule throwRule)
		{
			if (logTitle)
			{
				LogTitle();
			}
			if (logProfilingInfoIfExceedsThreshold)
			{
				LogProfilingInfoIfExceedsThreshold();
			}
			Throw(throwRule);
		}

		#endregion

		#region Proceed To

		public void ProceedToArrayItem(string arrayFieldName, int arrayIndex, IConsistencyChecker nextTarget, bool setNextTargetAsContextObject = true)
		{
			ProceedTo(arrayFieldName + "[" + arrayIndex + "]", nextTarget, setNextTargetAsContextObject);
		}

		public void ProceedTo(string fieldName, IConsistencyChecker nextTarget, bool setNextTargetAsContextObject = true)
		{
			if (nextTarget == null)
			{
				AddError("Tried to do consistency check on a null object.");
				return;
			}

			var previousPath = CurrentPath;
			var previousContextObject = CurrentCallerContextObject;

			CurrentPath += string.IsNullOrEmpty(CurrentPath)
				? fieldName
				: "." + fieldName;
			if (setNextTargetAsContextObject)
			{
#if UNITY
				// Only accept UnityEngine.Object types as context objects when working in Unity.
				// That makes it easier to write consistency checks that start from a MonoBehaviour
				// and then proceed to its non-MonoBehaviour serialized fields. That way, any consistency
				// logs of these class objects would be logged using their MonoBehaviour as log context
				// and would highlight the GameObject when clicked on their logs in console.
				if (nextTarget is UnityEngine.Object nextTargetAsUnityObject)
				{
					CurrentCallerContextObject = nextTargetAsUnityObject;
				}
#else
				CurrentCallerContextObject = nextTarget;
#endif
			}

#if _DetailedProfilingEnabled
			var startTime = PrecisionTiming.PreciseTime;
#endif
			try
			{
				nextTarget.CheckConsistency(this);
			}
			catch (Exception exception)
			{
				AddError($"Threw an exception when processing consistency checks. Exception: {exception}");
			}
			finally
			{
#if _DetailedProfilingEnabled
				var duration = PrecisionTiming.PreciseTime - startTime;
				RegisterProfiling(nextTarget.GetType(), duration);
#endif
				CurrentPath = previousPath;
				CurrentCallerContextObject = previousContextObject;
			}
		}

		#endregion

		#region Log

		public bool IsInstantLoggingEnabled;

		public void LogAllInOnce()
		{
			if (HasAnyInconsistencies)
			{
				var stringBuilder = new StringBuilder();
				WriteFullLogTo(stringBuilder);
				if (HasAnyErrors)
				{
					Log.Error(stringBuilder.ToString(), MainContextObject as ContextObject);
				}
				else
				{
					Log.Warning(stringBuilder.ToString(), MainContextObject as ContextObject);
				}
			}
		}

		public void LogAll()
		{
			if (HasAnyInconsistencies)
			{
				LogInconsistencies();
				LogTitle();
			}
		}

		public void LogTitle()
		{
			if (HasAnyInconsistencies)
			{
				if (HasAnyErrors)
				{
					Log.Error(BuildTitleMessage(), MainContextObject as ContextObject);
				}
				else
				{
					Log.Warning(BuildTitleMessage(), MainContextObject as ContextObject);
				}
			}
		}

		public void LogInconsistencies()
		{
			if (HasAnyInconsistencies)
			{
				foreach (var inconsistency in _Inconsistencies)
				{
					if (inconsistency.IsError)
					{
						Log.Error(inconsistency.Message, inconsistency.Target as ContextObject);
					}
					else
					{
						Log.Warning(inconsistency.Message, inconsistency.Target as ContextObject);
					}
				}
			}
		}

		public void LogAllAndThrow(ThrowRule throwRule)
		{
			if (HasAnyInconsistencies)
			{
				LogAll();
				Throw(throwRule);
			}
		}

		public void Throw(ThrowRule throwRule)
		{
			if (HasAnyInconsistencies)
			{
				switch (throwRule)
				{
					case ThrowRule.NoThrow:
						return;

					case ThrowRule.OnlyOnErrors:
						if (!HasAnyErrors)
						{
							return;
						}
						break; // Proceed to throw;

					case ThrowRule.OnErrorsAndWarnings:
						break; // Proceed to throw;

					default:
						throw new ArgumentOutOfRangeException(nameof(throwRule), throwRule, null);
				}

				throw new Exception(BuildTitleMessage() + " See previous logs for details.");
			}
		}

		public void WriteFullLogTo(StringBuilder stringBuilder)
		{
			if (HasAnyInconsistencies)
			{
				foreach (var inconsistency in _Inconsistencies)
				{
					stringBuilder.AppendLine(inconsistency.ToString());
				}

				stringBuilder.AppendLine(BuildTitleMessage());
			}
		}

		#endregion

		#region Log Title

		public string OverriddenTitleName;

		private string TitleName
		{
			get => !string.IsNullOrEmpty(OverriddenTitleName)
				? OverriddenTitleName
				: GetContextObjectLogName(MainContextObject);
		}

		private string BuildTitleMessage()
		{
			return $"'{TitleName}' has {InconsistencyCount.ToStringWithEnglishPluralWord("inconsistency", "inconsistencies")}.";
		}

		#endregion

		#region Log Tools

		public string GetContextObjectLogName(ConsistencyContextObject context)
		{
#if UNITY
			// Try to get Unity Object info.
			var meAsUnityObject = context as UnityEngine.Object;
			if (meAsUnityObject != null)
			{
				return meAsUnityObject.FullObjectName();
			}
#endif
			if (context != null)
			{
				var meType = context.GetType();
				return meType.FullName;
			}
			return "[Null]";
		}

		private string AppendPathToMessage(string message)
		{
			return message + '\n' + CurrentPath;
		}

		#endregion

		#region Profiling

#if _ProfilingEnabled
		private float ThresholdDurationToConsiderLogging;
		private double MainStartTime;
		private double MainDuration => PrecisionTiming.PreciseTime - MainStartTime;
#endif
#if _DetailedProfilingEnabled
		private Dictionary<Type, double> ProfilingTimes;
#endif

		[Conditional("_ProfilingEnabled")]
		private void InitializeProfiling(float thresholdDurationToConsiderLogging)
		{
#if _ProfilingEnabled
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			MainStartTime = PrecisionTiming.PreciseTime;
#endif
#if _DetailedProfilingEnabled
#if UNITY
			ProfilingTimes = DictionaryPool<Type, double>.Get();
#else
			ProfilingTimes = new Dictionary<Type, double>();
#endif
#endif
		}

		[Conditional("_DetailedProfilingEnabled")]
		private void DeinitializeProfiling()
		{
#if _DetailedProfilingEnabled
#if UNITY
			DictionaryPool<Type, double>.Release(ProfilingTimes);
			ProfilingTimes = null;
#else
			ProfilingTimes.Clear();
			ProfilingTimes = null;
#endif
#endif
		}

		[Conditional("_DetailedProfilingEnabled")]
		private void RegisterProfiling(Type type, double duration)
		{
#if _DetailedProfilingEnabled
			if (ProfilingTimes.TryGetValue(type, out var alreadyRegisteredTotalTime))
			{
				ProfilingTimes[type] = alreadyRegisteredTotalTime + duration;
			}
			else
			{
				ProfilingTimes.Add(type, duration);
			}
#endif
		}

		[Conditional("_ProfilingEnabled")]
		private void LogProfilingInfoIfExceedsThreshold()
		{
#if _ProfilingEnabled
			var mainDuration = MainDuration;
			if (mainDuration > ThresholdDurationToConsiderLogging)
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append($"'{TitleName}' process took '{mainDuration.ToStringMinutesSecondsMillisecondsFromSeconds()}' which is more than expected.");

#if _DetailedProfilingEnabled
				stringBuilder.AppendLine(" Details:");

				var results = ProfilingTimes.OrderByDescending(item => item.Value).Select(item => (Duration: item.Value, Type: item.Key)).ToList();

				foreach (var result in results)
				{
					stringBuilder.AppendLine($"   {result.Duration:F3} ms total for {result.Type.Name}");
				}
#endif

				Log.Warning(stringBuilder.ToString(), MainContextObject as ContextObject);
			}
#endif
		}

		#endregion
	}

}
