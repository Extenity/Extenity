using System;
using System.Collections.Generic;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace ExtenityTests.InspectorToolbox
{

	public enum FixtureKind
	{
		Fixed,
		Daily,
	}

	[Serializable]
	public struct FixtureStruct
	{
		public string Name;
		public bool HasExtra;
		public float Extra;
	}

	[Serializable]
	public class FixtureNested
	{
		public string Id = "nested";
		public FixtureKind Kind;
		public int OnlyForDaily;
		public string Version = "";
	}

	[Serializable]
	public abstract class FixtureShape
	{
		public string Name = "shape";
		public float Size = 1f;
	}

	[Serializable]
	public class FixtureCircle : FixtureShape
	{
		public float Radius = 2f;
	}

	[Serializable]
	public class FixtureSquare : FixtureShape
	{
		public float Side = 3f;
	}

	[Serializable]
	public struct FixtureWrappedId
	{
		public string Value;

		public FixtureWrappedId(string value)
		{
			Value = value;
		}
	}

	public class FixtureObject : ScriptableObject
	{
		public int First;
		public bool ShowDetails;
		public int Details;
		public string Required = "";
		public int Limited;
		public string Choice = "A";
		public string OptionalChoice = "";
		public FixtureWrappedId WrappedChoice = new FixtureWrappedId("one");
		public int NumberChoice = 1;
		public float Renamed;
		public float DynamicallyLabelled;
		public float Locked;
		public float Gated;
		public float Observed;
		public float MovedToTop;
		public float Hidden;
		public int Unmentioned;
		[SerializeField] private int PrivateSerialized;
		public FixtureStruct Struct;
		public FixtureNested Nested = new FixtureNested();
		public FixtureStruct Inlined;
		public FixtureWrappedId Flattened = new FixtureWrappedId("flat");
		public List<FixtureNested> NestedList = new List<FixtureNested>();
		public List<FixtureNested> LabelledList = new List<FixtureNested>();
		public List<string> Names = new List<string>();
		public List<string> PickedNames = new List<string>();
		[SerializeReference] public List<FixtureShape> Shapes = new List<FixtureShape>();
		public bool ToggleOnLeft;
		public string Folder = "";
		public Texture2D Icon;
		public ScriptableObject Linked;
		public int GroupedA;
		public int GroupedB;
		public List<FixtureNested> Generated = new List<FixtureNested>();
		public List<FixtureStruct> Table = new List<FixtureStruct>();
		public List<int> InitiallyClosed = new List<int>();
		public List<int> InitiallyOpen = new List<int>();
		public int TabbedA;
		public int TabbedB;
		public float PlacedAfterFirst;
		public float PlacedBeforeFirst;
		public int Last;

		[NonSerialized] public int LiveCounter;
		[NonSerialized] public float LiveEditable;
		[NonSerialized] public int ObservedChangeCount;
		[NonSerialized] public int GroupChangeCount;
		[NonSerialized] public int ImguiDrawCount;

		public int PrivateSerializedValue => PrivateSerialized;

		public static readonly DropdownItem<string>[] Choices =
		{
			new DropdownItem<string>("Choice A", "A"),
			new DropdownItem<string>("Choice B", "B", "More"),
			new DropdownItem<string>("Choice C", "C", "More/Deeper"),
		};

		public static readonly DropdownItem<FixtureWrappedId>[] WrappedChoices =
		{
			new DropdownItem<FixtureWrappedId>("One", new FixtureWrappedId("one")),
			new DropdownItem<FixtureWrappedId>("Two", new FixtureWrappedId("two")),
		};

		public static readonly DropdownItem<int>[] NumberChoices =
		{
			new DropdownItem<int>("First", 1),
			new DropdownItem<int>("Second", 2),
		};
	}

	[CustomEditor(typeof(FixtureObject))]
	[CanEditMultipleObjects]
	public class FixtureObjectInspector : ObjectInspector<FixtureObject>
	{
		protected override void Configure(InspectorBuilder<FixtureObject> b)
		{
			b.Validate((t, context) => t.Details < 0 ? ValidationResult.Error("Details must not be negative.") : ValidationResult.Ok);

			b.Field(x => x.Details).VisibleWhen(t => t.ShowDetails);
			b.Field(x => x.Required).Required();
			b.Field(x => x.Limited).Validate(value => value <= 10, "Limited must be at most 10.");
			b.Field(x => x.Choice).Dropdown((t, context) => FixtureObject.Choices);
			b.Field(x => x.OptionalChoice).Dropdown((t, context) => FixtureObject.Choices, new DropdownOptions { EmptyLabel = "(None)", Sorted = true });
			b.Field(x => x.WrappedChoice).Dropdown((t, context) => FixtureObject.WrappedChoices);
			b.Field(x => x.NumberChoice).Dropdown((t, context) => FixtureObject.NumberChoices);
			b.Field(x => x.Renamed).Label("Custom Label").Suffix("s");
			b.Field(x => x.DynamicallyLabelled).Label(t => "Dynamic " + t.First);
			b.Field(x => x.Locked).ReadOnly();
			b.Field(x => x.Gated).EnabledWhen((t, context) => t.ShowDetails);
			b.Field(x => x.Observed).OnChanged(t => t.ObservedChangeCount++);
			b.Field(x => x.MovedToTop).AtTop();
			b.Field(x => x.Hidden).Hide();
			b.Field("PrivateSerialized").Label("Private");
			b.Field(x => x.Inlined).Inline();

			b.List(x => x.LabelledList).ElementLabel(element => "Item " + element.Id).ShowIndex()
				.Validate((t, context) => HasUniqueIds(t.LabelledList) ? ValidationResult.Ok : ValidationResult.Warning("Ids must be unique."));
			b.List(x => x.Names).Searchable();
			b.List(x => x.PickedNames).ElementDropdown((t, context) => FixtureObject.Choices);
			b.List(x => x.Shapes).ManagedReferences(type => "Shapes/" + ObjectNames.NicifyVariableName(type.Name));

			b.Field(x => x.ToggleOnLeft).ToggleLeft();
			b.Field(x => x.Folder).FolderPicker();
			b.Field(x => x.Icon).Preview();
			b.Field(x => x.Linked).InlineObjectEditor();

			b.Group("Grouped", GroupStyle.Box, g =>
			{
				g.Fields(x => x.GroupedA, x => x.GroupedB);
				g.Info("A is positive.").VisibleWhen(t => t.GroupedA > 0);
				g.OnAnyChanged(t => t.GroupChangeCount++);
			});

			b.List(x => x.Generated).CustomAdd(t => new FixtureNested { Id = "generated " + t.Generated.Count });
			b.List(x => x.Table).AsTable(x => x.Name, x => x.Extra);
			b.List(x => x.InitiallyClosed).CollapsedByDefault().MaxVisibleHeight(120f);
			b.List(x => x.InitiallyOpen).ExpandedByDefault();
			b.Tabs(tabs =>
			{
				tabs.Tab("Tab A", page => page.Fields(x => x.TabbedA));
				tabs.Tab("Tab B", page => page.Fields(x => x.TabbedB));
			});
			b.Field(x => x.PlacedAfterFirst).After(x => x.First);
			b.Field(x => x.PlacedBeforeFirst).Before(x => x.First);
			b.IMGUI(t => t.ImguiDrawCount++);
			b.Custom(context => new UnityEngine.UIElements.Label("custom for " + context.RootObject.name) { name = "custom-node" }).AtTop();

			b.Info(t => t.Details > 100 ? "Details is large." : null, MessageSeverity.Warning);
			b.Button("Reset Details", t => t.Details = 0);
			b.Button("Add To Details", (FixtureObject t, int amount) => t.Details += amount).Defaults(5);
			b.Live("Live Counter", t => t.LiveCounter);
			b.LiveField("Live Editable", t => t.LiveEditable, (t, value) => t.LiveEditable = value);
			b.Button("Shown With Details", t => t.Details = 1).VisibleWhen(t => t.ShowDetails);
			b.LiveField("Live Gated", t => t.LiveEditable, (t, value) => t.LiveEditable = value)
				.EnabledWhen((t, context) => t.ShowDetails, (t, context) => "Needs details.");
			b.Live("Live Tinted", t => t.LiveCounter).Range(0f, 10f).Tint(t => t.LiveCounter > 5 ? Color.green : (Color?)null);
		}

		private static bool HasUniqueIds(List<FixtureNested> list)
		{
			var seen = new HashSet<string>();
			foreach (var element in list)
			{
				if (!seen.Add(element.Id))
					return false;
			}
			return true;
		}
	}

	[CustomPropertyDrawer(typeof(FixtureNested))]
	public class FixtureNestedInspector : NestedInspector<FixtureNested>
	{
		protected override void Configure(InspectorBuilder<FixtureNested> b)
		{
			b.Title(t => t.Id);
			b.Field(x => x.OnlyForDaily).VisibleWhen(t => t.Kind == FixtureKind.Daily);
			b.Button("Stamp Version", t => t.Version = "stamped");
		}
	}

	[CustomPropertyDrawer(typeof(FixtureStruct))]
	public class FixtureStructInspector : NestedInspector<FixtureStruct>
	{
		protected override void Configure(InspectorBuilder<FixtureStruct> b)
		{
			b.Field(x => x.Extra).VisibleWhen(t => t.HasExtra);
			b.Row(r =>
			{
				r.Field(x => x.Name).Grow();
				r.Button("Set Name", (ref FixtureStruct target) => target.Name = "set by button");
			});
		}
	}

	[CustomPropertyDrawer(typeof(FixtureShape), true)]
	public class FixtureShapeInspector : NestedInspector<FixtureShape>
	{
		protected override void Configure(InspectorBuilder<FixtureShape> b)
		{
			b.Title(t => t.Name);
			b.Field(x => x.Size).Label("Size (m)");
		}
	}

	[CustomPropertyDrawer(typeof(FixtureWrappedId))]
	public class FixtureWrapperDrawers : FlattenedStructDrawer
	{
	}

	public class FixtureSectionsObject : ScriptableObject
	{
		public int Plain;
		public FixtureNested First = new FixtureNested();
		public FixtureStruct Second;
		public List<string> Names = new List<string>();
		/// <summary>Has a drawer of its own, so it is a field and not a section.</summary>
		public FixtureWrappedId WrappedId;
		[Space(7f)]
		[Header("Decorated")]
		[Tooltip("The decorated names.")]
		public List<string> DecoratedNames = new List<string>();
		public List<FixtureWrappedId> WrappedIds = new List<FixtureWrappedId>();
	}

	[CustomEditor(typeof(FixtureSectionsObject))]
	public class FixtureSectionsObjectInspector : ObjectInspector<FixtureSectionsObject>
	{
		protected override void Configure(InspectorBuilder<FixtureSectionsObject> b)
		{
			b.TopLevelFieldsAsSections(searchable: true);
			b.HideScriptField();
			b.List(x => x.DecoratedNames).ShowIndex();
			b.List(x => x.WrappedIds).ShowIndex();
		}
	}

	/// <summary>An object without a custom editor, to check nested drawers inside Unity's default inspector.</summary>
	public class FixturePlainObject : ScriptableObject
	{
		public FixtureNested Nested = new FixtureNested();
		public List<FixtureNested> NestedList = new List<FixtureNested>();
		public FixtureStruct Struct;
		[SerializeReference] public FixtureShape Shape;
		[SerializeReference] public List<FixtureShape> Shapes = new List<FixtureShape>();
	}

	#region Numbers on one line, tables with declared columns, empty lists

	[Serializable]
	public struct FixtureSize
	{
		public float Width;
		public int Count;
	}

	[Serializable]
	public struct FixtureNumbersRow
	{
		public string Name;
		public Vector3 Offset;
	}

	public class FixtureNumbersObject : ScriptableObject
	{
		public Vector3 Position;
		public FixtureSize Size;
		public List<Vector3> Points = new List<Vector3>();
		public List<FixtureNumbersRow> Rows = new List<FixtureNumbersRow>();
		public List<int> Numbers = new List<int>();
		public List<FixtureStruct> Structs = new List<FixtureStruct>();
	}

	[CustomEditor(typeof(FixtureNumbersObject))]
	public class FixtureNumbersObjectInspector : ObjectInspector<FixtureNumbersObject>
	{
		public const string DestroyButtonLabel = "Destroy This Object";

		protected override void Configure(InspectorBuilder<FixtureNumbersObject> b)
		{
			b.Field(x => x.Position).NumbersOnOneLine();
			b.Field(x => x.Size).NumbersOnOneLine().Label("Size Of It");
			b.List(x => x.Points).ShowIndex().ElementNumbersOnOneLine();
			b.List(x => x.Rows).AsTable(table =>
			{
				table.Column(x => x.Name).Title("Row");
				table.Column(x => x.Offset).Title("Offset (X Y Z)").NumbersOnOneLine().Stretch(2f).MinWidth(90f);
			});
			b.List(x => x.Numbers).ShowIndex();
			b.List(x => x.Structs).ShowIndex();
			b.Button(DestroyButtonLabel, target => UnityEngine.Object.DestroyImmediate(target)).SpaceBefore(10f).NoUndo();
		}
	}

	#endregion

	#region Declarations with mistakes. Not registered with Unity: the tests build them directly.

	public class UnknownFieldNameInspector : NestedInspector<FixtureNested>
	{
		protected override void Configure(InspectorBuilder<FixtureNested> b)
		{
			b.Field("DoesNotExist").Label("Nope");
		}
	}

	public class FieldClaimedTwiceInspector : NestedInspector<FixtureNested>
	{
		protected override void Configure(InspectorBuilder<FixtureNested> b)
		{
			b.Group("One", GroupStyle.Box, g => g.Fields(x => x.Id));
			b.Group("Two", GroupStyle.Box, g => g.Fields(x => x.Id, x => x.Version));
		}
	}

	public class ActionOnStructInspector : NestedInspector<FixtureStruct>
	{
		protected override void Configure(InspectorBuilder<FixtureStruct> b)
		{
			b.Button("Lost Change", target => target.Name = "lost");
		}
	}

	public class UnsupportedButtonParameterInspector : NestedInspector<FixtureNested>
	{
		protected override void Configure(InspectorBuilder<FixtureNested> b)
		{
			b.Button("Bad Parameter", (FixtureNested target, List<int> numbers) => target.OnlyForDaily = numbers.Count);
		}
	}

	public class NonSerializedFieldInspector : ObjectInspector<FixtureObject>
	{
		protected override void Configure(InspectorBuilder<FixtureObject> b)
		{
			b.Field(x => x.LiveCounter).Label("Not a property");
		}
	}

	public class TwoDrawersForOneFieldInspector : ObjectInspector<FixtureObject>
	{
		protected override void Configure(InspectorBuilder<FixtureObject> b)
		{
			b.Field(x => x.ToggleOnLeft).ToggleLeft().Replace((context, property) => new UnityEngine.UIElements.Toggle());
		}
	}

	public class PlacedInsideGroupInspector : ObjectInspector<FixtureObject>
	{
		protected override void Configure(InspectorBuilder<FixtureObject> b)
		{
			b.Group("Group", GroupStyle.Title, g => g.Field(x => x.GroupedA).AtTop());
		}
	}

	public class SectionsOnNestedInspector : NestedInspector<FixtureNested>
	{
		protected override void Configure(InspectorBuilder<FixtureNested> b)
		{
			b.TopLevelFieldsAsSections();
		}
	}

	#endregion

}
