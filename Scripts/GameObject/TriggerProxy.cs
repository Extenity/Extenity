using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class TriggerProxy : MonoBehaviour
	{
		public GameObject target;

		void OnTriggerEnter(Collider other) { if (target != null) target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver); }
		void OnTriggerExit(Collider other) { if (target != null) target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver); }
		void OnTriggerStay(Collider other) { if (target != null) target.SendMessage("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver); }
	}

}
