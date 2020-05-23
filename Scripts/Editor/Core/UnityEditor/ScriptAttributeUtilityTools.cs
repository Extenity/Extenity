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
			// TODO MAINTENANCE: Update that in new Unity versions.
			// Revealed internals (Unity version 2020.2.0a11)
			var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
			if (type == null)
				throw new InternalException(1128672612); // See 117392721.

			type.GetStaticMethodAsFunc("GetHandler", out _GetHandler);
		}

		//internal static PropertyHandler GetHandler(SerializedProperty property)

		private static readonly Func<SerializedProperty, /*PropertyHandler*/object> _GetHandler; // Can't use PropertyHandler because it's internal.

		public static /*PropertyHandler*/object GetHandler(SerializedProperty property) { return _GetHandler(property); } // Can't use PropertyHandler because it's internal.

		#endregion
	}

}
