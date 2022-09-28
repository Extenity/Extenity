#if UNITY

using System.Collections.Generic;
using Extenity.MathToolbox;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_IOS || UNITY_EDITOR
using UnityEngine.iOS;
#endif

namespace Extenity.ScreenToolbox
{

	public struct MobileSafeAreaOverride
	{
		public Rect Landscape;
		public Rect Portrait;

		public MobileSafeAreaOverride(Rect landscape, Rect portrait)
		{
			Landscape = landscape;
			Portrait = portrait;
		}
	}

	public static class ScreenTools
	{
		#region Customizable SafeArea

#if UNITY_IOS || UNITY_EDITOR
		public static DeviceGeneration IOSSafeAreaDeviceSimulation = DeviceGeneration.Unknown;

		public static Dictionary<DeviceGeneration, MobileSafeAreaOverride> IOSSafeAreaOverrides = new Dictionary<DeviceGeneration, MobileSafeAreaOverride>()
		{
			// TODO: Portrait modes needs configuration.

			// @formatter:off
			{ DeviceGeneration.iPhoneX       , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhoneXR      , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhoneXS      , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhoneXSMax   , new (RectTools.FromMinMax(0.034f, 0f, 0.966f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			{ DeviceGeneration.iPhone11      , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone11Pro   , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone11ProMax, new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			{ DeviceGeneration.iPhone12      , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone12Pro   , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone12ProMax, new (RectTools.FromMinMax(0.035f, 0f, 0.965f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone12Mini  , new (RectTools.FromMinMax(0.042f, 0f, 0.958f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			{ DeviceGeneration.iPhone13      , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone13Pro   , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone13ProMax, new (RectTools.FromMinMax(0.035f, 0f, 0.965f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone13Mini  , new (RectTools.FromMinMax(0.042f, 0f, 0.958f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			// A hopefully acceptable safe area that will be used for new Apple models until Unity adds support for them.
			{ DeviceGeneration.iPhoneUnknown , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			// @formatter:on
		};
#endif

		public static Rect SafeArea
		{
			get
			{
#if UNITY_IOS || UNITY_EDITOR
				var generation = IOSSafeAreaDeviceSimulation != DeviceGeneration.Unknown
					? IOSSafeAreaDeviceSimulation
					: Device.generation;
				if (IOSSafeAreaOverrides.TryGetValue(generation, out var safeArea))
				{
					// Log.Info($"Using safe area override for iOS device {generation}: " + safeArea);
					return ScreenTracker.Info.IsPortrait
						? safeArea.Portrait.MultipliedAndRounded(ScreenTracker.Info.Resolution)
						: safeArea.Landscape.MultipliedAndRounded(ScreenTracker.Info.Resolution);
				}
#endif
				return Screen.safeArea;
			}
		}

		#endregion

		#region Apply SafeArea To Transform

		public static void ApplyCustomizableSafeArea([NotNull] Canvas containerCanvas, [NotNull] RectTransform panelThatFitsIntoSafeAreaOfCanvas)
		{
			ApplySafeArea(containerCanvas, panelThatFitsIntoSafeAreaOfCanvas, ScreenTools.SafeArea);
		}

		public static void ApplyUnitySafeArea([NotNull] Canvas containerCanvas, [NotNull] RectTransform panelThatFitsIntoSafeAreaOfCanvas)
		{
			ApplySafeArea(containerCanvas, panelThatFitsIntoSafeAreaOfCanvas, Screen.safeArea);
		}

		public static void ApplySafeArea([NotNull] Canvas containerCanvas, [NotNull] RectTransform panelThatFitsIntoSafeAreaOfCanvas, Rect safeArea)
		{
			var anchorMin = safeArea.position;
			var anchorMax = safeArea.position + safeArea.size;
			var pixelRect = containerCanvas.pixelRect;
			anchorMin.x /= pixelRect.width;
			anchorMin.y /= pixelRect.height;
			anchorMax.x /= pixelRect.width;
			anchorMax.y /= pixelRect.height;

			panelThatFitsIntoSafeAreaOfCanvas.anchorMin = anchorMin;
			panelThatFitsIntoSafeAreaOfCanvas.anchorMax = anchorMax;
			panelThatFitsIntoSafeAreaOfCanvas.sizeDelta = Vector2.zero;
			panelThatFitsIntoSafeAreaOfCanvas.anchoredPosition = Vector2.zero;
		}

		#endregion

		#region Screen Areas

		public static Rect GetCenteredRect(int sizeX, int sizeY)
		{
			var info = ScreenTracker.Info;
			return new Rect(
				(int)((info.Width - sizeX) / 2),
				info.Height - (int)((info.Height - sizeY) / 2),
				sizeX,
				sizeY);
		}

		#endregion
	}

}

#endif
