using Delegate = System.Delegate;
#if UNITY
using Component = UnityEngine.Component;
using GameObject = UnityEngine.GameObject;
#endif

namespace Extenity.DataToolbox
{

	public static class NamingTools
	{
		#region Default Names

#if UNITY
		public const string NullGameObjectName = "[NA/GO]";
		public const string NullComponentName = "[NA/COM]";
		public const string NullObjectName = "[NA/OBJ]";
#endif
		public const string NullDelegateName = "[NA/DEL]";
		public static string NullDelegateNameWithMethod(string methodName) => "[NA/DEL/" + methodName + "]";
		public const string NullName = "[NA]";

		#endregion

		#region Name In Hierarchy

		public const int DefaultMaxHierarchyLevels = 100;

		#endregion

		#region GameObject Name Safe

#if UNITY
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
#endif

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
#if UNITY
				if (del.IsUnityObjectTargetedAndDestroyed())
				{
					return NullDelegateNameWithMethod(del.Method.Name);
				}
				else
#endif
				{
					return del.Method.Name + methodAndTargetSeparator + FullNameOfTarget(del, maxHierarchyLevels);
				}
			}
			return NullDelegateName;
		}

		#endregion

		#region Full Name

#if UNITY
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
#endif

		public static string FullObjectName(this object me, int maxHierarchyLevels = DefaultMaxHierarchyLevels, char gameObjectNameSeparator = '/', char componentNameSeparator = '|')
		{
#if UNITY
			if (me is Component component)
			{
				return component
					? component.FullName(maxHierarchyLevels, gameObjectNameSeparator: gameObjectNameSeparator, componentNameSeparator: componentNameSeparator)
					: NullComponentName;
			}
			if (me is GameObject gameObject)
			{
				return gameObject
					? gameObject.FullName(maxHierarchyLevels, separator: gameObjectNameSeparator)
					: NullGameObjectName;
			}
			if (me is UnityEngine.Object unityObject)
			{
				return unityObject
					? unityObject.ToString()
					: NullObjectName;
			}
#endif
			if (me is Delegate asDelegate)
			{
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
