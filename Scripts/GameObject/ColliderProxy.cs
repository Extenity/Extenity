using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class ColliderProxy : MonoBehaviour
	{
		public GameObject target;

		void OnColliderEnter(Collision collision) { if (target != null) target.SendMessage("OnColliderEnter", collision, SendMessageOptions.DontRequireReceiver); }
		void OnColliderExit(Collision collision) { if (target != null) target.SendMessage("OnCollisionExit", collision, SendMessageOptions.DontRequireReceiver); }
		void OnColliderStay(Collision collision) { if (target != null) target.SendMessage("OnCollisionStay", collision, SendMessageOptions.DontRequireReceiver); }
	}

}
