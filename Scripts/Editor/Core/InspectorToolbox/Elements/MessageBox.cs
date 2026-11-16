using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A message with a severity icon and wrapping text: information, a warning or an error.
	/// </summary>
	public class MessageBox : VisualElement
	{
		public const string UssClassName = "ext-message";
		public const string IconUssClassName = UssClassName + "__icon";
		public const string TextUssClassName = UssClassName + "__text";

		private readonly Image Icon;
		private readonly Label TextLabel;
		private MessageSeverity _Severity;

		public MessageBox() : this(null, MessageSeverity.Info)
		{
		}

		public MessageBox(string text, MessageSeverity severity = MessageSeverity.Info)
		{
			AddToClassList(UssClassName);
			Icon = new Image { pickingMode = PickingMode.Ignore };
			Icon.AddToClassList(IconUssClassName);
			Add(Icon);
			TextLabel = new Label { enableRichText = true };
			TextLabel.selection.isSelectable = true;
			TextLabel.AddToClassList(TextUssClassName);
			Add(TextLabel);

			_Severity = severity;
			ApplySeverity();
			Text = text;
		}

		public string Text
		{
			get => TextLabel.text;
			set => TextLabel.text = value ?? "";
		}

		public MessageSeverity Severity
		{
			get => _Severity;
			set
			{
				if (_Severity == value)
					return;
				RemoveFromClassList(GetSeverityClass(_Severity));
				_Severity = value;
				ApplySeverity();
			}
		}

		private void ApplySeverity()
		{
			AddToClassList(GetSeverityClass(_Severity));
			var iconName = _Severity switch
			{
				MessageSeverity.Info => "console.infoicon.sml",
				MessageSeverity.Warning => "console.warnicon.sml",
				MessageSeverity.Error => "console.erroricon.sml",
				_ => null,
			};
			Icon.image = iconName != null ? EditorGUIUtility.IconContent(iconName).image : null;
			Icon.style.display = iconName != null ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private static string GetSeverityClass(MessageSeverity severity)
		{
			return severity switch
			{
				MessageSeverity.Info => UssClassName + "--info",
				MessageSeverity.Warning => UssClassName + "--warning",
				MessageSeverity.Error => UssClassName + "--error",
				_ => UssClassName + "--none",
			};
		}
	}

}
