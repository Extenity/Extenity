#if UNITY

using System.Collections.Generic;
using Extenity.MathToolbox;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.iOS;

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

		public static Dictionary<DeviceGeneration, MobileSafeAreaOverride> IOSSafeAreaOverrides = new Dictionary<DeviceGeneration, MobileSafeAreaOverride>()
		{
			// TODO: Portrait modes needs configuration.

			// @formatter:off
			{ DeviceGeneration.iPhoneX       , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhoneXR      , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhoneXS      , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhoneXSMax   , new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			{ DeviceGeneration.iPhone11      , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone11Pro   , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone11ProMax, new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			{ DeviceGeneration.iPhone12      , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone12Pro   , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone12ProMax, new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone12Mini  , new (RectTools.FromMinMax(0.045f, 0f, 0.955f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },

			{ DeviceGeneration.iPhone13      , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone13Pro   , new (RectTools.FromMinMax(0.040f, 0f, 0.960f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone13ProMax, new (RectTools.FromMinMax(0.037f, 0f, 0.963f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			{ DeviceGeneration.iPhone13Mini  , new (RectTools.FromMinMax(0.045f, 0f, 0.955f, 1f), RectTools.FromMinMax(0f, 0.000f, 1f, 1.000f)) },
			// @formatter:on
		};

		public static Rect SafeArea
		{
			get
			{
#if UNITY_IOS || UNITY_EDITOR
				var generation = Device.generation;
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
