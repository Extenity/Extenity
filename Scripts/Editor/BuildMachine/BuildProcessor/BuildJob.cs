using System;
using System.IO;
using System.Linq;
using System.Text;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using UnityEditor;
using Guid = System.Guid;

namespace Extenity.BuildMachine.Editor
{

	public enum BuildJobOverallState
	{
		Unknown,
		JobInitialized,
		JobRunning,
		JobFinished,
	}

	//public enum BuildJobPhaseState
	//{
	//	Unknown,
	//	PhaseRunning,
	//	PhaseFinalizing,
	//	PhaseFinished,
	//}

	public enum BuildJobStepState
	{
		Unknown,
		StepRunning,
		StepHalt,
		StepFinished,
	}

	public enum BuildJobResult
	{
		Incomplete,
		Succeeded,
		Failed,
	}

	/// <summary>
	/// A Build Job is created when user requests a build. It keeps the build options that are
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
	///
	/// After getting done with a <see cref="Builder"/>, a finalization procedure starts. Finalization
	/// is there to provide what 'finally' does for try-catch blocks. Whether the build fails or
	/// succeeds, Finalization Build Steps will be run so the user of Build Machine would revert back
	/// any changes made in the build run.
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

			UnityEditorPath = EditorApplication.applicationPath;

			OverallState = BuildJobOverallState.JobInitialized;
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

		#region Schedule Unity Editor Restart

		[JsonProperty]
		public readonly string UnityEditorPath;

		[JsonProperty]
		public bool IsUnityEditorRestartScheduled;

		public void ScheduleUnityEditorRestart()
		{
			BuilderLog.Info("Scheduling Unity Editor restart.");
			IsUnityEditorRestartScheduled = true;
			throw new NotImplementedException();
		}

		#endregion

		#region Schedule Assembly Reload

		[JsonProperty]
		public bool IsAssemblyReloadScheduled = true; // Every job starts with an assembly reload first.

		public void ScheduleAssemblyReload()
		{
			BuilderLog.Info("Scheduling assembly reload.");
			IsAssemblyReloadScheduled = true;
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

		public BuildJobOverallState OverallState = BuildJobOverallState.Unknown;
		//public BuildJobPhaseState PhaseState = BuildJobPhaseState.Unknown;
		public BuildJobStepState StepState = BuildJobStepState.Unknown;

		/// <summary>
		/// Tells if the job is in finalization mode. In finalization mode, <see cref="BuildJobRunner"/>
		/// executes Finalization Build Steps.
		///
		/// Finalization starts for two reasons. It starts after completing all Build Steps of a
		/// <see cref="Builder"/> or starts if anything goes wrong in the middle of a <see cref="Builder"/> execution.
		/// </summary>
		public bool Finalizing;

		public BuildJobResult Result = BuildJobResult.Incomplete;

		public int CurrentPhase = -1;
		public int CurrentBuilder = -1;
		public string PreviousBuildStep = "";
		public string CurrentBuildStep = "";
		public string PreviousFinalizationStep = "";
		public string CurrentFinalizationStep = "";
		/// <summary>
		/// This is only used for calling the method of current Step. Do not use it elsewhere.
		/// </summary>
		[JsonIgnore]
		public BuildStepInfo _CurrentStepInfoCached = BuildStepInfo.Empty;

		public bool IsLastBuilder => CurrentBuilder >= Builders.Length - 1;
		public bool IsLastPhase => CurrentPhase >= Plan.BuildPhases.Length - 1;
		public bool IsCurrentBuilderAssigned => Builders.IsInRange(CurrentBuilder);

		public bool IsPreviousBuildStepAssigned => !string.IsNullOrEmpty(PreviousBuildStep);
		public bool IsCurrentBuildStepAssigned => !string.IsNullOrEmpty(CurrentBuildStep);
		public bool IsPreviousFinalizationStepAssigned => !string.IsNullOrEmpty(PreviousFinalizationStep);
		public bool IsCurrentFinalizationStepAssigned => !string.IsNullOrEmpty(CurrentFinalizationStep);

		internal void SetResult(BuildJobResult result)
		{
			BuilderLog.Info($"Setting result to '{result}'");
			if (result == BuildJobResult.Succeeded)
			{
				if (Result == BuildJobResult.Failed)
				{
					throw new Exception(BuilderLog.Prefix + $"Tried to set '{BuildJobResult.Succeeded}' result over '{BuildJobResult.Failed}' job.");
				}
			}
			Result = result;
		}

		#endregion

		#region Profiling

		public DateTime StartTime;
		public DateTime LastStepStartTime;
		public DateTime LastHaltTime;

		#endregion

		#region Version Increment

		// public BuildTools.TemporarilyIncrementVersion TemporarilyIncrementVersion;

		#endregion

		#region Start

		public void Start()
		{
			BuildJobRunner.Start(this);
		}

		#endregion

		#region Build Run Initialization

		internal void BuildRunInitialization()
		{
			// Commented out for future needs.
			//TemporarilyIncrementVersion = BuildTools.TemporarilyIncrementVersion.Create(Plan.AddMajorVersion, Plan.AddMinorVersion, Plan.AddBuildVersion);
		}

		#endregion

		#region Build Run Finalization

		internal void BuildRunFinalization()
		{
			switch (Result)
			{
				case BuildJobResult.Succeeded:
					break;
				case BuildJobResult.Failed:
					{
						// TemporarilyIncrementVersion.Revert();
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Result), Result, "");
			}
		}

		#endregion

		#region Quit In Batch Mode

		/// <summary>
		/// Automatically quits Unity Editor after finishing the build job if the Editor is launched in batch mode.
		/// Disable this in a build step or before running the job if Unity Editor should continue to live.
		/// </summary>
		public bool IsSetToQuitInBatchMode = true;

		#endregion

		#region Error Handling

		[JsonIgnore]
		public string ErrorReceivedInLastStep;

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
				BuilderLog.Error("Json-1:\n" + json);
				BuilderLog.Error("Json-2:\n" + json2);
				throw new Exception(BuilderLog.Prefix + "Serialization consistency check failed.");
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
				BuilderLog.Error("Json-1:\n" + json);
				BuilderLog.Error("Json-2:\n" + json2);
				throw new Exception(BuilderLog.Prefix + "Serialization consistency check failed.");
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

		public string ToStringCurrentPhaseBuilderAndStep()
		{
			return $"[Phase: {ToStringCurrentPhase()}, Builder: {ToStringCurrentBuilder()}, Step: {CurrentBuildStep}, StepState: {StepState}]";
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
