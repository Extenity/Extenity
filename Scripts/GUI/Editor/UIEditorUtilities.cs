using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	}

}
