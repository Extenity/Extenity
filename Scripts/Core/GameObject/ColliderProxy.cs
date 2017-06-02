using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class ColliderProxy : MonoBehaviour
	{
		public GameObject Target;

		private void OnColliderEnter(Collision collision) { if (Target != null) Target.SendMessage("OnColliderEnter", collision, SendMessageOptions.DontRequireReceiver); }
		private void OnColliderExit(Collision collision) { if (Target != null) Target.SendMessage("OnCollisionExit", collision, SendMessageOptions.DontRequireReceiver); }
		private void OnColliderStay(Collision collision) { if (Target != null) Target.SendMessage("OnCollisionStay", collision, SendMessageOptions.DontRequireReceiver); }
	}

}
