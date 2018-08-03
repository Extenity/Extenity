using UnityEngine;

namespace Extenity.UIToolbox
{

	public static class UIFaderTools
	{
		public static float FadeInAllChildren(this GameObject gameObject)
		{
			return gameObject.transform.FadeInAllChildren();
		}

		public static float FadeInAllChildren(this Transform transform)
		{
			var faders = transform.GetComponentsInChildren<UIFader>();
			float maxDuration = 0f;

			for (int i = 0; i < faders.Length; i++)
			{
				var duration = faders[i].FadeIn();
				if (maxDuration < duration)
					maxDuration = duration;
			}
			return maxDuration;
		}

		public static float FadeOutAllChildren(this GameObject gameObject)
		{
			return gameObject.transform.FadeOutAllChildren();
		}

		public static float FadeOutAllChildren(this Transform transform)
		{
			var faders = transform.GetComponentsInChildren<UIFader>();
			float maxDuration = 0f;

			for (int i = 0; i < faders.Length; i++)
			{
				var duration = faders[i].FadeOut();
				if (maxDuration < duration)
					maxDuration = duration;
			}
			return maxDuration;
		}
	}

}
