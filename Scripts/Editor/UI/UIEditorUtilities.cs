using System;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.GameObjectToolbox;
using Extenity.GameObjectToolbox.Editor;
using UnityEngine.UI;

namespace Extenity.UIToolbox.Editor
{

	public static class UIEditorUtilities
	{
		public const string Menu = "Tools/UI/";

		[MenuItem(Menu + "Disable All Navigation In Selection")]
		public static void DisableAllNavigationInSelectedObjectAndChildren()
		{
			var selectedObjects = Selection.gameObjects;
			if (selectedObjects.Length == 0)
				throw new Exception("You should select some objects first.");

			foreach (var selectedObject in selectedObjects)
			{
				var selectables = selectedObject.GetComponentsInChildren<Selectable>();
				foreach (var selectable in selectables)
				{
					var navigation = selectable.navigation;
					navigation.mode = Navigation.Mode.None;
					selectable.navigation = navigation;
				}
			}
		}

		[MenuItem(Menu + "Reset All Scales In Selection")]
		public static void ResetAllScalesInSelectedObjectAndChildren()
		{
			var selectedObjects = Selection.gameObjects;
			if (selectedObjects.Length == 0)
				throw new Exception("You should select some objects first.");

			foreach (var selectedObject in selectedObjects)
			{
				var rectTransforms = selectedObject.GetComponentsInChildren<RectTransform>();
				foreach (var rectTransform in rectTransforms)
				{
					if (rectTransform.GetComponent<Canvas>() != null)
						continue; // Skip if this is a canvas

					if (rectTransform.localScale.x != 1f || rectTransform.localScale.y != 1f || rectTransform.localScale.z != 1f)
					{
						if (EditorUtility.DisplayDialog(
							"Reset Scale",
							$"Resetting the scale of object. Proceed?\n\n{rectTransform.gameObject.FullName()}\n{rectTransform.localScale.ToSerializableString()}",
							"Yes", "Skip"))
						{
							Undo.RecordObject(rectTransform, "Reset scale to one");
							rectTransform.localScale = Vector3.one;
						}
					}
				}
			}
		}

		#region Get Or Add Component For Clickable

		public static TNewComponent GetOrAddComponentForClickable<TTarget, TNewComponent>(this TTarget clickable, bool moveComponentAboveClickableComponent)
			where TTarget : Selectable
			where TNewComponent : Component
		{
			// Get or add the component
			var component = clickable.gameObject.GetComponent<TNewComponent>();
			var isCreated = false;
			if (!component)
			{
				component = Undo.AddComponent<TNewComponent>(clickable.gameObject);
				isCreated = true;
			}

			// Move it above Clickable
			int movedBy = 0;
			if (moveComponentAboveClickableComponent)
			{
				movedBy = component.MoveComponentAbove(clickable);
			}

			// Log
			if (isCreated)
			{
				Log.Info($"{typeof(TNewComponent).Name} component created in object '{clickable.gameObject.name}'.", clickable.gameObject);
			}
			else if (movedBy != 0)
			{
				Log.Info($"{typeof(TNewComponent).Name} component moved above {typeof(TTarget).Name} component in object '{clickable.gameObject.name}'.", clickable.gameObject);
			}
			else
			{
				Log.Info($"{typeof(TTarget).Name} '{clickable.gameObject.name}' already has a {typeof(TNewComponent).Name}.", clickable.gameObject);
			}

			return component;
		}

		#endregion
	}

}
