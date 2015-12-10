using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SingleLayer))]
public class SingleLayerPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, GUIContent.none, property);

		SerializedProperty layerIndex = property.FindPropertyRelative("_LayerIndex");

		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		if (layerIndex != null)
		{
			layerIndex.intValue = EditorGUI.LayerField(position, layerIndex.intValue);
		}

		EditorGUI.EndProperty();
	}
}
