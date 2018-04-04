using UnityEngine;
using System;

namespace Extenity.UnityEditorToolbox
{

	public enum HideOrDisable
	{
		Hide,
		Disable,
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class ConditionalHideInInspectorAttribute : PropertyAttribute
	{
		public string FieldName;
		public object ExpectedValue;
		public bool Inverse = false;
		public HideOrDisable HideOrDisable = HideOrDisable.Hide;

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
		}

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue, HideOrDisable hideOrDisable)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
			HideOrDisable = hideOrDisable;
		}

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue, bool inverse)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
		}

		public ConditionalHideInInspectorAttribute(string fieldName, object expectedValue, bool inverse, HideOrDisable hideOrDisable)
		{
			FieldName = fieldName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
			HideOrDisable = hideOrDisable;
		}
	}

}
