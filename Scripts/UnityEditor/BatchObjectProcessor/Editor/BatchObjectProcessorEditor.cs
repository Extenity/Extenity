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
				count += ProcessSelection(entry.Objects[i], configuration);
			}
			return count;
		}

		/// <returns>Changed object count.</returns>
		public static int ProcessSelection(BatchObjectProcessorSelection selection, BatchObjectProcessorConfiguration configuration)
		{
			var count = 0;
			if (selection.IncludeChildren)
			{
				selection.Object.ForeachChildren(child =>
				{
					if (ProcessObject(child, configuration))
						count++;
				}, true);
			}
			else
			{
				if (ProcessObject(selection.Object, configuration))
					count++;
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
				if (go.layer != configuration.Layer)
				{
					changed = true;
					go.layer = configuration.Layer;
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
