using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class DestroyTimer : MonoBehaviour
	{
		public float Timer = 0f;
		public bool DestroyImmediately = false;
		public bool RemoveFromParentOnAwake = false;
		public bool RemoveFromParentOnStart = false;

		private void Awake()
		{
			if (RemoveFromParentOnAwake)
				transform.SetParent(null);
		}

		private void Start()
		{
			if (RemoveFromParentOnStart)
				transform.SetParent(null);

			Invoke("DestroyOnTimer", Timer);
		}

		private void DestroyOnTimer()
		{
			if (DestroyImmediately)
				DestroyImmediate(gameObject);
			else
				Destroy(gameObject);
		}

		public void SetDestroyTimer(float value)
		{
			Timer = value;

			if (IsInvoking("DestroyOnTimer"))
				CancelInvoke("DestroyOnTimer");
			Invoke("DestroyOnTimer", Timer);
		}
	}

}
