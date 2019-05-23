using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

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
	/// A Build Job can work in phases (See <see cref="BuildPhaseInfo"/>). A Build Phase contains info
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

		public static BuildJob Create(BuildPlan plan)
		{
			return new BuildJob(plan);
		}

		private BuildJob(BuildPlan plan)
		{
			if (plan == null)
				throw new ArgumentNullException(nameof(plan));

			var builders = CreateBuilderInstancesMimickingBuilderOptions(plan.BuilderOptionsList);

			Plan = plan;
			Builders = builders;
		}

		private static Builder[] CreateBuilderInstancesMimickingBuilderOptions(BuilderOptions[] builderOptionsList)
		{
			var builders = new Builder[builderOptionsList.Length];
			for (var i = 0; i < builderOptionsList.Length; i++)
			{
				var builderOptions = builderOptionsList[i];

				// Find the related Builder via its BuilderOptions
				var builderOptionsType = builderOptions.GetType();
				var builderInfo = BuilderManager.BuilderInfos.Single(entry => entry.OptionsType == builderOptionsType);

				// Create Builder instance and assign its Options
				var builder = (Builder)Activator.CreateInstance(builderInfo.Type);
				builder.GetType().GetField(nameof(Builder<BuilderOptions>.Options)).SetValue(builder, builderOptions); // Unfortunately Options field is defined in the Builder<> generic class and not the Builder class.

				builders[i] = builder;
			}
			return builders;
		}

		#endregion

		#region Plan

		public readonly BuildPlan Plan;

		#endregion

		#region Builders

		public readonly Builder[] Builders;

		#endregion

		#region State

		public int CurrentPhase = -1;
		public int CurrentBuilder = -1;
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
			var config = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
			};
			var stringBuilder = new StringBuilder();
			using (var stringWriter = new StringWriter(stringBuilder))
			{
				using (var jsonTextWriter = new JsonTextWriter(stringWriter)
				{
					Formatting = Formatting.Indented,
					Indentation = 1,
					IndentChar = '\t',
				})
				{
					(JsonSerializer.CreateDefault(config)).Serialize(jsonTextWriter, this);
				}
			}
			return stringBuilder.ToString();
			//return JsonConvert.SerializeObject(this, config); Unfortunately there is no way to specify IndentChar when using this single-liner.

			//return JsonUtility.ToJson(this, true); Unfortunately Unity's Json implementation does not support inheritance.
		}

		public static BuildJob DeserializeFromJson(string json)
		{
			return JsonConvert.DeserializeObject<BuildJob>(json);

			//return JsonUtility.FromJson<BuildJob>(json); Unfortunately Unity's Json implementation does not support inheritance.
		}

		#endregion
	}

}
