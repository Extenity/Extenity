using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class GameObjectUtilityTools
	{
		#region Static Flags

		public static bool IsStaticEditorFlagSet(this GameObject gameObject, StaticEditorFlags flag)
		{
			return (gameObject.GetStaticEditorFlags() & flag) == flag;
		}

		public static bool IsStaticFlagSet(this GameObject gameObject, StaticFlags flag)
		{
			return (gameObject.GetStaticFlags() & flag) == flag;
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
