/* Not needed as Unity now can save lighting configuration in assets. Keep it for future needs. See 119283231.
using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public struct LightingConfiguration : IConsistencyChecker
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
*/
