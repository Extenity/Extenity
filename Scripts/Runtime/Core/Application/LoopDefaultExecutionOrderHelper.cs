using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	[HideMonoScript]
	public class LoopDefaultExecutionOrderHelper : MonoBehaviour
	{
		private void FixedUpdate()
		{
			Loop.Instance.FixedUpdateCallbacks.InvokeSafe();
		}

		private void Update()
		{
			Loop.Instance.UpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			Loop.Instance.LateUpdateCallbacks.InvokeSafe();
		}
	}

}
