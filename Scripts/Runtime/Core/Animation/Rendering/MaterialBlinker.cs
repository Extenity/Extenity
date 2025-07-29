#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.AnimationToolbox
{

	public class MaterialBlinker : MonoBehaviour
	{
		public bool UseScaledTime = true;
		public float BlinkInterval = 0.15f;
		public Material MaterialA;
		public Material MaterialB;
		public MeshRenderer MeshRenderer;

		private float NextBlinkTime;
		private bool State;

		protected void LateUpdate()
		{
			var now = UseScaledTime
				? Time.time
				: Time.unscaledTime;

			if (NextBlinkTime < now)
			{
				NextBlinkTime += BlinkInterval;

				// Reset timing for huge time gaps.
				if (NextBlinkTime < now)
				{
					NextBlinkTime = now;
				}

				SwitchMaterial();
			}
		}

		public void SwitchMaterial()
		{
			State = !State;

			MeshRenderer.sharedMaterial = State
				? MaterialA
				: MaterialB;
		}
	}

}

#endif
