using Extenity.UnityEditorToolbox.Editor;
using TMPro;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	public static class TextMeshProEditorTools
	{
		#region Warning For Warning Configuration

		[InitializeOnEditorLaunchMethod]
		private static void CheckWarningConfiguration()
		{
			// There is a possibility that TMP_Settings may not be initialized yet.
			// So delay the call, and make sure it won't fail by wrapping in try-catch.
			EditorApplication.delayCall += () =>
			{
				try
				{
					if (TMP_Settings.warningsDisabled)
					{
						Log.With("TextMeshPro", TMP_Settings.instance).Warning("Please note that TextMesh Pro warnings are disabled. You should enable the warnings and fix them as soon as possible.");
					}
				}
				catch { }
			};
		}

		#endregion
	}

}
