#if UNITY

using System;
using Object = UnityEngine.Object;

namespace Extenity.DataToolbox
{

	public static class DelegateTools
	{
		public static bool IsUnityObjectTargeted(this Delegate me)
		{
			if (me == null)
				return false;
			return me.Target is Object;
		}

		public static bool IsUnityObjectTargetedAndAlive(this Delegate me)
		{
			if (me == null || !me.IsUnityObjectTargeted())
				return false;
			return me.Target as Object;
		}

		public static bool IsUnityObjectTargetedAndDestroyed(this Delegate me)
		{
			if (me == null || !me.IsUnityObjectTargeted())
				return false;
			return !(me.Target as Object);
		}
	}

}

#endif
