using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class TriggerProxy : MonoBehaviour
	{
		public GameObject Target;

		void OnTriggerEnter(Collider other) { if (Target != null) Target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver); }
		void OnTriggerExit(Collider other) { if (Target != null) Target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver); }
		void OnTriggerStay(Collider other) { if (Target != null) Target.SendMessage("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver); }
	}

}
