using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Draws a struct that consists of a few numbers (a vector, a size, a range) on one line: the label of the
	/// struct, then one small number field per member, each under its own short name ("X", "Y", "Z"). Every
	/// number field is bound to its member, so undo, prefab override marks and dragging a label work as usual.
	/// <para>
	/// Unlike an IMGUI vector drawer it never falls back to two lines in a narrow place such as a list row, and
	/// its label is a regular field label, so a long one wraps instead of being cut.
	/// </para>
	/// </summary>
	public class NumberLineField : BaseField<int>
	{
		public new const string ussClassName = "ext-number-line";
		public const string NumberUssClassName = ussClassName + "__number";
		public const int MaxNumberCount = 6;

		/// <summary>The number fields, in member order.</summary>
		public IReadOnlyList<VisualElement> NumberFields => _NumberFields;
		private readonly List<VisualElement> _NumberFields = new List<VisualElement>();

		/// <param name="label">Null for the property's own name, empty for no label.</param>
		/// <param name="showMemberNames">False leaves out the short names in front of the numbers, for the cells of a table whose column header names the value.</param>
		public NumberLineField(SerializedProperty property, string label = null, bool showMemberNames = true)
			: this(property, label, showMemberNames, new VisualElement())
		{
		}

		private NumberLineField(SerializedProperty property, string label, bool showMemberNames, VisualElement input)
			: base(label ?? (property ?? throw new ArgumentNullException(nameof(property))).displayName, input)
		{
			if (!TryGetNumberMembers(property, out var members, out var problem))
				throw new ArgumentException($"'{property.propertyPath}' cannot be drawn as numbers on one line: {problem}", nameof(property));

			AddToClassList(ussClassName);
			AddToClassList(alignedFieldUssClassName);
			input.AddToClassList(ussClassName + "__input");

			foreach (var member in members)
			{
				var numberField = CreateNumberField(member, showMemberNames ? member.displayName : null);
				numberField.AddToClassList(NumberUssClassName);
				if (_NumberFields.Count == 0)
					numberField.AddToClassList(NumberUssClassName + "--first");
				numberField.tooltip = member.displayName;
				_NumberFields.Add(numberField);
				input.Add(numberField);
			}
		}

		/// <summary>True when the property is a struct or class whose visible members are all numbers, between one and six of them.</summary>
		public static bool CanDraw(SerializedProperty property)
		{
			return property != null && TryGetNumberMembers(property, out _, out _);
		}

		private static bool TryGetNumberMembers(SerializedProperty property, out List<SerializedProperty> members, out string problem)
		{
			members = new List<SerializedProperty>();
			problem = null;
			if (property.propertyType == SerializedPropertyType.Generic && !property.isArray)
			{
				var end = property.GetEndProperty();
				var iterator = property.Copy();
				if (iterator.NextVisible(true))
				{
					while (!SerializedProperty.EqualContents(iterator, end))
					{
						if (iterator.propertyType != SerializedPropertyType.Float && iterator.propertyType != SerializedPropertyType.Integer)
						{
							problem = $"its member '{iterator.name}' is a '{iterator.propertyType}', not a number.";
							return false;
						}
						members.Add(iterator.Copy());
						if (!iterator.NextVisible(false))
							break;
					}
				}
			}
			else if (property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector3 || property.propertyType == SerializedPropertyType.Vector4)
			{
				foreach (var memberName in new[] { "x", "y", "z", "w" })
				{
					var member = property.FindPropertyRelative(memberName);
					if (member != null)
						members.Add(member);
				}
			}
			else
			{
				problem = $"it is a '{property.propertyType}', not a struct of numbers.";
				return false;
			}

			if (members.Count == 0 || members.Count > MaxNumberCount)
			{
				problem = $"it has {members.Count} number members. Between 1 and {MaxNumberCount} fit on a line.";
				return false;
			}
			return true;
		}

		private static VisualElement CreateNumberField(SerializedProperty member, string memberLabel)
		{
			if (member.propertyType == SerializedPropertyType.Float)
			{
				if (member.numericType == SerializedPropertyNumericType.Double)
					return new DoubleField(memberLabel) { bindingPath = member.propertyPath };
				return new FloatField(memberLabel) { bindingPath = member.propertyPath };
			}
			switch (member.numericType)
			{
				case SerializedPropertyNumericType.Int64:
					return new LongField(memberLabel) { bindingPath = member.propertyPath };
				case SerializedPropertyNumericType.UInt64:
					return new UnsignedLongField(memberLabel) { bindingPath = member.propertyPath };
				case SerializedPropertyNumericType.UInt32:
					return new UnsignedIntegerField(memberLabel) { bindingPath = member.propertyPath };
				default:
					return new IntegerField(memberLabel) { bindingPath = member.propertyPath };
			}
		}
	}

}
