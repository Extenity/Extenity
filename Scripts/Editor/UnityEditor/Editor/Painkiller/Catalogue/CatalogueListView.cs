using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Extenity.PainkillerToolbox.Editor
{

	public abstract class CatalogueListView<TElement> : TreeViewWithTreeModel<TElement>
		where TElement : TreeElement
	{
		#region Configuration

		protected const float RowHeights = 20f;
		protected const float ToggleWidth = 18f;

		#endregion

		#region Initialization

		public CatalogueListView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<TElement> model)
			: base(state, multiColumnHeader, model)
		{
			rowHeight = RowHeights;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
		}

		#endregion

		#region Drag and Drop

		protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
		{
			// Drag&Drop should not allow dropping an entry as a child of another entry. Then the sorting fails miserably.
			if (args.dragAndDropPosition == DragAndDropPosition.UponItem)
				return DragAndDropVisualMode.Rejected;
			if (args.parentItem.depth != -1)
				return DragAndDropVisualMode.Rejected;
			return base.HandleDragAndDrop(args);
		}

		#endregion
	}

}
