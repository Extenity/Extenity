#if UNITY_5_3_OR_NEWER

using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	[HideMonoScript]
	public class LoopDefaultExecutionOrderHelper : MonoBehaviour
	{
		internal LoopHelper LoopHelper;

		private void FixedUpdate()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.FixedUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.FixedUpdateCallbacks.InvokeUnsafe();
			}
		}

		private void Update()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.UpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.UpdateCallbacks.InvokeUnsafe();
			}
		}

		private void LateUpdate()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.LateUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.LateUpdateCallbacks.InvokeUnsafe();
			}
		}
	}

}

#endif
