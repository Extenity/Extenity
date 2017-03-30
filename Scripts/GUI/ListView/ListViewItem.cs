using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extenity.StringUtilities;
using UnityEngine.UI;

namespace Extenity.UserInterface
{

	public class ListViewItem : ListViewItemBase
	{
		#region Initialization

		protected override void OnItemCreated(object itemData)
		{
			throw new System.NotImplementedException();
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

		#region Data

		private string _Label;

		public string Label
		{
			get { return _Label; }
			set
			{
				_Label = value;
				Text.text = value;
			}
		}

		public string DisplayedLabel
		{
			get { return Text.text; }
			set { Text.text = value; }
		}

		public void SetLabel(string label, bool separateWordsInDisplayedText = true)
		{
			_Label = label;
			Text.text = separateWordsInDisplayedText ? label.SeparateCamelCasedAdjointWords() : label;
		}

		public void SetLabel(string label, string displayedLabel)
		{
			_Label = label;
			Text.text = displayedLabel;
		}

		#endregion

		#region Link With Data

		public bool IsLinkedWithData(string label)
		{
			return _Label.Equals(label);
		}

		public override bool IsLinkedWithData(object itemData)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		#region UI Elements

		public Text Text;

		#endregion

		protected override void OnItemModified(object newItemData)
		{
			throw new NotImplementedException();
		}
	}

}
