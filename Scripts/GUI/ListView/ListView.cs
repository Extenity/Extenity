using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Extenity.SceneManagement;

namespace Extenity.UserInterface
{

	[RequireComponent(typeof(UIWidgets.ListView))]
	public class ListView : MonoBehaviour
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

		#region Update

		//protected void Update()
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

		public ListViewItemBase TemplateItem;

		#endregion

		#region Items

		private List<ListViewItemBase> _Items = new List<ListViewItemBase>();
		private ReadOnlyCollection<ListViewItemBase> _ItemsAsReadOnly;

		public ReadOnlyCollection<ListViewItemBase> Items
		{
			get
			{
				if (_ItemsAsReadOnly == null) _ItemsAsReadOnly = _Items.AsReadOnly();
				return _ItemsAsReadOnly;
			}
		}

		public bool IsItemExists(object itemData)
		{
			TypeCheck(itemData);

			for (var i = 0; i < _Items.Count; i++)
			{
				if (_Items[i] != null && _Items[i].IsLinkedWithData(itemData))
					return true;
			}
			return false;
		}

		public void AddItem(object itemData, bool checkForDuplicates = false)
		{
			TypeCheck(itemData);

			if (!TemplateItem.IsOkayToCreateItem(itemData))
				return;

			if (checkForDuplicates)
			{
				if (IsItemExists(itemData))
					return;
			}

			var item = GameObjectTools.InstantiateAndGetComponent<ListViewItemBase>(TemplateItem.gameObject,
				TemplateItem.transform.parent, true);
			item.Initialize(itemData);
		}

		public bool RemoveItem(object itemData)
		{
			TypeCheck(itemData);

			for (var i = 0; i < _Items.Count; i++)
			{
				if (_Items[i] != null && _Items[i].IsLinkedWithData(itemData))
				{
					Destroy(_Items[i]);
					_Items.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public void ClearItems()
		{
			while (_Items.Count > 0)
			{
				var i = _Items.Count - 1;
				Destroy(_Items[i]);
				_Items.RemoveAt(i);
			}
		}

		public bool ModifyItem(object oldItemData, object newItemData)
		{
			TypeCheck(oldItemData);
			TypeCheck(newItemData);

			for (var i = 0; i < _Items.Count; i++)
			{
				if (_Items[i] != null && _Items[i].IsLinkedWithData(oldItemData))
				{
					_Items[i].Modify(newItemData);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Item Type

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

		#endregion

		#region Validate

		private void OnValidate()
		{
			if (_UIWidgetsListView == null)
				_UIWidgetsListView = GetComponent<UIWidgets.ListView>();

			if (TemplateItem == null)
				TemplateItem = transform.GetComponentInChildren<ListViewItemBase>();
		}

		#endregion
	}

}
