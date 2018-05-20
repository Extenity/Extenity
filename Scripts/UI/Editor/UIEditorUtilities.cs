using System;
using Extenity.BeyondAudio.UI;
using UnityEngine;
using UnityEditor;
using Extenity.GameObjectToolbox;
using Extenity.GameObjectToolbox.Editor;
using UnityEngine.UI;

namespace Extenity.UIToolbox.Editor
{

	public static class UIEditorUtilities
	{
		private const string Menu = "Tools/UI/";

		[MenuItem(Menu + "Disable All Navigation")]
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

		[MenuItem(Menu + "Reset All Scales")]
		public static void DisableResetAllScalesInSelectedObjectAndChildren()
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
							string.Format("Resetting current scale '{0}' of object '{1}'. Proceed?", rectTransform.localScale, rectTransform.gameObject.FullName()),
							"Yes", "Skip"))
						{
							Undo.RecordObject(rectTransform, "Reset scale to one");
							rectTransform.localScale = Vector3.one;
						}
					}
				}
			}
		}

		[MenuItem(Menu + "Add Button Click Sound")]
		public static void AddButtonClickSoundsToSelectedObjectAndChildren()
		{
			var selectedObjects = Selection.gameObjects;
			if (selectedObjects.Length == 0)
				throw new Exception("You should select some objects first.");

			foreach (var selectedObject in selectedObjects)
			{
				var buttons = selectedObject.GetComponentsInChildren<Button>(true);
				foreach (var button in buttons)
				{
					// Create ButtonClickSound
					var buttonSound = button.gameObject.GetComponent<ButtonClickSound>();
					var isCreated = false;
					if (!buttonSound)
					{
						buttonSound = Undo.AddComponent<ButtonClickSound>(button.gameObject);
						isCreated = true;
					}

					// Move it above Button
					var movedBy = buttonSound.MoveComponentAbove(button);

					// Log
					if (isCreated)
					{
						Debug.Log($"{typeof(ButtonClickSound).Name} component created in object '{button.gameObject.name}'.", button.gameObject);
					}
					else if (movedBy != 0)
					{
						Debug.Log($"{typeof(ButtonClickSound).Name} component moved above {typeof(Button).Name} component in object '{button.gameObject.name}'.", button.gameObject);
					}
					else
					{
						Debug.Log($"{typeof(Button).Name} '{button.gameObject.name}' already has a {typeof(ButtonClickSound).Name}.", button.gameObject);
					}
				}
			}
		}
	}

}
