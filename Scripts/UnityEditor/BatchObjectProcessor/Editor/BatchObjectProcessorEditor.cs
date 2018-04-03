using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class BatchObjectProcessorEditor
	{
		#region Process

		/// <returns>Changed object count.</returns>
		public static int ProcessAll(this BatchObjectProcessor processor)
		{
			var count = 0;
			for (var i = 0; i < processor.Entries.Length; i++)
			{
				count += processor.ProcessEntry(i);
			}
			return count;
		}

		/// <returns>Changed object count.</returns>
		public static int ProcessEntry(this BatchObjectProcessor processor, int entryIndex)
		{
			var count = 0;
			var entry = processor.Entries[entryIndex];
			var configuration = processor.GetConfiguration(entry.Configuration);
			for (var i = 0; i < entry.Objects.Length; i++)
			{
				count += ProcessSelection(processor, entryIndex, i, configuration);
			}
			return count;
		}

		/// <returns>Changed object count.</returns>
		public static int ProcessSelection(this BatchObjectProcessor processor, int entryIndex, int objectIndex, BatchObjectProcessorConfiguration configuration)
		{
			var selection = processor.Entries[entryIndex].Objects[objectIndex];
			if (!selection.Object)
			{
				Debug.LogErrorFormat("Batch object processor has a null reference in entry '{0}' (at index {1}) and object at index '{2}'.", processor.Entries[entryIndex].Configuration, entryIndex, objectIndex);
				return 0;
			}

			var count = 0;
			if (ProcessObject(selection.Object, configuration))
				count++;
			if (selection.IncludeChildren)
			{
				selection.Object.ForeachChildren(child =>
				{
					if (ProcessObject(child, configuration))
						count++;
				}, true);
			}
			return count;
		}

		/// <returns>True if anything changed in object.</returns>
		public static bool ProcessObject(GameObject go, BatchObjectProcessorConfiguration configuration)
		{
			var changed = false;

			if (configuration.ChangeStatic)
			{
				var flags = GameObjectUtility.GetStaticEditorFlags(go);
				var staticEditorFlags = (StaticEditorFlags)configuration.StaticFlags;
				if (flags != staticEditorFlags)
				{
					changed = true;
					GameObjectUtility.SetStaticEditorFlags(go, staticEditorFlags);
				}
			}

			if (configuration.ChangeLayers)
			{
				if (go.layer != configuration.Layer.LayerIndex)
				{
					changed = true;
					go.layer = configuration.Layer.LayerIndex;
				}
			}

			if (configuration.ChangeTags)
			{
				if (!go.CompareTag(configuration.Tag))
				{
					changed = true;
					go.tag = configuration.Tag;
				}
			}

			if (configuration.ChangeNavMeshArea)
			{
				var areaIndex = GameObjectUtility.GetNavMeshArea(go);
				if (areaIndex != configuration.AreaIndex)
				{
					changed = true;
					GameObjectUtility.SetNavMeshArea(go, configuration.AreaIndex);
				}
			}

			return changed;
		}

		#endregion
	}

}
