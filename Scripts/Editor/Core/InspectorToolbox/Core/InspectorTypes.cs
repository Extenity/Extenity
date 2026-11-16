using System;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>How a nested inspector presents itself inside its parent.</summary>
	public enum HeaderMode
	{
		/// <summary>A foldout that follows the property's expanded state. The content is built when first opened.</summary>
		Foldout,
		/// <summary>No header: the fields are drawn directly in the parent.</summary>
		Inline,
		/// <summary>A bordered box with the label as its header, always open.</summary>
		Box,
	}

	public enum GroupStyle
	{
		/// <summary>A bold title above the fields, like Unity's Header attribute.</summary>
		Title,
		/// <summary>A bordered box with an optional header.</summary>
		Box,
		/// <summary>A foldout. Its state is kept per target type and title for the editor session.</summary>
		Foldout,
	}

	public enum MessageSeverity
	{
		None,
		Info,
		Warning,
		Error,
	}

	public enum ButtonTone
	{
		Neutral,
		Positive,
		Destructive,
	}

	public enum ButtonSize
	{
		Small,
		Normal,
		Large,
	}

	/// <summary>The result of a validation rule.</summary>
	public readonly struct ValidationResult
	{
		public readonly MessageSeverity Severity;
		public readonly string Message;

		public bool IsOk => Severity == MessageSeverity.None;

		private ValidationResult(MessageSeverity severity, string message)
		{
			Severity = severity;
			Message = message;
		}

		public static ValidationResult Ok => default;

		public static ValidationResult Error(string message)
		{
			return Create(MessageSeverity.Error, message);
		}

		public static ValidationResult Warning(string message)
		{
			return Create(MessageSeverity.Warning, message);
		}

		public static ValidationResult Info(string message)
		{
			return Create(MessageSeverity.Info, message);
		}

		private static ValidationResult Create(MessageSeverity severity, string message)
		{
			if (string.IsNullOrEmpty(message))
				throw new ArgumentException("A validation result that is not Ok needs a message.", nameof(message));
			return new ValidationResult(severity, message);
		}
	}

	/// <summary>A validation message an inspector currently shows.</summary>
	public readonly struct ValidationMessage
	{
		public readonly MessageSeverity Severity;
		public readonly string Message;
		/// <summary>The property the message belongs to. Null or empty for a message about the whole object.</summary>
		public readonly string PropertyPath;
		/// <summary>
		/// True when the message does not come from a validation but reports that a rule of the inspector threw
		/// (a predicate, a validator, a title): a bug of the inspector, not a problem of the data.
		/// </summary>
		public readonly bool IsRuleFailure;

		public ValidationMessage(MessageSeverity severity, string message, string propertyPath, bool isRuleFailure = false)
		{
			Severity = severity;
			Message = message;
			PropertyPath = propertyPath;
			IsRuleFailure = isRuleFailure;
		}

		public override string ToString()
		{
			return $"{Severity}: {Message} ({PropertyPath})";
		}
	}

	/// <summary>One choice of a dropdown.</summary>
	public readonly struct DropdownItem<TValue>
	{
		public readonly string Label;
		public readonly TValue Value;
		/// <summary>Optional submenu path, levels separated with '/'.</summary>
		public readonly string Group;

		public DropdownItem(string label, TValue value, string group = null)
		{
			Label = label;
			Value = value;
			Group = group;
		}
	}

	public struct DropdownOptions
	{
		/// <summary>Sort the items by label. Otherwise they keep the order of the source.</summary>
		public bool Sorted;
		/// <summary>
		/// When set, the dropdown offers this entry for the default value of the field (empty string, 0, no object),
		/// e.g. "(None)".
		/// </summary>
		public string EmptyLabel;
	}

	/// <summary>An action that may replace the target, used for buttons on structs and other nested values.</summary>
	public delegate void RefAction<T>(ref T target);

}
