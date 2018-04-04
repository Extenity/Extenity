using System;
using System.Reflection;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public static class SerializedObjectTools
	{
		public static FieldInfo GetFieldInfo(this SerializedProperty property)
		{
			// TODO: A proper cache mechanism would skyrocket the performance.

			var slices = property.propertyPath.Split('.');
			var objectType = property.serializedObject.targetObject.GetType();

			var fieldType = objectType; // Starting point of the search.
			//Type parentFieldType = null; This was a cool method to get the field info. Intentionally kept here in case needed in the future.
			FieldInfo subFieldInfo = null;

			for (int i = 0; i < slices.Length; i++)
			{
				if (slices[i] == "Array")
				{
					// Skip "data[x]" part of the path.
					i++;

					if (fieldType.IsArray)
					{
						// This is how to get the 'array' element type
						//parentFieldType = fieldType;
						fieldType = fieldType.GetElementType(); //gets info on array elements
					}
					else
					{
						// This is how to get the 'list' element type
						//parentFieldType = fieldType;
						fieldType = fieldType.GetGenericArguments()[0];
					}
				}
				else
				{
					//parentFieldType = fieldType;
					subFieldInfo = fieldType.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
					fieldType = subFieldInfo.FieldType;
				}
			}

			//var subFieldInfo = parentFieldType.GetField(fieldName);

			return subFieldInfo;
		}

		public static object GetValueAsObject(this SerializedProperty property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			switch (property.propertyType)
			{
				//case SerializedPropertyType.Generic:
				case SerializedPropertyType.Integer:
					return property.intValue;
				case SerializedPropertyType.Boolean:
					return property.boolValue;
				case SerializedPropertyType.Float:
					return property.floatValue;
				case SerializedPropertyType.String:
					return property.stringValue;
				case SerializedPropertyType.Color:
					return property.colorValue;
				case SerializedPropertyType.ObjectReference:
					return property.objectReferenceValue;
				//case SerializedPropertyType.LayerMask:
				case SerializedPropertyType.Enum:
					return property.enumValueIndex;
				case SerializedPropertyType.Vector2:
					return property.vector2Value;
				case SerializedPropertyType.Vector3:
					return property.vector3Value;
				case SerializedPropertyType.Vector4:
					return property.vector4Value;
				case SerializedPropertyType.Rect:
					return property.rectValue;
				case SerializedPropertyType.ArraySize:
					return property.arraySize;
				//case SerializedPropertyType.Character:
				case SerializedPropertyType.AnimationCurve:
					return property.animationCurveValue;
				case SerializedPropertyType.Bounds:
					return property.boundsValue;
				//case SerializedPropertyType.Gradient:
				case SerializedPropertyType.Quaternion:
					return property.quaternionValue;
				case SerializedPropertyType.ExposedReference:
					return property.exposedReferenceValue;
				case SerializedPropertyType.FixedBufferSize:
					return property.fixedBufferSize;
				case SerializedPropertyType.Vector2Int:
					return property.vector2IntValue;
				case SerializedPropertyType.Vector3Int:
					return property.vector3IntValue;
				case SerializedPropertyType.RectInt:
					return property.rectIntValue;
				case SerializedPropertyType.BoundsInt:
					return property.boundsIntValue;
				default:
					throw new Exception(string.Format("Property type '{0}' of the property '{1}' is currently not supported.", property.propertyType, property.propertyType));
			}
		}

		public static bool CheckEquals(this SerializedProperty property, object value, bool useUnderlyingTypeForEnums = true)
		{
			if (value == null)
			{
				return !property.objectReferenceValue;
			}

			var propertyValue = property.GetValueAsObject();
			if (propertyValue == null)
			{
				// We have already checked for 'null' value above. 
				// This means the value in method parameter is not null 
				// while the property value is null.
				return false;
			}

			var propertyType = propertyValue.GetType();
			var valueType = value.GetType();

			// Special care for enum types.
			if (valueType.IsEnum && useUnderlyingTypeForEnums)
			{
				valueType = valueType.GetEnumUnderlyingType();
				value = Convert.ChangeType(value, valueType);
			}

			if (propertyType == valueType)
			{
				return propertyValue.Equals(value);
			}
			throw new Exception(string.Format("SerializedProperty type '{0}' does not match the compared value type '{1}'.", propertyType, valueType));
		}

		#region SerializedProperty Path

		/// <summary>
		/// Gets the property path as in 'SerializedProperty.propertyPath'. Except if there is an array 
		/// part at the end of the path, it will be stripped out.
		/// </summary>
		public static string GetPropertyPathWithStrippedLastArrayPart(this SerializedProperty property)
		{
			var path = property.propertyPath;

			var dotIndex = path.LastIndexOf('.');
			if (path.Length > dotIndex + ".data[".Length &&
			    dotIndex > ".Array".Length &&
			    path.Substring(dotIndex - ".Array".Length, ".Array.data[".Length) == ".Array.data[")
			{
				var arrayStartIndex = path.LastIndexOf(".Array.data[", StringComparison.InvariantCulture);
				return path.Substring(0, arrayStartIndex);
			}

			return path;
		}

		#endregion
	}

}
