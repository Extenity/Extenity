using System;
using Extenity.GameObjectToolbox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public static class ExtenityEventTools
	{
		#region Log

		public static bool VerboseLogging = false;

		public static void LogVerbose(string message)
		{
			//if (!VerboseLogging) Check this at the location of method call for performance reasons.
			//	return;
			Debug.Log("<b><i>ExtEvent | </i></b>" + message);
		}

		public static void LogError(string message)
		{
			Debug.LogError("<b><i>ExtEvent | </i></b>" + message);
		}

		#endregion

		#region ToString

		public static string FullNameOfTarget(this Delegate del)
		{
			if (del == null)
				return "[NA]";
			var asComponent = del.Target as Component;
			if (asComponent)
			{
				return asComponent.FullName();
			}
			var asGameObject = del.Target as GameObject;
			if (asGameObject)
			{
				return asGameObject.FullName();
			}
			var asObject = del.Target as Object;
			if (asObject)
			{
				return asObject.ToString();
			}
			return "[NA]";
		}

		#endregion
	}

}
