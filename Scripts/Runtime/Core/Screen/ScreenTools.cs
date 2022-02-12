#if UNITY

using UnityEngine;

namespace Extenity.ScreenToolbox
{

	public static class ScreenTools
	{
		#region SafeArea

		public static Rect SafeArea => Screen.safeArea;

		#endregion

		#region Apply SafeArea To Transform

		public static void ApplySafeArea(Canvas containerCanvas, RectTransform panelThatFitsIntoSafeAreaOfCanvas)
		{
			var safeArea = SafeArea;
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
