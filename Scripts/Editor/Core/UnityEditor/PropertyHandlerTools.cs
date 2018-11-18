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
			// TODO: Update that in new Unity versions.
			// Revealed internals (Unity version 2018.1.1f1)
			var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.PropertyHandler");
			if (type == null)
				throw new InternalException(38672); // See 837379.

			type.GetMethodAsFunc("OnGUI", out _OnGUI);
		}

		//public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)

		private static readonly InstanceFunc<object, Rect, SerializedProperty, GUIContent, bool, bool> _OnGUI;

		public static bool OnGUI(/*PropertyHandler*/object propertyHandler, Rect position, SerializedProperty property, GUIContent label, bool includeChildren) { return _OnGUI(propertyHandler, position, property, label, includeChildren); }

		#endregion
	}

}
