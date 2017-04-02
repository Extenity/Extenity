using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extenity.UserInterface
{

	[RequireComponent(typeof(UIWidgets.ListViewItem))]
	public abstract class ListViewItemBase<TItemID, TItemData> : MonoBehaviour
	{
		#region Initialization

		protected abstract void OnItemCreated(TItemData itemData);

		public void Initialize(TItemID itemID, TItemData itemData)
		{
			ID = itemID;
			OnItemCreated(itemData);
		}

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Metadata

		public TItemID ID { get; private set; }

		#endregion

		#region Item Create Verification

		internal virtual bool IsOkayToCreateItem(TItemData itemData)
		{
			return true;
		}

		#endregion

		#region UIWidgets ListViewItem

		[SerializeField]
		private UIWidgets.ListViewItem _UIWidgetsListViewItem;

		public UIWidgets.ListViewItem UIWidgetsListViewItem
		{
			get { return _UIWidgetsListViewItem; }
		}

		#endregion

		#region Link With Data

		/*
		/// <summary>
		/// Tells if this list view item is created using the data specified with 'itemData'.
		/// 
		/// For example, let's say you have a unique ID that represents the data. You may want to keep the 
		/// ID value also in derived ListViewItemBase class. Then this method compares the both IDs and
		/// returns true if IDs match.
		/// </summary>
		public abstract bool IsLinkedWithData(object itemData);
		*/

		#endregion

		#region Modify Data

		/// <summary>
		/// Allows a generalized data modify operation. There also could be other modification methods defined in derives list view item class.
		/// </summary>
		/// <param name="newItemData"></param>
		public abstract void Modify(TItemData newItemData);

		#endregion

		#region Validate

		private void OnValidate()
		{
			if (_UIWidgetsListViewItem == null)
				_UIWidgetsListViewItem = GetComponent<UIWidgets.ListViewItem>();
		}

		#endregion
	}

}
