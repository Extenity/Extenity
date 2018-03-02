using Extenity.IMGUIToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
	public class EnumMaskAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label)
		{
			var castAttribute = (EnumMaskAttribute)attribute;
			if (castAttribute.ShowValueInLabel)
			{
				label.text = label.text + " (" + serializedProperty.intValue + ")";
			}
			EditorGUI.BeginChangeCheck();
			int newValue = EditorGUITools.EnumMaskField(position, label, serializedProperty.intValue, fieldInfo.FieldType);
			if (EditorGUI.EndChangeCheck())
			{
				serializedProperty.intValue = newValue;
			}
		}
	}

}
