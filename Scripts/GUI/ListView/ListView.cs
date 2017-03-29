using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

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
		public UIWidgets.ListView UIWidgetsListView { get { return _UIWidgetsListView; } }

		#endregion

		#region Validate

		private void OnValidate()
		{
			if (_UIWidgetsListView == null)
				_UIWidgetsListView = GetComponent<UIWidgets.ListView>();
		}

		#endregion

		public void AddItem(object itemData)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveItem(object itemData)
		{
			throw new System.NotImplementedException();
		}

		public void ModifyItem(object oldItemData, object newItemData)
		{
			throw new System.NotImplementedException();
		}
	}

}
