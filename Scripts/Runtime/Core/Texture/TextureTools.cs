#if UNITY_5_3_OR_NEWER

using System;
using System.Text;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Extenity.TextureToolbox
{

	public static class TextureTools
	{
		public static Color Color(int r, int g, int b)
		{
			return new Color(r / 256f, g / 256f, b / 256f);
		}

		public static Texture2D Tint(Texture2D texture, Color tintColor)
		{
			var graphicsFormat = texture.graphicsFormat;
			int mipCount = texture.mipmapCount;

			var newTexture = new Texture2D(texture.width, texture.height, graphicsFormat, TextureCreationFlags.DontInitializePixels);

			for (int mip = 0; mip < mipCount; ++mip)
			{
				Color[] colors = texture.GetPixels(mip);

				switch (graphicsFormat)
				{
					case GraphicsFormat.R8G8B8_SRGB:
					{
						// ReSharper disable once CompareOfFloatsByEqualityOperator
						if (tintColor.a != 1)
						{
							Log.With(nameof(TextureTools)).Warning($"Trying to Tint a '{graphicsFormat}' texture that does not have alpha channel with a color that has alpha channel '{tintColor}'.");
						}
						for (int i = 0; i < colors.Length; ++i)
						{
							colors[i].r *= tintColor.r;
							colors[i].g *= tintColor.g;
							colors[i].b *= tintColor.b;
						}
						break;
					}
					case GraphicsFormat.R8G8B8A8_SRGB:
					{
						for (int i = 0; i < colors.Length; ++i)
						{
							colors[i].r *= tintColor.r;
							colors[i].g *= tintColor.g;
							colors[i].b *= tintColor.b;
							colors[i].a *= tintColor.a;
						}
						break;
					}
					default:
					{
						throw new NotImplementedException($"Texture Tint feature needs implementation for format {graphicsFormat}");
					}
				}

				newTexture.SetPixels(colors, mip);
			}

			newTexture.Apply(false);
			return newTexture;
		}

		#region Create Simple Textures

		public static Texture2D CreateSimpleTexture(Color32 color)
		{
			return CreateSimpleTexture(2, 2, color);
		}

		public static Texture2D CreateSimpleTexture(int width, int height, Color32 color)
		{
			// Automatically decide if the texture should have alpha channel
			var hasAlpha = color.a != 1;

			return CreateSimpleTexture(width, height, color, hasAlpha, true);
		}

		public static Texture2D CreateSimpleTexture(int width, int height, Color32 color, bool hasAlpha, bool isSRGB)
		{
			var texture = InternalCreateTexture(width, height, hasAlpha, isSRGB);
			// texture.wrapMode = TextureWrapMode.Clamp; Not sure if it will do any good.
			var pixels = CollectionTools.NewFilledArray(width * height, color);
			texture.SetPixels32(pixels);
			texture.Apply(false, true);
			return texture;
		}

		#endregion

		#region Create Gradient Textures

		public static Texture2D CreateVerticalGradientTexture(params Color32[] colorStops)
		{
			return CreateVerticalGradientTexture(true, colorStops);
		}

		public static Texture2D CreateVerticalGradientTexture(bool isSRGB, params Color32[] colorStops)
		{
			var width = 2;
			var height = colorStops.Length;

			// Automatically decide if the texture should have alpha channel
			var hasAlpha = colorStops.HasAnyTransparentColor();

			var texture = InternalCreateTexture(width, height, hasAlpha, isSRGB);

			texture.wrapMode = TextureWrapMode.Clamp;
			var pixels = texture.GetPixels32();
			var i = 0;
			for (int y = 0; y < height; y++)
			{
				var color = colorStops[height - y - 1];
				for (int x = 0; x < width; x++)
				{
					pixels[i++] = color;
				}
			}
			texture.SetPixels32(pixels);
			texture.Apply(false, true);
			return texture;
		}

		public static Texture2D CreateHorizontalGradientTexture(params Color32[] colorStops)
		{
			return CreateHorizontalGradientTexture(true, colorStops);
		}

		public static Texture2D CreateHorizontalGradientTexture(bool isSRGB, params Color32[] colorStops)
		{
			var height = 2;
			var width = colorStops.Length;

			// Automatically decide if the texture should have alpha channel
			var hasAlpha = colorStops.HasAnyTransparentColor();

			var texture = InternalCreateTexture(width, height, hasAlpha, isSRGB);

			texture.wrapMode = TextureWrapMode.Clamp;
			var pixels = texture.GetPixels32();
			var i = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					pixels[i++] = colorStops[x];
				}
			}
			texture.SetPixels32(pixels);
			texture.Apply(false, true);
			return texture;
		}

		#endregion

		#region Create Texture Internal

		private static Texture2D InternalCreateTexture(int width, int height, bool hasAlpha, bool isSRGB)
		{
			var textureFormat = hasAlpha
				? TextureFormat.RGBA32
				: TextureFormat.RGB24;
			var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(textureFormat, isSRGB);

			return InternalCreateTexture(width, height, graphicsFormat);
		}

		private static Texture2D InternalCreateTexture(int width, int height, GraphicsFormat graphicsFormat)
		{
			var texture = new Texture2D(width, height, graphicsFormat, TextureCreationFlags.DontInitializePixels);
			texture.hideFlags = HideFlags.DontSave;
			return texture;
		}

		#endregion

		#region Copy Texture As Readable

		/// <summary>
		/// Source: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
		/// </summary>
		public static Texture2D CopyTextureAsReadable(this Texture2D texture)
		{
			bool isSRGB = GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
			RenderTextureReadWrite readWrite = isSRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

			// Create a temporary RenderTexture of the same size as the texture
			RenderTexture tmp = RenderTexture.GetTemporary(
								texture.width,
								texture.height,
								0,
								RenderTextureFormat.Default,
								readWrite);

			// Blit the pixels on texture to the RenderTexture
			Graphics.Blit(texture, tmp);

			// Backup the currently set RenderTexture
			RenderTexture previous = RenderTexture.active;

			// Set the current RenderTexture to the temporary one we created
			RenderTexture.active = tmp;

			// Create a new readable Texture2D to copy the pixels to it. This must always be RGBA32
			// — NOT texture.graphicsFormat, and NOT RGB24 even for a source with no alpha channel
			// — because plenty of formats (the source's own, e.g. R8G8B8_SRGB or a compressed
			// format like ASTC/BC7; but also plain RGB24/R8G8B8 itself) are not valid formats for a
			// plain sampled Texture2D on every graphics API (confirmed failing under Metal here).
			// Creating one with such a format either fails outright ("not supported for Sample
			// usage", leaving a null-backed Texture2D that throws on the next call) or silently
			// fails to receive the ReadPixels copy ("Unable to retrieve image reference"). RGBA32
			// is universally supported and tmp is already an uncompressed RenderTextureFormat.
			// Default target, so no data is lost by always requesting 4 channels here.
			GraphicsFormat readableFormat = GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, isSRGB);
			var myTexture2D = InternalCreateTexture(texture.width, texture.height, readableFormat);

			// Copy the pixels from the RenderTexture to the new Texture
			myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
			myTexture2D.Apply();

			// Reset the active RenderTexture
			RenderTexture.active = previous;

			// Release the temporary RenderTexture
			RenderTexture.ReleaseTemporary(tmp);

			// "myTexture2D" now has the same pixels from "texture" and it's readable.
			return myTexture2D;
		}

		/// <summary>
		/// Source: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
		/// </summary>
		public static Texture2D ResizeAndCopyTextureAsReadable(this Texture2D texture, int newWidth, int newHeight)
		{
			bool isSRGB = GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
			RenderTextureReadWrite readWrite = isSRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

			// Create a temporary RenderTexture of the same size as the texture
			RenderTexture tmp = RenderTexture.GetTemporary(
								newWidth,
								newHeight,
								0,
								RenderTextureFormat.Default,
								readWrite);

			// Blit the pixels on texture to the RenderTexture
			Graphics.Blit(texture, tmp);

			// Backup the currently set RenderTexture
			RenderTexture previous = RenderTexture.active;

			// Set the current RenderTexture to the temporary one we created
			RenderTexture.active = tmp;

			// See CopyTextureAsReadable above for why this must always be RGBA32, never
			// texture.graphicsFormat and never RGB24.
			GraphicsFormat readableFormat = GraphicsFormatUtility.GetGraphicsFormat(TextureFormat.RGBA32, isSRGB);
			var myTexture2D = InternalCreateTexture(newWidth, newHeight, readableFormat);

			// Copy the pixels from the RenderTexture to the new Texture
			myTexture2D.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
			myTexture2D.Apply();

			// Reset the active RenderTexture
			RenderTexture.active = previous;

			// Release the temporary RenderTexture
			RenderTexture.ReleaseTemporary(tmp);

			// "myTexture2D" now has the same pixels from "texture" and it's readable.
			return myTexture2D;
		}

		#endregion

		#region Generate Embedded Code For Texture

		public static string GenerateEmbeddedCodeForTexture(byte[] data, string textureName, TextureFormat format, bool mipmapEnabled, bool linear, string indentation, ref StringBuilder stringBuilder)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}

			stringBuilder.AppendLine(indentation + "#region Embedded Texture - " + textureName);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(indentation + "private static Texture2D _Texture_" + textureName + ";");
			stringBuilder.AppendLine(indentation + "public static Texture2D Texture_" + textureName + "");
			stringBuilder.AppendLine(indentation + "{");
			stringBuilder.AppendLine(indentation + "	get");
			stringBuilder.AppendLine(indentation + "	{");
			stringBuilder.AppendLine(indentation + "		if (_Texture_" + textureName + " == null)");
			stringBuilder.AppendLine(indentation + "		{");
			stringBuilder.AppendLine("#if OverrideTextures");
			stringBuilder.AppendLine(indentation + "			_Texture_" + textureName + " = LoadTexture(\"" + textureName + "\");");
			stringBuilder.AppendLine("#else");
			stringBuilder.AppendLine(indentation + "			_Texture_" + textureName + " = new Texture2D(2, 2, TextureFormat." + format + ", " + mipmapEnabled.ToString().ToLower() + ", " + linear.ToString().ToLower() + ");");
			stringBuilder.AppendLine(indentation + "			_Texture_" + textureName + ".LoadImage(_TextureData_" + textureName + ", true);");
			stringBuilder.AppendLine("#endif");
			stringBuilder.AppendLine(indentation + "			Texture.DontDestroyOnLoad(_Texture_" + textureName + ");");
			stringBuilder.AppendLine(indentation + "			_Texture_" + textureName + ".hideFlags = HideFlags.HideAndDontSave;");
			stringBuilder.AppendLine(indentation + "		}");
			stringBuilder.AppendLine(indentation + "		return _Texture_" + textureName + ";");
			stringBuilder.AppendLine(indentation + "	}");
			stringBuilder.AppendLine(indentation + "}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(indentation + "private static readonly byte[] _TextureData_" + textureName + " = ");
			stringBuilder.AppendLine(indentation + "{");
			stringBuilder.AppendLine(indentation + "	// @formatter:off");
			stringBuilder.Append(indentation + '	');
			var counter = 0;
			for (int i = 0; i < data.Length; i++)
			{
				stringBuilder.Append(((int)data[i]).ToString());
				stringBuilder.Append(',');
				if (++counter > 50)
				{
					counter = 0;
					if (i < data.Length - 1) // Line break if there are more bytes to write
					{
						stringBuilder.AppendLine();
						stringBuilder.Append(indentation + '	');
					}
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(indentation + "	// @formatter:on");
			stringBuilder.AppendLine(indentation + "};");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(indentation + "#endregion");
			return "Texture_" + textureName;
		}

		#endregion
	}

}

#endif
