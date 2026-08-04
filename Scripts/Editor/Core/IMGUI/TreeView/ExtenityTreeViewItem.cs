using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

#if UNITY_6000_4_OR_NEWER
	public class ExtenityTreeViewItem<T> : TreeViewItem<EntityId> where T : TreeElement
#else
	public class ExtenityTreeViewItem<T> : TreeViewItem<int> where T : TreeElement
#endif
	{
		public T Data { get; set; }

#if UNITY_6000_4_OR_NEWER
		public ExtenityTreeViewItem(EntityId id, int depth, string displayName, T data) : base(id, depth, displayName)
#else
		public ExtenityTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
#endif
		{
			this.Data = data;
		}
	}

}
