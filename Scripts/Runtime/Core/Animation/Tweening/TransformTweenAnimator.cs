#if UNITY_5_3_OR_NEWER

namespace Extenity.AnimationToolbox
{

	public class TransformTweenAnimator : TransformTweenAnimatorBase
	{
		public TransformTweenSetup Animation;

		public void Play()
		{
			Play(Animation);
		}

		public void Jump()
		{
			Jump(Animation);
		}
	}

}

#endif
