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
							$"Resetting current scale '{rectTransform.localScale}' of object '{rectTransform.gameObject.FullName()}'. Proceed?",
							"Yes", "Skip"))
						{
							Undo.RecordObject(rectTransform, "Reset scale to one");
							rectTransform.localScale = Vector3.one;
						}
					}
				}
			}
		}

		[MenuItem(Menu + "Add Button Click Sound To Selection", priority = 200)]
		public static void AddButtonClickSoundsToSelectedObjectAndChildren()
		{
			var selectedObjects = Selection.gameObjects;
			if (selectedObjects.Length == 0)
				throw new Exception("You should select some objects first.");

			var includeInactive = true;
			foreach (var selectedObject in selectedObjects)
			{
				foreach (var clickable in selectedObject.GetComponentsInChildren<Button>(includeInactive))
					clickable.CreateButtonClickSoundFor();
				foreach (var clickable in selectedObject.GetComponentsInChildren<Toggle>(includeInactive))
					clickable.CreateButtonClickSoundFor();
			}
		}

		[MenuItem(Menu + "Toggle Global Button Click Sound Logging", priority = 203)]
		public static void ToggleGlobalButtonClickSoundLogging()
		{
			ButtonClickSound.IsLoggingEnabled = !ButtonClickSound.IsLoggingEnabled;
			Log.Info("Global Button Click Sound logging " + (ButtonClickSound.IsLoggingEnabled ? "enabled" : "disabled"));
		}

		public static void CreateButtonClickSoundFor<TTarget>(this TTarget clickable) where TTarget : Selectable
		{
			if (!ButtonClickSound.IsTypeSupported<TTarget>())
				throw new Exception($"{typeof(ButtonClickSound).Name} does not support component {typeof(TTarget).Name}.");

			// Create ButtonClickSound
			var buttonSound = clickable.gameObject.GetComponent<ButtonClickSound>();
			var isCreated = false;
			if (!buttonSound)
			{
				buttonSound = Undo.AddComponent<ButtonClickSound>(clickable.gameObject);
				isCreated = true;
			}

			// Move it above Button
			var movedBy = buttonSound.MoveComponentAbove(clickable);

			// Log
			if (isCreated)
			{
				Log.Info($"{typeof(ButtonClickSound).Name} component created in object '{clickable.gameObject.name}'.", clickable.gameObject);
			}
			else if (movedBy != 0)
			{
				Log.Info($"{typeof(ButtonClickSound).Name} component moved above {typeof(TTarget).Name} component in object '{clickable.gameObject.name}'.", clickable.gameObject);
			}
			else
			{
				Log.Info($"{typeof(TTarget).Name} '{clickable.gameObject.name}' already has a {typeof(ButtonClickSound).Name}.", clickable.gameObject);
			}
		}
	}

}
