using System;
using Extenity.GameObjectToolbox;
using UnityEngine;
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

		public static string FullNameOfTarget(this Delegate del, char separator = '/')
		{
			return FullNameOfTarget(del, 10000, separator);
		}

		public static string FullNameOfTarget(this Delegate del, int maxHierarchyLevels, char separator = '/')
		{
			if (del != null)
			{
				var asComponent = del.Target as Component;
				if (asComponent)
				{
					return asComponent.FullName(maxHierarchyLevels, separator);
				}
				var asGameObject = del.Target as GameObject;
				if (asGameObject)
				{
					return asGameObject.FullName(maxHierarchyLevels, separator);
				}
				var asObject = del.Target as Object;
				if (asObject)
				{
					return asObject.ToString();
				}
			}
			return "[NA]";
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, char separator = '/', string methodAndTargetSeparator = " in ")
		{
			return FullNameOfTargetAndMethod(del, 10000, separator, methodAndTargetSeparator);
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, int maxHierarchyLevels, char separator = '/', string methodAndTargetSeparator = " in ")
		{
			if (del != null)
			{
				return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del, maxHierarchyLevels, separator);
			}
			return "[NA]";
		}

		#endregion
	}

}
