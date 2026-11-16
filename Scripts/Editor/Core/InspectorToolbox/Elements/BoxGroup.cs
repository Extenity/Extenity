using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A bordered box with an optional bold header. Children are added to its content area.
	/// </summary>
	public class BoxGroup : VisualElement
	{
		public const string UssClassName = "ext-box";
		public const string HeaderUssClassName = UssClassName + "__header";
		public const string ContentUssClassName = UssClassName + "__content";

		private readonly Label Header;
		private readonly VisualElement Content;

		public override VisualElement contentContainer => Content;

		public BoxGroup() : this(null)
		{
		}

		public BoxGroup(string title)
		{
			AddToClassList(UssClassName);
			Header = new Label();
			Header.AddToClassList(HeaderUssClassName);
			hierarchy.Add(Header);
			Content = new VisualElement();
			Content.AddToClassList(ContentUssClassName);
			hierarchy.Add(Content);
			Title = title;
		}

		public string Title
		{
			get => Header.text;
			set
			{
				Header.text = value ?? "";
				Header.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
			}
		}
	}

}
