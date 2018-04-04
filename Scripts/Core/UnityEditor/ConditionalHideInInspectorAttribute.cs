using System;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public enum HideOrDisable
	{
		Hide,
		Disable,
	}

	public enum ConditionalHideResult
	{
		Show,
		Hide,
		ShowDisabled,
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class ConditionalHideInInspectorAttribute : PropertyAttribute
	{
		public string FieldName;
		public object ExpectedValue;
		public bool Inverse = false;
		public HideOrDisable HideOrDisable = HideOrDisable.Hide;

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
		}

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue, HideOrDisable hideOrDisable)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
			HideOrDisable = hideOrDisable;
		}

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue, bool inverse)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
		}

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue, bool inverse, HideOrDisable hideOrDisable)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
			HideOrDisable = hideOrDisable;
		}

		private SerializedProperty ResolveTargetedProperty(SerializedProperty property)
		{
			// Check if this is an array item. Remove the array part from the end of the path
			// which leaves us the path of the field that has the attribute.
			var path = property.GetPropertyPathWithStrippedLastArrayPart();

			// Property path is the path to the field that has the attribute.
			// Replace the last part of the path with the FieldName specified in attribute.
			var dotIndex = path.LastIndexOf('.');
			var variableFieldPropertyPath = dotIndex < 0
				? FieldName
				: path.Substring(0, dotIndex + 1) + FieldName;

			// Get the field which will be used to check for condition.
			return property.serializedObject.FindProperty(variableFieldPropertyPath);
		}

		public ConditionalHideResult DecideIfEnabled(SerializedProperty propertyWithAttribute)
		{
			var targetedProperty = ResolveTargetedProperty(propertyWithAttribute);

			// Check condition.
			bool enabled;
			if (targetedProperty != null)
			{
				enabled = targetedProperty.CheckEquals(ExpectedValue);
			}
			else
			{
				enabled = false;
				Debug.LogErrorFormat("Conditional hiding failed. Could not resolve field '{0}' via property path '{1}'.", FieldName, propertyWithAttribute.propertyPath);
			}

			if (Inverse)
			{
				enabled = !enabled;
			}

			if (enabled)
				return ConditionalHideResult.Show;
			return HideOrDisable == HideOrDisable.Hide
				? ConditionalHideResult.Hide
				: ConditionalHideResult.ShowDisabled;
		}
	}

}
