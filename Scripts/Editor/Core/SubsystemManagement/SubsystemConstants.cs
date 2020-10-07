using Extenity.ApplicationToolbox;
using Extenity.FileSystemToolbox;

namespace Extenity.SubsystemManagementToolbox
{

	public static class SubsystemConstants
	{
		internal const string ConfigurationFileNameWithoutExtension = "SubsystemSettings";
		internal const string ConfigurationFileName = ConfigurationFileNameWithoutExtension + ".asset";
		internal static readonly string ConfigurationDefaultFilePath = ApplicationTools.UnityProjectPaths.TopLevelResourcesRelativePath.AppendFileToPath(ConfigurationFileName);

		internal const string Version = "1";
	}

}
