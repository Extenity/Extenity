using System;
using UnityEngine;
using Object = UnityEngine.Object;

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

		#region GameObject Name Safe

		public static string GameObjectNameSafe(this Component me)
		{
			if (me == null)
				return NullComponentName;
			return me.gameObject.name;
		}

		public static string NameSafe(this GameObject me)
		{
			if (me == null)
				return NullGameObjectName;
			return me.name;
		}

		#endregion

		#region Delegate Naming

		public static string FullNameOfTarget(this Delegate del, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			return FullObjectName(del?.Target, maxHierarchyLevels);
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

		#region Full Name

		public static string FullName(this GameObject me, int maxHierarchyLevels = DefaultMaxHierarchyLevels, char separator = '/')
		{
			if (!me || maxHierarchyLevels <= 0)
				return NullGameObjectName;
			var name = me.name;
			var parent = me.transform.parent;
			maxHierarchyLevels--;
			while (maxHierarchyLevels > 0 && parent)
			{
				name = parent.name + separator + name;
				parent = parent.parent;
				maxHierarchyLevels--;
			}
			return parent
				? "..." + separator + name
				: name;
		}

		public static string FullName(this Component me, int maxHierarchyLevels = DefaultMaxHierarchyLevels, char gameObjectNameSeparator = '/', char componentNameSeparator = '|')
		{
			if (!me)
				return NullComponentName;
			return me.gameObject.FullName(maxHierarchyLevels, gameObjectNameSeparator) + componentNameSeparator + me.GetType().Name;
		}

		public static string FullGameObjectName(this Component me, int maxHierarchyLevels = DefaultMaxHierarchyLevels, char separator = '/')
		{
			if (!me)
				return NullGameObjectName; // Note that we are interested in gameobject name rather than component name. So we return NullGameObjectName instead of NullComponentName.
			return me.gameObject.FullName(maxHierarchyLevels, separator);
		}

		public static string FullObjectName(this object me, int maxHierarchyLevels = DefaultMaxHierarchyLevels, char gameObjectNameSeparator = '/', char componentNameSeparator = '|')
		{
			if (me is Component)
			{
				var asComponent = me as Component;
				return asComponent
					? asComponent.FullName(maxHierarchyLevels, gameObjectNameSeparator: gameObjectNameSeparator, componentNameSeparator: componentNameSeparator)
					: NullComponentName;
			}
			if (me is GameObject)
			{
				var asGameObject = me as GameObject;
				return asGameObject
					? asGameObject.FullName(maxHierarchyLevels, separator: gameObjectNameSeparator)
					: NullGameObjectName;
			}
			if (me is Object)
			{
				var asObject = me as Object;
				return asObject
					? asObject.ToString()
					: NullObjectName;
			}
			if (me is Delegate)
			{
				var asDelegate = me as Delegate;
				return asDelegate != null
					? asDelegate.FullNameOfTargetAndMethod()
					: NullDelegateName;
			}
			return me != null
				? me.ToString()
				: NullName;
		}

		#endregion
	}

}
