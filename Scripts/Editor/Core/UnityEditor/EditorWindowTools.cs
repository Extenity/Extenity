using System;
using System.Linq;
using System.Reflection;
using Extenity.IMGUIToolbox.Editor;
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
			return GetAllEditorWindowsOfType<T>().Length > 0;
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
			return GetAllEditorWindowsOfType<EditorWindow>();
		}

		public static EditorWindow[] GetAllEditorWindowsOfType<T>() where T : EditorWindow
		{
			return GetAllEditorWindowsOfType(typeof(T));
		}

		public static EditorWindow[] GetAllEditorWindowsOfType(Type type)
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

		#region Unity Built-in Editor Windows / Game View

		private static EditorWindow _GameView;
		public static EditorWindow GameView
		{
			get
			{
				if (_GameView == null)
				{
					var gameViews = GameViews;
					_GameView = gameViews != null && gameViews.Length > 0
						? gameViews[0]
						: null;
				}
				return _GameView;
			}
		}

		public static EditorWindow[] GameViews
		{
			get
			{
				return GetAllEditorWindowsOfType(GameViewType);
			}
		}

		private static Type _GameViewType;
		public static Type GameViewType
		{
			get
			{
				if (_GameViewType == null)
				{
					_GameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
				}
				return _GameViewType;
			}
		}

		private static PropertyInfo _GameViewTargetSizeProperty;
		private static PropertyInfo GameViewTargetSizeProperty
		{
			get
			{
				if (_GameViewTargetSizeProperty == null)
				{
					_GameViewTargetSizeProperty = GameViewType.GetProperty("targetSize", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return _GameViewTargetSizeProperty;
			}
		}

		public static bool TryGetGameViewResolution(out Vector2 resolution)
		{
			var gameView = GameView;
			if (gameView == null)
			{
				resolution = Vector2.zero;
				return false;
			}

			resolution = (Vector2)GameViewTargetSizeProperty.GetValue(gameView);
			return true;
		}
 
		#endregion

		#region Make An Editor Window Full-Screen

		public static void MakeFullScreen(this EditorWindow window, bool fullScreen)
		{
			window.maximized = fullScreen;
			EditorGUITools.SafeRepaintAllViews();
		}

		#endregion

		#region Repaint All Editor Windows

		public static void RepaintAllViews()
		{
			EditorGUITools.SafeRepaintAllViews();
		}

		#endregion
	}

}
