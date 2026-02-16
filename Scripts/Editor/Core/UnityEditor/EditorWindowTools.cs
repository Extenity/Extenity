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

		public static bool TryGetGameViewWindowSize(out Vector2 size)
		{
			var gameView = GameView;
			if (gameView == null)
			{
				size = Vector2.zero;
				return false;
			}

			size = gameView.position.size;
			return true;
		}

		public static bool TryGetGameViewScale(out float scale)
		{
			var gameView = GameView;
			if (gameView == null)
			{
				scale = 0f;
				return false;
			}

			var zoomArea = GameViewZoomAreaField.GetValue(gameView);
			var scaleVector = (Vector2)ZoomableAreaScaleProperty.GetValue(zoomArea);
			scale = scaleVector.y;
			return true;
		}

		/// <summary>
		/// Note that Unity will apply its own clipping when we try to set the scale here.
		/// So, the result would not always be the same as the value we set here.
		/// </summary>
		public static bool TrySetGameViewScale(float scale)
		{
			var gameView = GameView;
			if (gameView == null)
			{
				return false;
			}

			// Not sure if this is the correct way to calculate the focus point. Not tested properly.
			var zoomArea = GameViewZoomAreaField.GetValue(gameView);
			var shownArea = (Rect)GameViewZoomableAreaShownAreaInsideMarginsProperty.GetValue(zoomArea);
			var focusPoint = shownArea.position + shownArea.size * 0.5f;

			GameViewZoomableAreaSetScaleFocusedMethod.Invoke(zoomArea, new object[] { focusPoint, Vector2.one * scale });
			gameView.Repaint();
			return true;
		}

		public static bool TryGetCurrentGameViewSizeIndex(out int index)
		{
			var gameView = GameView;
			if (gameView == null)
			{
				index = -1;
				return false;
			}

			index = (int)GameViewSelectedSizeIndexProperty.GetValue(gameView);
			return true;

		}

		public static bool TrySetCurrentGameViewSizeIndex(int index)
		{
			var gameView = GameView;
			if (gameView == null)
			{
				return false;
			}

			GameViewSelectedSizeIndexProperty.SetValue(gameView, index);
			gameView.Repaint();
			return true;
		}

		public static bool GameViewSizeExists(int width, int height)
		{
			return IndexOfFirstGameViewSize(width, height) >= 0;
		}

		public static int IndexOfFirstGameViewSize(int width, int height)
		{
			var group = GameViewSizeGroup;

			int totalCount = (int)GameViewSizeGroupGetTotalCountMethod.Invoke(group, null);

			for (int index = 0; index < totalCount; index++)
			{
				var size = GameViewSizeGroupGetGameViewSizeMethod.Invoke(group, new object[] { index });
				if (size == null)
				{
					continue;
				}

				int sizeWidth = (int)GameViewSizeWidthProperty.GetValue(size);
				int sizeHeight = (int)GameViewSizeHeightProperty.GetValue(size);

				if (sizeWidth == width && sizeHeight == height)
				{
					return index;
				}
			}

			return -1;
		}

		public static bool TryFindGameViewSizeIndexByName(string name, out int index)
		{
			var group = GameViewSizeGroup;

			int totalCount = (int)GameViewSizeGroupGetTotalCountMethod.Invoke(group, null);

			for (index = 0; index < totalCount; index++)
			{
				var size = GameViewSizeGroupGetGameViewSizeMethod.Invoke(group, new object[] { index });
				if (size == null)
				{
					throw new InternalException(11845126);
				}

				var baseText = (string)GameViewSizeBaseTextProperty.GetValue(size);
				if (baseText == name)
				{
					return true;
				}
			}

			index = -1;
			return false;
		}

		public static void AddGameViewSize(string name, int width, int height)
		{
			var fixedResolutionValue = Enum.ToObject(GameViewSizeTypeEnum, 1);
			var constructor = GameViewSizeType.GetConstructor(new[] { GameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) });
			var newSize = constructor!.Invoke(new[] { fixedResolutionValue, width, height, name });
			GameViewSizeGroupAddCustomSizeMethod.Invoke(GameViewSizeGroup, new[] { newSize });
		}

		public static void RemoveGameViewSizeAtIndex(int index)
		{
			GameViewSizeGroupRemoveCustomSizeMethod.Invoke(GameViewSizeGroup, new object[] { index });
		}

		public static bool TryUpdateGameViewSize(int index, int width, int height)
		{
			var size = GameViewSizeGroupGetGameViewSizeMethod.Invoke(GameViewSizeGroup, new object[] { index });
			if (size == null)
			{
				return false;
			}

			GameViewSizeWidthProperty.SetValue(size, width);
			GameViewSizeHeightProperty.SetValue(size, height);
			return true;
		}

		private static Type _GameViewType;
		private static Type GameViewType
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

		private static PropertyInfo _GameViewSelectedSizeIndexProperty;
		private static PropertyInfo GameViewSelectedSizeIndexProperty
		{
			get
			{
				if (_GameViewSelectedSizeIndexProperty == null)
				{
					_GameViewSelectedSizeIndexProperty = GameViewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.Instance);
				}
				return _GameViewSelectedSizeIndexProperty;
			}
		}

		private static FieldInfo _GameViewZoomAreaField;
		private static FieldInfo GameViewZoomAreaField
		{
			get
			{
				if (_GameViewZoomAreaField == null)
				{
					_GameViewZoomAreaField = GameViewType.GetField("m_ZoomArea", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return _GameViewZoomAreaField;
			}
		}

		private static Type _ZoomableAreaType;
		private static Type ZoomableAreaType
		{
			get
			{
				if (_ZoomableAreaType == null)
				{
					_ZoomableAreaType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ZoomableArea");
				}
				return _ZoomableAreaType;
			}
		}

		private static PropertyInfo _ZoomableAreaScaleProperty;
		private static PropertyInfo ZoomableAreaScaleProperty
		{
			get
			{
				if (_ZoomableAreaScaleProperty == null)
				{
					_ZoomableAreaScaleProperty = ZoomableAreaType.GetProperty("scale", BindingFlags.Public | BindingFlags.Instance);
				}
				return _ZoomableAreaScaleProperty;
			}
		}

		private static PropertyInfo _GameViewZoomableAreaShownAreaInsideMarginsProperty;
		private static PropertyInfo GameViewZoomableAreaShownAreaInsideMarginsProperty
		{
			get
			{
				if (_GameViewZoomableAreaShownAreaInsideMarginsProperty == null)
				{
					_GameViewZoomableAreaShownAreaInsideMarginsProperty = ZoomableAreaType.GetProperty("shownAreaInsideMargins", BindingFlags.Public | BindingFlags.Instance);
				}
				return _GameViewZoomableAreaShownAreaInsideMarginsProperty;
			}
		}

		private static MethodInfo _GameViewZoomableAreaSetScaleFocusedMethod;
		private static MethodInfo GameViewZoomableAreaSetScaleFocusedMethod
		{
			get
			{
				if (_GameViewZoomableAreaSetScaleFocusedMethod == null)
				{
					_GameViewZoomableAreaSetScaleFocusedMethod = ZoomableAreaType.GetMethod("SetScaleFocused", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Vector2), typeof(Vector2) }, null);
				}
				return _GameViewZoomableAreaSetScaleFocusedMethod;
			}
		}

		private static Type _GameViewSizesType;
		private static Type GameViewSizesType
		{
			get
			{
				if (_GameViewSizesType == null)
				{
					_GameViewSizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
				}
				return _GameViewSizesType;
			}
		}

		private static Type _GameViewSizeType;
		private static Type GameViewSizeType
		{
			get
			{
				if (_GameViewSizeType == null)
				{
					_GameViewSizeType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSize");
				}
				return _GameViewSizeType;
			}
		}

		private static Type _GameViewSizeTypeEnum;
		private static Type GameViewSizeTypeEnum
		{
			get
			{
				if (_GameViewSizeTypeEnum == null)
				{
					_GameViewSizeTypeEnum = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizeType");
				}
				return _GameViewSizeTypeEnum;
			}
		}

		private static object _GameViewSizesInstance;

		private static object GameViewSizesInstance
		{
			get
			{
				if (_GameViewSizesInstance != null)
				{
					return _GameViewSizesInstance;
				}

				var instanceProperty = GameViewSizesType.BaseType!.GetProperty("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				var instance = instanceProperty!.GetValue(null);
				if (instance == null)
				{
					throw new InternalException(11845124);
				}

				_GameViewSizesInstance = instance;
				return instance;
			}
		}

		private static object GameViewSizeGroup
		{
			get
			{
				var currentGroup = GameViewSizesCurrentGroupProperty.GetValue(GameViewSizesInstance);
				if (currentGroup == null)
				{
					throw new InternalException(11845125);
				}

				return currentGroup;
			}
		}

		private static PropertyInfo _GameViewSizesCurrentGroupProperty;
		private static PropertyInfo GameViewSizesCurrentGroupProperty
		{
			get
			{
				if (_GameViewSizesCurrentGroupProperty == null)
				{
					_GameViewSizesCurrentGroupProperty = GameViewSizesInstance.GetType().GetProperty("currentGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				}
				return _GameViewSizesCurrentGroupProperty;
			}
		}

		private static MethodInfo _GameViewSizeGroupGetTotalCountMethod;
		private static MethodInfo GameViewSizeGroupGetTotalCountMethod
		{
			get
			{
				if (_GameViewSizeGroupGetTotalCountMethod == null)
				{
					_GameViewSizeGroupGetTotalCountMethod = GameViewSizeGroup.GetType().GetMethod("GetTotalCount");
				}
				return _GameViewSizeGroupGetTotalCountMethod;
			}
		}

		private static MethodInfo _GameViewSizeGroupGetGameViewSizeMethod;
		private static MethodInfo GameViewSizeGroupGetGameViewSizeMethod
		{
			get
			{
				if (_GameViewSizeGroupGetGameViewSizeMethod == null)
				{
					_GameViewSizeGroupGetGameViewSizeMethod = GameViewSizeGroup.GetType().GetMethod("GetGameViewSize");
				}
				return _GameViewSizeGroupGetGameViewSizeMethod;
			}
		}

		private static MethodInfo _GameViewSizeGroupAddCustomSizeMethod;
		private static MethodInfo GameViewSizeGroupAddCustomSizeMethod
		{
			get
			{
				if (_GameViewSizeGroupAddCustomSizeMethod == null)
				{
					_GameViewSizeGroupAddCustomSizeMethod = GameViewSizeGroup.GetType().GetMethod("AddCustomSize");
				}
				return _GameViewSizeGroupAddCustomSizeMethod;
			}
		}

		private static MethodInfo _GameViewSizeGroupRemoveCustomSizeMethod;
		private static MethodInfo GameViewSizeGroupRemoveCustomSizeMethod
		{
			get
			{
				if (_GameViewSizeGroupRemoveCustomSizeMethod == null)
				{
					_GameViewSizeGroupRemoveCustomSizeMethod = GameViewSizeGroup.GetType().GetMethod("RemoveCustomSize");
				}
				return _GameViewSizeGroupRemoveCustomSizeMethod;
			}
		}

		private static PropertyInfo _GameViewSizeWidthProperty;
		private static PropertyInfo GameViewSizeWidthProperty
		{
			get
			{
				if (_GameViewSizeWidthProperty == null)
				{
					var sampleSize = GameViewSizeGroupGetGameViewSizeMethod.Invoke(GameViewSizeGroup, new object[] { 0 });
					if (sampleSize != null)
					{
						_GameViewSizeWidthProperty = sampleSize.GetType().GetProperty("width", BindingFlags.Public | BindingFlags.Instance);
					}
				}
				return _GameViewSizeWidthProperty;
			}
		}

		private static PropertyInfo _GameViewSizeHeightProperty;
		private static PropertyInfo GameViewSizeHeightProperty
		{
			get
			{
				if (_GameViewSizeHeightProperty == null)
				{
					var sampleSize = GameViewSizeGroupGetGameViewSizeMethod.Invoke(GameViewSizeGroup, new object[] { 0 });
					if (sampleSize != null)
					{
						_GameViewSizeHeightProperty = sampleSize.GetType().GetProperty("height", BindingFlags.Public | BindingFlags.Instance);
					}
				}
				return _GameViewSizeHeightProperty;
			}
		}

		private static PropertyInfo _GameViewSizeBaseTextProperty;
		private static PropertyInfo GameViewSizeBaseTextProperty
		{
			get
			{
				if (_GameViewSizeBaseTextProperty == null)
				{
					var sampleSize = GameViewSizeGroupGetGameViewSizeMethod.Invoke(GameViewSizeGroup, new object[] { 0 });
					if (sampleSize != null)
					{
						_GameViewSizeBaseTextProperty = sampleSize.GetType().GetProperty("baseText", BindingFlags.Public | BindingFlags.Instance);
					}
				}
				return _GameViewSizeBaseTextProperty;
			}
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
