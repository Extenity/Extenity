using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>Small alternative ways to draw a single field.</summary>
	public static class FieldVariantFeatures
	{
		/// <summary>Draws a bool with the checkbox first and the label after it.</summary>
		public static FieldRule<T, bool> ToggleLeft<T>(this FieldRule<T, bool> rule)
		{
			rule.Data.SetFactory(nameof(ToggleLeft), (instance, property, label) =>
			{
				var toggle = new Toggle
				{
					text = label ?? property.displayName,
					bindingPath = property.propertyPath,
				};
				toggle.AddToClassList("ext-toggle-left");
				return toggle;
			});
			return rule;
		}

		/// <summary>
		/// Draws a string that holds a folder path with a button that opens a folder dialog. A folder inside the
		/// project is stored relative to the project folder, like "Assets/Textures".
		/// </summary>
		public static FieldRule<T, string> FolderPicker<T>(this FieldRule<T, string> rule)
		{
			rule.Data.SetFactory(nameof(FolderPicker), (instance, property, label) =>
			{
				var field = new TextField(label ?? property.displayName)
				{
					bindingPath = property.propertyPath,
				};
				field.AddToClassList(TextField.alignedFieldUssClassName);
				field.AddToClassList("ext-folder-picker");
				var button = new Button { text = "...", tooltip = "Choose a folder" };
				button.AddToClassList("ext-folder-picker__button");
				button.clicked += () =>
				{
					var projectFolder = Path.GetDirectoryName(Application.dataPath)?.Replace('\\', '/') ?? "";
					var start = string.IsNullOrEmpty(field.value) ? "Assets" : field.value;
					var picked = EditorUtility.OpenFolderPanel("Choose a folder", start, "");
					if (string.IsNullOrEmpty(picked))
						return; // The dialog was cancelled.
					picked = picked.Replace('\\', '/');
					if (picked.StartsWith(projectFolder + "/", StringComparison.Ordinal))
						picked = picked.Substring(projectFolder.Length + 1);
					field.value = picked;
				};
				field.Add(button);
				return field;
			});
			return rule;
		}

		/// <summary>
		/// Draws a struct that consists of a few numbers (a vector, a size) on one line: its label, then a small
		/// field per member under the member's short name. Unlike an IMGUI vector drawer it stays on one line in
		/// narrow places, and a long label wraps instead of being cut. Throws when the field is drawn for a type
		/// that has other members than numbers.
		/// </summary>
		public static FieldRule<T, TField> NumbersOnOneLine<T, TField>(this FieldRule<T, TField> rule) where TField : struct
		{
			rule.Data.SetFactory(nameof(NumbersOnOneLine), (instance, property, label) => new NumberLineField(property, label));
			return rule;
		}

		/// <summary>Draws an object reference with a thumbnail of the object next to the field: for sprites, textures, prefabs.</summary>
		public static FieldRule<T, TField> Preview<T, TField>(this FieldRule<T, TField> rule, int size = 32) where TField : UnityEngine.Object
		{
			if (size < 16)
				throw new ArgumentOutOfRangeException(nameof(size), "A preview needs at least 16 pixels.");
			rule.Data.SetFactory(nameof(Preview), (instance, property, label) =>
				new PreviewObjectField(property, label, typeof(TField), size));
			return rule;
		}

		/// <summary>
		/// Draws an object reference with a foldout that shows the inspector of the referenced object in place.
		/// </summary>
		public static FieldRule<T, TField> InlineObjectEditor<T, TField>(this FieldRule<T, TField> rule, bool expanded = false) where TField : UnityEngine.Object
		{
			rule.Data.SetFactory(nameof(InlineObjectEditor), (instance, property, label) =>
				new InlineObjectEditor(property, label, typeof(TField), expanded));
			return rule;
		}
	}

}
