#if UNITY

using Extenity.DesignPatternsToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.RenderingToolbox
{

	public class ScreenManager : SingletonUnity<ScreenManager>
	{
		#region Initialization

		protected override void AwakeDerived()
		{
			Calculate();
		}

		#endregion

		#region Update

		private void Update()
		{
			if (IsFullScreenAutoNativeSizeAdjusterEnabled)
				CalculateFullScreenAutoNativeSizeAdjuster();

			var isFullScreen = Screen.fullScreen ? 1 : 0;
			if (
				currentScreenWidth == Screen.width &&
				currentScreenHeight == Screen.height &&
				previousFullScreen == isFullScreen
			)
				return;

			Calculate();
			OnScreenSizeChanged.Invoke();

			if (previousFullScreen != isFullScreen)
			{
				previousFullScreen = isFullScreen;
				OnFullScreenChanged.Invoke(isFullScreen == 1);
			}
		}

		#endregion

		#region Events

		public class FullScreenEvent : UnityEvent<bool> { }

		public UnityEvent OnScreenSizeChanged = new UnityEvent();
		public FullScreenEvent OnFullScreenChanged = new FullScreenEvent();

		#endregion

		#region Full-Screen Auto Native Size Adjuster

		private bool FullScreenAutoNativeSizeAdjuster_WasFullScreen;
		private int FullScreenAutoNativeSizeAdjuster_PostponeCounter = -1;

		private bool IsFullScreenAutoNativeSizeAdjusterEnabled;
		public bool IsFullScreenAutoNativeSizeAdjusterLoggingEnabled { get; set; }

		public void ActivateFullScreenAutoNativeSizeAdjuster(bool activate)
		{
			if (IsFullScreenAutoNativeSizeAdjusterEnabled == activate)
				return;
			IsFullScreenAutoNativeSizeAdjusterEnabled = activate;

			if (activate)
			{
				// Just activated
				FullScreenAutoNativeSizeAdjuster_WasFullScreen = !Screen.fullScreen; // This will make sure calculation below will be invalidated
				CalculateFullScreenAutoNativeSizeAdjuster();
			}
			else
			{
				// Just deactivated
			}
		}

		private void CalculateFullScreenAutoNativeSizeAdjuster()
		{
			if (FullScreenAutoNativeSizeAdjuster_PostponeCounter >= 0)
			{
				if (!Screen.fullScreen)
				{
					FullScreenAutoNativeSizeAdjuster_PostponeCounter = -1;
				}
				else if (--FullScreenAutoNativeSizeAdjuster_PostponeCounter == 0)
				{
					FullScreenAutoNativeSizeAdjuster_PostponeCounter = -1;
					InternalSetToNativeSizeInFullScreen();
				}
			}

			var isFullScreen = Screen.fullScreen;
			if (FullScreenAutoNativeSizeAdjuster_WasFullScreen != isFullScreen)
			{
				FullScreenAutoNativeSizeAdjuster_WasFullScreen = isFullScreen;
				if (isFullScreen)
				{
					FullScreenAutoNativeSizeAdjuster_PostponeCounter = 2;
				}
			}
		}

		private void InternalSetToNativeSizeInFullScreen()
		{
			var activeDisplayIndex = GetActiveDisplayIndex();
			if (activeDisplayIndex >= 0)
			{
				var activeDisplay = Display.displays[activeDisplayIndex];
				var width = activeDisplay.systemWidth;
				var height = activeDisplay.systemHeight;
				if (width > 0 && height > 0)
				{
					if (IsFullScreenAutoNativeSizeAdjusterLoggingEnabled)
						Log.Info($"Adjusting screen resolution to native {width}x{height} on monitor {activeDisplayIndex}.");
					//activeDisplay.SetParams(width, height, 0, 0);
					//activeDisplay.SetRenderingResolution(width, height);
					Screen.SetResolution(width, height, true);
				}
				else
				{
					if (IsFullScreenAutoNativeSizeAdjusterLoggingEnabled)
						Log.Info($"Failed to get screen resolution of active monitor {activeDisplayIndex}.");
				}
			}
			else
			{
				if (IsFullScreenAutoNativeSizeAdjusterLoggingEnabled)
					Log.Info("Failed to find active display.");
			}
		}

		#endregion

		#region Displays

		public int GetActiveDisplayIndex()
		{
			for (var i = 0; i < Display.displays.Length; i++)
			{
				if (Display.displays[i].active)
					return i;
			}
			return -1;
		}

		#endregion

		#region Calculations

		private void Calculate()
		{
			previousScreenWidth = currentScreenWidth;
			previousScreenHeight = currentScreenHeight;
			currentScreenWidth = Screen.width;
			currentScreenHeight = Screen.height;

			screenSizeInchesAvailable = !Screen.dpi.IsZero();

			if (screenSizeInchesAvailable)
			{
				screenWidthInches = (float)Screen.width / Screen.dpi;
				screenHeightInches = (float)Screen.height / Screen.dpi;
				screenDiagonalInches = Mathf.Sqrt(
					screenWidthInches * screenWidthInches +
					screenHeightInches * screenHeightInches);
			}
			else
			{
				screenWidthInches = float.NaN;
				screenHeightInches = float.NaN;
				screenDiagonalInches = float.NaN;
			}
		}

		#endregion

		#region Tools

		public static void SwapResolution()
		{
			Screen.SetResolution(Screen.height, Screen.width, false);
		}

		public static Rect GetCenteredRect(int sizeX, int sizeY)
		{
			return new Rect(
				(int)((Screen.width - sizeX) / 2),
				Screen.height - (int)((Screen.height - sizeY) / 2),
				sizeX, sizeY);
		}

		#endregion

		#region Data

		private int previousScreenWidth = int.MinValue;
		private int previousScreenHeight = int.MinValue;
		private int currentScreenWidth = int.MinValue;
		private int currentScreenHeight = int.MinValue;
		private int previousFullScreen = int.MinValue;

		private float screenWidthInches = float.NaN;
		private float screenHeightInches = float.NaN;
		private float screenDiagonalInches = float.NaN;
		private bool screenSizeInchesAvailable = false;

		public float CurrentAspectRatio
		{
			get { return (float)currentScreenHeight / currentScreenWidth; }
		}

		public int CurrentScreenWidth
		{
			get { return currentScreenWidth; }
		}

		public int CurrentScreenHeight
		{
			get { return currentScreenHeight; }
		}

		public int PreviousScreenWidth
		{
			get { return previousScreenWidth; }
		}

		public int PreviousScreenHeight
		{
			get { return previousScreenHeight; }
		}

		public float ScreenWidthInches
		{
			get { return screenWidthInches; }
		}

		public float ScreenHeightInches
		{
			get { return screenHeightInches; }
		}

		public float ScreenDiagonalInches
		{
			get { return screenDiagonalInches; }
		}

		public bool ScreenSizeInchesAvailable
		{
			get { return screenSizeInchesAvailable; }
		}

		#endregion
	}

}

#endif
