using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	[HideMonoScript]
	public class LoopPreExecutionOrderHelper : MonoBehaviour
	{
		private void FixedUpdate()
		{
			Loop.Instance.PreFixedUpdateCallbacks.InvokeSafe();
		}

		private void Update()
		{
			Loop.Instance.PreUpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			Loop.Instance.PreLateUpdateCallbacks.InvokeSafe();
		}
	}

}
