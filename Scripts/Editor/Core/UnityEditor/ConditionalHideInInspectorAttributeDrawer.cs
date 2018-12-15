using System;
using System.Reflection;
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
			var result = DecideIfEnabled(theAttribute, property);

			if (result != ConditionalHideResult.Hide)
			{
				var wasEnabled = GUI.enabled;
				GUI.enabled = result == ConditionalHideResult.Show;
				EditorGUI.PropertyField(position, property, label, true);
				GUI.enabled = wasEnabled;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var theAttribute = (ConditionalHideInInspectorAttribute)attribute;
			var result = DecideIfEnabled(theAttribute, property);

			if (result != ConditionalHideResult.Hide)
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				// Roll back what Unity has done for us.
				return -EditorGUIUtility.standardVerticalSpacing;
			}
		}

		#endregion

		#region Show/Hide Deciding

		// This method is abandoned since it can only resolve serialized fields. Though we also need non-serialized fields, properties and methods.
		//private SerializedProperty ResolveTargetedProperty(SerializedProperty property)
		//{
		//	// Check if this is an array item. Remove the array part from the end of the path
		//	// which leaves us the path of the field that has the attribute.
		//	var path = property.GetPropertyPathWithStrippedLastArrayPart();

		//	// Property path is the path to the field that has the attribute.
		//	// Replace the last part of the path with the FieldName specified in attribute.
		//	var dotIndex = path.LastIndexOf('.');
		//	var variableFieldPath = dotIndex < 0
		//		? FieldName
		//		: path.Substring(0, dotIndex + 1) + FieldName;

		//	// Get the field which will be used to check for condition.
		//	return property.serializedObject.FindProperty(variableFieldPath);
		//}

		public static ConditionalHideResult DecideIfEnabled(ConditionalHideInInspectorAttribute attribute, SerializedProperty propertyWithAttribute)
		{
			const BindingFlags bindingFlags =
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.FlattenHierarchy;

			object targetValue = null;
			bool enabled = false;
			bool compareValues;

			propertyWithAttribute.GetDeclaringTypeAndObject(out var declaringType, out var declaringObject);

			// See if this is a 'Field'
			var targetFieldInfo = declaringType.GetField(attribute.FieldPropertyMethodName, bindingFlags);
			if (targetFieldInfo != null)
			{
				// Make sure ExpectedValue is specified (See: 5761573)
				if (attribute.ExpectedValue == null)
				{
					throw new Exception($"{nameof(attribute.ExpectedValue)} to compare with '{attribute.FieldPropertyMethodName}' must be specified for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
				}
				// Make sure that 'Field' type is the same with ExpectedValue type (See: 5761574)
				if (attribute.ExpectedValue.GetType() != targetFieldInfo.FieldType)
				{
					throw new Exception($"Field '{attribute.FieldPropertyMethodName}' must be type of '{attribute.ExpectedValue.GetType()}' for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
				}
				targetValue = targetFieldInfo.GetValue(declaringObject);
				compareValues = true;
			}
			else
			{
				// See if this is a 'Property'
				var targetPropertyInfo = declaringType.GetProperty(attribute.FieldPropertyMethodName, bindingFlags);
				if (targetPropertyInfo != null)
				{
					// Make sure ExpectedValue is specified (See: 5761573)
					if (attribute.ExpectedValue == null)
					{
						throw new Exception($"{nameof(attribute.ExpectedValue)} to compare with '{attribute.FieldPropertyMethodName}' must be specified for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
					}
					// Make sure that 'Property' type is the same with ExpectedValue type (See: 5761574)
					if (attribute.ExpectedValue.GetType() != targetPropertyInfo.PropertyType)
					{
						throw new Exception($"Property '{attribute.FieldPropertyMethodName}' must be type of '{attribute.ExpectedValue.GetType()}' for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
					}
					targetValue = targetPropertyInfo.GetValue(declaringObject);
					compareValues = true;
				}
				else
				{
					// See if this is a 'Method' that has a parameter with type of the field that has the ConditionalHideInInspector attribute
					var targetMethodInfo = declaringType.GetMethod(attribute.FieldPropertyMethodName, bindingFlags, null, new Type[] { propertyWithAttribute.GetFieldInfo().FieldType }, null);
					if (targetMethodInfo != null)
					{
						// Make sure that 'Method' return type is ConditionalHideResult (See: 5761571)
						if (targetMethodInfo.ReturnType != typeof(bool))
						{
							throw new Exception($"Method '{attribute.FieldPropertyMethodName}' must have a return type of '{typeof(bool).Name}' for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
						}

						enabled = (bool)targetMethodInfo.Invoke(declaringObject, new object[] { propertyWithAttribute.GetValueAsObject() });
						compareValues = false;
					}
					else
					{
						// See if this is a 'Method' that has no parameter (See: 5761572)
						targetMethodInfo = declaringType.GetMethod(attribute.FieldPropertyMethodName, bindingFlags, null, new Type[0], null);
						if (targetMethodInfo != null)
						{
							// Make sure that 'Method' return type is ConditionalHideResult (See: 5761571)
							if (targetMethodInfo.ReturnType != typeof(bool))
							{
								throw new Exception($"Method '{attribute.FieldPropertyMethodName}' must have a return type of '{typeof(bool).Name}' for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
							}

							enabled = (bool)targetMethodInfo.Invoke(declaringObject, null);
							compareValues = false;
						}
						else
						{
							// Warn user if there is a 'Method' with the expected name but unexpected parameters (See: 5761572)
							targetMethodInfo = declaringType.GetMethod(attribute.FieldPropertyMethodName,
								BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
							if (targetMethodInfo != null)
							{
								throw new Exception($"Method '{attribute.FieldPropertyMethodName}' must have a parameter of type '{propertyWithAttribute.type}' or has no parameters for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
							}
							else
							{
								throw new Exception($"There is no Field, Property or Method with the name '{attribute.FieldPropertyMethodName}' for '{typeof(ConditionalHideInInspectorAttribute).Name}' to work.");
							}
						}
					}
				}
			}

			// Check condition.
			if (compareValues)
			{
				enabled = targetValue.Equals(attribute.ExpectedValue);
			}

			if (attribute.Inverse)
			{
				enabled = !enabled;
			}

			if (enabled)
				return ConditionalHideResult.Show;
			return attribute.HideOrDisable == HideOrDisable.Hide
				? ConditionalHideResult.Hide
				: ConditionalHideResult.ShowDisabled;
		}

		#endregion
	}

}
