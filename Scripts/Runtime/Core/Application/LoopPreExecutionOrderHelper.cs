#if UNITY

using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	[HideMonoScript]
	public class LoopPreExecutionOrderHelper : MonoBehaviour
	{
		internal LoopHelper LoopHelper;

		private void FixedUpdate()
		{
			LoopHelper.PreFixedUpdateCallbacks.InvokeSafe();
		}

		private void Update()
		{
			LoopHelper.PreUpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			LoopHelper.PreLateUpdateCallbacks.InvokeSafe();
		}
	}

}

#endif
