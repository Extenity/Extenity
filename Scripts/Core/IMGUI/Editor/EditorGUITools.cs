using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	public static class EditorGUITools
	{
		#region Thread Safe RepaintAllViews

		private static bool IsSafeRepaintInProgress;

		public static void SafeRepaintAllViews()
		{
			if (IsSafeRepaintInProgress)
				return;
			IsSafeRepaintInProgress = true;
			EditorApplication.delayCall += () =>
			{
				IsSafeRepaintInProgress = false;
				InternalEditorUtility.RepaintAllViews();
			};
		}

		#endregion

		#region MaskField

		//public static T EnumMaskField<T>(Rect position, GUIContent label, T value) where T : Enum
		//{
		//	return (T)EnumMaskField(position, (int)value, value.GetType(), label);
		//}

		// Source: https://answers.unity.com/questions/393992/custom-inspector-multi-select-enum-dropdown.html
		public static int EnumMaskField(Rect position, GUIContent label, int mask, Type enumType)
		{
			var itemNames = Enum.GetNames(enumType);
			var itemValues = (int[])Enum.GetValues(enumType);

			var val = mask;
			var maskVal = 0;
			for (int i = 0; i < itemValues.Length; i++)
			{
				if (itemValues[i] != 0)
				{
					if ((val & itemValues[i]) == itemValues[i])
						maskVal |= 1 << i;
				}
				else if (val == 0)
					maskVal |= 1 << i;
			}
			var newMaskVal = EditorGUI.MaskField(position, label, maskVal, itemNames);
			var changes = maskVal ^ newMaskVal;

			for (int i = 0; i < itemValues.Length; i++)
			{
				if ((changes & (1 << i)) != 0)            // has this list item changed?
				{
					if ((newMaskVal & (1 << i)) != 0)     // has it been set?
					{
						if (itemValues[i] == 0)           // special case: if "0" is set, just set the val to 0
						{
							val = 0;
							break;
						}
						else
							val |= itemValues[i];
					}
					else                                  // it has been reset
					{
						val &= ~itemValues[i];
					}
				}
			}
			return val;
		}

		#endregion
	}

}
