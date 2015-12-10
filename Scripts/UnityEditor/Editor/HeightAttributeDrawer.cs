using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditor
{

	[CustomPropertyDrawer(typeof(HeightAttribute))]
	public class HeightAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			HeightAttribute heightAttribute = (HeightAttribute)attribute;
			return heightAttribute.height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			HeightAttribute heightAttribute = (HeightAttribute)attribute;
			EditorGUI.PropertyField(position, property, label, heightAttribute.includeChildren);
		}
	}

}
