using DG.Tweening;
using Sirenix.OdinInspector;

namespace Extenity.AnimationToolbox
{

	public class TwoWayTransformTweenAnimator : TransformTweenAnimatorBase
	{
		[TitleGroup("Animation - To B"), InlineProperty, HideLabel]
		public TransformTweenSetup AnimationToB;
		[TitleGroup("Animation - To A"), InlineProperty, HideLabel]
		public TransformTweenSetup AnimationToA;

		[HorizontalGroup("Playback/Buttons/ToB"), Button(ButtonSizes.Large, Name = "Play to B", DrawResult = false)]
		public Sequence PlayToB()
		{
			return Play(AnimationToB);
		}

		[HorizontalGroup("Playback/Buttons/ToB", Width = 40), Button(ButtonSizes.Large, Name = "Jump")]
		public void JumpToB()
		{
			Jump(AnimationToB);
		}

		[HorizontalGroup("Playback/Buttons/ToA"), Button(ButtonSizes.Large, Name = "Play to A", DrawResult = false)]
		public Sequence PlayToA()
		{
			return Play(AnimationToA);
		}

		[HorizontalGroup("Playback/Buttons/ToA", Width = 40), Button(ButtonSizes.Large, Name = "Jump")]
		public void JumpToA()
		{
			Jump(AnimationToA);
		}
	}

}
