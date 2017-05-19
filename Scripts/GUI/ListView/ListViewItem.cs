using UnityEngine.UI;
using Extenity.DataToolbox;

namespace Extenity.UserInterface
{

	public class ListViewItem : ListViewItemBase<string, string>
	{
		#region Initialization

		protected override void OnItemCreated(string itemData)
		{
			Modify(itemData);
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

		public override void Modify(string newItemData)
		{
			SetLabel(newItemData);
		}

		#endregion

		#region UI Elements

		public Text Text;

		#endregion
	}

}
