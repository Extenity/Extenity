using Extenity.DataToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	[CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
	public class MinMaxRangeAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + 16;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.type != typeof(MinMaxRange).Name)
			{
				Log.Error($"MinMaxRange attribute can only be used on fields with MinMaxRange type. Check '{property.name}' field which is a type of '{property.type}'.");
			}
			else
			{
				var range = (MinMaxRangeAttribute)attribute;
				var minProperty = property.FindPropertyRelative("Min");
				var maxProperty = property.FindPropertyRelative("Max");
				var min = minProperty.floatValue;
				var max = maxProperty.floatValue;

				var xDivision = position.width * 0.37f;
				var yDivision = position.height * 0.5f;
				EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, yDivision), label);
				EditorGUI.LabelField(new Rect(position.x, position.y + yDivision, position.width, yDivision), range.MinLimit.ToString("0.##"));
				EditorGUI.LabelField(new Rect(position.x + position.width - 28f, position.y + yDivision, position.width, yDivision), range.MaxLimit.ToString("0.##"));
				EditorGUI.MinMaxSlider(new Rect(position.x + 24f, position.y + yDivision, position.width - 48f, yDivision), ref min, ref max, range.MinLimit, range.MaxLimit);

				EditorGUI.LabelField(new Rect(position.x + xDivision, position.y, xDivision, yDivision), "From: ");
				var newMin = EditorGUI.FloatField(new Rect(position.x + xDivision + 30, position.y, xDivision - 30, yDivision), min);
				min = Mathf.Clamp(newMin, range.MinLimit, max);
				EditorGUI.LabelField(new Rect(position.x + xDivision * 2f, position.y, xDivision, yDivision), "To: ");
				var newMax = EditorGUI.FloatField(new Rect(position.x + xDivision * 2f + 24, position.y, xDivision - 24, yDivision), max);
				max = Mathf.Clamp(newMax, min, range.MaxLimit);

				minProperty.floatValue = min;
				maxProperty.floatValue = max;
			}
		}

		#region Log

		private static readonly Logger Log = new(nameof(MinMaxRangeAttributeDrawer));

		#endregion
	}

}
