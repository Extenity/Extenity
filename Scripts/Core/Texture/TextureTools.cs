using System;
using System.Text;
using UnityEngine;

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
			var newTexture = new Texture2D(texture.width, texture.height);
			int mipCount = texture.mipmapCount;

			for (int mip = 0; mip < mipCount; ++mip)
			{
				Color[] cols = texture.GetPixels(mip);

				for (int i = 0; i < cols.Length; ++i)
				{
					cols[i].r *= tintColor.r;
					cols[i].g *= tintColor.g;
					cols[i].b *= tintColor.b;
				}

				newTexture.SetPixels(cols, mip);
			}

			newTexture.Apply(false);
			return newTexture;
		}

		public static Texture2D CreateSimpleTexture(int width, int height, Color color)
		{
			var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

			for (int y = 0; y < texture.height; ++y)
			{
				for (int x = 0; x < texture.width; ++x)
				{
					texture.SetPixel(x, y, color);
				}
			}

			texture.Apply(false);
			texture.hideFlags = HideFlags.DontSave;
			return texture;
		}

		public static Texture2D CreateVerticalGradientTexture(params Color32[] colors)
		{
			var width = 2;
			var height = colors.Length;
			var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
			var pixels = texture.GetPixels32();

			var i = 0;
			for (int y = 0; y < height; y++)
			{
				var color = colors[height - y - 1];
				for (int x = 0; x < width; x++)
				{
					pixels[i++] = color;
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply(false);
			texture.hideFlags = HideFlags.DontSave;
			return texture;
		}

		/// <summary>
		/// Source: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
		/// </summary>
		public static Texture2D CopyTextureAsReadable(this Texture2D texture)
		{
			// Create a temporary RenderTexture of the same size as the texture
			RenderTexture tmp = RenderTexture.GetTemporary(
								texture.width,
								texture.height,
								0,
								RenderTextureFormat.Default,
								RenderTextureReadWrite.Linear);

			// Blit the pixels on texture to the RenderTexture
			Graphics.Blit(texture, tmp);

			// Backup the currently set RenderTexture
			RenderTexture previous = RenderTexture.active;

			// Set the current RenderTexture to the temporary one we created
			RenderTexture.active = tmp;

			// Create a new readable Texture2D to copy the pixels to it
			Texture2D myTexture2D = new Texture2D(texture.width, texture.width);

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
			// Create a temporary RenderTexture of the same size as the texture
			RenderTexture tmp = RenderTexture.GetTemporary(
								newWidth,
								newHeight,
								0,
								RenderTextureFormat.Default,
								RenderTextureReadWrite.sRGB);

			// Blit the pixels on texture to the RenderTexture
			Graphics.Blit(texture, tmp);

			// Backup the currently set RenderTexture
			RenderTexture previous = RenderTexture.active;

			// Set the current RenderTexture to the temporary one we created
			RenderTexture.active = tmp;

			// Create a new readable Texture2D to copy the pixels to it
			Texture2D myTexture2D = new Texture2D(newWidth, newHeight);

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
			stringBuilder.AppendLine(indentation + "			_Texture_" + textureName + " = new Texture2D(1, 1, TextureFormat." + format + ", " + mipmapEnabled.ToString().ToLower() + ", " + linear.ToString().ToLower() + ");");
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
			stringBuilder.Append(indentation + '	');
			var counter = 0;
			for (int i = 0; i < data.Length; i++)
			{
				stringBuilder.Append(((int)data[i]) + ",");
				if (++counter > 50)
				{
					counter = 0;
					stringBuilder.Length--;
					if (i < data.Length - 1) // Line break if there are more bytes to write
					{
						stringBuilder.Append(Environment.NewLine + indentation + '	');
					}
				}
			}
			stringBuilder.AppendLine(Environment.NewLine + indentation + "};");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(indentation + "#endregion");
			return "Texture_" + textureName;
		}

		#endregion
	}

}
