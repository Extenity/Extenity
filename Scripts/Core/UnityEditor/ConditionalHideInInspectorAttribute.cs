using System;
using System.Reflection;
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
		/// <summary>
		/// A Field, Property or Method name that will be used to decide if 
		/// the field that has ConditionalHideInInspector should be displayed
		/// or not. If a Field or a Property is specified, the decision 
		/// will be made based on if the result of Field or Property value 
		/// is the same with ExpectedValue. If a Method is specified, the 
		/// decision is made by the user in the specified method and returned
		/// as a boolean from that method, meaning 'true' corresponds to be shown.
		/// 
		/// If a Method is specified, the method must have a return type of bool. 
		/// Optionally, it may have a parameter that gets the field value or
		/// it may have no parameters.
		/// </summary>
		public string FieldPropertyMethodName;
		/// <summary>
		/// Expected value is compared with Field and Property values to decide if 
		/// the field should be displayed or not. Expected value is ineffective 
		/// when ConditionalHideInInspector attribute is used with a Method, 
		/// instead of a Field or a Property.
		/// </summary>
		public object ExpectedValue = null;
		/// <summary>
		/// Inverses the result of the decision whether the field that has the
		/// ConditionalHideInInspector should be enabled or disabled.
		/// </summary>
		public bool Inverse = false;
		/// <summary>
		/// Whether the field that has the ConditionalHideInInspector attribute
		/// should be totally hidden or shown as disabled when the condition is
		/// not met.
		/// </summary>
		public HideOrDisable HideOrDisable = HideOrDisable.Hide;

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue, HideOrDisable hideOrDisable)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
			HideOrDisable = hideOrDisable;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue, bool inverse)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue, bool inverse, HideOrDisable hideOrDisable)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
			HideOrDisable = hideOrDisable;
		}

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

		public ConditionalHideResult DecideIfEnabled(SerializedProperty propertyWithAttribute)
		{
			const BindingFlags bindingFlags =
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.FlattenHierarchy;

			object targetValue = null;
			bool enabled = false;
			bool compareValues;

			Type declaringType;
			object declaringObject;
			propertyWithAttribute.GetDeclaringTypeAndObject(out declaringType, out declaringObject);

			// See if this is a 'Field'
			var targetFieldInfo = declaringType.GetField(FieldPropertyMethodName, bindingFlags);
			if (targetFieldInfo != null)
			{
				// Make sure ExpectedValue is specified (See: 5761573)
				if (ExpectedValue == null)
				{
					throw new Exception(string.Format("{0} to compare with '{1}' must be specified for '{2}' to work.",
						nameof(ExpectedValue),
						FieldPropertyMethodName,
						typeof(ConditionalHideInInspectorAttribute).Name));
				}
				// Make sure that 'Field' type is the same with ExpectedValue type (See: 5761574)
				if (ExpectedValue.GetType() != targetFieldInfo.FieldType)
				{
					throw new Exception(string.Format("Field '{0}' must be type of '{1}' for '{2}' to work.",
						FieldPropertyMethodName,
						ExpectedValue.GetType(),
						typeof(ConditionalHideInInspectorAttribute).Name));
				}
				targetValue = targetFieldInfo.GetValue(declaringObject);
				compareValues = true;
			}
			else
			{
				// See if this is a 'Property'
				var targetPropertyInfo = declaringType.GetProperty(FieldPropertyMethodName, bindingFlags);
				if (targetPropertyInfo != null)
				{
					// Make sure ExpectedValue is specified (See: 5761573)
					if (ExpectedValue == null)
					{
						throw new Exception(string.Format("{0} to compare with '{1}' must be specified for '{2}' to work.",
							nameof(ExpectedValue),
							FieldPropertyMethodName,
							typeof(ConditionalHideInInspectorAttribute).Name));
					}
					// Make sure that 'Property' type is the same with ExpectedValue type (See: 5761574)
					if (ExpectedValue.GetType() != targetPropertyInfo.PropertyType)
					{
						throw new Exception(string.Format("Property '{0}' must be type of '{1}' for '{2}' to work.",
							FieldPropertyMethodName,
							ExpectedValue.GetType(),
							typeof(ConditionalHideInInspectorAttribute).Name));
					}
					targetValue = targetPropertyInfo.GetValue(declaringObject);
					compareValues = true;
				}
				else
				{
					// See if this is a 'Method' that has a parameter with type of the field that has the ConditionalHideInInspector attribute
					var targetMethodInfo = declaringType.GetMethod(FieldPropertyMethodName, bindingFlags, null, new Type[] { propertyWithAttribute.GetFieldInfo().FieldType }, null);
					if (targetMethodInfo != null)
					{
						// Make sure that 'Method' return type is ConditionalHideResult (See: 5761571)
						if (targetMethodInfo.ReturnType != typeof(bool))
						{
							throw new Exception(string.Format("Method '{0}' must have a return type of '{1}' for '{2}' to work.",
								FieldPropertyMethodName,
								typeof(bool).Name,
								typeof(ConditionalHideInInspectorAttribute).Name));
						}

						enabled = (bool)targetMethodInfo.Invoke(declaringObject, new object[] { propertyWithAttribute.GetValueAsObject() });
						compareValues = false;
					}
					else
					{
						// See if this is a 'Method' that has no parameter (See: 5761572)
						targetMethodInfo = declaringType.GetMethod(FieldPropertyMethodName, bindingFlags, null, new Type[0], null);
						if (targetMethodInfo != null)
						{
							// Make sure that 'Method' return type is ConditionalHideResult (See: 5761571)
							if (targetMethodInfo.ReturnType != typeof(bool))
							{
								throw new Exception(string.Format("Method '{0}' must have a return type of '{1}' for '{2}' to work.",
									FieldPropertyMethodName,
									typeof(bool).Name,
									typeof(ConditionalHideInInspectorAttribute).Name));
							}

							enabled = (bool)targetMethodInfo.Invoke(declaringObject, null);
							compareValues = false;
						}
						else
						{
							// Warn user if there is a 'Method' with the expected name but unexpected parameters (See: 5761572)
							targetMethodInfo = declaringType.GetMethod(FieldPropertyMethodName,
								BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
							if (targetMethodInfo != null)
							{
								throw new Exception(string.Format("Method '{0}' must have a parameter of type '{1}' or has no parameters for '{2}' to work.",
									FieldPropertyMethodName,
									propertyWithAttribute.type,
									typeof(ConditionalHideInInspectorAttribute).Name));
							}
							else
							{
								throw new Exception(string.Format("There is no Field, Property or Method with the name '{0}' for '{1}' to work.",
									FieldPropertyMethodName,
									typeof(ConditionalHideInInspectorAttribute).Name));
							}
						}
					}
				}
			}

			// Check condition.
			if (compareValues)
			{
				enabled = targetValue.Equals(ExpectedValue);
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
