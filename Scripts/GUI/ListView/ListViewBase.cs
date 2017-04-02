using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Extenity.SceneManagement;

namespace Extenity.UserInterface
{

	[RequireComponent(typeof(UIWidgets.ListView))]
	public class ListViewBase<TItem, TItemID, TItemData> : MonoBehaviour where TItem : ListViewItemBase
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region UIWidgets ListView

		[SerializeField]
		private UIWidgets.ListView _UIWidgetsListView;

		public UIWidgets.ListView UIWidgetsListView
		{
			get { return _UIWidgetsListView; }
		}

		#endregion

		#region Template Item

		public TItem TemplateItem;

		#endregion

		#region Items

		private List<KeyValuePair<TItemID, TItem>> _Items = new List<KeyValuePair<TItemID, TItem>>();
		private ReadOnlyCollection<KeyValuePair<TItemID, TItem>> _ItemsAsReadOnly;

		public ReadOnlyCollection<KeyValuePair<TItemID, TItem>> Items
		{
			get
			{
				if (_ItemsAsReadOnly == null) _ItemsAsReadOnly = _Items.AsReadOnly();
				return _ItemsAsReadOnly;
			}
		}

		public bool IsItemExists(TItemID itemID)
		{
			//TypeCheck(itemData);

			for (var i = 0; i < _Items.Count; i++)
			{
				if (_Items[i].Key.Equals(itemID) && _Items[i].Value != null)
					return true;
			}
			return false;
		}

		public TItem GetItem(TItemID itemID)
		{
			for (var i = 0; i < _Items.Count; i++)
			{
				if (_Items[i].Key.Equals(itemID) && _Items[i].Value != null)
					return _Items[i].Value;
			}
			return default(TItem);
		}

		public void AddItem(TItemID itemID, TItemData itemData, bool checkForDuplicates = false)
		{
			//TypeCheck(itemData);

			if (!TemplateItem.IsOkayToCreateItem(itemData))
				return;

			if (checkForDuplicates)
			{
				if (IsItemExists(itemID))
					return;
			}

			var item = GameObjectTools.InstantiateAndGetComponent<TItem>(TemplateItem.gameObject, TemplateItem.transform.parent, true);
			item.Initialize(itemData);
		}

		public bool RemoveItem(TItemID itemID)
		{
			//TypeCheck(itemData);

			var found = false;
			for (var i = 0; i < _Items.Count; i++)
			{
				if (_Items[i].Key.Equals(itemID))
				{
					if (_Items[i].Value != null)
					{
						Destroy(_Items[i].Value);
					}
					_Items.RemoveAt(i);
					found = true;
				}
			}
			return found;
		}

		public void ClearItems()
		{
			while (_Items.Count > 0)
			{
				var i = _Items.Count - 1;
				if (_Items[i].Value != null)
				{
					Destroy(_Items[i].Value);
				}
				_Items.RemoveAt(i);
			}
		}

		public bool ModifyItem(TItemID itemID, TItemData newItemData)
		{
			//TypeCheck(oldItemData);
			//TypeCheck(newItemData);

			var item = GetItem(itemID);
			if (item !=null)
			{
				item.Modify(newItemData);
				return true;
			}
			return false;
		}

		#endregion

		#region Item Type

		/*
		private Type _ItemType;

		public Type ItemType
		{
			get { return _ItemType; }
		}

		private void TypeCheck(object itemData)
		{
			if (itemData == null)
				return; // Do nothing.

			if (_ItemType == null)
			{
				// Initial type checking means we will interpret the type of itemData as the type of this list view. 
				// This type then will be checked against new items brought to the further operations.
				_ItemType = itemData.GetType();
			}
			else
			{
				if (_ItemType != itemData.GetType())
					throw new ArrayTypeMismatchException(string.Format("Tried to use item of type '{0}' in a list of type '{1}'.",
						itemData.GetType().FullName, _ItemType.FullName));
			}
		}
		*/

		#endregion

		#region Validate

		private void OnValidate()
		{
			if (_UIWidgetsListView == null)
				_UIWidgetsListView = GetComponent<UIWidgets.ListView>();

			if (TemplateItem == null)
				TemplateItem = transform.GetComponentInChildren<TItem>();
		}

		#endregion
	}

}
