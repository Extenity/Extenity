#if UNITY

using System;
using Sirenix.OdinInspector;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.ScreenToolbox
{

	public class ScreenInfo : IEquatable<ScreenInfo>
	{
		public FullScreenMode FullScreenMode;
		public ScreenOrientation ScreenOrientation;
		public int Width;
		public int Height;
		public float Diagonal => sqrt(Width * Width + Height * Height);
		public float AspectRatio => (float)Height / Width;
		public bool IsPortrait => Height > Width;
		public bool IsLandscape => !IsPortrait;
		public float DPI;
		[InlineProperty]
		public Rect SafeArea;

		public float WidthInches => IsInchesInfoAvailable ? Width / DPI : float.NaN;
		public float HeightInches => IsInchesInfoAvailable ? Height / DPI : float.NaN;
		public float DiagonalInches => IsInchesInfoAvailable
			? sqrt(WidthInches * WidthInches + HeightInches * HeightInches)
			: float.NaN;
		public bool IsInchesInfoAvailable => DPI != 0f;

		#region Initialization

		public void CopyTo(ScreenInfo other)
		{
			other.FullScreenMode = FullScreenMode;
			other.ScreenOrientation = ScreenOrientation;
			other.Width = Width;
			other.Height = Height;
			other.DPI = DPI;
			other.SafeArea = SafeArea;
		}

		#endregion

		#region Collect Unity Screen Info

		public void CollectInfoFromUnity()
		{
			FullScreenMode = Screen.fullScreenMode;
			ScreenOrientation = Screen.orientation;
			Width = Screen.width;
			Height = Screen.height;
			DPI = Screen.dpi;
			SafeArea = ScreenTools.SafeArea;
		}

		#endregion

		#region Equality

		public bool Equals(ScreenInfo other)
		{
			return FullScreenMode == other.FullScreenMode &&
			       ScreenOrientation == other.ScreenOrientation &&
			       Width == other.Width &&
			       Height == other.Height &&
			       DPI.Equals(other.DPI) &&
			       SafeArea.Equals(other.SafeArea);
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return $"Mode: {FullScreenMode}/{ScreenOrientation}, Resolution: {Width}x{Height}, DPI: {DPI}, SafeArea: {SafeArea}";
		}

		#endregion
	}

}

#endif
