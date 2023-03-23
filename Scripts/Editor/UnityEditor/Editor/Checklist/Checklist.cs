using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	// TODO: Fail the build if the list is not completed. Not every item should fail though.

	[CreateAssetMenu(fileName = "Checklist", menuName = "Checklist")]
	[HideMonoScript]
	[Serializable]
	[ExcludeFromPresetAttribute]
	[Searchable]
	public class Checklist : ExtenityScriptableObject<Checklist>
	{
		#region Configuration

		protected override string LatestVersion => "1";

		#endregion

		#region Data

		[ListDrawerSettings(Expanded = true)]
		public List<ChecklistCategory> Categories;

		#endregion

		#region Check If Completed

		public bool CheckIfAllCategoriesAreCompleted()
		{
			foreach (var category in Categories)
			{
				if (!category.CheckIfAllGroupsAreCompleted())
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckIfAllCategoriesAreCompletedOrSkipped()
		{
			foreach (var category in Categories)
			{
				if (!category.CheckIfAllGroupsAreCompletedOrSkipped())
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckIfAllCategoriesAreCompletedUntil(string categoryTitle)
		{
			foreach (var category in Categories)
			{
				if (!category.CheckIfAllGroupsAreCompleted())
				{
					return false;
				}
				if (category.CategoryTitle.Equals(categoryTitle, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			Log.Error($"{nameof(Checklist)} category '{categoryTitle}' does not exist.");
			return false;
		}

		public bool CheckIfAllCategoriesAreCompletedOrSkippedUntil(string categoryTitle)
		{
			foreach (var category in Categories)
			{
				if (!category.CheckIfAllGroupsAreCompletedOrSkipped())
				{
					return false;
				}
				if (category.CategoryTitle.Equals(categoryTitle, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			Log.Error($"{nameof(Checklist)} category '{categoryTitle}' does not exist.");
			return false;
		}

		#endregion

		#region Version Migration

		protected override void ApplyMigration(string targetVersion)
		{
			switch (Version)
			{
				// Example
				case "0":
				{
					// Do the migration here.
					// MigrationsToUpdateFromVersion0ToVersion1();

					// Mark the settings with resulting migration.
					Version = "1";
					break;
				}

				default:
					Version = targetVersion;
					return;
			}

			// Apply migration over and over until we reach the target version.
			ApplyMigration(targetVersion);
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Checklist));

		#endregion
	}

}
