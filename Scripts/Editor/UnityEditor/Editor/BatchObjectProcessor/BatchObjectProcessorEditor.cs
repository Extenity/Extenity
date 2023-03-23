using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class BatchObjectProcessorEditor
	{
		#region Process

		/// <param name="processTags">If an entry requires a tag to be processed, the tag must be specified in this list.</param>
		/// <returns>Changed object count.</returns>
		public static int ProcessAll(this BatchObjectProcessor processor, string[] processTags)
		{
			var count = 0;
			for (var iJob = 0; iJob < processor.Jobs.Length; iJob++)
			{
				var job = processor.Jobs[iJob];
				if (!job.ShouldBeIncludedInProcess(processTags))
				{
					Log.InfoWithContext(processor,
					                    $"Skipping job '{job.Name}' because process does not have " +
					                    (job.RequiredTags.Length > 1
						                    ? $"any of the tags '{string.Join(", ", job.RequiredTags)}'."
						                    : $"the tag '{job.RequiredTags[0]}'."
					                    ));
					continue;
				}

				var instruction = processor.GetInstruction(job.AppliedInstructionName);
				if (instruction == null)
					throw new Exception($"Batch object instruction definition '{job.AppliedInstructionName}' does not exist.");

				for (var i = 0; i < job.Objects.Length; i++)
				{
					count += processor.ProcessReferencedObject(job, i, instruction);
				}
			}
			return count;
		}

		/// <returns>Changed object count.</returns>
		private static int ProcessReferencedObject(this BatchObjectProcessor processor, BatchObjectProcessor.Job job, int objectIndex, BatchObjectProcessor.Instruction instruction)
		{
			var reference = job.Objects[objectIndex];
			if (!reference.Object)
			{
				Log.ErrorWithContext(processor, $"Batch object processor encountered to a null reference in job '{job.Name}' object list at index '{objectIndex}'.");
				return 0;
			}

			var count = 0;
			var appliedModificationCount = 0;
			var appliedModifications = new List<string>();
			if (ProcessObject(reference.Object, instruction, ref appliedModificationCount, ref appliedModifications))
				count++;
			if (reference.IncludeChildren)
			{
				reference.Object.transform.ForeachChildren(child =>
				{
					if (ProcessObject(child.gameObject, instruction, ref appliedModificationCount, ref appliedModifications))
						count++;
				}, true);
			}

			Log.InfoWithContext(processor,
			                    $"Job '{job.Name}' applied on object "                                                       +
			                    $"{(reference.IncludeChildren ? "(and its children)" : "")} '{reference.FullPathOfObject}' " +
			                    $"which applied '{appliedModificationCount}' modification(s) over '{count}' object(s)."      +
			                    (appliedModifications.Count == 0 ? "" : " Details:" + Environment.NewLine + string.Join(Environment.NewLine, appliedModifications)));

			return count;
		}

		/// <returns>True if anything changed in object.</returns>
		private static bool ProcessObject(GameObject go, BatchObjectProcessor.Instruction instruction, ref int appliedModificationCount, ref List<string> appliedModifications)
		{
			var changed = false;

			// Change State
			{
				switch (instruction.GameObjectState)
				{
					case BatchObjectProcessor.GameObjectState.Unchanged:
						break;
					case BatchObjectProcessor.GameObjectState.Enable:
						{
							if (!go.activeSelf)
							{
								go.SetActive(true);
								changed = true;
							}
						}
						break;
					case BatchObjectProcessor.GameObjectState.Disable:
						{
							if (go.activeSelf)
							{
								go.SetActive(false);
								changed = true;
							}
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			// Change Static
			if (instruction.ChangeStatic)
			{
				var flags = GameObjectUtility.GetStaticEditorFlags(go);
				var staticEditorFlags = (StaticEditorFlags)instruction.StaticFlags;
				if (flags != staticEditorFlags)
				{
					_MarkChange(1, $"Static flags set to '{staticEditorFlags}' for object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
					GameObjectUtility.SetStaticEditorFlags(go, staticEditorFlags);
				}
			}

			// Change Layers
			if (instruction.ChangeLayers)
			{
				if (go.layer != instruction.Layer.LayerIndex)
				{
					_MarkChange(1, $"Layer set to '{instruction.Layer.Name}' for object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
					go.layer = instruction.Layer.LayerIndex;
				}
			}

			// Change Tags
			if (instruction.ChangeTags)
			{
				if (!go.CompareTag(instruction.Tag))
				{
					_MarkChange(1, $"Tag set to '{instruction.Tag}' for object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
					go.tag = instruction.Tag;
				}
			}

			// Change NavMesh Area
			if (instruction.ChangeNavMeshArea)
			{
				var areaIndex = GameObjectUtility.GetNavMeshArea(go);
				if (areaIndex != instruction.AreaIndex)
				{
					_MarkChange(1, $"NavMesh Area set to '{instruction.AreaIndex}' for object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
					GameObjectUtility.SetNavMeshArea(go, instruction.AreaIndex);
				}
			}

			// Unpack Prefab
			if (instruction.UnpackPrefab != BatchObjectProcessor.PrefabUnpackingType.No)
			{
				if (PrefabUtility.IsPartOfAnyPrefab(go))
				{
					switch (instruction.UnpackPrefab)
					{
						case BatchObjectProcessor.PrefabUnpackingType.Unpack:
							_MarkChange(1, $"Unpacking prefab for object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
							PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
							break;
						case BatchObjectProcessor.PrefabUnpackingType.UnpackCompletely:
							_MarkChange(1, $"Unpacking prefab completely for object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
							PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
							break;
					}
				}
			}

			// Detach
			if (instruction.Detach)
			{
				if (go.transform.parent)
				{
					_MarkChange(1, $"Detaching object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
					go.transform.SetParent(null, instruction.WorldPositionStaysWhenDeparenting);
				}
			}

			// Detach Children Recursive
			if (instruction.DetachChildrenRecursive)
			{
				if (go.transform.childCount > 0)
				{
					int detachedObjectCount = 0;
					go.transform.DetachChildrenRecursive(ref detachedObjectCount, instruction.WorldPositionStaysWhenDeparenting);
					_MarkChange(detachedObjectCount, $"Detaching '{detachedObjectCount}' children recursively under object '{go.FullName()}'", ref changed, ref appliedModificationCount, ref appliedModifications);
				}
			}

			return changed;
		}

		private static void _MarkChange(int modificationIncrement, string message, ref bool changed, ref int appliedModificationCount, ref List<string> appliedModifications)
		{
			changed = true;
			appliedModificationCount += modificationIncrement;
			appliedModifications.Add(message);
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(BatchObjectProcessorEditor));

		#endregion
	}

}
