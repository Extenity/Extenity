using System;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.BuildMachine.Editor
{

	[Serializable]
	public class BuildJob
	{
		#region Initialization

		public static BuildJob Create(BuildJobPhaseDefinition[] buildPhases, params BuildProcessorBase[] buildProcessors)
		{
			if (buildProcessors.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(buildProcessors));

			return new BuildJob
			{
				BuildPhases = buildPhases,
				BuildProcessors = buildProcessors,
			};
		}

		#endregion

		#region Options

		public BuildJobPhaseDefinition[] BuildPhases;

		#endregion

		#region Build Processors

		public BuildProcessorBase[] BuildProcessors;

		#endregion

		#region Start

		public void Start()
		{
			throw new NotImplementedException();
			//EditorCoroutineUtility.StartCoroutineOwnerless(RunProcess());
		}

		#endregion

		#region Serialization

		public string SerializeToJson()
		{
			return JsonUtility.ToJson(this, true);
		}

		public static BuildJob DeserializeFromJson(string json)
		{
			return JsonUtility.FromJson<BuildJob>(json);
		}

		#endregion
	}

}
