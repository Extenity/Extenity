using System.Collections.Generic;
using System.Collections.Generic.Extenity;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;

namespace Extenity.Kernel.UnityInterface
{

	public abstract class ViewContainerBehaviour<TItem, TItemView> : ViewBehaviour
		where TItemView : ViewBehaviour
		where TItem : KernelObject
	{
		#region Views

		// [NonSerialized] Nope! Let Unity serialize the created views to survive assembly refreshes.
		[ShowInInspector, ReadOnly]
		public List<TItemView> Views;

		#endregion

		#region Sync List

		protected abstract TItemView InstantiateItem(TItem item);
		protected abstract void DestroyItem(TItemView item); // TODO IMMEDIATE: Make this optional and use GameObjectTools.Destroy(item.gameObject)

		protected abstract SyncList<TItem> GetList();

		private class ItemViewComparer : IEqualityComparer<TItemView, TItem>
		{
			public static readonly ItemViewComparer Default = new ItemViewComparer();

			public bool Equals(TItemView x, TItem y)
			{
				return x.ID == y.ID;
			}
		}

		protected sealed override void OnDataInvalidated()
		{
			var list = GetList();

			Views.EqualizeTo<TItemView, TItem>(
				list.List,
				ItemViewComparer.Default,
				item =>
				{
					var itemView = InstantiateItem(item);
					
					// Connect the view object to kernel object.
					itemView.DataLink.ID = item.ID;
					itemView.RefreshDataLink();
					
					Views.Add(itemView);
				},
				(itemView, i) =>
				{
					DestroyItem(itemView);
					Views.RemoveAt(i);
				}
			);
		}

		#endregion
	}

}
