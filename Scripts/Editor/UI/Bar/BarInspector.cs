using Extenity.UnityEditorToolbox;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	public static class BarInspector
	{
		#region Hierarchy Right Click Menu

		[MenuItem(ExtenityMenu.WidgetsContext + "Bar")]
		private static void AddToScene(MenuCommand menuCommand)
		{
			UIEditorUtilities.InstantiateUIWidgetFromPrefab("Extenity/Widgets/Bar", menuCommand);
		}

		#endregion
	}

}
