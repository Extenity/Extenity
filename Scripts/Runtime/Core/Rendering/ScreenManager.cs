#if UNITY

using Extenity.DesignPatternsToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.RenderingToolbox
{

	public class ScreenManager : SingletonUnity<ScreenManager>
	{
		#region Configuration

		public float heightBaseInches = 2.31f; // iPhone 5 size

		#endregion

		#region Initialization

		protected override void AwakeDerived()
		{
			Calculate();
		}

		#endregion

		#region Update

		private void Update()
		{
			if (IsFullscreenAutoNativeSizeAdjusterEnabled)
				CalculateFullscreenAutoNativeSizeAdjuster();

			var isFullscreen = Screen.fullScreen ? 1 : 0;
			if (
				currentScreenWidth == Screen.width &&
				currentScreenHeight == Screen.height &&
				previousFullscreen == isFullscreen
			)
				return;

			Calculate();
			OnScreenSizeChanged.Invoke();

			if (previousFullscreen != isFullscreen)
			{
				previousFullscreen = isFullscreen;
				OnFullscreenChanged.Invoke(isFullscreen == 1);
			}
		}

		#endregion

		#region Events

		public class FullscreenEvent : UnityEvent<bool> { }

		public UnityEvent OnScreenSizeChanged = new UnityEvent();
		public FullscreenEvent OnFullscreenChanged = new FullscreenEvent();

		#endregion

		#region Fullscreen Auto Native Size Adjuster

		private bool FullscreenAutoNativeSizeAdjuster_WasFullscreen;
		private int FullscreenAutoNativeSizeAdjuster_PostponeCounter = -1;

		private bool IsFullscreenAutoNativeSizeAdjusterEnabled;
		public bool IsFullscreenAutoNativeSizeAdjusterLoggingEnabled { get; set; }

		public void ActivateFullscreenAutoNativeSizeAdjuster(bool activate)
		{
			if (IsFullscreenAutoNativeSizeAdjusterEnabled == activate)
				return;
			IsFullscreenAutoNativeSizeAdjusterEnabled = activate;

			if (activate)
			{
				// Just activated
				FullscreenAutoNativeSizeAdjuster_WasFullscreen = !Screen.fullScreen; // This will make sure calculation below will be invalidated
				CalculateFullscreenAutoNativeSizeAdjuster();
			}
			else
			{
				// Just deactivated
			}
		}

		private void CalculateFullscreenAutoNativeSizeAdjuster()
		{
			if (FullscreenAutoNativeSizeAdjuster_PostponeCounter >= 0)
			{
				if (!Screen.fullScreen)
				{
					FullscreenAutoNativeSizeAdjuster_PostponeCounter = -1;
				}
				else if (--FullscreenAutoNativeSizeAdjuster_PostponeCounter == 0)
				{
					FullscreenAutoNativeSizeAdjuster_PostponeCounter = -1;
					InternalSetToNativeSizeInFullscreen();
				}
			}

			var isFullscreen = Screen.fullScreen;
			if (FullscreenAutoNativeSizeAdjuster_WasFullscreen != isFullscreen)
			{
				FullscreenAutoNativeSizeAdjuster_WasFullscreen = isFullscreen;
				if (isFullscreen)
				{
					FullscreenAutoNativeSizeAdjuster_PostponeCounter = 2;
				}
			}
		}

		private void InternalSetToNativeSizeInFullscreen()
		{
			var activeDisplayIndex = GetActiveDisplayIndex();
			if (activeDisplayIndex >= 0)
			{
				var activeDisplay = Display.displays[activeDisplayIndex];
				var width = activeDisplay.systemWidth;
				var height = activeDisplay.systemHeight;
				if (width > 0 && height > 0)
				{
					if (IsFullscreenAutoNativeSizeAdjusterLoggingEnabled)
						Log.Info($"Adjusting screen resolution to native {width}x{height} on monitor {activeDisplayIndex}.");
					//activeDisplay.SetParams(width, height, 0, 0);
					//activeDisplay.SetRenderingResolution(width, height);
					Screen.SetResolution(width, height, true);
				}
				else
				{
					if (IsFullscreenAutoNativeSizeAdjusterLoggingEnabled)
						Log.Info($"Failed to get screen resolution of active monitor {activeDisplayIndex}.");
				}
			}
			else
			{
				if (IsFullscreenAutoNativeSizeAdjusterLoggingEnabled)
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

				heightBasedSizeFactor = screenHeightInches / heightBaseInches;
				heightBasedSizeFactorInverted = 1f / heightBasedSizeFactor;
			}
			else
			{
				screenWidthInches = float.NaN;
				screenHeightInches = float.NaN;
				screenDiagonalInches = float.NaN;

				heightBasedSizeFactor = 1f;
				heightBasedSizeFactorInverted = 1f;
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
		private int previousFullscreen = int.MinValue;

		private float screenWidthInches = float.NaN;
		private float screenHeightInches = float.NaN;
		private float screenDiagonalInches = float.NaN;
		private bool screenSizeInchesAvailable = false;

		private float heightBasedSizeFactor = float.NaN;
		private float heightBasedSizeFactorInverted = float.NaN;

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

		public float HeightBasedSizeFactor
		{
			get { return heightBasedSizeFactor; }
		}

		public float HeightBasedSizeFactorInverted
		{
			get { return heightBasedSizeFactorInverted; }
		}

		#endregion
	}

}

#endif
