using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extenity.BuildToolbox.Editor;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using Guid = System.Guid;

namespace Extenity.BuildMachine.Editor
{

	public enum BuildJobState
	{
		Unknown,
		JobInitialized,

		// Step
		StepRunning,
		StepHalt,
		StepFinalization,
		StepFailed,
		StepSucceeded,

		// Build job
		JobFinalization,
		JobFailed,
		JobSucceeded,
	}

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
	[JsonObject]
	public class BuildJob
	{
		#region Initialization

		public static BuildJob Create(BuildPlan plan)
		{
			return new BuildJob(plan);
		}

		private BuildJob()
		{
			// Nothing to do here. This empty constructor allows json deserialization.
		}

		private BuildJob(BuildPlan plan)
		{
			if (plan == null)
				throw new ArgumentNullException(nameof(plan));

			var builders = CreateBuilderInstancesMimickingBuilderOptions(plan.BuilderOptionsList);

			ID = Guid.NewGuid().ToString();
			CreatedDate = DateTime.Now;
			Plan = plan;
			Builders = builders;

			CurrentState = BuildJobState.JobInitialized;
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

		#region Metadata

		[JsonProperty]
		public readonly string ID;

		[JsonProperty]
		public readonly DateTime CreatedDate;

		public string Name
		{
			get
			{
				if (Plan == null)
					return "[NA-Plan]";
				return Plan.Name;
			}
		}

		#endregion

		#region Plan

		[JsonProperty]
		public readonly BuildPlan Plan;

		#endregion

		#region Builders

		[JsonProperty]
		public readonly Builder[] Builders;

		#endregion

		#region State

		public BuildJobState CurrentState = BuildJobState.Unknown;
		public int CurrentPhase = -1;
		public int CurrentBuilder = -1;
		public string PreviousStep = "";
		public string CurrentStep = "";
		/// <summary>
		/// This is only used for calling the method of current step. Do not use it elsewhere.
		/// </summary>
		[JsonIgnore]
		public BuildStepInfo _CurrentStepCached = BuildStepInfo.Empty;

		public bool IsLastBuilder => CurrentBuilder >= Builders.Length - 1;
		public bool IsLastPhase => CurrentPhase >= Plan.BuildPhases.Length - 1;
		public bool IsCurrentBuilderAssigned => Builders.IsInRange(CurrentBuilder);
		public bool IsPreviousStepAssigned => !string.IsNullOrEmpty(PreviousStep);
		public bool IsCurrentStepAssigned => !string.IsNullOrEmpty(CurrentStep);

		#endregion

		#region Profiling

		public DateTime StartTime;
		public DateTime LastStepStartTime;
		public DateTime LastHaltTime;

		#endregion

		#region Version Increment

		public BuildTools.TemporarilyIncrementVersion TemporarilyIncrementVersion;

		#endregion

		#region Start

		public void Start()
		{
			BuildJobRunner.Start(this);
		}

		#endregion

		#region Delayed Assembly-Reloading Operations

		internal List<Action> DelayedAssemblyReloadingOperations;

		public void DelayAssemblyReloadingOperation(Action action)
		{
			if (DelayedAssemblyReloadingOperations == null)
				DelayedAssemblyReloadingOperations = new List<Action>();

			DelayedAssemblyReloadingOperations.Add(action);
		}

		#endregion

		#region Build Run Initialization

		internal void BuildRunInitialization()
		{
			TemporarilyIncrementVersion = BuildTools.TemporarilyIncrementVersion.Create(Plan.AddMajorVersion, Plan.AddMinorVersion, Plan.AddBuildVersion);
		}

		#endregion

		#region Build Run Finalization

		internal void BuildRunFinalization(bool succeeded)
		{
			if (!succeeded)
			{
				TemporarilyIncrementVersion.Revert();
			}
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Creates json string. While doing that, also test if the string deserializes correctly.
		/// </summary>
		public string SerializeToJson()
		{
			var json = InternalSerializeToJson(this);

			// Consistency check
			var deserialized = InternalDeserializeFromJson(json);
			var json2 = InternalSerializeToJson(deserialized);
			if (json != json2)
			{
				Log.Error("Json-1:\n" + json);
				Log.Error("Json-2:\n" + json2);
				throw new Exception("Serialization consistency check failed.");
			}

			return json;
		}

		private static string InternalSerializeToJson(BuildJob job)
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
					(JsonSerializer.CreateDefault(config)).Serialize(jsonTextWriter, job);
				}
			}
			return stringBuilder.ToString();
			//return JsonConvert.SerializeObject(job, config); Unfortunately there is no way to specify IndentChar when using this single-liner.

			//return JsonUtility.ToJson(job, true); Unfortunately Unity's Json implementation does not support inheritance.
		}

		public static BuildJob DeserializeFromJson(string json)
		{
			var job = InternalDeserializeFromJson(json);

			// Consistency check
			var json2 = InternalSerializeToJson(job);
			if (json != json2)
			{
				Log.Error("Json-1:\n" + json);
				Log.Error("Json-2:\n" + json2);
				throw new Exception("Serialization consistency check failed.");
			}

			return job;
		}

		private static BuildJob InternalDeserializeFromJson(string json)
		{
			var config = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto,
			};
			return JsonConvert.DeserializeObject<BuildJob>(json, config);

			//return JsonUtility.FromJson<BuildJob>(json); Unfortunately Unity's Json implementation does not support inheritance.
		}

		#endregion

		#region ToString

		public string ToStringCurrentPhase()
		{
			if (Plan == null)
			{
				return CurrentPhase + "-[NullPlan]";
			}
			if (Plan.BuildPhases.IsNullOrEmpty())
			{
				return CurrentPhase + "-[NullPhases]";
			}
			if (!Plan.BuildPhases.IsInRange(CurrentPhase))
			{
				return CurrentPhase + "-[OutOfRange]";
			}
			return CurrentPhase + "-" + Plan.BuildPhases[CurrentPhase].Name;
		}

		public string ToStringCurrentBuilder()
		{
			if (Builders.IsNullOrEmpty())
			{
				return CurrentBuilder + "-[NullBuilders]";
			}
			if (!Builders.IsInRange(CurrentBuilder))
			{
				return CurrentBuilder + "-[OutOfRange]";
			}
			if (Builders[CurrentBuilder] == null)
			{
				return CurrentBuilder + "-[NullBuilder]";
			}
			return CurrentBuilder + "-" + Builders[CurrentBuilder].Info.Name;
		}

		#endregion
	}

	public static class BuildJobTools
	{
		#region Metadata

		public static string NameSafe(this BuildJob job)
		{
			if (job == null)
				return "[NA]";
			return job.Name;
		}

		#endregion
	}

}
