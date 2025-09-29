#if UNITY_5_3_OR_NEWER

using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	[HideMonoScript]
	public class LoopPostExecutionOrderHelper : MonoBehaviour
	{
		internal LoopHelper LoopHelper;

		private void FixedUpdate()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.PostFixedUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.PostFixedUpdateCallbacks.InvokeUnsafe();
			}
		}

		private void Update()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.PostUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.PostUpdateCallbacks.InvokeUnsafe();
			}
		}

		private void LateUpdate()
		{
			if (Loop.EnableCatchingExceptionsInUpdateCallbacks)
			{
				LoopHelper.PostLateUpdateCallbacks.InvokeSafe();
			}
			else
			{
				LoopHelper.PostLateUpdateCallbacks.InvokeUnsafe();
			}
		}
	}

}

#endif
