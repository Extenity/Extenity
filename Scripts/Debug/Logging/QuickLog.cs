using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class QuickLog
{
	#region Data

	public StringBuilder StringBuilder;

	public void Clear(bool deallocate = false)
	{
		if (StringBuilder != null)
		{
			if (deallocate)
			{
				StringBuilder = null;
			}
			else
			{
				StringBuilder.Length = 0;
			}
		}
	}

	private void CreateStringBuilderIfNecessary()
	{
		if (StringBuilder == null)
		{
			StringBuilder = new StringBuilder();
		}
	}

	#endregion

	#region Add Text

	public void Append(string text)
	{
		CreateStringBuilderIfNecessary();

		StringBuilder.Append(text);
	}

	public void AppendLine(string text)
	{
		CreateStringBuilderIfNecessary();

		StringBuilder.AppendLine(text);
	}

	public void AppendFormat(string format, params object[] args)
	{
		CreateStringBuilderIfNecessary();

		StringBuilder.AppendFormat(format, args);
	}

	public void AppendLineFormat(string format, params object[] args)
	{
		CreateStringBuilderIfNecessary();

		StringBuilder.AppendLine(string.Format(format, args));
	}

	#endregion

	#region Result

	public string Text
	{
		get
		{
			if (StringBuilder == null)
			{
				return "";
			}
			return StringBuilder.ToString();
		}
	}

	#endregion

	#region Loggers

	public static readonly QuickLog Logger1 = new QuickLog();
	public static readonly QuickLog Logger2 = new QuickLog();
	public static readonly QuickLog Logger3 = new QuickLog();

	#endregion
}
