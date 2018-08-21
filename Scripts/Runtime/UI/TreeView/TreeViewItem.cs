using UnityEngine;

namespace Extenity.UIToolbox
{

	public abstract class TreeViewItem<TData> : MonoBehaviour
	{
		public virtual void OnItemCreated(TreeView<TData>.Node node) { }
		public virtual void OnItemSelected() { }
		public virtual void OnItemDeselected() { }
		public virtual void OnItemExpanded() { }
		public virtual void OnItemCollapsed() { }
		public virtual void OnLeafStateChanged(bool isLeaf) { }
	}

}
