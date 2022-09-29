#if UNITY

using UnityEngine;

namespace Extenity.ParallelToolbox
{

	public static class Yields
	{
		public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
		public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
	}

}

#endif
