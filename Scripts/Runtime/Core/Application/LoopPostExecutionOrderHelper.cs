using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	[HideMonoScript]
	public class LoopPostExecutionOrderHelper : MonoBehaviour
	{
		private void FixedUpdate()
		{
			Loop.Instance.PostFixedUpdateCallbacks.InvokeSafe();
		}

		private void Update()
		{
			Loop.Instance.PostUpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			Loop.Instance.PostLateUpdateCallbacks.InvokeSafe();
		}
	}

}
