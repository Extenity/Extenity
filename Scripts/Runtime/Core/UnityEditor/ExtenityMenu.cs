#if UNITY_EDITOR

using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public static class ExtenityMenu
	{
		private const string RefreshHelper = ""; // Appends a postfix to menu entries. Change this regularly when working with priorities to help Unity to refresh menu layout cache.
		private const int Start = -2200;
		private const int Group = 20;
		private const int Subgroup = 10;

		// Unity Editor window top bar Tools menu
		public const string Path = "Tools/Extenity" + RefreshHelper + "/";

		public const string CleanUp = Path + "Clean Up/";
		public const int CleanUpPriority = Start;
		public const int CleanUpPriorityEnd = CleanUpPriority + 23;

		public const string Analysis = Path + "Analysis/";
		public const int AnalysisPriority = CleanUpPriorityEnd + Group;

		public const string Application = Path + "Application/";
		public const int ApplicationPriority = AnalysisPriority + Group;
		public const string GameObjectOperations = Path + "GameObject Operations/";
		public const int GameObjectOperationsPriority = ApplicationPriority + Subgroup;
		public const int GameObjectOperationsPriorityEnd = GameObjectOperationsPriority + 82;
		public const string Logging = Path + "Logging/";
		public const int LoggingPriority = GameObjectOperationsPriorityEnd + Subgroup; // TODO: Something causes the Logging entry to be listed as a new group, rather than listed right after GameObject Operations.
		public const string UI = Path + "UI/";
		public const int UIPriority = LoggingPriority + Subgroup;
		public const string Terrain = Path + "Terrain/";
		public const int TerrainPriority = UIPriority + Subgroup;

		public const string Painkiller = Path + "Painkiller/";
		public const int PainkillerPriority = TerrainPriority + Group;

		public const string PackageManagerTools = Path + "Package Manager Tools/";
		public const int PackageManagerToolsPriority = PainkillerPriority + Group;
		public const string System = Path + "System/";
		public const int SystemPriority = PackageManagerToolsPriority + Subgroup;
		public const int SystemPriorityEnd = SystemPriority + 23;

		public const string Update = Path + "";
		public const int UpdatePriority = SystemPriorityEnd + Group;
		public const string Examples = Path + "Examples/";
		public const int ExamplesPriority = UpdatePriority + Group;
		public const string Maintenance = Path + "Maintenance Internals/";
		public const int MaintenancePriority = ExamplesPriority + Subgroup;

		// Right-click Asset context menu
		public const string AssetsBaseContext = "Assets/";
		public const string CreateAssetBaseContext = AssetsBaseContext + "Create/";
		public const int UnityCreateCSScriptMenuPriority = 81;
		public const string AssetsContext = AssetsBaseContext + "Extenity" + RefreshHelper + "/";
		public const string AssetOperationsContext = AssetsContext + "Asset Operations/";

		// Right-click Game Object context menu
		public const string UIContext = "GameObject/UI/";
		public const string WidgetsContext = UIContext + "Extenity Widgets" + RefreshHelper + "/";

		// Right-click Component context menu
		public const string ComponentContext = "CONTEXT/Component/";

		// Edit menu
		public const int UnitySnapSettingsMenuPriority = 1000; // Use this to add a MenuItem just below Unity's "Edit/Grid and Snap Settings"
		public const int UnityPlayMenuPriority = 159; // Use this to add a MenuItem just above Unity's "Edit/Play"
		public const string Edit = "Edit/";

		// Prioritize menu parents
		[MenuItem(WidgetsContext, priority = 1000)] // 1000 is above all Unity default entries.
		private static void __WidgetsContext(MenuCommand menuCommand) { }
	}

}

#endif
