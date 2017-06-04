
namespace Extenity.DLLBuilder
{

	public static class Constants
	{
		public const string MenuItemPrefix = "Window/" + DLLBuilderName + "/";

		public const string DLLBuilderName = "Extenity DLL Builder";
		public static readonly string DefaultConfigurationPath = "Assets/ExtenityDLLBuilder.asset";

		public static readonly string OutputProjectPath = "Extenity.Output/";
		public static readonly string OutputProjectExtenityDirectory = OutputProjectPath + "Assets/Plugins/Extenity/";
		public static readonly string OutputProjectDLLDirectory = OutputProjectPath + "Assets/Plugins/Extenity/Libraries/";

		public static readonly string ExtenityDirectory = "Assets/Extenity/";
		public static readonly string[] CopiedPackageDirectories =
		{
			"Animations",
			//"Examples", We can't include examples yet. First we need to convert all script references in Examples to point to the objects in DLLs. Maybe we can use DLLSwitcher for that.
			"Models",
			"Shaders",
		};
	}

}
