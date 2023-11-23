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
		public const string NullDelegateNameWithMethod_Start = "[NA/DEL/";
		public const string NullDelegateNameWithMethod_End = "]";
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

		private const string MethodAndTargetSeparator = " in ";

		public static string FullNameOfTarget(this Delegate del, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			return FullObjectName(del?.Target, maxHierarchyLevels);
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (del != null)
			{
#if UNITY
				if (del.IsUnityObjectTargetedAndDestroyed())
				{
					return NullDelegateNameWithMethod_Start + del.Method.Name + NullDelegateNameWithMethod_End;
				}
				else
#endif
				{
					return del.Method.Name + MethodAndTargetSeparator + FullNameOfTarget(del, maxHierarchyLevels);
				}
			}
			return NullDelegateName;
		}

		#endregion

		#region Full Name

#if UNITY
		private const char GameObjectPathSeparator = '/';
		private const char ComponentPathSeparator = '|';

		public static string FullName(this GameObject me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (!me || maxHierarchyLevels <= 0)
				return NullGameObjectName;

			var stringBuilder = StringTools.SharedStringBuilder.Value;
			StringTools.ClearSharedStringBuilder(stringBuilder);

			stringBuilder.Append(me.name);
			var parent = me.transform.parent;
			maxHierarchyLevels--;
			while (maxHierarchyLevels > 0 && parent)
			{
				stringBuilder.Insert(0, GameObjectPathSeparator);
				stringBuilder.Insert(0, parent.name);
				parent = parent.parent;
				maxHierarchyLevels--;
			}

			if (parent)
			{
				stringBuilder.Insert(0, GameObjectPathSeparator);
				stringBuilder.Insert(0, "...");
			}

			return stringBuilder.ToString();
		}

		public static string FullName(this Component me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (!me)
				return NullComponentName;
			return me.gameObject.FullName(maxHierarchyLevels) + ComponentPathSeparator + me.GetType().Name;
		}

		public static string FullGameObjectName(this Component me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (!me)
				return NullGameObjectName; // Note that we are interested in gameobject name rather than component name. So we return NullGameObjectName instead of NullComponentName.
			return me.gameObject.FullName(maxHierarchyLevels);
		}
#endif

		public static string FullObjectName(this object me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			switch (me)
			{
#if UNITY
				case Component component:
					return component.FullName(maxHierarchyLevels);
				case GameObject gameObject:
					return gameObject.FullName(maxHierarchyLevels);
				case UnityEngine.Object unityObject:
					return unityObject
						? unityObject.ToString()
						: NullObjectName;
#endif
				case Delegate asDelegate:
					return asDelegate.FullNameOfTargetAndMethod();
				default:
					return me != null
						? me.ToString()
						: NullName;
			}
		}

		#endregion
	}

}
