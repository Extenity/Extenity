using System;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public static class SerializedObjectTools
	{
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

		public static bool CheckEquals(this SerializedProperty property, object value)
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
			if (valueType.IsEnum)
			{
				valueType = valueType.GetEnumUnderlyingType();
			}

			if (propertyType == valueType)
			{
				return propertyValue.Equals(value);
			}
			throw new Exception(string.Format("SerializedProperty type '{0}' does not match the compared value type '{1}'.", propertyType, valueType));
		}
	}

}
