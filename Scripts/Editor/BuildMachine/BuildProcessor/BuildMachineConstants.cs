using Extenity.ApplicationToolbox.Editor;
using Extenity.FileSystemToolbox;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildMachineConstants
	{
		#region Paths

		// Survival file
		public static readonly string BuildMachineSurvivalDirectory = EditorApplicationTools.LibraryDirectory.AppendDirectoryToPath("BuildMachine");
		public static readonly string RunningJobSurvivalFileName = "_RunningJob.json";
		public static readonly string RunningJobSurvivalFilePath = BuildMachineSurvivalDirectory + RunningJobSurvivalFileName;
		public static readonly string RunningJobSurvivalFileLogDirectoryFormat = BuildMachineSurvivalDirectory.AppendDirectoryToPath("{0}");

		// Unity Editor layout
		public static readonly string ConsoleOnlyLayoutFileName = "BuildMachine.wlt";

		#endregion
	}

}
