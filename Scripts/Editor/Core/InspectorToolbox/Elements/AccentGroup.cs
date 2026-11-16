using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A group with a coloured bar on the left, a header tinted in the same colour and a faintly tinted
	/// background. Can be a foldout.
	/// </summary>
	public class AccentGroup : VisualElement
	{
		public const string UssClassName = "ext-accent-group";
		public const string HeaderUssClassName = UssClassName + "__header";
		public const string ContentUssClassName = UssClassName + "__content";

		private static readonly CustomStyleProperty<float> BackgroundAlphaProperty = new CustomStyleProperty<float>("--ext-accent-bg-alpha");
		private static readonly CustomStyleProperty<float> HeaderAlphaProperty = new CustomStyleProperty<float>("--ext-accent-header-alpha");

		private readonly VisualElement Header;
		private readonly VisualElement Content;
		private readonly Foldout Foldout;
		private readonly Label TitleLabel;
		private Color _Accent;
		private float BackgroundAlpha = 0.08f;
		private float HeaderAlpha = 0.16f;

		public override VisualElement contentContainer => Content;

		/// <param name="foldable">Makes the header a foldout toggle.</param>
		public AccentGroup(string title, Color accent, bool foldable = false, bool expanded = true)
		{
			AddToClassList(UssClassName);
			if (foldable)
			{
				Foldout = new Foldout { text = title, value = expanded };
				Foldout.AddToClassList(UssClassName + "__foldout");
				hierarchy.Add(Foldout);
				Header = Foldout.Q<Toggle>(className: Foldout.toggleUssClassName);
				Content = Foldout.contentContainer;
			}
			else
			{
				TitleLabel = new Label(title);
				Header = TitleLabel;
				hierarchy.Add(Header);
				Content = new VisualElement();
				hierarchy.Add(Content);
			}
			Header.AddToClassList(HeaderUssClassName);
			Content.AddToClassList(ContentUssClassName);

			_Accent = accent;
			ApplyColors();
			RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
		}

		public string Title
		{
			get => Foldout != null ? Foldout.text : TitleLabel.text;
			set
			{
				if (Foldout != null)
					Foldout.text = value;
				else
					TitleLabel.text = value ?? "";
			}
		}

		public Color Accent
		{
			get => _Accent;
			set
			{
				_Accent = value;
				ApplyColors();
			}
		}

		/// <summary>The foldout of a foldable group, to read or set its state. Null when the group is not foldable.</summary>
		public Foldout FoldoutElement => Foldout;

		private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
		{
			var changed = false;
			if (evt.customStyle.TryGetValue(BackgroundAlphaProperty, out var backgroundAlpha) && !Mathf.Approximately(backgroundAlpha, BackgroundAlpha))
			{
				BackgroundAlpha = backgroundAlpha;
				changed = true;
			}
			if (evt.customStyle.TryGetValue(HeaderAlphaProperty, out var headerAlpha) && !Mathf.Approximately(headerAlpha, HeaderAlpha))
			{
				HeaderAlpha = headerAlpha;
				changed = true;
			}
			if (changed)
				ApplyColors();
		}

		private void ApplyColors()
		{
			var solid = new Color(_Accent.r, _Accent.g, _Accent.b, 1f);
			style.borderLeftColor = solid;
			style.backgroundColor = new Color(_Accent.r, _Accent.g, _Accent.b, BackgroundAlpha);
			Header.style.backgroundColor = new Color(_Accent.r, _Accent.g, _Accent.b, HeaderAlpha);
		}
	}

}
