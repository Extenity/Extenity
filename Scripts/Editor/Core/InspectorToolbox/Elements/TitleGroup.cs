using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A bold title above its children, with the metrics of Unity's Header attribute.
	/// </summary>
	public class TitleGroup : VisualElement
	{
		public const string UssClassName = "ext-title";
		public const string LabelUssClassName = UssClassName + "__label";

		private readonly Label TitleLabel;
		private readonly VisualElement Content;

		public override VisualElement contentContainer => Content;

		public TitleGroup(string title)
		{
			AddToClassList(UssClassName);
			TitleLabel = new Label(title);
			TitleLabel.AddToClassList(LabelUssClassName);
			hierarchy.Add(TitleLabel);
			Content = new VisualElement();
			hierarchy.Add(Content);
		}

		public string Title
		{
			get => TitleLabel.text;
			set => TitleLabel.text = value ?? "";
		}
	}

}
