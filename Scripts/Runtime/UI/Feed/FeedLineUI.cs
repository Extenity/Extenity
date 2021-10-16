using System;
using TMPro;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class FeedLineUI : MonoBehaviour
	{
		public TextMeshProUGUI Text;
		public RectTransform RectTransform;

		[NonSerialized]
		public Vector2 TargetPosition;
	}

}
