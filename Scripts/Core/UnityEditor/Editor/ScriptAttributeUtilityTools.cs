using System;
using Extenity.ReflectionToolbox;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class ScriptAttributeUtilityTools
	{
		#region ScriptAttributeUtility Exposed Internals

		static ScriptAttributeUtilityTools()
		{
			// TODO: Update that in new Unity versions.
			// Revealed internals (Unity version 2018.1.1f1)
			var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
			if (type == null)
				throw new Exception("Internal error 18672-2!");

			type.GetStaticMethodAsFunc("GetHandler", out _GetHandler);
		}

		//internal static PropertyHandler GetHandler(SerializedProperty property)

		private static readonly Func<SerializedProperty, /*PropertyHandler*/object> _GetHandler; // Can't use PropertyHandler because it's internal.

		public static /*PropertyHandler*/object GetHandler(SerializedProperty property) { return _GetHandler(property); } // Can't use PropertyHandler because it's internal.

		#endregion
	}

}
