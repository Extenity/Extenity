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

#if UNITY
		public static T Cast<T>(this UnityEngine.Object obj) where T : UnityEngine.Object
		{
			return (T)obj;
		}
#endif

		// TODO MAINTENANCE: Update that in new Unity versions.
		// Originally copied from UnityEngine.Object.CheckNullArgument (Unity version 2020.2.0a11)
		public static void CheckNullArgument(object arg, string message)
		{
			if (arg == null)
				throw new System.ArgumentException(message);
		}
	}

}
