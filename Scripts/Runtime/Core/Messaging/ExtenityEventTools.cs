using System;
using Extenity.GameObjectToolbox;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public enum ListenerLifeSpan
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
				return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del);
			}
			return GameObjectTools.NullGameObjectNamePlaceholder;
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, int maxHierarchyLevels, string methodAndTargetSeparator = " in ")
		{
			if (del != null)
			{
				return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del, maxHierarchyLevels);
			}
			return GameObjectTools.NullGameObjectNamePlaceholder;
		}

		#endregion
	}

}
