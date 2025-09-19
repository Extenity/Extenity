using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class UIFadeOnClick : MonoBehaviour, IPointerClickHandler
	{
		public UIFader Fader;
		public FadeState ActionOnClick;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!Fader)
				return;

			switch (ActionOnClick)
			{
				case FadeState.Untouched:
					break;
				case FadeState.FadedIn:
					Fader.FadeIn();
					break;
				case FadeState.FadedOut:
					Fader.FadeOut();
					break;
			}
		}
	}

}
