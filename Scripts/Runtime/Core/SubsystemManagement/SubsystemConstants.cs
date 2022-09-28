#if UNITY
#if ExtenitySubsystems

using Extenity.ApplicationToolbox;
using Extenity.FileSystemToolbox;

namespace Extenity.SubsystemManagementToolbox
{

	public static class SubsystemConstants
	{
		public const string ConfigurationFileNameWithoutExtension = "SubsystemSettings";
		public const string ConfigurationFileName = ConfigurationFileNameWithoutExtension + ".asset";
		public static readonly string ConfigurationDefaultFilePath = ApplicationTools.UnityProjectPaths.TopLevelResourcesRelativePath.AppendFileToPath(ConfigurationFileName);

		internal const string Version = "1";
	}

}

#endif
#endif
