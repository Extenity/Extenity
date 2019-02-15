// QuickPlay shortcuts are not supported outside of Windows environment.
#if UNITY_EDITOR_WIN

using Extenity.DebugToolbox.Editor;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class QuickPlayCommand_ClearConsole : QuickPlayCommand
	{
		#region Configuration

		public override string Name => "ClearConsole";
		public override string PrettyName => "Clear Console";
		public override bool IsPostponable => false;

		#endregion

		#region Process

		protected override void DoProcess()
		{
			EditorDebugTools.ClearDeveloperConsole();
		}

		#endregion
	}

	public class QuickPlayCommand_RefreshAssetDatabase : QuickPlayCommand
	{
		#region Configuration

		public override string Name => "RefreshAssetDatabase";
		public override string PrettyName => "Refresh Asset Database";
		public override bool IsPostponable => false;

		#endregion

		#region Process

		protected override void DoProcess()
		{
			AssetDatabase.Refresh();
		}

		#endregion
	}

	public class QuickPlayCommand_StopPlaying : QuickPlayCommand
	{
		#region Configuration

		public override string Name => "StopPlaying";
		public override string PrettyName => "Stop Playing";
		public override bool IsPostponable => true;

		#endregion

		#region Process

		protected override void DoProcess()
		{
			if (EditorApplication.isPlaying)
			{
				PostponeAfterAssemblyReload(QuickPlayPostponeType.WaitingForDelayedCall);
				EditorApplication.isPlaying = false;
			}
		}

		#endregion
	}

	public class QuickPlayCommand_StartPlaying : QuickPlayCommand
	{
		#region Configuration

		public override string Name => "StartPlaying";
		public override string PrettyName => "Start Playing";
		public override bool IsPostponable => true;

		#endregion

		#region Process

		protected override void DoProcess()
		{
			if (!EditorApplication.isPlaying)
			{
				PostponeAfterAssemblyReload(QuickPlayPostponeType.WaitingForAssemblyReload);
				EditorApplication.isPlaying = true;
			}
		}

		#endregion
	}

}

#endif
