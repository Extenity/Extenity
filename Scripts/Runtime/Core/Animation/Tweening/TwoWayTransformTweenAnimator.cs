#if UNITY_5_3_OR_NEWER

namespace Extenity.AnimationToolbox
{

	public class TwoWayTransformTweenAnimator : TransformTweenAnimatorBase
	{
		public TransformTweenSetup AnimationToB;
		public TransformTweenSetup AnimationToA;

		public void PlayToB()
		{
			Play(AnimationToB);
		}

		public void JumpToB()
		{
			Jump(AnimationToB);
		}

		public void PlayToA()
		{
			Play(AnimationToA);
		}

		public void JumpToA()
		{
			Jump(AnimationToA);
		}
	}

}

#endif
