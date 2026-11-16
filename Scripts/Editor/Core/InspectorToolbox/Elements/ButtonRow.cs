using System;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A row of buttons of equal width. Also the place where the toolbox's button look is defined.
	/// </summary>
	public class ButtonRow : VisualElement
	{
		public const string UssClassName = "ext-button-row";
		public const string ButtonUssClassName = "ext-button";

		public ButtonRow()
		{
			AddToClassList(UssClassName);
		}

		public Button AddButton(string text, Action clicked, ButtonTone tone = ButtonTone.Neutral, ButtonSize size = ButtonSize.Normal)
		{
			var button = CreateButton(text, clicked, tone, size);
			Add(button);
			return button;
		}

		public static Button CreateButton(string text, Action clicked, ButtonTone tone = ButtonTone.Neutral, ButtonSize size = ButtonSize.Normal)
		{
			var button = new Button(clicked) { text = text };
			button.AddToClassList(ButtonUssClassName);
			switch (tone)
			{
				case ButtonTone.Positive:
					button.AddToClassList(ButtonUssClassName + "--positive");
					break;
				case ButtonTone.Destructive:
					button.AddToClassList(ButtonUssClassName + "--destructive");
					break;
			}
			switch (size)
			{
				case ButtonSize.Small:
					button.AddToClassList(ButtonUssClassName + "--small");
					break;
				case ButtonSize.Large:
					button.AddToClassList(ButtonUssClassName + "--large");
					break;
			}
			return button;
		}
	}

}
