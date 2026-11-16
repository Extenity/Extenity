using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>An object field with a thumbnail of the referenced object next to it.</summary>
	public class PreviewObjectField : VisualElement
	{
		public const string UssClassName = "ext-preview";

		public ObjectField Field { get; }
		private readonly Image Preview;
		private int RetriesLeft;

		/// <param name="label">Null for the property's own name, empty for no label.</param>
		public PreviewObjectField(SerializedProperty property, string label, Type objectType, int size)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (property.propertyType != SerializedPropertyType.ObjectReference)
				throw new ArgumentException($"A preview needs an object reference. '{property.propertyPath}' is a '{property.propertyType}'.", nameof(property));

			AddToClassList(UssClassName);
			Field = new ObjectField(label ?? property.displayName)
			{
				objectType = objectType ?? typeof(UnityEngine.Object),
				allowSceneObjects = !EditorUtility.IsPersistent(property.serializedObject.targetObject),
				bindingPath = property.propertyPath,
			};
			Field.AddToClassList(ObjectField.alignedFieldUssClassName);
			Field.AddToClassList(UssClassName + "__field");
			Add(Field);

			Preview = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
			Preview.AddToClassList(UssClassName + "__image");
			Preview.style.width = size;
			Preview.style.height = size;
			Add(Preview);

			Field.RegisterValueChangedCallback(evt => ShowPreviewOf(evt.newValue));
			ShowPreviewOf(property.objectReferenceValue);
		}

		private void ShowPreviewOf(UnityEngine.Object target)
		{
			RetriesLeft = 20;
			UpdatePreview(target);
		}

		private void UpdatePreview(UnityEngine.Object target)
		{
			Preview.style.display = target != null ? DisplayStyle.Flex : DisplayStyle.None;
			if (target == null)
			{
				Preview.image = null;
				return;
			}
			var texture = AssetPreview.GetAssetPreview(target);
			if (texture == null)
			{
				// Previews are made in the background. Show the small icon meanwhile and look again shortly.
				Preview.image = AssetPreview.GetMiniThumbnail(target);
				if (RetriesLeft-- > 0 && AssetPreview.IsLoadingAssetPreview(target.GetInstanceID()))
				{
					schedule.Execute(() =>
					{
						if (Field.value == target)
							UpdatePreview(target);
					}).StartingIn(100);
				}
				return;
			}
			Preview.image = texture;
		}
	}

}
