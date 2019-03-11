using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Allows easily picking a Sprite from the Sprites list and assign it to Image component.
	/// It can also set the Image's size to Sprite's native size if required.
	/// </summary>
	public class SpriteList : MonoBehaviour
	{
		public Sprite[] Sprites;

		public Image Target;

		public void ClearSprite(bool setZeroSize)
		{
			SelectSprite(-1, setZeroSize);
		}

		public void SelectSprite(int index, bool setNativeSize)
		{
			var sprite = index < 0 || index >= Sprites.Length
				? null
				: Sprites[index];
			Target.sprite = sprite;
			if (setNativeSize)
			{
				if (sprite)
				{
					Target.SetNativeSize();
				}
				else
				{
					Target.rectTransform.sizeDelta = Vector2.zero;
				}
			}
		}
	}

}
