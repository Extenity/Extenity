using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// The Header and Space attributes and the tooltip of a field, for a field that is not drawn by a property
	/// field. A property field draws them itself. An element that replaces it (a list, a dropdown, a section)
	/// would lose them without this.
	/// </summary>
	public static class FieldDecorators
	{
		public const string UssClassName = "ext-decorators";
		public const string HeaderUssClassName = UssClassName + "__header";
		public const string SpaceUssClassName = UssClassName + "__space";

		/// <summary>The headers and spaces declared on the field, in the order Unity draws them. Null when the field has none.</summary>
		public static VisualElement Create(FieldInfo field)
		{
			if (field == null)
				return null;

			List<PropertyAttribute> decorators = null;
			foreach (var attribute in field.GetCustomAttributes<PropertyAttribute>(true))
			{
				if (attribute is HeaderAttribute || attribute is SpaceAttribute)
				{
					decorators ??= new List<PropertyAttribute>();
					decorators.Add(attribute);
				}
			}
			if (decorators == null)
				return null;

			// Unity draws decorators by their order value. An insertion sort keeps the declared order of equal values.
			for (var i = 1; i < decorators.Count; i++)
			{
				var moved = decorators[i];
				var j = i - 1;
				while (j >= 0 && decorators[j].order > moved.order)
				{
					decorators[j + 1] = decorators[j];
					j--;
				}
				decorators[j + 1] = moved;
			}

			var container = new VisualElement { pickingMode = PickingMode.Ignore };
			container.AddToClassList(UssClassName);
			foreach (var decorator in decorators)
			{
				if (decorator is HeaderAttribute header)
				{
					var title = new VisualElement { pickingMode = PickingMode.Ignore };
					title.AddToClassList(TitleGroup.UssClassName);
					title.AddToClassList(HeaderUssClassName);
					var label = new Label(header.header) { pickingMode = PickingMode.Ignore };
					label.AddToClassList(TitleGroup.LabelUssClassName);
					title.Add(label);
					container.Add(title);
				}
				else
				{
					var space = new VisualElement { pickingMode = PickingMode.Ignore };
					space.AddToClassList(SpaceUssClassName);
					space.style.height = ((SpaceAttribute)decorator).height;
					container.Add(space);
				}
			}
			return container;
		}

		/// <summary>
		/// Shows the tooltip where the name of the field is: on its label, or on the header of its foldout.
		/// Falls back to the whole element when it has neither.
		/// </summary>
		public static void ApplyTooltip(VisualElement fieldElement, string tooltip)
		{
			if (fieldElement == null || string.IsNullOrEmpty(tooltip))
				return;
			var target = FindNameElement(fieldElement, 0) ?? fieldElement;
			if (string.IsNullOrEmpty(target.tooltip))
				target.tooltip = tooltip;
		}

		private const string BaseFieldLabelClass = "unity-base-field__label";
		private const int MaxNameElementDepth = 3;

		private static VisualElement FindNameElement(VisualElement element, int depth)
		{
			if (depth > MaxNameElementDepth)
				return null;
			if ((element is Label && element.ClassListContains(BaseFieldLabelClass))
			    || (element is Toggle && element.ClassListContains(Foldout.toggleUssClassName)))
				return element;
			// The name is near the top of the element. Rows and nested fields are deeper and have names of their own.
			var childCount = element.hierarchy.childCount;
			for (var i = 0; i < childCount; i++)
			{
				var found = FindNameElement(element.hierarchy[i], depth + 1);
				if (found != null)
					return found;
			}
			return null;
		}
	}

}
