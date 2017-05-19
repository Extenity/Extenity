using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class ColliderMouseProxy : MonoBehaviour
	{
		public GameObject target;

		private void OnMouseDown() { if (target != null) target.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseDrag() { if (target != null) target.SendMessage("OnMouseDrag", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseEnter() { if (target != null) target.SendMessage("OnMouseEnter", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseExit() { if (target != null) target.SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseOver() { if (target != null) target.SendMessage("OnMouseOver", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseUp() { if (target != null) target.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseUpAsButton() { if (target != null) target.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver); }
	}

}
