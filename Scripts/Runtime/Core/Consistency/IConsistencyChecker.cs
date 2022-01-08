using System.Collections.Generic;
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

	public struct ConsistencyError
	{
		public string Message;
		public object Target;
		public bool IsCritical;

		public ConsistencyError(object target, string message, bool isCritical = true)
		{
			Target = target;
			Message = message;
			IsCritical = isCritical;
		}

		public override string ToString()
		{
			return (IsCritical ? "Error: " : "Warning: ") + Message;
		}
	}

	public interface IConsistencyChecker
	{
		void CheckConsistency(ref List<ConsistencyError> errors);
	}

	public static class ConsistencyCheckerExtensions
	{
		public static List<ConsistencyError> CheckConsistency(this IConsistencyChecker me)
		{
			if (me == null)
				throw new ArgumentNullException(nameof(me), "Tried to do consistency check on a null object.");

			var errors = new List<ConsistencyError>();
			me.CheckConsistency(ref errors);
			return errors;
		}

		public static List<ConsistencyError> CheckConsistencyAndLog(this IConsistencyChecker me, ContextObject context = null)
		{
			return CheckConsistencyAndLog(me, SeverityCategory.Error, context);
		}

		public static List<ConsistencyError> CheckConsistencyAndLog(this IConsistencyChecker me, SeverityCategory severityCategory, ContextObject context = null)
		{
            var titleMessage = GenerateCommonTitleMessageForObject(me);
			return CheckConsistencyAndLog(me, titleMessage, severityCategory, context);
		}

		public static List<ConsistencyError> CheckConsistencyAndLog(this IConsistencyChecker me, string titleMessage, ContextObject context = null)
		{
			return CheckConsistencyAndLog(me, titleMessage, SeverityCategory.Error, context);
		}

		public static List<ConsistencyError> CheckConsistencyAndLog(this IConsistencyChecker me, string titleMessage, SeverityCategory severityCategory, ContextObject context = null)
		{
			var errors = me.CheckConsistency();
			if (errors.Count > 0)
			{
				string message = "";
				if (!string.IsNullOrEmpty(titleMessage))
					message = titleMessage + "\n";
				message += errors.Serialize('\n');

				Log.Severe(message, severityCategory, context);
			}
			return errors;
		}

		public static void CheckConsistencyAndThrow(this IConsistencyChecker me)
        {
            var titleMessage = GenerateCommonTitleMessageForObject(me);
			CheckConsistencyAndThrow(me, titleMessage);
		}

		public static void CheckConsistencyAndThrow(this IConsistencyChecker me, string titleMessage)
		{
			var errors = me.CheckConsistency();
			if (errors.Count > 0)
			{
				string message = "";
				if (!string.IsNullOrEmpty(titleMessage))
					message = titleMessage + "\n";
				message += errors.Serialize('\n');

				throw new Exception(message);
			}
		}

        private static string GenerateCommonTitleMessageForObject(IConsistencyChecker me)
        {
#if UNITY
            // Try to get Unity Object info.
			var meAsUnityObject = me as UnityEngine.Object;
            if (meAsUnityObject != null)
            {
                return $"'{meAsUnityObject.FullObjectName()}' has some inconsistencies.";
            }
#endif
	        if (me != null)
	        {
		        var meType = me.GetType();
		        return $"'{meType.FullName}' has some inconsistencies.";
	        }
	        else
	        {
		        return "Tried to do consistency check on a null object.";
	        }
		}
	}

}
