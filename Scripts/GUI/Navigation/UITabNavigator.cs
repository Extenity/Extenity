using UnityEngine;

namespace Extenity.UIToolbox
{

	public class UITabNavigator : MonoBehaviour
	{
		private static int LastProcessedFrame = -1;

		protected void Update()
		{
			var currentFrame = Time.frameCount;
			if (LastProcessedFrame == currentFrame)
				return; // Ignore processing tab presses multiple times in the same frame if there are more than one active UITabNavigators in the scene.
			LastProcessedFrame = currentFrame;

			if (Input.GetKeyDown(KeyCode.Tab))
			{
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					UINavigationTools.NavigateToNextSelectableOnUp();
				}
				else
				{
					UINavigationTools.NavigateToNextSelectableOnDown();
				}
			}
		}
	}

}
