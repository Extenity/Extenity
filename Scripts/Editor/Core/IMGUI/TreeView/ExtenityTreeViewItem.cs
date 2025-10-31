using UnityEditor.IMGUI.Controls;

namespace Extenity.IMGUIToolbox.Editor
{

	public class ExtenityTreeViewItem<T> : TreeViewItem<int> where T : TreeElement
	{
		public T Data { get; set; }

		public ExtenityTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
		{
			this.Data = data;
		}
	}

}
