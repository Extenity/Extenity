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
		public const string Maintenance = Path + "Maintenance internals/";
		public const string Logging = Path + "Logging/";
		public const string Terrain = Path + "Terrain/";
		public const string Analysis = Path + "Analysis/";
		public const string UI = Path + "UI/";
		public const string CleanUp = Path + "CleanUp/";

		// Right-click asset context menu
		public const string UIContext = "GameObject/UI/";
		public const string AssetsContext = "Assets/Extenity/";
		public const string WidgetsContext = UIContext + "Extenity Widgets/";
		public const string AssetOperationsContext = AssetsContext + "Asset Operations/";

		// Right-click component context menu
		public const string ComponentContext = "CONTEXT/Component/";


		// Prioritize menu parents
#if UNITY_EDITOR
		[MenuItem(WidgetsContext, priority = 1000)] // 1000 is above all Unity default entries.
		private static void Dummy(MenuCommand menuCommand) { }
#endif
	}

}

#endif
