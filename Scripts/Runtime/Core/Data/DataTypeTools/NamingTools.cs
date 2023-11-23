using Delegate = System.Delegate;
#if UNITY
using Cysharp.Text;
using Component = UnityEngine.Component;
using GameObject = UnityEngine.GameObject;
using Transform = UnityEngine.Transform;
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
			var stringBuilder = ZString.CreateStringBuilder(true);
			del.FullNameOfTarget(ref stringBuilder, maxHierarchyLevels);
			var result = stringBuilder.ToString();
			stringBuilder.Dispose();
			return result;
		}

		public static void FullNameOfTarget(this Delegate del, ref Utf16ValueStringBuilder stringBuilder, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			FullObjectName(del?.Target, ref stringBuilder, maxHierarchyLevels);
		}

		public static string FullNameOfTargetAndMethod(this Delegate del, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			var stringBuilder = ZString.CreateStringBuilder(true);
			del.FullNameOfTargetAndMethod(ref stringBuilder, maxHierarchyLevels);
			var result = stringBuilder.ToString();
			stringBuilder.Dispose();
			return result;
		}

		public static void FullNameOfTargetAndMethod(this Delegate del, ref Utf16ValueStringBuilder stringBuilder, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (del != null)
			{
#if UNITY
				if (del.IsUnityObjectTargetedAndDestroyed())
				{
					stringBuilder.Append(NullDelegateNameWithMethod_Start);
					stringBuilder.Append(del.Method.Name);
					stringBuilder.Append(NullDelegateNameWithMethod_End);
				}
				else
#endif
				{
					stringBuilder.Append(del.Method.Name);
					stringBuilder.Append(MethodAndTargetSeparator);
					FullNameOfTarget(del, ref stringBuilder, maxHierarchyLevels);
				}
			}
			else
			{
				stringBuilder.Append(NullDelegateName);
			}
		}

		#endregion

		#region Full Name

#if UNITY
		private const char GameObjectPathSeparator = '/';
		private const char ComponentPathSeparator = '|';

		public static string FullName(this GameObject me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			var stringBuilder = ZString.CreateStringBuilder(true);
			me.FullName(ref stringBuilder, maxHierarchyLevels);
			var result = stringBuilder.ToString();
			stringBuilder.Dispose();
			return result;
		}

		public static void FullName(this GameObject me, ref Utf16ValueStringBuilder stringBuilder, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (!me || maxHierarchyLevels <= 0)
			{
				stringBuilder.Append(NullGameObjectName);
				return;
			}

			var transform = me.transform;
			_IterateFullNameParents(ref stringBuilder, transform, maxHierarchyLevels);
		}

		private static void _IterateFullNameParents(ref Utf16ValueStringBuilder stringBuilder, Transform transform, int maxHierarchyLevels)
		{
			maxHierarchyLevels--;
			if (maxHierarchyLevels >= 0)
			{
				var parent = transform.parent;
				if (parent)
				{
					_IterateFullNameParents(ref stringBuilder, parent, maxHierarchyLevels);
					stringBuilder.Append(GameObjectPathSeparator);
					stringBuilder.Append(transform.name);
				}
				else
				{
					stringBuilder.Append(transform.name);
				}
			}
			else
			{
				stringBuilder.Append("...");
			}
		}

		public static string FullName(this Component me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			var stringBuilder = ZString.CreateStringBuilder(true);
			me.FullName(ref stringBuilder, maxHierarchyLevels);
			var result = stringBuilder.ToString();
			stringBuilder.Dispose();
			return result;
		}

		public static void FullName(this Component me, ref Utf16ValueStringBuilder stringBuilder, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (!me)
			{
				stringBuilder.Append(NullComponentName);
				return;
			}

			me.gameObject.FullName(ref stringBuilder, maxHierarchyLevels);
			stringBuilder.Append(ComponentPathSeparator);
			stringBuilder.Append(me.GetType().Name);
		}

		public static string FullGameObjectName(this Component me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			var stringBuilder = ZString.CreateStringBuilder(true);
			me.FullGameObjectName(ref stringBuilder, maxHierarchyLevels);
			var result = stringBuilder.ToString();
			stringBuilder.Dispose();
			return result;
		}

		public static void FullGameObjectName(this Component me, ref Utf16ValueStringBuilder stringBuilder, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			if (!me)
			{
				stringBuilder.Append(NullGameObjectName); // Note that we are interested in gameobject name rather than component name. So we return NullGameObjectName instead of NullComponentName.
				return;
			}

			me.gameObject.FullName(ref stringBuilder, maxHierarchyLevels);
		}
#endif

		public static string FullObjectName(this object me, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			var stringBuilder = ZString.CreateStringBuilder(true);
			me.FullObjectName(ref stringBuilder, maxHierarchyLevels);
			var result = stringBuilder.ToString();
			stringBuilder.Dispose();
			return result;
		}

		public static void FullObjectName(this object me, ref Utf16ValueStringBuilder stringBuilder, int maxHierarchyLevels = DefaultMaxHierarchyLevels)
		{
			switch (me)
			{
#if UNITY
				case Component component:
				{
					component.FullName(ref stringBuilder, maxHierarchyLevels);
					return;
				}
				case GameObject gameObject:
				{
					gameObject.FullName(ref stringBuilder, maxHierarchyLevels);
					return;
				}
				case UnityEngine.Object unityObject:
				{
					if (unityObject)
					{
						stringBuilder.Append(unityObject.ToString());
					}
					else
					{
						stringBuilder.Append(NullObjectName);
					}

					return;
				}
#endif
				case Delegate asDelegate:
				{
					asDelegate.FullNameOfTargetAndMethod(ref stringBuilder, maxHierarchyLevels);
					return;
				}
				default:
				{
					if (me != null)
					{
						stringBuilder.Append(me.ToString());
					}
					else
					{
						stringBuilder.Append(NullName);
					}

					return;
				}
			}
		}

		#endregion
	}

}