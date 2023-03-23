using System;
using System.Reflection;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.GameObjectToolbox.Editor;
using Extenity.UnityEditorToolbox;
using UnityEngine.UI;

namespace Extenity.UIToolbox.Editor
{

	public static class UIEditorUtilities
	{
		public const string Menu = ExtenityMenu.UI;

		[MenuItem(Menu + "Disable All Navigation In Selection", priority = ExtenityMenu.UIPriority + 1)]
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
					if (navigation.mode != Navigation.Mode.None)
					{
						Undo.RecordObject(selectable, "Disable navigation");
						navigation.mode = Navigation.Mode.None;
						selectable.navigation = navigation;
					}
				}
			}
		}

		[MenuItem(Menu + "Reset All Scales In Selection", priority = ExtenityMenu.UIPriority + 4)]
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
							$"Resetting the scale of object. Proceed?\n\n{rectTransform.FullGameObjectName()}\n{rectTransform.localScale.ToSerializableString()}",
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
				Log.InfoWithContext(clickable, $"{typeof(TNewComponent).Name} component created in object '{clickable.GameObjectNameSafe()}'.");
			}
			else if (movedBy != 0)
			{
				Log.InfoWithContext(clickable, $"{typeof(TNewComponent).Name} component moved above {typeof(TTarget).Name} component in object '{clickable.GameObjectNameSafe()}'.");
			}
			else
			{
				Log.InfoWithContext(clickable, $"{typeof(TTarget).Name} '{clickable.gameObject.name}' already has a {typeof(TNewComponent).Name}.");
			}

			return component;
		}

		#endregion

		#region PlaceUIElementRoot as in UnityEditor.UI.MenuOptions

		private static MethodInfo _PlaceUIElementRoot;

		/// <summary>
		/// Places the UI element just like selecting Unity UI right click menu items (buttons, texts, etc.).
		/// </summary>
		public static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
		{
			if (_PlaceUIElementRoot == null)
			{
				var menuOptionsType = typeof(UnityEditor.UI.ButtonEditor).Assembly.GetType("UnityEditor.UI.MenuOptions");

				_PlaceUIElementRoot = menuOptionsType.GetMethod(nameof(PlaceUIElementRoot),
				                                       BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
				                                       null,
				                                       new[] {typeof(GameObject), typeof(MenuCommand)},
				                                       null);
			}

			_PlaceUIElementRoot.Invoke(null, new object[] {element, menuCommand});
		}

		#endregion

		#region Instantiate Widget From Prefab

		public static GameObject InstantiateUIWidgetFromPrefab(string prefabPathInResources, MenuCommand menuCommand)
		{
			// Get prefab from editor Resources folder.
			var prefab = Resources.Load<GameObject>(prefabPathInResources);
			if (!prefab)
			{
				throw new Exception("UI widget prefab does not exist at resource path: " + prefabPathInResources);
			}

			// Instantiate the prefab.
			var instance = GameObject.Instantiate(prefab);
			instance.name = prefab.name;
			Undo.RegisterCreatedObjectUndo(instance, "New " + prefab.name);

			// Place the widget in scene, just like Unity does with its internal widgets.
			PlaceUIElementRoot(instance, menuCommand);

			return instance;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(UIEditorUtilities));

		#endregion
	}

}
