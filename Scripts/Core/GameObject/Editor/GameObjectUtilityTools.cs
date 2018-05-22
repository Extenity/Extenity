using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class GameObjectUtilityTools
	{
		#region Static Flags

		public static bool IsStaticEditorFlagsSetToAtLeast(this GameObject gameObject, StaticEditorFlags leastExpectedFlags)
		{
			return (GameObjectUtility.GetStaticEditorFlags(gameObject) & leastExpectedFlags) == leastExpectedFlags;
		}

		public static bool IsStaticFlagsSetToAtLeast(this GameObject gameObject, StaticFlags leastExpectedFlags)
		{
			return ((StaticFlags)GameObjectUtility.GetStaticEditorFlags(gameObject) & leastExpectedFlags) == leastExpectedFlags;
		}

		public static bool IsStaticEditorFlagsSetTo(this GameObject gameObject, StaticEditorFlags expectedFlags)
		{
			return GameObjectUtility.GetStaticEditorFlags(gameObject) == expectedFlags;
		}

		public static bool IsStaticFlagsSetTo(this GameObject gameObject, StaticFlags expectedFlags)
		{
			return (StaticFlags)GameObjectUtility.GetStaticEditorFlags(gameObject) == expectedFlags;
		}

		public static StaticEditorFlags GetStaticEditorFlags(this GameObject gameObject)
		{
			return GameObjectUtility.GetStaticEditorFlags(gameObject);
		}

		public static StaticFlags GetStaticFlags(this GameObject gameObject)
		{
			return (StaticFlags)GameObjectUtility.GetStaticEditorFlags(gameObject);
		}

		public static void SetStaticEditorFlags(this GameObject gameObject, StaticEditorFlags flags)
		{
			GameObjectUtility.SetStaticEditorFlags(gameObject, flags);
		}

		public static void SetStaticFlags(this GameObject gameObject, StaticFlags flags)
		{
			GameObjectUtility.SetStaticEditorFlags(gameObject, (StaticEditorFlags)flags);
		}

		#endregion
	}

}
