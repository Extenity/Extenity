using System;

namespace Extenity.DataToolbox
{

	public static class ObjectTools
	{
		public static T ThrowIfNull<T>(this T obj) where T : class
		{
			if (obj == null)
				throw new NullReferenceException();
			return obj;
		}

		public static T Cast<T>(this UnityEngine.Object obj) where T : UnityEngine.Object
		{
			return (T)obj;
		}

		// TODO: Update that in new Unity versions.
		// Copied directy from UnityEngine.Object.CheckNullArgument (Unity version 2017.3.1)
		public static void CheckNullArgument(object arg, string message)
		{
			if (arg == null)
				throw new ArgumentException(message);
		}
	}

}
