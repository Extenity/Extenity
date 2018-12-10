using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public class BatchObjectProcessor : MonoBehaviour, IConsistencyChecker
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		public enum PrefabUnpackingType
		{
			No,
			Unpack,
			UnpackCompletely,
		}

		[Serializable]
		public class ObjectReference
		{
			// TODO: Also support ability to specify game object path in scene, instead of game object reference. See 715971261282.
			//public ReferenceType Type; GameObjectReference, GameObjectPathInScene
			//public string PathInScene;

			public GameObject Object;
			public bool IncludeChildren;

			public string FullPathOfObject
			{
				get
				{
					//switch (Type) See 715971261282.
					//{
					//	case ReferenceType.GameObjectReference: return Object ? Object.FullName() : "[NULL]";
					//	case ReferenceType.GameObjectReference: return PathInScene;
					//}
					return Object ? Object.FullName() : "[NULL]";
				}
			}
		}

		[Serializable]
		public class Job : IConsistencyChecker
		{
			public string Name;

			[Tooltip(
				"Tags can be used for selectively running or skipping jobs in a batch process. Specifying no required tags means the job " +
				"will be run in any process, whether the process started with tags or not. Specifying one or more required tags means " +
				"the process should be started with at least one of the tags specified here, so that the job will be run as part of that " +
				"process.")]
			public string[] RequiredTags;

			public string AppliedInstructionName;

			public ObjectReference[] Objects;

			#region Tags

			public bool ShouldBeIncludedInProcess(string[] processTags)
			{
				// No RequiredTags means the job should always be included in the process.
				if (RequiredTags.IsNullOrEmpty())
					return true;

				// Process should be started with one of the tags in RequiredTags list. See if that's the case.
				if (processTags != null)
				{
					foreach (var processTag in processTags)
					{
						if (RequiredTags.Contains(processTag))
						{
							return true;
						}
					}
				}

				return false;
			}

			#endregion

			#region Consistency

			public class DuplicateNameChecker : IEqualityComparer<Job>
			{
				public static readonly DuplicateNameChecker Instance = new DuplicateNameChecker();

				public bool Equals(Job x, Job y)
				{
					if (x == null || y == null || string.IsNullOrEmpty(x.Name))
						return false;
					return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
				}

				public int GetHashCode(Job obj)
				{
					return obj.Name?.GetHashCode() ?? 0;
				}
			}

			public void CheckConsistency(ref List<ConsistencyError> errors)
			{
				if (string.IsNullOrEmpty(Name))
				{
					errors.Add(new ConsistencyError(this, $"Empty name in {nameof(Job)} entry."));
				}
				if (string.IsNullOrEmpty(AppliedInstructionName))
				{
					errors.Add(new ConsistencyError(this, $"Empty {nameof(AppliedInstructionName)} in {nameof(Job)} '{Name}' entry."));
				}
			}

			#endregion
		}

		[Serializable]
		public class Instruction : IConsistencyChecker
		{
			public string Name;

			[Header("Change Layers")]
			public bool ChangeLayers = false;
			public SingleLayer Layer;

			[Header("Change Tags")]
			public bool ChangeTags = false;
			public string Tag;

			[Header("Change Static")]
			public bool ChangeStatic = false;
			[EnumMask]
			public StaticFlags StaticFlags;

			[Header("Change Navigation")]
			public bool ChangeNavMeshArea = false;
			public int AreaIndex = -1;

			[Header("Unpack Prefab")]
			public PrefabUnpackingType UnpackPrefab = PrefabUnpackingType.No;

			[Header("Deparent")]
			public bool Detach = false;
			public bool DetachChildrenRecursive = false;
			public bool WorldPositionStaysWhenDeparenting = true;

			#region Consistency

			public class DuplicateNameChecker : IEqualityComparer<Instruction>
			{
				public static readonly DuplicateNameChecker Instance = new DuplicateNameChecker();

				public bool Equals(Instruction x, Instruction y)
				{
					if (x == null || y == null || string.IsNullOrEmpty(x.Name))
						return false;
					return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
				}

				public int GetHashCode(Instruction obj)
				{
					return obj.Name?.GetHashCode() ?? 0;
				}
			}

			public void CheckConsistency(ref List<ConsistencyError> errors)
			{
				if (string.IsNullOrEmpty(Name))
				{
					errors.Add(new ConsistencyError(this, $"Empty name in {nameof(Instruction)} entry."));
				}
			}

			#endregion
		}

		#region Data

		public Instruction[] InstructionDefinitions;

		public Job[] Jobs;

		public Instruction GetInstruction(string instructionName)
		{
			return InstructionDefinitions?.FirstOrDefault(instruction => instruction.Name.Equals(instructionName, StringComparison.Ordinal));
		}

		public Job GetJob(string jobName)
		{
			return Jobs?.FirstOrDefault(job => job.Name.Equals(jobName, StringComparison.Ordinal));
		}

		#endregion

		#region Consistency

#if UNITY_EDITOR
		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			Log.RegisterPrefix(this, "BatchObjectProcessor");
			this.CheckConsistencyAndLog($"{nameof(BatchObjectProcessor)} has some inconsistencies.", this);
		}
#endif

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			if (InstructionDefinitions != null)
			{
				if (InstructionDefinitions.Duplicates(Instruction.DuplicateNameChecker.Instance).Any())
					errors.Add(new ConsistencyError(this, $"There are duplicate {nameof(Instruction)} names."));

				foreach (var instruction in InstructionDefinitions)
				{
					instruction.CheckConsistency(ref errors);
				}
			}
			if (Jobs != null)
			{
				if (Jobs.Duplicates(Job.DuplicateNameChecker.Instance).Any())
					errors.Add(new ConsistencyError(this, $"There are duplicate {nameof(Job)} names."));

				foreach (var job in Jobs)
				{
					job.CheckConsistency(ref errors);

					if (!string.IsNullOrEmpty(job.AppliedInstructionName) &&
						GetInstruction(job.AppliedInstructionName) == null)
					{
						errors.Add(new ConsistencyError(this, $"Job '{job.Name}' points to an unknown {nameof(Instruction)} named '{job.AppliedInstructionName}'."));
					}
				}
			}
		}

		#endregion
	}

}
