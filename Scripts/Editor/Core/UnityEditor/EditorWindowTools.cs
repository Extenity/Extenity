using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
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
			return GetAllEditorWindows<T>().Length > 0;
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

		#region Get Editor Window

		public static EditorWindow[] GetAllEditorWindows()
		{
			return GetAllEditorWindows<EditorWindow>();
		}

		public static EditorWindow[] GetAllEditorWindows<T>() where T : EditorWindow
		{
			return GetAllEditorWindows(typeof(T));
		}

		public static EditorWindow[] GetAllEditorWindows(Type type)
		{
			return ((EditorWindow[])Resources.FindObjectsOfTypeAll(type))
			       .Where(window => window != null)
			       .ToArray();
		}

		public static EditorWindow GetEditorWindowByTitle(string title)
		{
			var windows = GetAllEditorWindows();
			EditorWindow foundWindow = null;
			foreach (var window in windows)
			{
				if (window.titleContent.text.Equals(title, StringComparison.Ordinal))
				{
					if (foundWindow != null)
					{
						throw new Exception($"There are more than one window with the same title '{title}'.");
					}
					foundWindow = window;
				}
			}
			if (foundWindow)
				return foundWindow;
			throw new Exception($"Window with title '{title}' does not exist.");
		}

		#endregion

		#region Get Editor Window - Game View

		private static EditorWindow _GameView;
		public static EditorWindow GameView
		{
			get
			{
				if (_GameView == null)
				{
					var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
					var gameViews = GetAllEditorWindows(type);
					_GameView = gameViews != null && gameViews.Length > 0
						? gameViews[0]
						: null;
				}
				return _GameView;
			}
		}

		#endregion

		#region Make An Editor Window Fullscreen

		public static void MakeFullscreen(this EditorWindow window, bool fullscreen)
		{
			window.maximized = fullscreen;
			InternalEditorUtility.RepaintAllViews();
		}

		#endregion
	}

}
