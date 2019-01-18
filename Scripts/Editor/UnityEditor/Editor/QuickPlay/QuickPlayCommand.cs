// QuickPlay shortcuts are not supported outside of Windows environment.
#if UNITY_EDITOR_WIN

using System;
using System.IO;
using Extenity.ApplicationToolbox.Editor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public enum QuickPlayPostponeType
	{
		NotPostponed,
		WaitingForAssemblyReload,
		WaitingForDelayedCall,
	}

	public abstract class QuickPlayCommand
	{
		#region Configuration

		private const string PostponeMarkFilePrefix = "AssemblyReloadMark_";
		private const string PostponeMarkFileExtension = ".tmp";

		public abstract string Name { get; }
		public abstract string PrettyName { get; }
		public abstract bool IsPostponable { get; }

		#endregion

		#region Process

		protected abstract void DoProcess();

		public void Process()
		{
			// Reset previously set postpone state.
			PostponeType = QuickPlayPostponeType.NotPostponed;

			DoProcess();
		}

		#endregion

		#region Postpone After Assembly Reload

		public QuickPlayPostponeType PostponeType { get; private set; }

		private string PostponeMarkFilePath => Path.Combine(
			EditorApplicationTools.EditorTempDirectoryPath,
			PostponeMarkFilePrefix + Name + PostponeMarkFileExtension
		);

		protected void PostponeAfterAssemblyReload(QuickPlayPostponeType postponeType)
		{
			if (!IsPostponable)
				throw new Exception("Trying to postpone recklessly.");

			PostponeType = postponeType;

			//Log.Info($"Postponing command '{PrettyName}'.");

			var path = PostponeMarkFilePath;
			File.WriteAllText(path, "");
		}

		/// <summary>
		/// Returns true if QuickPlay execution should continue from this command.
		/// </summary>
		public bool CheckAfterAssemblyReload()
		{
			if (!IsPostponable)
				return false;

			var path = PostponeMarkFilePath;
			if (File.Exists(path))
			{
				var fileInfo = new FileInfo(path);
				var age = DateTime.UtcNow.Subtract(fileInfo.LastWriteTimeUtc);
				if (age.TotalMinutes < 1.0)
				{
					File.Delete(path);
					return true;
				}
				else
				{
					//Log.Info($"Ignoring postpone mark file for command '{PrettyName}' because it was outdated by '{age.TotalMinutes:N0}' minutes.");
					File.Delete(path);
				}
			}
			return false;
		}

		#endregion
	}

}

#endif
