using System.Collections.Generic;
using System.Text;
using Extenity.DataToolbox;
using Exception = System.Exception;
using IDisposable = System.IDisposable;
using Type = System.Type;

// This is the way that Log system supports various Context types in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY
using ContextObject = UnityEngine.Object;
#else
using ContextObject = System.Object;
#endif

namespace Extenity.ConsistencyToolbox
{

	public readonly struct InconsistencyEntry
	{
		public readonly string Message;
		public readonly ContextObject Target;
		public readonly bool IsError;

		internal InconsistencyEntry(string message, ContextObject target, bool isError)
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

	public class ConsistencyChecker : IDisposable
	{
		#region Data

		private List<InconsistencyEntry> _Inconsistencies;
		public IReadOnlyList<InconsistencyEntry> Inconsistencies => _Inconsistencies;
		public ContextObject MainContextObject;
		public ContextObject CurrentCallerContextObject;

		public int InconsistencyCount => _Inconsistencies != null ? _Inconsistencies.Count : 0;
		public bool HasAnyInconsistencies => _Inconsistencies != null && _Inconsistencies.Count > 0;

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

		public ConsistencyChecker(ContextObject mainContextObject)
		{
			MainContextObject = mainContextObject;
			LogTitleWriterCallback = GenerateCommonTitleMessageForMainContextObject;
		}

		private void InitializeEntriesIfRequired()
		{
			if (_Inconsistencies == null)
			{
				_Inconsistencies = New.List<InconsistencyEntry>();
			}
		}

		private void Reset()
		{
			MainContextObject = default;
			CurrentCallerContextObject = default;

			if (_Inconsistencies != null)
			{
				Release.List(ref _Inconsistencies);
			}
		}

		public void Dispose()
		{
			Reset();
		}

		#endregion

		#region Add Consistency Entry

		public void AddError(string message, ContextObject overrideContext)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(message, overrideContext, isError: true));
		}

		public void AddError(string message)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(message, CurrentCallerContextObject, isError: true));
		}

		public void AddWarning(string message, ContextObject overrideContext)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(message, overrideContext, isError: false));
		}

		public void AddWarning(string message)
		{
			InitializeEntriesIfRequired();
			_Inconsistencies.Add(new InconsistencyEntry(message, CurrentCallerContextObject, isError: false));
		}

		#endregion

		#region Check Consistency

		public static ConsistencyChecker CheckConsistency(IConsistencyChecker target)
		{
			var checker = new ConsistencyChecker(target as ContextObject);
			checker.ProceedTo(target);
			return checker;
		}

		public static ConsistencyChecker CheckConsistencyAndLog(IConsistencyChecker target)
		{
			var checker = CheckConsistency(target);
			checker.LogAll();
			return checker;
		}

		public static ConsistencyChecker CheckConsistencyAndThrow(IConsistencyChecker target, bool throwOnlyOnErrors = false)
		{
			var checker = CheckConsistency(target);
			checker.LogAllAndThrow(throwOnlyOnErrors);
			return checker;
		}

		#endregion

		#region Proceed To

		public void ProceedTo(IConsistencyChecker nextTarget)
		{
			if (nextTarget == null)
			{
				AddError("Tried to do consistency check on a null object.");
				return;
			}

			var previousContextObject = CurrentCallerContextObject;
			if (nextTarget is UnityEngine.Object nextTargetAsUnityObject)
			{
				CurrentCallerContextObject = nextTargetAsUnityObject;
			}
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
				CurrentCallerContextObject = previousContextObject;
			}
		}

		#endregion

		#region Log

		public void LogAllInOnce()
		{
			if (HasAnyInconsistencies)
			{
				var stringBuilder = new StringBuilder();
				WriteFullLogTo(stringBuilder);
				if (HasAnyErrors)
				{
					Log.Error(stringBuilder.ToString());
				}
				else
				{
					Log.Warning(stringBuilder.ToString());
				}
			}
		}

		public void LogAll()
		{
			if (HasAnyInconsistencies)
			{
				var title = LogTitleWriterCallback(this);
				if (HasAnyErrors)
				{
					Log.Error(title);
				}
				else
				{
					Log.Warning(title);
				}

				foreach (var inconsistency in _Inconsistencies)
				{
					if (inconsistency.IsError)
					{
						Log.Error(inconsistency.Message);
					}
					else
					{
						Log.Warning(inconsistency.Message);
					}
				}
			}
		}

		public void LogAllAndThrow(bool throwOnlyOnErrors = false)
		{
			if (HasAnyInconsistencies)
			{
				LogAll();
				if (!throwOnlyOnErrors || HasAnyErrors)
				{
					var title = LogTitleWriterCallback(this);
					throw new Exception(title + " See previous logs for details.");
				}
			}
		}

		public void WriteFullLogTo(StringBuilder stringBuilder)
		{
			if (HasAnyInconsistencies)
			{
				stringBuilder.AppendLine(LogTitleWriterCallback(this));

				foreach (var inconsistency in _Inconsistencies)
				{
					stringBuilder.AppendLine(inconsistency.ToString());
				}
			}
		}

		#endregion

		#region Log Title / Custom log title writer callback

		public delegate string TitleWriterMethod(ConsistencyChecker checker);

		public TitleWriterMethod LogTitleWriterCallback;

		private static string GenerateCommonTitleMessageForMainContextObject(ConsistencyChecker checker)
		{
#if UNITY
			// Try to get Unity Object info.
			var meAsUnityObject = checker.MainContextObject as UnityEngine.Object;
			if (meAsUnityObject != null)
			{
				return $"'{meAsUnityObject.FullObjectName()}' has {checker.InconsistencyCount.ToStringWithEnglishPluralWord("inconsistency", "inconsistencies")}.";
			}
#endif
			if (checker.MainContextObject != null)
			{
				var meType = checker.MainContextObject.GetType();
				return $"'{meType.FullName}' has {checker.InconsistencyCount.ToStringWithEnglishPluralWord("inconsistency", "inconsistencies")}.";
			}
			else
			{
				return $"Detected {checker.InconsistencyCount.ToStringWithEnglishPluralWord("inconsistency", "inconsistencies")}.";
			}
		}

		#endregion
	}

}
