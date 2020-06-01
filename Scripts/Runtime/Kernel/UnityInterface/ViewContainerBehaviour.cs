using System;
using System.Collections.Generic;
using System.Collections.Generic.Extenity;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
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

		#region Instantiate Item

		protected abstract TItemView InstantiateItem(TItem item);

		#endregion

		#region Destroy Item

		protected virtual void DestroyItem(TItemView item)
		{
			GameObjectTools.Destroy(item.gameObject);
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
			// Item: A Kernel object, derived from KernelObject.
			// ItemView: Interface representation of a kernel object, derived from ViewBehaviour.

			var items = GetList();

			Views.EqualizeTo<TItemView, TItem>(
				items.List,
				ItemViewComparer.Default,
				item =>
				{
					try // Prevent any possible exceptions to break list equalization. Also allows logging the erroneous item's ID in catch block.
					{
						var itemView = InstantiateItem(item);

						// Connect the view object to kernel object.
						itemView.DataLink.ID = item.ID;
						itemView.RefreshDataLink(itemView.enabled); // TODO IMMEDIATE: Use itemView.IsEnabled

						Views.Add(itemView);
					}
					catch (Exception exception)
					{
						Log.Exception(exception);
						Log.Error($"Failed to instantiate the view of the object '{item.ID}'. See previous error for more details.");
					}
				},
				(itemView, iItemView) =>
				{
					try // Prevent any possible exceptions to break list equalization. Also allows logging the erroneous item's ID in catch block.
					{
						Views.RemoveAt(iItemView);
						
						if (itemView) // Do not allow a null object to leak into DestroyItem calls.
						{
							DestroyItem(itemView);
						}
					}
					catch (Exception exception)
					{
						Log.Exception(exception);
						Log.Error($"Failed to destroy the view of the object '{itemView.ID}'. See previous error for more details.");
					}
				}
			);
		}

		#endregion
	}

}
