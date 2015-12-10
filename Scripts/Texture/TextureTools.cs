using UnityEngine;

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
}
