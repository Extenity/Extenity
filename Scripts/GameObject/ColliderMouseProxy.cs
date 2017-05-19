using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class ColliderMouseProxy : MonoBehaviour
	{
		public GameObject Target;

		private void OnMouseDown() { if (Target != null) Target.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseDrag() { if (Target != null) Target.SendMessage("OnMouseDrag", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseEnter() { if (Target != null) Target.SendMessage("OnMouseEnter", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseExit() { if (Target != null) Target.SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseOver() { if (Target != null) Target.SendMessage("OnMouseOver", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseUp() { if (Target != null) Target.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver); }
		private void OnMouseUpAsButton() { if (Target != null) Target.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver); }
	}

}
