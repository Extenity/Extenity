
namespace Extenity.DLLBuilder
{

	public static class Constants
	{
		public const string MenuItemPrefix = "Window/" + DLLBuilderName + "/";

		public const string DLLBuilderName = "Extenity DLL Builder";
		public static readonly string DefaultConfigurationPath = "Assets/ExtenityDLLBuilder.asset";

		public static class RemoteBuilder
		{
			public static readonly float RequestCheckerInterval = 1f;
			public static readonly string RequestFilePath = "Temp/ExtenityDLLBuilder/RemoteRequest.json";
			public static readonly string ResponseFilePath = "Temp/ExtenityDLLBuilder/RemoteResponse-{0}.json";
		}

		public static class BuildJob
		{
			public static readonly string CurrentBuildJobFilePath = "Temp/ExtenityDLLBuilder/CurrentBuildJob.json";
		}
	}

}
