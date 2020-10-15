using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public static class ChecklistConstants
	{
		internal const string Version = "1";

		#region GUI

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

		#endregion
	}

}
