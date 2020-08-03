#if UNITY_EDITOR

using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public static class ExtenityMenu
	{
		// Editor window top bar menu
		public const string Path = "Tools/Extenity/";
		public const string Application = Path + "Application/";
		public const string System = Path + "System/";
		public const string Painkiller = Path + "Painkiller/";
		public const string GameObjectOperations = Path + "GameObject Operations/";
		public const string PackageManagerTools = Path + "Package Manager Tools/";
		public const string Maintenance = Path + "Maintenance internals/";
		public const string Logging = Path + "Logging/";
		public const string Terrain = Path + "Terrain/";
		public const string Analysis = Path + "Analysis/";
		public const string UI = Path + "UI/";
		public const string CleanUp = Path + "CleanUp/";

		// Right-click asset context menu
		public const string UIContext = "GameObject/UI/";
		public const string AssetsBaseContext = "Assets/";
		public const string AssetsContext = AssetsBaseContext + "Extenity/";
		public const string WidgetsContext = UIContext + "Extenity Widgets/";
		public const string AssetOperationsContext = AssetsContext + "Asset Operations/";

		// Right-click component context menu
		public const string ComponentContext = "CONTEXT/Component/";

		// Edit menu
		public const int UnitySnapSettingsMenuPriority = 1000; // Use this to add a MenuItem just below Unity's "Edit/Grid and Snap Settings"
		public const string Edit = "Edit/";

		// Prioritize menu parents
#if UNITY_EDITOR
		[MenuItem(WidgetsContext, priority = 1000)] // 1000 is above all Unity default entries.
		private static void _WidgetsContext(MenuCommand menuCommand) { }

		[MenuItem(GameObjectOperations, priority = 0)]
		private static void _GameObjectOperations() { }
#endif
	}

}

#endif
