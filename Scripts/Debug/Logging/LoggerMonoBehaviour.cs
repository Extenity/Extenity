using System;
using UnityEngine;
using System.Collections;

public class LoggerMonoBehaviour : MonoBehaviour
{
	#region Initialization

	public void Initialize(string prefix, bool logToTextPad = true)
	{
		this.prefix = prefix;
		this.logToTextPad = logToTextPad;
	}

	public void Initialize(PrefixOverrider prefixOverrider, bool logToTextPad = true)
	{
		this.prefixOverrider = prefixOverrider;
		this.logToTextPad = logToTextPad;
	}

	#endregion

	#region Message Format

	public string prefix;

	public delegate string PrefixOverrider();
	public event PrefixOverrider prefixOverrider;

	public void SetPrefix(string prefix)
	{
		this.prefix = prefix;
	}

	private string CreateDebugLogMessage(string text)
	{
		if (prefixOverrider != null)
		{
			return prefixOverrider() + " : " + text;
		}

		return prefix + " : " + text;
	}

	#endregion

	#region Log Methods

	public bool logToTextPad = true;

	public void Log(string text)
	{
		var message = CreateDebugLogMessage(text);
		Debug.Log(message, gameObject);

		if (logToTextPad)
			TextPad.Write(message, false);
	}

	public void LogWarning(string text)
	{
		var message = CreateDebugLogMessage(text);
		Debug.LogWarning(message, gameObject);

		if (logToTextPad)
			TextPad.Write(message, false);
	}

	public void LogError(string text)
	{
		var message = CreateDebugLogMessage(text);
		Debug.LogError(message, gameObject);

		if (logToTextPad)
			TextPad.Write(message, false);
	}

	public void LogException(Exception exception)
	{
		Debug.LogException(exception, gameObject);

		if (logToTextPad)
			TextPad.Write(exception.Message, false);
	}

	#endregion
}
