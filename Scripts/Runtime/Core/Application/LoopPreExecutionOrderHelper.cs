#if UNITY_5_3_OR_NEWER

using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	[HideMonoScript]
	public class LoopPreExecutionOrderHelper : MonoBehaviour
	{
		internal LoopHelper LoopHelper;

		private void FixedUpdate()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.PreFixedUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.PreFixedUpdateCallbacks.InvokeUnsafe();
			}
		}

		private void Update()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.PreUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.PreUpdateCallbacks.InvokeUnsafe();
			}
		}

		private void LateUpdate()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.PreLateUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.PreLateUpdateCallbacks.InvokeUnsafe();
			}
		}
	}

}

#endif
