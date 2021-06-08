using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public static class ChecklistStyles
	{
		internal const int SmallIconSize = 20;
		internal const int MidIconSize = 30;
		internal const int BigIconSize = 40;

		internal static GUILayoutOption[] SmallIconLayoutOptions = new GUILayoutOption[]
		{
			GUILayout.Width(SmallIconSize),
			GUILayout.Height(SmallIconSize)
		};

		internal static GUILayoutOption[] MidIconLayoutOptions = new GUILayoutOption[]
		{
			GUILayout.Width(MidIconSize),
			GUILayout.Height(MidIconSize)
		};

		internal static GUILayoutOption[] BigIconLayoutOptions = new GUILayoutOption[]
		{
			GUILayout.Width(BigIconSize),
			GUILayout.Height(BigIconSize)
		};

		private static GUIStyle _StatusDropdownStyle;
		internal static GUIStyle StatusDropdownStyle
		{
			get
			{
				if (_StatusDropdownStyle == null)
				{
					_StatusDropdownStyle = new GUIStyle(EditorStyles.popup);
					_StatusDropdownStyle.fixedHeight = SmallIconSize;
					_StatusDropdownStyle.margin = new RectOffset();
					_StatusDropdownStyle.border = new RectOffset();
				}
				return _StatusDropdownStyle;
			}
		}

		private static GUIStyle _IconStyle;
		internal static GUIStyle IconStyle
		{
			get
			{
				if (_IconStyle == null)
				{
					_IconStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
					_IconStyle.margin = new RectOffset();
					_IconStyle.padding = new RectOffset(2, 2, 2, 2);
				}
				return _IconStyle;
			}
		}
	}

}
