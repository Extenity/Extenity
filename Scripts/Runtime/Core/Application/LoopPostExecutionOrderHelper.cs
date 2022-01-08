#if UNITY

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
			LoopHelper.PostFixedUpdateCallbacks.InvokeSafe();
		}

		private void Update()
		{
			LoopHelper.PostUpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			LoopHelper.PostLateUpdateCallbacks.InvokeSafe();
		}
	}

}

#endif
