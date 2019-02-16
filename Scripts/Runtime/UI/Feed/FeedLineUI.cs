using TMPro;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class FeedLineUI : MonoBehaviour
	{
		public TextMeshProUGUI Text;
		public RectTransform RectTransform;

		public void Set(string content)
		{
			Text.text = content;
		}
	}

}
