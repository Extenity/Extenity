using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.UnityEditorToolbox
{

	[Serializable]
	public struct LightingBuildConfiguration : IConsistencyChecker
	{
		public bool RealtimeGlobalIlluminationEnabled;
		public bool BakedGlobalIlluminationEnabled;
		public int DirectSamples;
		public int IndirectSamples;
		public int Bounces;
		public bool CompressLightmaps;
		public bool AmbientOcclusion;

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
		}

		#endregion
	}

}
