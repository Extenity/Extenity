using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Eases the use of TextMeshProUGUI sprite feature when used for displaying numbers.
	/// Just call SetNumberedText(123) and the required sprite tagged string is generated
	/// automatically to be interpreted by TextMeshPro.
	/// </summary>
	public class SpriteNumberedText : MonoBehaviour
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		public TextMeshProUGUI Text;
		public string SpriteNamePrefix;
		public string SpritePack;
		[ReadOnly]
		public string SpriteFormat;

		private void RefreshSpriteFormat()
		{
			if (string.IsNullOrEmpty(SpritePack))
			{
				SpriteFormat = $"<sprite name=\"{SpriteNamePrefix}{{0}}\">";
			}
			else
			{
				SpriteFormat = $"<sprite=\"{SpritePack}\" name=\"{SpriteNamePrefix}{{0}}\">";
			}
		}

		[Title("Test")]
		[Button(34, ButtonStyle.FoldoutButton, DisplayParameters = true, Expanded = true)]
		public void SetNumberedText(int number)
		{
			Text.SetText(BuildSpriteNumberedText(number));
		}

		//public void SetNumberedText(string prefix, int number)
		//{
		//	Text.SetText(prefix + BuildSpriteNumberedText(number));
		//}

		//public void SetNumberedText(int number, string postfix)
		//{
		//	Text.SetText(BuildSpriteNumberedText(number) + postfix);
		//}

		//public void SetNumberedText(string prefix, int number, string postfix)
		//{
		//	Text.SetText(prefix + BuildSpriteNumberedText(number) + postfix);
		//}

		//public void SetNumberedTextWithSprites(string prefix, int number)
		//{
		//	Text.SetText(BuildSpriteText(prefix) + BuildSpriteNumberedText(number));
		//}

		//public void SetNumberedTextWithSprites(int number, string postfix)
		//{
		//	Text.SetText(BuildSpriteNumberedText(number) + BuildSpriteText(postfix));
		//}

		//public void SetNumberedTextWithSprites(string prefix, int number, string postfix)
		//{
		//	Text.SetText(BuildSpriteText(prefix) + BuildSpriteNumberedText(number) + BuildSpriteText(postfix));
		//}

		#region Build Sprite-Numbered Text

		private static readonly char[] Chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		// Source: https://stackoverflow.com/questions/17575375/how-do-i-convert-an-int-to-a-string-in-c-sharp-without-using-tostring
		private string BuildSpriteNumberedText(int number)
		{
			var str = string.Empty;
			if (number == 0)
			{
				str = BuildSpriteText('0');
			}
			else if (number == int.MinValue) // Special case for MinValue
			{
				//str = "-2147483648";
				str = BuildSpriteText('-') + BuildSpriteNumberedText(int.MaxValue);
			}
			else
			{
				bool isNegative = (number < 0);
				if (isNegative)
				{
					number = -number;
				}

				while (number > 0)
				{
					str = BuildSpriteText(Chars[number % 10]) + str;
					number /= 10;
				}

				if (isNegative)
				{
					str = BuildSpriteText('-') + str;
				}
			}

			return str;
		}

		private string BuildSpriteText(char value)
		{
			return string.Format(SpriteFormat, value);
		}

		private string BuildSpriteText(string value)
		{
			return string.Format(SpriteFormat, value);
		}

		private string BuildSpriteText(object value)
		{
			return string.Format(SpriteFormat, value);
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		private void OnValidate()
		{
			RefreshSpriteFormat();
		}

		public void OnBeforeSerialize()
		{
			RefreshSpriteFormat();
		}

		public void OnAfterDeserialize()
		{
		}

#endif

		#endregion
	}

}
