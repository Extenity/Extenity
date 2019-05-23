using System;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.BuildMachine.Editor
{

	/// <summary>
	/// A build job is created when user requests a build. It keeps the build options that are
	/// specified by the user and also keeps track of whole build progress.
	///
	/// Build Job can be serialized so the data can survive assembly reloads, recompilations and
	/// editor restarts.
	/// 
	/// A Build Job can handle multiple platform builds in one go. Better yet, multiple builds
	/// on the same platform is also supported, if needed. Failing in one platform, fails the whole
	/// build run. This feature is made possible by specifying multiple <see cref="BuildProcessor"/>
	/// configurations.
	///
	/// A Build Job can work in phases (See <see cref="BuildJobPhaseDefinition"/>). A Build Phase
	/// contains info about which Build Steps are run (See  <see cref="BuildStepAttribute"/>).
	///
	/// Understanding the flow of the whole build run is essential. For each Build Phase, all
	/// Build Processors are run with the Build Steps defined in this Build Phase. This allows
	/// designing multi-platform builds that first outputs the binaries and then deploys them.
	/// Designing such builds allows failing the whole multi-platform builds if a single platform
	/// fails.
	/// </summary>
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

		#region State

		public int CurrentPhase = -1;
		public int CurrentProcessor = -1;

		#endregion

		#region Start

		public void Start()
		{
			BuildJobRunner.Start(this);
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
