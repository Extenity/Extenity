using UnityEditor;

namespace Extenity.OperatingSystemToolbox
{

#if UNITY_STANDALONE_WIN

	public static class ClipboardEditorInitializer
	{
		[InitializeOnLoadMethod]
		private static void Register()
		{
			Clipboard.Initialize();
		}
	}

#endif

}
