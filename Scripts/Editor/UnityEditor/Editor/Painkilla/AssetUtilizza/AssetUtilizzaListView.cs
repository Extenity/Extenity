using Extenity.IMGUIToolbox.Editor;
using UnityEditor.IMGUI.Controls;

namespace Extenity.PainkillaTool.Editor
{

	public abstract class AssetUtilizzaListView<TElement> : TreeViewWithTreeModel<TElement>
		where TElement : TreeElement
	{
		#region Configuration

		protected const float RowHeights = 20f;
		protected const float ToggleWidth = 18f;

		#endregion

		#region Initialization

		public AssetUtilizzaListView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<TElement> model)
			: base(state, multiColumnHeader, model)
		{
			rowHeight = RowHeights;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
		}

		#endregion
	}

}
