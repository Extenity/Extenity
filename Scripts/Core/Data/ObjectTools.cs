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
	}

}
