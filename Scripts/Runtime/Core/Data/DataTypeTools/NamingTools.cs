using System;
using Extenity.GameObjectToolbox;

namespace Extenity.DataToolbox
{

	public static class NamingTools
	{
		#region Default Names

		public const string NullGameObjectName = "[NA/GO]";
		public const string NullComponentName = "[NA/COM]";
		public const string NullObjectName = "[NA/OBJ]";
		public const string NullDelegateName = "[NA/DEL]";
		public static string NullDelegateNameWithMethod(string methodName) => "[NA/DEL/" + methodName + "]";
		public const string NullName = "[NA]";

		#endregion

		#region Name In Hierarchy

		public const int DefaultMaxHierarchyLevels = 100;

		#endregion

		#region Delegate Naming

		public static string FullNameOfTarget(this Delegate del, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			return GameObjectTools.FullObjectName(del?.Target, maxHierarchyLevels);
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, int maxHierarchyLevels = DefaultMaxHierarchyLevels, string methodAndTargetSeparator = " in ")
		{
			if (del != null)
			{
				if (del.IsUnityObjectTargetedAndDestroyed())
				{
					return NullDelegateNameWithMethod(del.Method.Name);
				}
				else
				{
					return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del, maxHierarchyLevels);
				}
			}
			return NullDelegateName;
		}

		#endregion
	}

}
