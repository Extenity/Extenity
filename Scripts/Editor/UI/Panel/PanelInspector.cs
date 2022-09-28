#if ExtenityScreenManagement

using Extenity.UnityEditorToolbox;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	public static class PanelInspector
	{
		#region Hierarchy Right Click Menu

		[MenuItem(ExtenityMenu.WidgetsContext + "Panel" + ExtenityMenu.WidgetsContextPostfix, priority = 2062)] // Just below the Unity's Panel
		private static void AddToScene(MenuCommand menuCommand)
		{
			UIEditorUtilities.InstantiateUIWidgetFromPrefab("Extenity/Widgets/Panel", menuCommand);
		}

		#endregion
	}

}

#endif
