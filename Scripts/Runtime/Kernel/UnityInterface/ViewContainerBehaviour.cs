using System.Collections.Generic;
using System.Collections.Generic.Extenity;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

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

		#region Instantiate Item

		protected abstract TItemView InstantiateItem(TItem item);

		#endregion

		#region Destroy Item

		protected virtual void DestroyItem(TItemView item)
		{
			if (item)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) // Use DestroyImmediate in edit mode.
				{
					DestroyImmediate(item.gameObject);
				}
#endif
				Destroy(item.gameObject);
			}
		}

		#endregion

		#region Sync List

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
					itemView.RefreshDataLink(itemView.enabled); // TODO IMMEDIATE: Use itemView.IsEnabled

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
