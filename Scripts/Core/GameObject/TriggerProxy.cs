using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class TriggerProxy : MonoBehaviour
	{
		public GameObject Target;

		private void OnTriggerEnter(Collider other) { if (Target != null) Target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver); }
		private void OnTriggerExit(Collider other) { if (Target != null) Target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver); }
		private void OnTriggerStay(Collider other) { if (Target != null) Target.SendMessage("OnTriggerStay", other, SendMessageOptions.DontRequireReceiver); }
	}

}
