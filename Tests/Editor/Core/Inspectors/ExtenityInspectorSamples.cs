using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox;
using UnityEngine;

namespace ExtenityTests.Inspectors
{

	/// <summary>
	/// Values whose inspectors only show up inside another object: a string filter with an entry that gets a
	/// warning, and a filled checklist category.
	/// </summary>
	public class ExtenityInspectorSamples : ScriptableObject
	{
		public StringFilter Filter = new StringFilter(
			new StringFilterEntry(StringFilterType.StartsWith, "Assets/"),
			new StringFilterEntry(StringFilterType.Any, "ignored")
		);

		public ChecklistCategory Category = CreateCategory();

#if ExtenityAudio
		public Extenity.Audio.AudioEvent AudioEvent = new Extenity.Audio.AudioEvent
		{
			Name = "Sample",
			Clips = new List<AudioClip> { null, null },
		};
#endif

#if ExtenitySubsystems
		public Extenity.SubsystemManagementToolbox.ApplicationSubsystemGroup ApplicationSubsystems = new Extenity.SubsystemManagementToolbox.ApplicationSubsystemGroup
		{
			Subsystems = new[] { new Extenity.SubsystemManagementToolbox.ApplicationSubsystemDefinition { Type = Extenity.SubsystemManagementToolbox.SubsystemType.SingletonClass } },
		};

		public Extenity.SubsystemManagementToolbox.SceneSubsystemGroup SceneSubsystems = new Extenity.SubsystemManagementToolbox.SceneSubsystemGroup
		{
			Name = "Sample",
			Subsystems = new[] { new Extenity.SubsystemManagementToolbox.SubsystemDefinition { Type = Extenity.SubsystemManagementToolbox.SubsystemType.Resource } },
		};
#endif

		internal static ChecklistCategory CreateCategory()
		{
			return new ChecklistCategory("Category")
			{
				Groups = new List<ChecklistGroup>
				{
					new ChecklistGroup("Group")
					{
						Items = new List<ChecklistItem>
						{
							new ChecklistItem("Done", "Description") { Status = ChecklistItemStatus.Completed, Notes = "" },
							new ChecklistItem("", "") { Notes = "" },
						},
					},
				},
			};
		}
	}

}
