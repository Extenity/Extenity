using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Source: https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/
	/// </summary>
	public class FitToSafeArea : MonoBehaviour
	{
		private static readonly List<FitToSafeArea> helpers = new List<FitToSafeArea>();

		void Awake()
		{
			InitializeScreenChangeDetection();

			if (!helpers.Contains(this))
				helpers.Add(this);

			_Canvas = GetComponentInParent<Canvas>();
			_RectTransform = GetComponent<RectTransform>();
		}

		void Start()
		{
			ApplySafeArea();
		}

		void OnDestroy()
		{
			helpers.Remove(this);
		}

		void Update()
		{
			if (helpers[0] == this)
			{
				if (Application.isMobilePlatform && Screen.orientation != lastOrientation)
					OrientationChanged();

				if (Screen.safeArea != lastSafeArea)
					SafeAreaChanged();

				if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
					ResolutionChanged();
			}
		}

		#region Links

		private Canvas _Canvas;
		private Canvas Canvas
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
					return GetComponentInParent<Canvas>(); // Do not cache while working in Editor.
#endif
				return _Canvas;
			}
		}

		private RectTransform _RectTransform;
		private RectTransform RectTransform
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
					return GetComponent<RectTransform>(); // Do not cache while working in Editor.
#endif
				return _RectTransform;
			}
		}

		#endregion

		void ApplySafeArea()
		{
			if (!RectTransform || !Canvas)
				return;

			var safeArea = Screen.safeArea;
			var anchorMin = safeArea.position;
			var anchorMax = safeArea.position + safeArea.size;
			var pixelRect = Canvas.pixelRect;
			anchorMin.x /= pixelRect.width;
			anchorMin.y /= pixelRect.height;
			anchorMax.x /= pixelRect.width;
			anchorMax.y /= pixelRect.height;

			RectTransform.anchorMin = anchorMin;
			RectTransform.anchorMax = anchorMax;
		}

		#region Screen Change Detection

		public static readonly UnityEvent OnResolutionOrOrientationChanged = new UnityEvent();
		public static readonly UnityEvent OnSafeAreaChanged = new UnityEvent();

		private static bool IsInitialized;
		private static ScreenOrientation lastOrientation = ScreenOrientation.LandscapeLeft;
		private static Vector2 lastResolution = Vector2.zero;
		private static Rect lastSafeArea = Rect.zero;

		private void InitializeScreenChangeDetection()
		{
			if (!IsInitialized)
			{
				lastOrientation = Screen.orientation;
				lastResolution.x = Screen.width;
				lastResolution.y = Screen.height;
				lastSafeArea = Screen.safeArea;

				IsInitialized = true;
			}
		}

		private static void OrientationChanged()
		{
			// Log.Info("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);

			lastOrientation = Screen.orientation;
			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;

			OnResolutionOrOrientationChanged.Invoke();
		}

		private static void ResolutionChanged()
		{
			// Log.Info("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);

			lastResolution.x = Screen.width;
			lastResolution.y = Screen.height;

			OnResolutionOrOrientationChanged.Invoke();
		}

		private static void SafeAreaChanged()
		{
			// Log.Info("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);

			lastSafeArea = Screen.safeArea;

			for (int i = 0; i < helpers.Count; i++)
			{
				helpers[i].ApplySafeArea();
			}

			OnSafeAreaChanged.Invoke();
		}

		#endregion
	}

}
