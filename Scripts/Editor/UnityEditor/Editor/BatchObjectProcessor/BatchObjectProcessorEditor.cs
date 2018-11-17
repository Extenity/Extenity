using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class BatchObjectProcessorEditor
	{
		#region Process

		/// <param name="jobTags">If an entry requires a tag to be processed, the tag must be specified in this list.</param>
		/// <returns>Changed object count.</returns>
		public static int ProcessAll(this BatchObjectProcessor processor, string[] jobTags)
		{
			var count = 0;
			for (var i = 0; i < processor.Entries.Length; i++)
			{
				count += processor.ProcessEntry(i, jobTags);
			}
			return count;
		}

		/// <param name="jobTags">If an entry requires a tag to be processed, the tag must be specified in this list.</param>
		/// <returns>Changed object count.</returns>
		public static int ProcessEntry(this BatchObjectProcessor processor, int entryIndex, string[] jobTags)
		{
			var count = 0;
			var entry = processor.Entries[entryIndex];
			var jobDefinitions = processor.GetJobDefinitions(entry.AppliedJobName, jobTags);
			foreach (var jobDefinition in jobDefinitions)
			{
				for (var i = 0; i < entry.Objects.Length; i++)
				{
					count += ProcessReferencedObject(processor, entryIndex, i, jobDefinition);
				}
			}
			return count;
		}

		/// <returns>Changed object count.</returns>
		private static int ProcessReferencedObject(this BatchObjectProcessor processor, int entryIndex, int objectIndex, BatchObjectProcessor.JobDefinition jobDefinition)
		{
			var reference = processor.Entries[entryIndex].Objects[objectIndex];
			if (!reference.Object)
			{
				Log.Error($"Batch object processor has a null reference in entry '{processor.Entries[entryIndex].AppliedJobName}' (at index {entryIndex}) and object at index '{objectIndex}'.");
				return 0;
			}

			var count = 0;
			if (ProcessObject(reference.Object, jobDefinition))
				count++;
			if (reference.IncludeChildren)
			{
				reference.Object.ForeachChildren(child =>
				{
					if (ProcessObject(child, jobDefinition))
						count++;
				}, true);
			}
			return count;
		}

		/// <returns>True if anything changed in object.</returns>
		private static bool ProcessObject(GameObject go, BatchObjectProcessor.JobDefinition jobDefinition)
		{
			var changed = false;

			if (jobDefinition.ChangeStatic)
			{
				var flags = GameObjectUtility.GetStaticEditorFlags(go);
				var staticEditorFlags = (StaticEditorFlags)jobDefinition.StaticFlags;
				if (flags != staticEditorFlags)
				{
					changed = true;
					GameObjectUtility.SetStaticEditorFlags(go, staticEditorFlags);
				}
			}

			if (jobDefinition.ChangeLayers)
			{
				if (go.layer != jobDefinition.Layer.LayerIndex)
				{
					changed = true;
					go.layer = jobDefinition.Layer.LayerIndex;
				}
			}

			if (jobDefinition.ChangeTags)
			{
				if (!go.CompareTag(jobDefinition.Tag))
				{
					changed = true;
					go.tag = jobDefinition.Tag;
				}
			}

			if (jobDefinition.ChangeNavMeshArea)
			{
				var areaIndex = GameObjectUtility.GetNavMeshArea(go);
				if (areaIndex != jobDefinition.AreaIndex)
				{
					changed = true;
					GameObjectUtility.SetNavMeshArea(go, jobDefinition.AreaIndex);
				}
			}

			if (jobDefinition.DeparentAll)
			{
				go.transform.DetachChildrenRecursive();
			}

			return changed;
		}

		#endregion
	}

}
