using AttributeUsage = System.AttributeUsageAttribute;
using AttributeTargets = System.AttributeTargets;

// This is the way that Attributes are supported in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY
using BaseAttribute = UnityEngine.PropertyAttribute;
#else
using BaseAttribute = System.Attribute;
#endif

namespace Extenity.UnityEditorToolbox
{

	public enum HideOrDisable
	{
		Hide,
		Disable,
	}

	public enum ConditionalHideResult
	{
		Show,
		Hide,
		ShowDisabled,
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public class ConditionalHideInInspectorAttribute : BaseAttribute
	{
		/// <summary>
		/// A Field, Property or Method name that will be used to decide if 
		/// the field that has ConditionalHideInInspector should be displayed
		/// or not. If a Field or a Property is specified, the decision 
		/// will be made based on if the result of Field or Property value 
		/// is the same with ExpectedValue. If a Method is specified, the 
		/// decision is made by the user in the specified method and returned
		/// as a boolean from that method, meaning 'true' corresponds to be shown.
		/// 
		/// If a Method is specified, the method must have a return type of bool. 
		/// Optionally, it may have a parameter that gets the field value or
		/// it may have no parameters.
		/// </summary>
		public string FieldPropertyMethodName;
		/// <summary>
		/// Expected value is compared with Field and Property values to decide if 
		/// the field should be displayed or not. Expected value is ineffective 
		/// when ConditionalHideInInspector attribute is used with a Method, 
		/// instead of a Field or a Property.
		/// </summary>
		public object ExpectedValue = null;
		/// <summary>
		/// Inverses the result of the decision whether the field that has the
		/// ConditionalHideInInspector should be enabled or disabled.
		/// </summary>
		public bool Inverse = false;
		/// <summary>
		/// Whether the field that has the ConditionalHideInInspector attribute
		/// should be totally hidden or shown as disabled when the condition is
		/// not met.
		/// </summary>
		public HideOrDisable HideOrDisable = HideOrDisable.Hide;

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue, HideOrDisable hideOrDisable)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
			HideOrDisable = hideOrDisable;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue, bool inverse)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
		}

		public ConditionalHideInInspectorAttribute(string fieldPropertyMethodName, object expectedValue, bool inverse, HideOrDisable hideOrDisable)
		{
			FieldPropertyMethodName = fieldPropertyMethodName;
			ExpectedValue = expectedValue;
			Inverse = inverse;
			HideOrDisable = hideOrDisable;
		}
	}

}
