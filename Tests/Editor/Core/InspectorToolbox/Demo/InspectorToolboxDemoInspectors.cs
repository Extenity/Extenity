using System.Collections.Generic;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace ExtenityTests.InspectorToolbox.Demo
{

	public static class DemoChoices
	{
		public static readonly DropdownItem<string>[] Categories =
		{
			new DropdownItem<string>("Apple", "Fruit/Apple", "Fruit"),
			new DropdownItem<string>("Banana", "Fruit/Banana", "Fruit"),
			new DropdownItem<string>("Carrot", "Vegetable/Carrot", "Vegetable"),
			new DropdownItem<string>("Bread", "Bread"),
		};

		public static readonly DropdownItem<DemoIdentifier>[] Identifiers =
		{
			new DropdownItem<DemoIdentifier>("First Identifier", new DemoIdentifier("first")),
			new DropdownItem<DemoIdentifier>("Second Identifier", new DemoIdentifier("second")),
		};

		public static readonly DropdownItem<int>[] Levels =
		{
			new DropdownItem<int>("Level 1 - Tutorial", 1),
			new DropdownItem<int>("Level 2 - Forest", 2),
			new DropdownItem<int>("Level 3 - Castle", 3),
		};
	}

	[CustomEditor(typeof(InspectorToolboxDemo))]
	[CanEditMultipleObjects]
	public class InspectorToolboxDemoInspector : ObjectInspector<InspectorToolboxDemo>
	{
		protected override void Configure(InspectorBuilder<InspectorToolboxDemo> b)
		{
			b.Validate((t, context) => t.Count < 0 ? ValidationResult.Error("Count must not be negative.") : ValidationResult.Ok);

			// Field rules
			b.Field(x => x.Duration).Suffix("seconds");
			b.Field(x => x.DisplayName).Label("Custom Label");
			b.Field(x => x.ReadOnlyNumber).ReadOnly();
			b.Field(x => x.ReadOnlyText).ReadOnly();
			b.Field(x => x.RequiredText).Required();
			b.Field(x => x.Percentage).Validate(value => value <= 100, "A percentage above 100 is unusual.", MessageSeverity.Warning);
			b.Field(x => x.Details).VisibleWhen(t => t.HasDetails);
			b.Field(x => x.OnlyEditableWithDetails).EnabledWhen(t => t.HasDetails);

			// Dropdowns
			b.Field(x => x.Category).Dropdown((t, context) => DemoChoices.Categories);
			b.Field(x => x.UnknownCategory).Dropdown((t, context) => DemoChoices.Categories);
			b.Field(x => x.OptionalCategory).Dropdown((t, context) => DemoChoices.Categories, new DropdownOptions { EmptyLabel = "(None)", Sorted = true });
			b.Field(x => x.Identifier).Dropdown((t, context) => DemoChoices.Identifiers);
			b.Field(x => x.Level).Dropdown((t, context) => DemoChoices.Levels);

			// Field variants
			b.Field(x => x.ToggleOnTheLeft).ToggleLeft();
			b.Field(x => x.OutputFolder).FolderPicker();
			b.Field(x => x.Icon).Preview(40);
			b.Field(x => x.LinkedSettings).InlineObjectEditor();
			b.Field(x => x.Version).Inline();

			// Groups
			b.Group("Shadows", GroupStyle.Box, g =>
			{
				g.Fields(x => x.EnableShadows, x => x.ShadowQuality, x => x.ShadowStrength, x => x.FadeStartAngle, x => x.FadeEndAngle);
				g.Info("The shadow resolution is set globally in the render pipeline asset.").VisibleWhen(t => t.EnableShadows);
				g.Button("Open Render Pipeline Settings", t => { }).NoUndo().EnabledWhen(t => t.EnableShadows);
			}).Accent(new Color(0.95f, 0.75f, 0.2f));
			b.Field(x => x.ShadowQuality).VisibleWhen(t => t.EnableShadows);
			b.Field(x => x.ShadowStrength).VisibleWhen(t => t.EnableShadows);

			b.Group("Title Group", GroupStyle.Title, g => g.Fields(x => x.TitledValue, x => x.TitledText));
			b.Group("Box Group", GroupStyle.Box, g =>
			{
				g.Fields(x => x.BoxedNumber, x => x.BoxedText);
				g.Row(r =>
				{
					r.Field(x => x.RowLeft);
					r.Field(x => x.RowRight);
				});
			});
			b.Group("Foldout Group", GroupStyle.Foldout, g => g.Fields(x => x.FoldedValue, x => x.FoldedText));
			b.Group("Accent Group (foldout)", GroupStyle.Foldout, g =>
			{
				g.Fields(x => x.AccentNumber, x => x.AccentText);
				g.Group("A Title Inside", GroupStyle.Title, inner => inner.Info("Groups can hold groups, messages, rows, buttons and live values.", MessageSeverity.None));
			}).Accent(new Color(0.35f, 0.65f, 0.95f));

			b.Tabs(tabs =>
			{
				tabs.Tab("General", page => page.Fields(x => x.GeneralSpeed, x => x.GeneralRange));
				tabs.Tab("Advanced", page => page.Fields(x => x.AdvancedFlag, x => x.AdvancedIterations));
			});

			// Messages
			b.Group("Messages", GroupStyle.Title, g =>
			{
				g.Info("An information message. Messages wrap onto as many lines as they need, so nothing is cut off even in a narrow inspector.");
				g.Info("A warning message.", MessageSeverity.Warning);
				g.Info("An error message.", MessageSeverity.Error);
				g.Info("A neutral note without a severity.", MessageSeverity.None);
				g.Info(t => t.Count > 2 ? $"A computed message: Count is {t.Count}." : null);
			});

			// Buttons
			b.Group("Buttons", GroupStyle.Title, g =>
			{
				g.Button("A Full Width Button", t => t.ClickCount++).NoUndo();
				g.Row(r =>
				{
					r.Button("Neutral", t => t.ClickCount++).NoUndo();
					r.Button("Positive", t => t.ClickCount++).NoUndo().Tone(ButtonTone.Positive);
					r.Button("Destructive", t => t.ClickCount++).NoUndo().Tone(ButtonTone.Destructive);
				});
				g.Row(r =>
				{
					r.Button("Small", t => t.ClickCount++).NoUndo().Size(ButtonSize.Small);
					r.Button("Large", t => t.ClickCount++).NoUndo().Size(ButtonSize.Large);
					r.Button("Play Mode Only", t => t.ClickCount++).NoUndo().PlayModeOnly();
				});
				g.Button("Reset Count", t => t.Count = 0);
				g.Button("Set Count And Name", (InspectorToolboxDemo t, int count, string name) =>
				{
					t.Count = count;
					t.Name = name;
				}).Defaults(10, "Renamed");
			});

			// Live values
			b.Group("Live Values", GroupStyle.Box, g =>
			{
				g.LiveField("Debug Time (Normalized)", t => t.DebugTime, (t, value) => t.DebugTime = value).Range(0f, 1f);
				g.Live("Sun Angle (Degrees)", t => t.SunAngle).Range(-90f, 90f).Tint(t => Color.Lerp(new Color(0.4f, 0.5f, 0.9f), new Color(1f, 0.8f, 0.3f), t.DebugTime));
				g.Live("Click Count", t => t.ClickCount);
				g.Live("Summary", t => t.Summary).Multiline();
				g.Live("Frame Count", t => Time.frameCount).PlayModeOnly();
				g.LiveList("Runtime Names", t => t.RuntimeNames);
				g.LiveTable("Runtime Scores", t => t.RuntimeScores);
			}).Accent(new Color(0.55f, 0.6f, 0.65f));
		}
	}

	[CustomEditor(typeof(InspectorToolboxListDemo))]
	public class InspectorToolboxListDemoInspector : ObjectInspector<InspectorToolboxListDemo>
	{
		protected override void Configure(InspectorBuilder<InspectorToolboxListDemo> b)
		{
			b.List(x => x.Items).ShowIndex().ElementLabel(item => $"{item.Id} ({item.Price} gold)")
				.Validate((t, context) => HasUniqueIds(t.Items) ? ValidationResult.Ok : ValidationResult.Error("Items must not share the same Id."));
			b.List(x => x.SearchableNames).Searchable().ShowIndex();
			b.List(x => x.PickedCategories).ElementDropdown((t, context) => DemoChoices.Categories);
			b.List(x => x.Objectives).ManagedReferences(type => ObjectNames.NicifyVariableName(type.Name.Replace("Demo", "").Replace("Objective", "")) + " Objective");
			b.List(x => x.Bands).AsTable();
		}

		private static bool HasUniqueIds(List<DemoItem> items)
		{
			var seen = new HashSet<string>();
			foreach (var item in items)
			{
				if (!seen.Add(item.Id))
					return false;
			}
			return true;
		}
	}

	[CustomEditor(typeof(InspectorToolboxSectionsDemo))]
	public class InspectorToolboxSectionsDemoInspector : ObjectInspector<InspectorToolboxSectionsDemo>
	{
		protected override void Configure(InspectorBuilder<InspectorToolboxSectionsDemo> b)
		{
			b.TopLevelFieldsAsSections(searchable: true);
		}
	}

	[CustomPropertyDrawer(typeof(DemoItem))]
	public class DemoItemInspector : NestedInspector<DemoItem>
	{
		protected override void Configure(InspectorBuilder<DemoItem> b)
		{
			b.Title(item => item.Id);
			b.Field(x => x.Stock).VisibleWhen(item => item.IsLimited);
			b.Field(x => x.Price).Suffix("gold").Validate(value => value >= 0, "A price cannot be negative.");
		}
	}

	[CustomPropertyDrawer(typeof(DemoVersionSettings))]
	public class DemoVersionSettingsInspector : NestedInspector<DemoVersionSettings>
	{
		protected override void Configure(InspectorBuilder<DemoVersionSettings> b)
		{
			b.Row(r =>
			{
				r.Field(x => x.MinimumSupportedVersion).Grow();
				r.Button("Use Current Version", (ref DemoVersionSettings settings) => settings.MinimumSupportedVersion = Application.version);
			});
		}
	}

	[CustomPropertyDrawer(typeof(DemoBoxedSettings))]
	public class DemoBoxedSettingsInspector : NestedInspector<DemoBoxedSettings>
	{
		protected override void Configure(InspectorBuilder<DemoBoxedSettings> b)
		{
			b.Header(HeaderMode.Box);
			b.Field(x => x.Volume).EnabledWhen(settings => !settings.Muted);
		}
	}

	[CustomPropertyDrawer(typeof(DemoObjective), true)]
	public class DemoObjectiveInspector : NestedInspector<DemoObjective>
	{
		protected override void Configure(InspectorBuilder<DemoObjective> b)
		{
			b.Title(objective => $"{ObjectNames.NicifyVariableName(objective.GetType().Name.Replace("Demo", ""))}: {objective.Description}");
			b.Field(x => x.IsOptional).ToggleLeft();
		}
	}

	[CustomPropertyDrawer(typeof(DemoIdentifier))]
	public class DemoWrapperDrawers : FlattenedStructDrawer
	{
	}

}
