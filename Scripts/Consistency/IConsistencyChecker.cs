using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Extenity.Logging;
using Logger = Extenity.Logging.Logger;

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
			throw new ArgumentNullException("me");

		var errors = new List<ConsistencyError>();
		me.CheckConsistency(ref errors);
		return errors;
	}

	public static void CheckConsistencyAndLog(this IConsistencyChecker me, string titleMessage = null)
	{
		var errors = me.CheckConsistency();
		if (errors.Count > 0)
		{
			string message = "";
			if (!string.IsNullOrEmpty(titleMessage))
				message = titleMessage + "\n";
			message += errors.Serialize('\n');

			Logger.LogError(message);
		}
	}

	public static void CheckConsistencyAndThrow(this IConsistencyChecker me, string titleMessage = null)
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
}
