using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extenity.UserInterface
{

	[RequireComponent(typeof(UIWidgets.ListViewItem))]
	public abstract class ListViewItemBase : MonoBehaviour
	{
		#region Initialization

		internal virtual bool IsOkayToCreateItem(object itemData) { return true; }

		protected abstract void OnItemCreated(object itemData);

		public void Initialize(object itemData)
		{
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

		protected abstract void OnItemModified(object newItemData);

		public void Modify(object newItemData)
		{
			OnItemModified(newItemData);
		}

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
