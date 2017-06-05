using System;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	[Serializable]
	public class DistributerConfiguration
	{
		[Serializable]
		public class DistributionTarget
		{
			public bool Enabled = true;
			public string Path;
		}

		[SerializeField]
		public DistributionTarget[] Targets;
	}

}
