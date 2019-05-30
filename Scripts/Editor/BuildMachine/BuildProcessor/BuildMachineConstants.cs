
namespace Extenity.BuildMachine.Editor
{

	public static class BuildMachineConstants
	{
		#region Paths

		// Survival file
		public static readonly string BuildMachineSurvivalDirectory = "Library/BuildMachine/";
		public static readonly string RunningJobSurvivalFileName = "_RunningJob.json";
		public static readonly string RunningJobSurvivalFilePath = BuildMachineSurvivalDirectory + RunningJobSurvivalFileName;
		public static readonly string RunningJobSurvivalFileLogDirectory = "Library/BuildMachine/{0}/";

		// Unity Editor layout
		public static readonly string LayoutPath = "Assets/Plugins/Extenity/Scripts/Editor/BuildMachine/Layout/BuildMachine.wlt";

		#endregion
	}

}
