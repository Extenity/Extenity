using Extenity.DataToolbox;
using Extenity.DataToolbox.Editor;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Copy of UnityEditor.AssetPipelineAutoRefreshMode in AssetPipelinePreferences.cs file.
	/// </summary>
	public enum AssetPipelineAutoRefreshMode
	{
		Disabled = 0,
		Enabled = 1,
		EnabledOutsidePlaymode = 2
	}

	/// <summary>
	/// Tools for programmatically setting configurations in Unity Editor's Preferences window.
	///
	/// See the files in this directory for more of these settings:
	/// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/PreferencesWindow/
	/// </summary>
	public static class EditorPreferences
	{
		public static readonly EnumEditorPref<AssetPipelineAutoRefreshMode> AutoRefresh =
			new("kAutoRefreshMode",
			    PathHashPostfix.No,
			    DefaultValueMethod<AssetPipelineAutoRefreshMode>.FailIfKeyDoesNotExist(),
			    EditorPrefLogOptions.LogOnWriteWhenChanged);
	}

}