using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Draws a struct or class that wraps a single serialized value as that value: one field under the name
	/// of the wrapper, without a foldout. Register it for wrapper types with stacked attributes on a derived
	/// class: <c>[CustomPropertyDrawer(typeof(A))] [CustomPropertyDrawer(typeof(B))] class WrapperDrawers : FlattenedStructDrawer { }</c>.
	/// Throws when the type has more or fewer than one visible serialized field.
	/// </summary>
	public class FlattenedStructDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var child = FindOnlyChild(property);
			return new PropertyField(child, preferredLabel ?? property.displayName);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var child = FindOnlyChild(property);
			label = EditorGUI.BeginProperty(position, label, property);
			EditorGUI.PropertyField(position, child, label, true);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(FindOnlyChild(property), label, true);
		}

		/// <summary>The single visible child of the property. Throws when there is not exactly one.</summary>
		public static SerializedProperty FindOnlyChild(SerializedProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			var end = property.GetEndProperty();
			var iterator = property.Copy();
			SerializedProperty onlyChild = null;
			var count = 0;
			if (iterator.NextVisible(true))
			{
				while (!SerializedProperty.EqualContents(iterator, end))
				{
					count++;
					onlyChild ??= iterator.Copy();
					if (!iterator.NextVisible(false))
						break;
				}
			}
			if (count != 1)
				throw new InvalidOperationException($"'{property.type}' at '{property.propertyPath}' has {count} visible serialized fields. Only a type that wraps exactly one value can be drawn as that value.");
			return onlyChild;
		}
	}

}
