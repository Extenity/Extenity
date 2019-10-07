using System;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public enum ListenerLifeSpan : byte
	{
		Permanent,
		RemovedAtFirstEmit,
	}

	public static class ExtenityEventTools
	{
		#region Log

		public static bool VerboseLogging = false; // Check this at the location of method call for performance reasons.

		#endregion

		#region ToString

		// TODO IMMEDIATE: Move these into NamingTools

		public static string FullNameOfTarget(this Delegate del)
		{
			return GameObjectTools.FullObjectName(del?.Target as Object);
		}

		public static string FullNameOfTarget(this Delegate del, int maxHierarchyLevels)
		{
			return GameObjectTools.FullObjectName(del?.Target as Object, maxHierarchyLevels);
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, string methodAndTargetSeparator = " in ")
		{
			if (del != null)
			{
				if (del.IsUnityObjectTargetedAndDestroyed())
				{
					return NamingTools.NullDelegateNameWithMethod(del.Method.Name);
				}
				else
				{
					return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del);
				}
			}
			return NamingTools.NullDelegateName;
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, int maxHierarchyLevels, string methodAndTargetSeparator = " in ")
		{
			if (del != null)
			{
				if (del.IsUnityObjectTargetedAndDestroyed())
				{
					return NamingTools.NullDelegateNameWithMethod(del.Method.Name);
				}
				else
				{
					return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del, maxHierarchyLevels);
				}
			}
			return NamingTools.NullDelegateName;
		}

		#endregion
	}

}
