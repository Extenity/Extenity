using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class PageIndicator : MonoBehaviour
	{
		public Image InnerIcon;
		public Image SelectionIcon;

		public void SetAsSelected()
		{
			SelectionIcon.enabled = true;
		}

		public void SetAsDeselected()
		{
			SelectionIcon.enabled = false;
		}
	}

}
