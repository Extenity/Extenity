#if UNITY

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
			LoopHelper.FixedUpdateCallbacks.InvokeSafe();
		}

		private void Update()
		{
			LoopHelper.UpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			LoopHelper.LateUpdateCallbacks.InvokeSafe();
		}
	}

}

#endif
