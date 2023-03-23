using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public static class SerializedObjectTools
	{
		#region SerializedProperty Type Getters

		/// <summary>
		/// Gets the FieldInfo of the field which is the target of specified SerializedProperty.
		/// If property points to an item of an array, resulting FieldInfo will be associated
		/// with the array's field, not the item of the array.
		/// </summary>
		public static FieldInfo GetFieldInfo(this SerializedProperty property)
		{
			// TODO: A proper cache mechanism would skyrocket the performance.

			var slices = property.propertyPath.Split('.');
			var objectType = property.serializedObject.targetObject.GetType();

			// 'objectType' is the starting point of the search.
			var fieldType = objectType;
			//Type parentFieldType = null; This was a cool method to get the field info. Intentionally kept here in case needed in the future.
			FieldInfo subFieldInfo = null;

			for (int i = 0; i < slices.Length; i++)
			{
				if (slices[i] == "Array")
				{
					// Skip "data[x]" or "size" part of the path.
					if (++i >= slices.Length)
						break;

					if (fieldType != typeof(System.String))
					{
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

		public static Type GetDeclaringType(this SerializedProperty property)
		{
			// TODO: A proper cache mechanism would skyrocket the performance.

			var objectType = property.serializedObject.targetObject.GetType();

			var path = property.propertyPath;
			if (path.IndexOf('.') < 0)
			{
				return objectType;
			}

			var slices = path.Split('.');

			// 'objectType' is the starting point of the search.
			var fieldType = objectType;
			//Type parentFieldType = null; This was a cool method to get the field info. Intentionally kept here in case needed in the future.
			FieldInfo subFieldInfo = null;

			for (int i = 0; i < slices.Length; i++)
			{
				if (slices[i] == "Array")
				{
					// Skip "data[x]" or "size" part of the path.
					if (++i >= slices.Length)
						break;

					if (fieldType != typeof(System.String))
					{
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
				}
				else
				{
					//parentFieldType = fieldType;
					subFieldInfo = fieldType.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
					fieldType = subFieldInfo.FieldType;
				}
			}

			//var subFieldInfo = parentFieldType.GetField(fieldName);

			return subFieldInfo.DeclaringType;
		}

		public static void GetDeclaringTypeAndObject(this SerializedProperty property, out Type declaringType, out object declaringObject)
		{
			// TODO: A proper cache mechanism would skyrocket the performance.

			// 'subObject' is the starting point of the search.
			object subObject = property.serializedObject.targetObject;
			var objectType = subObject.GetType();

			var path = property.propertyPath;
			if (path.IndexOf('.') < 0)
			{
				declaringType = objectType;
				declaringObject = subObject;
				return;
			}

			var slices = path.Split('.');

			// 'objectType' is the starting point of the search.
			var fieldType = objectType;
			//Type parentFieldType = null; This was a cool method to get the field info. Intentionally kept here in case needed in the future.
			FieldInfo subFieldInfo = null;
			object parentSubObject = null;

			for (int i = 0; i < slices.Length; i++)
			{
				if (slices[i] == "Array")
				{
					// Skip "size" part of the path or get index of "data[x]".
					int index = -1;
					if (++i >= slices.Length - 1)
						break;
					{
						var slice = slices[i];
						var bracketStartIndex = slice.IndexOf('[') + 1;
						if (bracketStartIndex > 0)
						{
							var bracketEndIndex = slice.IndexOf(']', bracketStartIndex);
							var indexString = slice.Substring(bracketStartIndex, bracketEndIndex - bracketStartIndex);
							index = int.Parse(indexString);
						}
						else if (slice == "size")
						{
							// Do nothing.
						}
						else
						{
							throw new InternalException(115993162);
						}
					}

					if (index >= 0 && fieldType != typeof(System.String))
					{
						if (fieldType.IsArray)
						{
							// This is how to get the 'array' element type
							//parentFieldType = fieldType;
							var array = (Array)subFieldInfo.GetValue(parentSubObject);
							parentSubObject = subObject;
							subObject = array.GetValue(index);
							fieldType = fieldType.GetElementType(); //gets info on array elements
						}
						else
						{
							// This is how to get the 'list' element type
							//parentFieldType = fieldType;
							var list = (IList)subFieldInfo.GetValue(parentSubObject);
							parentSubObject = subObject;
							subObject = list[index];
							fieldType = fieldType.GetGenericArguments()[0];
						}
					}
				}
				else
				{
					//parentFieldType = fieldType;
					subFieldInfo = fieldType.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
					fieldType = subFieldInfo.FieldType;
					parentSubObject = subObject;
					subObject = subFieldInfo.GetValue(subObject);
				}
			}

			//var subFieldInfo = parentFieldType.GetField(fieldName);

			declaringType = subFieldInfo.DeclaringType;
			declaringObject = parentSubObject;
		}

		#endregion

		#region SerializedProperty Value Getters

		public static object GetValueAsObject(this SerializedProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

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
					throw new Exception($"Property type '{property.propertyType}' of the property '{property.name}' is currently not supported.");
			}
		}

		#endregion

		#region SerializedProperty Comparison

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
			throw new Exception($"SerializedProperty type '{propertyType}' does not match the compared value type '{valueType}'.");
		}

		#endregion

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

		public static void LogAllPropertyPaths(this SerializedObject serializedObject, string initialLine = null)
		{
			// Initialize
			var stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(initialLine))
			{
				stringBuilder.AppendLine(initialLine);
			}

			// Do logging
			var iterator = serializedObject.GetIterator();
			do
			{
				var property = (SerializedProperty)iterator;
				stringBuilder.AppendLine(property.propertyPath);
			}
			while (iterator.Next(true));

			// Finalize
			var text = stringBuilder.ToString();
			Log.Info(text);
		}

		#endregion

		#region Get SerializedProperties

		public static void GatherSerializedPropertiesNotStartingWithM(this SerializedObject serializedObject, List<SerializedProperty> result)
		{
			var it = serializedObject.GetIterator();
			it.Next(true);
			do
			{
				if (!it.name.StartsWith("m_", StringComparison.Ordinal))
				{
					result.Add(it.Copy());
				}
			}
			while (it.NextVisible(false));
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(SerializedObjectTools));

		#endregion
	}

}
