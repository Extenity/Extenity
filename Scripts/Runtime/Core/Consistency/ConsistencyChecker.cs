using System.Collections.Generic;
using System.Text;
using Extenity.DataToolbox;
using Exception = System.Exception;
using ArgumentNullException = System.ArgumentNullException;

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

	public struct InconsistencyEntry
	{
		public string Message;
		public ContextObject Target;
		public bool IsError;

		internal InconsistencyEntry(string message, ContextObject target, bool isError)
		{
			Target = target;
			Message = message;
			IsError = isError;
		}

		public override string ToString()
		{
			return (IsError ? "Error: " : "Warning: ") + Message;
		}
	}

	public interface IConsistencyChecker
	{
		void CheckConsistency(ConsistencyChecker checker);
	}

	public class ConsistencyChecker
	{
		#region Data

		public List<InconsistencyEntry> Inconsistencies;
		public ContextObject StartingContextObject;
		public ContextObject CurrentCallerContextObject;

		public bool HasAnyInconsistencies => Inconsistencies != null && Inconsistencies.Count > 0;

		public bool HasAnyErrors
		{
			get
			{
				if (Inconsistencies != null)
				{
					foreach (var inconsistency in Inconsistencies)
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
				if (Inconsistencies != null)
				{
					foreach (var inconsistency in Inconsistencies)
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

		private void InitializeEntriesIfRequired()
		{
			if (Inconsistencies == null)
			{
				Inconsistencies = New.List<InconsistencyEntry>();
			}
		}

		private void Reset()
		{
			StartingContextObject = default;
			CurrentCallerContextObject = default;

			if (Inconsistencies != null)
			{
				Release.List(ref Inconsistencies);
			}
		}

		#endregion

		#region Add Consistency Entry

		public void AddError(string message, ContextObject context)
		{
			InitializeEntriesIfRequired();
			Inconsistencies.Add(new InconsistencyEntry(message, context, isError: true));
		}

		public void AddError(string message)
		{
			InitializeEntriesIfRequired();
			Inconsistencies.Add(new InconsistencyEntry(message, CurrentCallerContextObject, isError: true));
		}

		public void AddWarning(string message, ContextObject context)
		{
			InitializeEntriesIfRequired();
			Inconsistencies.Add(new InconsistencyEntry(message, context, isError: false));
		}

		public void AddWarning(string message)
		{
			InitializeEntriesIfRequired();
			Inconsistencies.Add(new InconsistencyEntry(message, CurrentCallerContextObject, isError: false));
		}

		#endregion

		#region Check Consistency

		public static ConsistencyChecker CheckConsistency(IConsistencyChecker target)
		{
			var checker = new ConsistencyChecker();

			if (target == null)
				throw new ArgumentNullException(nameof(target), "Tried to do consistency check on a null object.");

			checker.StartingContextObject = checker.CurrentCallerContextObject = target as ContextObject;
			target.CheckConsistency(checker);

			return checker;
		}

		public static ConsistencyChecker CheckConsistencyAndLog(IConsistencyChecker target)
		{
			var checker = CheckConsistency(target);
			if (checker.HasAnyInconsistencies)
			{
				checker.LogAll();
			}
			return checker;
		}

		public static ConsistencyChecker CheckConsistencyAndThrow(IConsistencyChecker target, bool throwOnlyOnErrors = false)
		{
			var checker = CheckConsistency(target);
			if (checker.HasAnyInconsistencies)
			{
				checker.LogAll();
				if (!throwOnlyOnErrors || checker.HasAnyErrors)
				{
					var title = GenerateCommonTitleMessageForObject(checker.StartingContextObject, checker.Inconsistencies.Count);
					throw new Exception(title + " See previous logs for details.");
				}
			}
			return checker;
		}

		#endregion

		#region Proceed To

		public void ProceedTo(IConsistencyChecker nextTarget, ContextObject newContextObject = default)
		{
			CurrentCallerContextObject = newContextObject;
			nextTarget.CheckConsistency(this);
		}

		#endregion

		#region Log

		public void LogAllInOnce()
		{
			if (HasAnyInconsistencies)
			{
				var stringBuilder = new StringBuilder();
				var title = GenerateCommonTitleMessageForObject(StartingContextObject, Inconsistencies.Count);
				stringBuilder.Append(title);
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
				var title = GenerateCommonTitleMessageForObject(StartingContextObject, Inconsistencies.Count);
				if (HasAnyErrors)
				{
					Log.Error(title);
				}
				else
				{
					Log.Warning(title);
				}

				foreach (var inconsistency in Inconsistencies)
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

		public void WriteFullLogTo(StringBuilder stringBuilder)
		{
			if (HasAnyInconsistencies)
			{
				stringBuilder.AppendLine(GenerateCommonTitleMessageForObject(StartingContextObject, Inconsistencies.Count));

				foreach (var inconsistency in Inconsistencies)
				{
					stringBuilder.Append(inconsistency.IsError ? "Error: " : "Warning: ");
					stringBuilder.AppendLine(inconsistency.Message);
				}
			}
		}

		private static string GenerateCommonTitleMessageForObject(ContextObject me, int inconsistencyCount)
		{
#if UNITY
			// Try to get Unity Object info.
			var meAsUnityObject = me as UnityEngine.Object;
			if (meAsUnityObject != null)
			{
				return $"'{meAsUnityObject.FullObjectName()}' has {inconsistencyCount.ToStringWithEnglishPluralWord("inconsistency", "inconsistencies")}.";
			}
#endif
			if (me != null)
			{
				var meType = me.GetType();
				return $"'{meType.FullName}' has {inconsistencyCount.ToStringWithEnglishPluralWord("inconsistency", "inconsistencies")}.";
			}
			else
			{
				return "Tried to do consistency check on a null object.";
			}
		}

		#endregion
	}

}
