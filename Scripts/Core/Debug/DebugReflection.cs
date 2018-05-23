using System;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace Extenity.DebugToolbox
{

	public static class DebugReflection
	{
		#region Stack

		public static string StackTraceToString(int skipFrames = 1)
		{
			StringBuilder sb = new StringBuilder(256);
			StackFrame[] frames = new System.Diagnostics.StackTrace(skipFrames).GetFrames();
			for (int i = 0; i < frames.Length; i++)
			{
				MethodBase method = frames[i].GetMethod();
				sb.Append($"{(method.ReflectedType != null ? method.ReflectedType.Name : string.Empty)}::{method.Name}\n");
			}
			return sb.ToString();
		}

		#endregion

		#region Methods

		public static string CurrentMethodName
		{
			get
			{
				StackFrame stackframe = new StackFrame(1, true);
				return stackframe.GetMethod().Name;
			}
		}
		public static string PreviousMethodName
		{
			get
			{
				StackFrame stackframe = new StackFrame(2, true);
				return stackframe.GetMethod().Name;
			}
		}
		public static string PrePreviousMethodName
		{
			get
			{
				StackFrame stackframe = new StackFrame(3, true);
				return stackframe.GetMethod().Name;
			}
		}

		public static string CurrentMethodNameWithType
		{
			get
			{
				StackFrame stackframe = new StackFrame(1, true);
				return stackframe.GetMethod().ReflectedType.Name + "." + stackframe.GetMethod().Name;
			}
		}
		public static string PreviousMethodNameWithType
		{
			get
			{
				StackFrame stackframe = new StackFrame(2, true);
				return stackframe.GetMethod().ReflectedType.Name + "." + stackframe.GetMethod().Name;
			}
		}
		public static string PrePreviousMethodNameWithType
		{
			get
			{
				StackFrame stackframe = new StackFrame(3, true);
				return stackframe.GetMethod().ReflectedType.Name + "." + stackframe.GetMethod().Name;
			}
		}

		public static string CurrentMethodType
		{
			get
			{
				StackFrame stackframe = new StackFrame(1, true);
				return stackframe.GetMethod().ReflectedType.Name;
			}
		}
		public static string PreviousMethodType
		{
			get
			{
				StackFrame stackframe = new StackFrame(2, true);
				return stackframe.GetMethod().ReflectedType.Name;
			}
		}
		public static string PrePreviousMethodType
		{
			get
			{
				StackFrame stackframe = new StackFrame(3, true);
				return stackframe.GetMethod().ReflectedType.Name;
			}
		}

		#endregion

		#region Variables

		public static string GetVariableName<T>(Expression<Func<T>> expr)
		{
			var body = (MemberExpression)expr.Body;
			return body.Member.Name;
		}

		#endregion

		#region Source Code

		public static int CurrentLine
		{
			get
			{
				StackFrame stackframe = new StackFrame(1, true);
				return stackframe.GetFileLineNumber();
			}
		}
		public static string CurrentFile
		{
			get
			{
				StackFrame stackframe = new StackFrame(1, true);
				return stackframe.GetFileName();
			}
		}
		public static string CurrentFileAndLine
		{
			get
			{
				StackFrame stackframe = new StackFrame(1, true);
				return stackframe.GetFileName() + " (" + stackframe.GetFileLineNumber() + ")";
			}
		}

		#endregion
	}

}
