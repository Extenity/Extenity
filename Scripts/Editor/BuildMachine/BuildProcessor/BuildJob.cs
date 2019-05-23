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
	/// build run. This feature is made possible by specifying multiple <see cref="Builder"/>
	/// configurations.
	///
	/// A Build Job can work in phases (See <see cref="BuildPhase"/>). A Build Phase contains info
	/// about which Build Steps are run (See  <see cref="BuildStepAttribute"/>).
	///
	/// Understanding the flow of the whole build run is essential. For each Build Phase, all
	/// Builders are run with the Build Steps defined in this Build Phase. This allows designing
	/// multi-platform builds that first outputs the binaries and then deploys them. Designing
	/// such builds allows failing the whole multi-platform builds if a single platform fails.
	/// </summary>
	[Serializable]
	public class BuildJob
	{
		#region Initialization

		public static BuildJob Create(BuildPhase[] buildPhases, params Builder[] builders)
		{
			if (builders.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(builders));

			return new BuildJob
			{
				BuildPhases = buildPhases,
				Builders = builders,
			};
		}

		#endregion

		#region Options

		public BuildPhase[] BuildPhases;

		#endregion

		#region Builders

		public Builder[] Builders;

		#endregion

		#region State

		public int CurrentPhase = -1;
		public int CurrentProcessor = -1;
		public string CurrentStep = "";

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
