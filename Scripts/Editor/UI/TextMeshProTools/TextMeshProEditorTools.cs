using UnityEditor;

namespace TMPro.Extensions
{

	public static class TextMeshProEditorTools
	{
		#region Warning For Warning Configuration

#if UNITY_EDITOR

		[InitializeOnLoadMethod]
		private static void CheckWarningConfiguration()
		{
			if (TMP_Settings.warningsDisabled)
			{
				Log.Warning("Please note that TextMesh Pro warnings are disabled. You should enable the warnings and fix them as soon as possible.");
			}
		}

#endif

		#endregion
	}

}