using System;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	[CustomPropertyDrawer(typeof(ConditionalHideInInspectorAttribute), true)]
	public class ConditionalHideInInspectorAttributeDrawer : PropertyDrawer
	{
		#region GUI

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var theAttribute = (ConditionalHideInInspectorAttribute)attribute;
			var enabled = DecideIfEnabled(theAttribute, property);

			if (enabled || theAttribute.HideOrDisable == HideOrDisable.Disable)
			{
				var wasEnabled = GUI.enabled;
				GUI.enabled = enabled;
				EditorGUI.PropertyField(position, property, label, true);
				GUI.enabled = wasEnabled;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var theAttribute = (ConditionalHideInInspectorAttribute)attribute;
			var enabled = DecideIfEnabled(theAttribute, property);

			if (enabled || theAttribute.HideOrDisable == HideOrDisable.Disable)
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				// The property is not being drawn
				// We want to undo the spacing added before and after the property
				return -EditorGUIUtility.standardVerticalSpacing;
				//return 0.0f;
			}

			/*
			// Get the base height when not expanded
			var height = base.GetPropertyHeight(property, label);

			// If the property is expanded, go thru all the children and get their height
			if (property.isExpanded)
			{
				var propEnum = property.GetEnumerator();
				while (propEnum.MoveNext())
					height += EditorGUI.GetPropertyHeight((SerializedProperty)propEnum.Current, GUIContent.none, true);
			}
			return height;
			*/
		}

		#endregion

		private bool DecideIfEnabled(ConditionalHideInInspectorAttribute theAttribute, SerializedProperty property)
		{
			var propertyPath = property.propertyPath;

			// Check if this is an array item. Remove the array part from the end of the path
			// which leaves us the path of the field that has the attribute.
			var dotIndex = propertyPath.LastIndexOf('.');
			if (propertyPath.Length > dotIndex + ".data[".Length &&
				dotIndex > ".Array".Length &&
				propertyPath.Substring(dotIndex - ".Array".Length, ".Array.data[".Length) == ".Array.data[")
			{
				var arrayStartIndex = propertyPath.LastIndexOf(".Array.data[", StringComparison.InvariantCulture);
				propertyPath = propertyPath.Substring(0, arrayStartIndex);
			}

			// Property path is the path to the field that has the attribute.
			// Replace the last part of the path with the FieldName specified in attribute.
			dotIndex = propertyPath.LastIndexOf('.');
			var variableFieldPropertyPath = dotIndex < 0
				? theAttribute.FieldName
				: propertyPath.Substring(0, dotIndex + 1) + theAttribute.FieldName;

			// Get the field which will be used to checked for condition.
			var variableFieldProperty = property.serializedObject.FindProperty(variableFieldPropertyPath);

			// Check condition.
			bool enabled;
			if (variableFieldProperty != null)
			{
				enabled = variableFieldProperty.CheckEquals(theAttribute.ExpectedValue);
			}
			else
			{
				enabled = false;
				Debug.LogErrorFormat("Conditional hiding failed. Field '{0}' does not exist at property path '{1}'.", theAttribute.FieldName, variableFieldPropertyPath);
			}

			return theAttribute.Inverse
				? !enabled
				: enabled;
		}
	}

}
