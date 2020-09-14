using DG.Tweening;
using Sirenix.OdinInspector;

namespace Extenity.AnimationToolbox
{

	public class TransformTweenAnimator : TransformTweenAnimatorBase
	{
		[TitleGroup("Animation"), InlineProperty, HideLabel]
		public TransformTweenSetup Animation;

		[HorizontalGroup("Playback/Buttons"), Button(ButtonSizes.Large, DrawResult = false)]
		public Sequence Play()
		{
			return Play(Animation);
		}

		[HorizontalGroup("Playback/Buttons", Width = 40), Button(ButtonSizes.Large)]
		public void Jump()
		{
			Jump(Animation);
		}
	}

}
