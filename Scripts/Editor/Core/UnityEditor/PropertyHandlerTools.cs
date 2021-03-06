using Extenity.DataToolbox;
using Extenity.ReflectionToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class PropertyHandlerTools
	{
		#region PropertyHandler Exposed Internals

		static PropertyHandlerTools()
		{
			// TODO MAINTENANCE: Update that in new Unity versions.
			// Revealed internals (Unity version 2020.2.0a11)
			var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.PropertyHandler");
			if (type == null)
				throw new InternalException(1138672612); // See 117392721.

			type.GetMethodAsFunc("OnGUI", out _OnGUI);
		}

		//public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)

		private static readonly InstanceFunc<object, Rect, SerializedProperty, GUIContent, bool, bool> _OnGUI;

		public static bool OnGUI(/*PropertyHandler*/object propertyHandler, Rect position, SerializedProperty property, GUIContent label, bool includeChildren) { return _OnGUI(propertyHandler, position, property, label, includeChildren); }

		#endregion
	}

}
