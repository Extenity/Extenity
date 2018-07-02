using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.UnityEditorToolbox
{

	public class BatchObjectProcessor : MonoBehaviour
	{
		[Serializable]
		public class ObjectReference
		{
			public GameObject Object;
			public bool IncludeChildren;
		}

		[Serializable]
		public class Entry
		{
			[FormerlySerializedAs("Configuration")]
			public string AppliedJobName;
			public ObjectReference[] Objects;
		}

		[Serializable]
		public class JobDefinition
		{
			[FormerlySerializedAs("Name")]
			public string JobName;
			/// <summary>
			/// Tags can be used for making the job launch only if the process is started with one of the tags specified here.
			/// Specifying no tags means job will be run for all processes. Specifying one or more tags means the process should
			/// be started with at least one of the tags specified here so that the job will be run as part of that process.
			/// </summary>
			public string[] JobTags;

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

			[Header("Deparent")]
			public bool DeparentAll = false;
		}

		#region Data

		[FormerlySerializedAs("Configurations")]
		public JobDefinition[] JobDefinitions;
		public Entry[] Entries;

		public List<JobDefinition> GetJobDefinitions(string jobName, string[] requiredTags)
		{
			var skippedSomething = false;

			var result = JobDefinitions.Where(definition =>
			{
				if (definition.JobName == jobName)
				{
					if (definition.JobTags.IsNotNullAndEmpty())
					{
						if (requiredTags.IsNullOrEmpty())
							return false;
						foreach (var requiredTag in requiredTags)
						{
							if (definition.JobTags.Contains(requiredTag))
								return true;
						}
						Debug.Log($"Skipping job '{jobName}' because it does not have "+ 
						          (requiredTags.Length > 1 
							          ? $"any of the tags '{string.Join(", ", requiredTags)}'." 
							          : $"the tag '{requiredTags[0]}'."
						          )
						);
						skippedSomething = true;
					}
					else
					{
						return true;
					}
				}
				return false;
			}).ToList();

			if (!skippedSomething && result.Count == 0) // Need to check if we have skipped a definition. So that we know the job actually exists but the tags does not match, which is okay.
				throw new Exception($"Batch object job definition '{jobName}' does not exist.");
			return result;
		}

		#endregion
	}

}
