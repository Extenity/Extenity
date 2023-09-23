using Extenity.DataToolbox;
using Extenity.DataToolbox.Editor;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Tools for programmatically setting configurations in Unity Editor's Preferences window.
	/// </summary>
	public static class EditorPreferences
	{
		public static readonly BoolEditorPref AutoRefresh = new("kAutoRefresh",
		                                                        PathHashPostfix.No,
		                                                        DefaultValueMethod<bool>.FailIfKeyDoesNotExist(),
		                                                        EditorPrefLogOptions.LogOnWriteWhenChanged);
	}

}
