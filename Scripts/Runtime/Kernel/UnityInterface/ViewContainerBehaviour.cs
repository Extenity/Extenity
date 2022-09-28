#if ExtenityKernel

using System;
using System.Collections.Generic;
using System.Collections.Generic.Extenity;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox.UnityInterface
{

	public abstract class ViewContainerBehaviour<TItem, TItemView, TKernel> : ViewBehaviour<SyncList<TItem, TKernel>, TKernel>
		where TItemView : ViewBehaviour<TItem, TKernel>
		where TItem : KernelObject<TKernel>, new()
		where TKernel : KernelBase<TKernel>
	{
		#region Views

		// [NonSerialized] Nope! Let Unity serialize the created views to survive assembly refreshes.
		[ShowInInspector, ReadOnly]
		public List<TItemView> Views;

		#endregion

		#region Instantiate Item

		protected abstract TItemView InstantiateItem([NotNull] TItem item);

		#endregion

		#region Destroy Item

		protected virtual void DestroyItem([NotNull] TItemView item)
		{
			GameObjectTools.Destroy(item.gameObject);
		}

		#endregion

		#region Sync List

		private class ItemViewComparer : IEqualityComparer<TItemView, TItem>
		{
			public static readonly ItemViewComparer Default = new ItemViewComparer();

			public bool Equals(TItemView x, TItem y)
			{
				return x.ID == y.ID;
			}
		}

		/// <summary>
		/// Called when an item is added to or removed from the container. Also called whenever the container object
		/// is invalidated. The system just assumes there might be a solid reason that required the container to be
		/// marked as invalidated and pass that invalidation info to the implementation of this callback respectively,
		/// even when the container is not actually modified. See 115459835.
		/// </summary>
		protected virtual void OnContainerModified(bool isReallyModified) { }

		protected sealed override void OnDataInvalidated(SyncList<TItem, TKernel> items)
		{
			// Item: A Kernel object, derived from KernelObject.
			// ItemView: Interface representation of a kernel object, derived from ViewBehaviour.

			var isAnythingChanged = Views.EqualizeTo<TItemView, TItem>(
				items.List,
				ItemViewComparer.Default,
				item =>
				{
					try // Prevent any possible exceptions to break list equalization. Also allows logging the erroneous item's ID in catch block.
					{
						Debug.Assert(item != null);
						Debug.Assert(item.IsAlive);

						var itemView = InstantiateItem(item);

						// Connect the view object to kernel object.
						itemView.DataLink.ID = new Ref<TItem, TKernel>(item.ID);
						// itemView.RefreshDataLink(); This is going to be called inside Start of ViewBehaviour. No need to call it here.

						Views.Add(itemView);
					}
					catch (Exception exception)
					{
						Log.Exception(exception);
						Log.Error($"Failed to instantiate the view of the object '{item.ToTypeAndIDStringSafe()}'. See previous error for more details.");
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
						Log.Error($"Failed to destroy the view of the object '{(itemView != null ? itemView.Object.ToTypeAndIDStringSafe() : "[NA]")}'. See previous error for more details.");
					}
				}
			);

			// Call container modification callback. See 115459835.
			try
			{
				OnContainerModified(isAnythingChanged);
			}
			catch (Exception exception)
			{
				Log.Exception(exception);
			}
		}

		#endregion
	}

}

#endif
