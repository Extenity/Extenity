using System;
using System.Collections.Generic;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;
using UnityEngine;

// The examples of the inspector toolbox's documentation, kept here so that they are compiled. The editors and
// drawers are registered with Unity like real ones, which also puts them through the tests that build every
// customized inspector.
namespace ExtenityTests.InspectorToolbox.Cookbook
{

	#region Types the examples customize

	public enum CookbookMode
	{
		Manual,
		Automatic,
	}

	[Serializable]
	public struct CookbookId
	{
		public string Value;
	}

	[Serializable]
	public struct CookbookSaveSettings
	{
		public string MinimumVersion;
	}

	[Serializable]
	public class CookbookEntry
	{
		public string Id = "entry";
		public CookbookMode Mode;
		public float Interval = 1f;
		public int Limit = 10;
	}

	[Serializable]
	public abstract class CookbookStep
	{
		public string Description = "";
	}

	[Serializable]
	public class CookbookWaitStep : CookbookStep
	{
		public float Seconds = 1f;
	}

	[Serializable]
	public class CookbookSpawnStep : CookbookStep
	{
		public GameObject Prefab;
	}

	public class CookbookComponent : ScriptableObject
	{
		public bool EnableShadows;
		public float ShadowStrength = 1f;
		public float ShadowDistance = 50f;
		public string Category = "";
		public CookbookId Owner;
		public Sprite Icon;
		public CookbookSaveSettings Save;
		public List<CookbookEntry> Entries = new List<CookbookEntry>();
		[SerializeReference] public List<CookbookStep> Steps = new List<CookbookStep>();
		[NonSerialized] public float RuntimeTime;
		public int SpawnedCount => Steps.Count;

		public void ApplyShadows()
		{
		}

		public void Spawn(int count, bool immediately)
		{
		}

		public static IEnumerable<DropdownItem<string>> GetCategories()
		{
			yield return new DropdownItem<string>("Drinks", "drinks");
			yield return new DropdownItem<string>("Food", "food");
		}

		public static IEnumerable<DropdownItem<CookbookId>> GetOwners()
		{
			yield return new DropdownItem<CookbookId>("First Owner", new CookbookId { Value = "first" });
		}
	}

	#endregion

	// 1. The editor of a component or ScriptableObject: conditions, validation, a dropdown, a group, a button.
	[CustomEditor(typeof(CookbookComponent))]
	public class CookbookComponentInspector : ObjectInspector<CookbookComponent>
	{
		protected override void Configure(InspectorBuilder<CookbookComponent> b)
		{
			// Fields that are not mentioned are drawn as usual, where they are declared.
			b.Group("Shadows", GroupStyle.Box, g =>
			{
				g.Fields(x => x.EnableShadows, x => x.ShadowStrength, x => x.ShadowDistance);
				g.Info("Shadows are costly on low-end devices.", MessageSeverity.Warning).VisibleWhen(t => t.EnableShadows);
				g.OnAnyChanged(t => t.ApplyShadows());
			});
			b.Field(x => x.ShadowStrength).VisibleWhen(t => t.EnableShadows).Label("Strength");
			b.Field(x => x.ShadowDistance).VisibleWhen(t => t.EnableShadows)
				.Validate(value => value > 0f, "The distance must be above zero.");

			// 2. Dropdowns: a string, and a struct that wraps a single value.
			b.Field(x => x.Category).Dropdown((t, context) => CookbookComponent.GetCategories(),
				new DropdownOptions { EmptyLabel = "(None)", Sorted = true });
			b.Field(x => x.Owner).Dropdown((t, context) => CookbookComponent.GetOwners());

			// 3. Lists: labels, index, search, validation under the list header.
			b.List(x => x.Entries).ShowIndex().Searchable()
				.Validate((t, context) => t.Entries.Count <= 100 ? ValidationResult.Ok : ValidationResult.Warning("More than 100 entries."));

			// 4. A SerializeReference list with a type picker.
			b.List(x => x.Steps).ManagedReferences(type => ObjectNames.NicifyVariableName(type.Name));

			// 5. Buttons and live values. Placement: nodes go to the bottom unless told otherwise.
			b.Button("Apply Shadows", t => t.ApplyShadows()).NoUndo();
			b.Button("Spawn", (CookbookComponent t, int count, bool immediately) => t.Spawn(count, immediately)).PlayModeOnly().Defaults(1, true);
			b.Live("Spawned Count", t => t.SpawnedCount);
			b.LiveField("Runtime Time", t => t.RuntimeTime, (t, value) => t.RuntimeTime = value).Range(0f, 1f).PlayModeOnly();
			// A button shown only while a rule holds; a live value tinted while a rule holds (null: default colours);
			// a live field that can be edited only while a rule holds (it still shows the value) with the reason as tooltip.
			b.Button("Clear Steps", t => t.Steps.Clear()).VisibleWhen(t => t.EnableShadows);
			b.Live("Step Count", t => t.SpawnedCount).Range(0f, 10f).Tint(t => t.SpawnedCount > 5 ? Color.yellow : (Color?)null);
			b.LiveField("Preview Time", t => t.RuntimeTime, (t, value) => t.RuntimeTime = value).Range(0f, 1f)
				.EnabledWhen((t, context) => !context.IsPlaying, (t, context) => "Edit Mode only.");

			b.Field(x => x.Icon).Preview(48);
			b.Field(x => x.Save).Inline();
		}
	}

	// 6. The drawer of a serializable class: found by Unity wherever a CookbookEntry is drawn, lists included.
	[CustomPropertyDrawer(typeof(CookbookEntry))]
	public class CookbookEntryInspector : NestedInspector<CookbookEntry>
	{
		protected override void Configure(InspectorBuilder<CookbookEntry> b)
		{
			b.Title(entry => entry.Id); // the label of the element's row in a list
			b.Field(x => x.Interval).VisibleWhen(entry => entry.Mode == CookbookMode.Automatic).Suffix("s");
			b.Field(x => x.Limit).EnabledWhen((entry, context) => !context.IsPrefabInstance);
		}
	}

	// 7. A struct: its button needs the ref form, and the result is written back with undo.
	[CustomPropertyDrawer(typeof(CookbookSaveSettings))]
	public class CookbookSaveSettingsInspector : NestedInspector<CookbookSaveSettings>
	{
		protected override void Configure(InspectorBuilder<CookbookSaveSettings> b)
		{
			b.Row(r =>
			{
				r.Field(x => x.MinimumVersion).Grow();
				r.Button("Use Current Version", (ref CookbookSaveSettings settings) => settings.MinimumVersion = Application.version);
			});
		}
	}

	// 8. One drawer for a base type and everything derived from it, and wrapper structs drawn as their value.
	[CustomPropertyDrawer(typeof(CookbookStep), true)]
	public class CookbookStepInspector : NestedInspector<CookbookStep>
	{
		protected override void Configure(InspectorBuilder<CookbookStep> b)
		{
			b.Title(step => $"{ObjectNames.NicifyVariableName(step.GetType().Name)}: {step.Description}");
		}
	}

	[CustomPropertyDrawer(typeof(CookbookId))]
	public class CookbookWrapperDrawers : FlattenedStructDrawer
	{
	}

}
