using UnityEngine;

namespace Extenity.AnimationToolbox
{

	public class SmoothFollow : MonoBehaviour
	{
		public Transform Target;
		public Transform Follower;
		public float MovementSmoothingFactor = 10f;

		protected void FixedUpdate()
		{
			var followerPosition = Follower.position;
			Follower.position = followerPosition + (Target.position - followerPosition) * (MovementSmoothingFactor * Time.deltaTime);
		}
	}

}
