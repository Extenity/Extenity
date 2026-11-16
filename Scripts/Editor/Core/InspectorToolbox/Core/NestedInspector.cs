using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// The base of a custom drawer for a serializable class or struct, or for a type used with
	/// SerializeReference. Derive, add <c>[CustomPropertyDrawer(typeof(T))]</c> and declare only what differs
	/// from the default in <see cref="Configure"/>. Unity finds the drawer wherever a property of that type is
	/// drawn: in default inspectors, in list rows and in other editors.
	/// <para>
	/// A parent that asks for an empty label (an inline field) gets the fields without a header.
	/// In an IMGUI host the fields are drawn plainly, without the customization.
	/// </para>
	/// </summary>
	public abstract class NestedInspector<T> : PropertyDrawer, ICustomizedInspector
	{
		public Type TargetType => typeof(T);

		/// <summary>Runs once per drawer class. The result is cached and shared by every property drawn with it.</summary>
		protected abstract void Configure(InspectorBuilder<T> b);

		public InspectorBlueprint GetBlueprint()
		{
			return InspectorBlueprints.GetOrBuild(GetType(), () =>
			{
				var builder = new InspectorBuilder<T>(GetType());
				Configure(builder);
				return builder.Build();
			});
		}

		public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			return GetBlueprint().Instantiate(property.serializedObject, property.propertyPath, preferredLabel).Root;
		}

		public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			PlainPropertyGUI.Draw(position, property, label);
		}

		public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return PlainPropertyGUI.GetHeight(property, label);
		}
	}

	/// <summary>
	/// Draws a property in IMGUI the way Unity does without any drawer: a foldout with the visible children.
	/// The fallback of the toolbox's drawers in hosts that still draw with IMGUI.
	/// </summary>
	public static class PlainPropertyGUI
	{
		public static float GetHeight(SerializedProperty property, GUIContent label)
		{
			if (!HasChildrenToDraw(property))
				return EditorGUIUtility.singleLineHeight;

			var hasHeader = HasHeader(label);
			var height = hasHeader ? EditorGUIUtility.singleLineHeight : 0f;
			if (hasHeader && !property.isExpanded)
				return height;

			var end = property.GetEndProperty();
			var iterator = property.Copy();
			var isFirst = !hasHeader;
			if (iterator.NextVisible(true))
			{
				while (!SerializedProperty.EqualContents(iterator, end))
				{
					if (!isFirst)
						height += EditorGUIUtility.standardVerticalSpacing;
					isFirst = false;
					height += EditorGUI.GetPropertyHeight(iterator, true);
					if (!iterator.NextVisible(false))
						break;
				}
			}
			return height;
		}

		public static void Draw(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!HasChildrenToDraw(property))
			{
				// A managed reference without a value.
				EditorGUI.LabelField(position, label, new GUIContent("None"));
				return;
			}

			var hasHeader = HasHeader(label);
			var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			if (hasHeader)
			{
				label = EditorGUI.BeginProperty(rect, label, property);
				property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);
				EditorGUI.EndProperty();
				if (!property.isExpanded)
					return;
				rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.indentLevel++;
			}

			var end = property.GetEndProperty();
			var iterator = property.Copy();
			if (iterator.NextVisible(true))
			{
				while (!SerializedProperty.EqualContents(iterator, end))
				{
					rect.height = EditorGUI.GetPropertyHeight(iterator, true);
					EditorGUI.PropertyField(rect, iterator, true);
					rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
					if (!iterator.NextVisible(false))
						break;
				}
			}

			if (hasHeader)
				EditorGUI.indentLevel--;
		}

		private static bool HasHeader(GUIContent label)
		{
			return label != null && label != GUIContent.none && !string.IsNullOrEmpty(label.text);
		}

		private static bool HasChildrenToDraw(SerializedProperty property)
		{
			return property.propertyType != SerializedPropertyType.ManagedReference || property.managedReferenceValue != null;
		}
	}

}
