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

			var builder = CreateBuilderInstanceMimickingBuilderOptions(plan.BuilderOptions);

			ID = Guid.NewGuid().ToString();
			CreatedDate = DateTime.Now;
			Plan = plan;
			Builder = builder;

			OverallState = BuildJobOverallState.JobInitialized;
		}

		private static Builder CreateBuilderInstanceMimickingBuilderOptions(BuilderOptions builderOptions)
		{
			// Find the related Builder via its BuilderOptions
			var builderOptionsType = builderOptions.GetType();
			var builderInfo = BuilderManager.BuilderInfos.Single(entry => entry.OptionsType == builderOptionsType);

			// Create Builder instance and assign its Options
			var builder = (Builder)Activator.CreateInstance(builderInfo.Type);
			builder.GetType().GetField(nameof(Builder<BuilderOptions>.Options)).SetValue(builder, builderOptions); // Unfortunately Options field is defined in the Builder<> generic class and not the Builder class.

			return builder;
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

		#region Schedule Assembly Reload

		[JsonProperty]
		public bool IsAssemblyReloadScheduled = true; // Every job starts with an assembly reload first.

		public void ScheduleAssemblyReload()
		{
			Log.Info("Scheduling assembly reload.");
			IsAssemblyReloadScheduled = true;
		}

		#endregion

		#region Plan

		[JsonProperty]
		public readonly BuildPlan Plan;

		#endregion

		#region Builders

		[JsonProperty]
		public readonly Builder Builder;

		#endregion

		#region State

		public BuildJobOverallState OverallState = BuildJobOverallState.Unknown;
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
		public string PreviousBuildStep = "";
		public string CurrentBuildStep = "";
		public string PreviousFinalizationStep = "";
		public string CurrentFinalizationStep = "";
		/// <summary>
		/// This is only used for calling the method of current Step. Do not use it elsewhere.
		/// </summary>
		[JsonIgnore]
		public BuildStepInfo _CurrentStepInfoCached = BuildStepInfo.Empty;

		public bool IsLastPhase => CurrentPhase >= Plan.BuildPhases.Length - 1;

		public bool IsPreviousBuildStepAssigned => !string.IsNullOrEmpty(PreviousBuildStep);
		public bool IsCurrentBuildStepAssigned => !string.IsNullOrEmpty(CurrentBuildStep);
		public bool IsPreviousFinalizationStepAssigned => !string.IsNullOrEmpty(PreviousFinalizationStep);
		public bool IsCurrentFinalizationStepAssigned => !string.IsNullOrEmpty(CurrentFinalizationStep);

		internal void SetResult(BuildJobResult result)
		{
			Log.Info($"Setting result to '{result}'");
			if (result == BuildJobResult.Succeeded)
			{
				if (Result == BuildJobResult.Failed)
				{
					throw new BuildMachineException($"Tried to set '{BuildJobResult.Succeeded}' result over '{BuildJobResult.Failed}' job.");
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

		#region Start

		public void Start()
		{
			BuildJobRunner.Start(this);
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
				Log.Error("Json-1:\n" + json);
				Log.Error("Json-2:\n" + json2);
				throw new BuildMachineException("Serialization consistency check failed.");
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
				throw new BuildMachineException("Serialization consistency check failed.");
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

		public string ToStringBuilderName()
		{
			return Builder.Info.Name;
		}

		public string ToStringCurrentPhaseBuilderAndStep()
		{
			return $"[Phase: {ToStringCurrentPhase()}, Builder: {ToStringBuilderName()}, Step: {CurrentBuildStep}, StepState: {StepState}]";
		}

		#endregion

		#region Log

		private static readonly Logger Log = new("Builder");

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
