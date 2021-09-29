using UnityEngine;
using UnityEditor;

namespace Extenity.MathToolbox.Editor
{

	[CustomPropertyDrawer(typeof(ClampedInt))]
	public class ClampedIntDrawer : PropertyDrawer
	{
		public bool IsLimitsVisible { get; private set; }

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return IsLimitsVisible ? 16f + 18f : 16f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var valueProperty = property.FindPropertyRelative("value");
			var minProperty = property.FindPropertyRelative("min");
			var maxProperty = property.FindPropertyRelative("max");

			var min = minProperty.intValue;
			var max = maxProperty.intValue;
			if (min > max)
			{
				min = default(int);
				max = default(int);
			}

			Rect contentPosition = EditorGUI.PrefixLabel(position, label);
			contentPosition.height = 16;
			EditorGUI.IntSlider(contentPosition, valueProperty, min, max, GUIContent.none);

			IsLimitsVisible = EditorGUI.Foldout(contentPosition, IsLimitsVisible, GUIContent.none);
			if (IsLimitsVisible)
			{
				contentPosition.y += 18;
				contentPosition.width /= 2;

				const int MinMaxLabelWidth = 30;

				var textBoxPosition = contentPosition;
				textBoxPosition.x += MinMaxLabelWidth;
				textBoxPosition.width -= MinMaxLabelWidth;
				GUI.Label(contentPosition, "Min");
				EditorGUI.PropertyField(textBoxPosition, minProperty, GUIContent.none);

				contentPosition.x += contentPosition.width;

				textBoxPosition.x = contentPosition.x + MinMaxLabelWidth;
				GUI.Label(contentPosition, "Max");
				EditorGUI.PropertyField(textBoxPosition, maxProperty, GUIContent.none);
			}

			EditorGUI.EndProperty();
		}
	}

}
