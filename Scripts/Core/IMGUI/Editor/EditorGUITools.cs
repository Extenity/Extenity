using UnityEditor;
using UnityEditorInternal;

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
	}

}
