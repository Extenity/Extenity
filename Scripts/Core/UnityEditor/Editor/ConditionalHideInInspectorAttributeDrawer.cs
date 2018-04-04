using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	[CustomPropertyDrawer(typeof(ConditionalHideInInspectorAttribute), true)]
	public class ConditionalHideInInspectorAttributeDrawer : PropertyDrawer
	{
		#region GUI

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var theAttribute = (ConditionalHideInInspectorAttribute)attribute;
			var result = theAttribute.DecideIfEnabled(property);

			if (result != ConditionalHideResult.Hide)
			{
				var wasEnabled = GUI.enabled;
				GUI.enabled = result == ConditionalHideResult.Show;
				EditorGUI.PropertyField(position, property, label, true);
				GUI.enabled = wasEnabled;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var theAttribute = (ConditionalHideInInspectorAttribute)attribute;
			var result = theAttribute.DecideIfEnabled(property);

			if (result != ConditionalHideResult.Hide)
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				// Roll back what Unity has done for us.
				return -EditorGUIUtility.standardVerticalSpacing;
			}
		}

		#endregion
	}

}
