using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class EditorWindowTools
	{
		#region EditorWindow.docked

		private static PropertyInfo DockedPropertyInfo;

		public static bool IsDocked(this EditorWindow window)
		{
			if (DockedPropertyInfo == null)
				DockedPropertyInfo = typeof(EditorWindow).GetProperty("docked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
			return (bool)DockedPropertyInfo.GetValue(window, null);
		}

		#endregion

		#region EditorWindow.hasFocus

		private static PropertyInfo HasFocusPropertyInfo;

		/// <summary>
		/// Tells if the window is docked and another tab is active where it's docked. Returns false for floating windows.
		/// </summary>
		public static bool IsDockedAndHidden(this EditorWindow window)
		{
			// EditorWindow.hasFocus property is working in mysterious ways.
			// I think it's not named correctly. Tells 'true' for floating
			// windows, even when the window has no focus. So we use 'hasFocus'
			// with a more meaningful name as 'IsDockedAndHidden'.
			if (HasFocusPropertyInfo == null)
				HasFocusPropertyInfo = typeof(EditorWindow).GetProperty("hasFocus", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
			return !(bool)HasFocusPropertyInfo.GetValue(window, null);
		}

		#endregion

		#region IsWindowOpen and ToggleWindow

		/// <summary>
		/// Check if there is at least one window of type T open. Checking method is directly copied from 'EditorWindow.GetWindow'.
		/// </summary>
		public static bool IsWindowOpen<T>() where T : EditorWindow
		{
			var objects = Resources.FindObjectsOfTypeAll<T>();
			var window = objects.Length > 0 ? (EditorWindow)objects[0] : null;
			return window;
		}

		public static void ToggleWindow<T>() where T : EditorWindow
		{
			if (IsWindowOpen<T>())
			{
				var window = EditorWindow.GetWindow<T>();
				window.Close();
			}
			else
			{
				EditorWindow.GetWindow<T>();
			}
		}

		#endregion
	}

}
