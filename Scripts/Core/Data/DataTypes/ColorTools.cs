using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public static class ColorTools
	{
		public static Color Red { get { return new Color(1f, 0.0f, 0.0f, 1f); } }
		public static Color DimmedRed { get { return new Color(0.8f, 0.0f, 0.0f, 1f); } }
		public static Color DarkRed { get { return new Color(0.5f, 0.0f, 0.0f, 1f); } }
		public static Color Green { get { return new Color(0.0f, 1f, 0.0f, 1f); } }
		public static Color DimmedGreen { get { return new Color(0.0f, 0.8f, 0.0f, 1f); } }
		public static Color DarkGreen { get { return new Color(0.0f, 0.5f, 0.0f, 1f); } }
		public static Color Blue { get { return new Color(0.0f, 0.0f, 1f, 1f); } }
		public static Color DimmedBlue { get { return new Color(0.0f, 0.0f, 0.8f, 1f); } }
		public static Color DarkBlue { get { return new Color(0.0f, 0.0f, 0.5f, 1f); } }

		public static bool IsAlmostEqualRGBA(this Color value1, Color value2, float precision)
		{
			double rDiff = value1.r - value2.r;
			double gDiff = value1.g - value2.g;
			double bDiff = value1.b - value2.b;
			double aDiff = value1.a - value2.a;
			return
				rDiff <= precision && rDiff >= -precision &&
				gDiff <= precision && gDiff >= -precision &&
				bDiff <= precision && bDiff >= -precision &&
				aDiff <= precision && aDiff >= -precision;
		}

		public static bool IsAlmostEqualRGB(this Color value1, Color value2, float precision)
		{
			double rDiff = value1.r - value2.r;
			double gDiff = value1.g - value2.g;
			double bDiff = value1.b - value2.b;
			return
				rDiff <= precision && rDiff >= -precision &&
				gDiff <= precision && gDiff >= -precision &&
				bDiff <= precision && bDiff >= -precision;
		}

		public static bool IsAlmostEqualA(this Color value1, Color value2, float precision)
		{
			double aDiff = value1.a - value2.a;
			return aDiff <= precision && aDiff >= -precision;
		}

		public static Int32 ToInt32(this Color value)
		{
			return
				(int)(value.r * 255f) +
				((int)(value.g * 255f) << 8) +
				((int)(value.b * 255f) << 16) +
				((int)(value.a * 255f) << 24);
		}

		public static Int32 ToInt32(this Color32 value)
		{
			return
				(value.r) +
				(value.g << 8) +
				(value.b << 16) +
				(value.a << 24);
		}

		public static Color32 ToColor32(this Int32 value)
		{
			if (value == 0)
				return new Color(0f, 0f, 0f, 0f);
			return new Color(
				((value & 0x000000FF)),
				((value & 0X0000FF00) >> 8),
				((value & 0X00FF0000) >> 16),
				((value & 0XFF000000) >> 24)
				);
		}

		public static Color ToColor(this Int32 value)
		{
			if (value == 0)
				return new Color(0f, 0f, 0f, 0f);
			return new Color(
				((value & 0x000000FF)) / 255f,
				((value & 0X0000FF00) >> 8) / 255f,
				((value & 0X00FF0000) >> 16) / 255f,
				((value & 0XFF000000) >> 24) / 255f
				);
		}

		public static Color ParseHexColor(this string hex)
		{
			if (hex.StartsWith("0x"))
				hex = hex.Remove(0, 2);
			else if (hex.StartsWith("#"))
				hex = hex.Remove(0, 1);

			if (hex.Length != 6 && hex.Length != 8)
			{
				throw new ArgumentException("Hex string should be length of 6 (RGB) or 8 (RGBA)", "hex");
			}

			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

			byte a = 255; //assume fully visible unless specified in hex
			if (hex.Length == 8)
			{
				a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			}

			return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		public static Color32 ParseHexColor32(this string hex)
		{
			if (hex.StartsWith("0x"))
				hex = hex.Remove(0, 2);
			else if (hex.StartsWith("#"))
				hex = hex.Remove(0, 1);

			if (hex.Length != 6 && hex.Length != 8)
			{
				throw new ArgumentException("Hex string should be length of 6 (RGB) or 8 (RGBA)", "hex");
			}

			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

			byte a = 255; //assume fully visible unless specified in hex
			if (hex.Length == 8)
			{
				a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			}

			return new Color32(r, g, b, a);
		}

		#region Lerp

		public static Color FastLerp(Color a, Color b, float t)
		{
			if (t < 0.0f)
				t = 0.0f;
			else if (t > 1.0f)
				t = 1.0f;

			return new Color(
				a.r + (b.r - a.r) * t,
				a.g + (b.g - a.g) * t,
				a.b + (b.b - a.b) * t,
				a.a + (b.a - a.a) * t);
		}

		public static Color32 FastLerp(Color32 a, Color32 b, float t)
		{
			if (t < 0.0f)
				t = 0.0f;
			else if (t > 1.0f)
				t = 1.0f;

			return new Color32(
				(byte)(a.r + (byte)(((int)b.r - (int)a.r) * t)),
				(byte)(a.g + (byte)(((int)b.g - (int)a.g) * t)),
				(byte)(a.b + (byte)(((int)b.b - (int)a.b) * t)),
				(byte)(a.a + (byte)(((int)b.a - (int)a.a) * t)));
		}

		#endregion

		#region HSL

		public static Color32 HSL2RGB(float h, float sl, float l)
		{
			var v = (l <= 0.5f) ? (l * (1.0f + sl)) : (l + sl - l * sl);

			if (v > 0f)
			{
				var m = l + l - v;
				var sv = (v - m) / v;
				h *= 6.0f;
				var sextant = (int)h;
				var fract = h - sextant;
				var vsf = v * sv * fract;
				var mid1 = m + vsf;
				var mid2 = v - vsf;

				switch (sextant)
				{
					case 0:
						return new Color32(
							(byte)(v * 255f),
							(byte)(mid1 * 255f),
							(byte)(m * 255f),
							255);
					case 1:
						return new Color32(
							(byte)(mid2 * 255f),
							(byte)(v * 255f),
							(byte)(m * 255f),
							255);
					case 2:
						return new Color32(
							(byte)(m * 255f),
							(byte)(v * 255f),
							(byte)(mid1 * 255f),
							255);
					case 3:
						return new Color32(
							(byte)(m * 255f),
							(byte)(mid2 * 255f),
							(byte)(v * 255f),
							255);
					case 4:
						return new Color32(
							(byte)(mid1 * 255f),
							(byte)(m * 255f),
							(byte)(v * 255f),
							255);
					case 5:
						return new Color32(
							(byte)(v * 255f),
							(byte)(m * 255f),
							(byte)(mid2 * 255f),
							255);
				}
			}

			return new Color32(
				(byte)(l * 255f),
				(byte)(l * 255f),
				(byte)(l * 255f),
				255);
		}

		public static void RGB2HSL(Color rgb, out float h, out float s, out float l)
		{
			var r = rgb.r;
			var g = rgb.g;
			var b = rgb.b;

			h = 0f; // default to black
			s = 0f;
			l = 0f;
			var v = Mathf.Max(Mathf.Max(r, g), b);
			var m = Mathf.Min(Mathf.Min(r, g), b);
			l = (m + v) / 2.0f;
			if (l <= 0.0f)
			{
				return;
			}
			var vm = v - m;
			s = vm;
			if (s > 0.0f)
			{
				s /= (l <= 0.5f) ? (v + m) : (2.0f - v - m);
			}
			else
			{
				return;
			}
			var r2 = (v - r) / vm;
			var g2 = (v - g) / vm;
			var b2 = (v - b) / vm;
			if (r == v)
			{
				h = (g == m ? 5.0f + b2 : 1.0f - g2);
			}
			else if (g == v)
			{
				h = (b == m ? 1.0f + r2 : 3.0f - b2);
			}
			else
			{
				h = (r == m ? 3.0f + g2 : 5.0f - r2);
			}
			h /= 6.0f;
		}

		#endregion
	}

}
