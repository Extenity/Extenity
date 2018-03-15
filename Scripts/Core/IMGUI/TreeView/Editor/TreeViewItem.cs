using UnityEditor.IMGUI.Controls;

namespace Extenity.IMGUIToolbox.Editor
{

	public class TreeViewItem<T> : TreeViewItem where T : TreeElement
	{
		public T Data { get; set; }

		public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
		{
			this.Data = data;
		}
	}

}
