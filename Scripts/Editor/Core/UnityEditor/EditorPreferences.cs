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

		/// <summary>
		/// Enable-disable "Timestamp Editor log entries" option in Unity Editor's Preferences window.
		/// </summary>
		public static readonly BoolEditorPref TimestampEditorLogEntries =
			new("EnableExtendedLogging",
			    PathHashPostfix.No,
			    DefaultValueMethod<bool>.FailIfKeyDoesNotExist(),
			    EditorPrefLogOptions.LogOnWriteWhenChanged);

		/// <summary>
		/// Enable-disable "Stop Gradle daemons on exit" option in Unity Editor's Preferences window.
		/// The documentation says:
		///    Enable this option so that when you close the Editor, Unity stops Gradle.
		///    If you are using Gradle to build multiple Android Projects at the same time,
		///    disable this option as it might cause your other builds to fail.
		/// </summary>
		public static readonly BoolEditorPref StopGradleDaemonsOnExit =
			new("AndroidGradleStopDaemonsOnExit",
			    PathHashPostfix.No,
			    DefaultValueMethod<bool>.FailIfKeyDoesNotExist(),
			    EditorPrefLogOptions.LogOnWriteWhenChanged);
	}

}