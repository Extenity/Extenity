using System;
using Extenity.ColoringToolbox;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class ColorScaleTexture : MonoBehaviour
	{
		#region Color Scale

		public IColorScale ColorScale;
		public ColorScaleAlignment Alignment = ColorScaleAlignment.Auto;

		public enum ColorScaleAlignment
		{
			Auto,
			Vertical,
			Horizontal,
		}

		#endregion

		#region Texture

		public Vector2Int TextureResolution;
		public TextureFormat TextureFormat = TextureFormat.RGBA32;
		public Texture2D Texture;

		#endregion

		#region Link To UI

		public RawImage UIImage;

		#endregion

		#region Create Texture

		public void RefreshTexture()
		{
			// TODO: refresh texture only on color scale changes

			if (ColorScale == null)
			{
				Log.Info("Color scale was not set.");
				return;
			}

			// Get configuration from UIImage
			if (UIImage != null)
			{
				//TextureResolution = UIImage.rectTransform.sizeDelta.ToVector2Int();
				TextureResolution = new Vector2Int(2, 1024); // TODO: TEMP

				if (Texture == null)
				{
					Texture = UIImage.texture as Texture2D;
				}
			}

			if (TextureResolution.x <= 0 || TextureResolution.y <= 0)
			{
				Log.Info("Texture resolution should be greater than zero.");
				return;
			}

			// Create texture if needed
			if (Texture == null || Texture.width != TextureResolution.x || Texture.height != TextureResolution.y || Texture.format != TextureFormat)
			{
				Texture = new Texture2D(TextureResolution.x, TextureResolution.y, TextureFormat, false);
			}

			var minimumValue = ColorScale.MinimumValue;
			var maximumValue = ColorScale.MaximumValue;
			var diff = maximumValue - minimumValue;

			var alignment = Alignment;
			if (Alignment == ColorScaleAlignment.Auto)
			{
				alignment = TextureResolution.x > TextureResolution.y
					? ColorScaleAlignment.Horizontal
					: ColorScaleAlignment.Vertical;
			}

			switch (alignment)
			{
				//case ColorScaleAlignment.Auto:  This is handled above
				case ColorScaleAlignment.Vertical:
					{
						var pixels = Texture.GetPixels32();
						int i = 0;
						for (int iy = 0; iy < TextureResolution.y; iy++)
						{
							var scalePoint = minimumValue + diff * ((float)iy / TextureResolution.y);
							var color = ColorScale.GetColor32(scalePoint);
							for (int ix = 0; ix < TextureResolution.x; ix++)
							{
								pixels[i] = color;
								i++;
							}
						}
						Texture.SetPixels32(pixels);
						Texture.Apply(false, false);
					}
					break;
				case ColorScaleAlignment.Horizontal:
					throw new NotImplementedException();
				default:
					throw new ArgumentOutOfRangeException();
			}

			// Set texture to UIImage
			if (UIImage != null)
			{
				UIImage.texture = Texture;
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(ColorScaleTexture));

		#endregion
	}

}
