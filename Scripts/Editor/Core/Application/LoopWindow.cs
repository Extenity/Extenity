using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Extenity.MessagingToolbox;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Extenity.FlowToolbox
{

	/// <summary>
	/// Shows Loop's callback lists in the order they run in a frame, the callbacks registered in each, and how long
	/// they take.
	///
	/// Timings are read with ProfilerRecorders from the profiler markers that already exist: Unity's own markers of
	/// Loop's player loop steps (one per callback list, except the infrequent update lists that share a step) and
	/// ExtenityEvent's marker of each listener method. So measuring adds nothing to Loop's invocation, and costs
	/// nothing while the window is closed. Timings are available in Play Mode.
	/// </summary>
	public class LoopWindow : EditorWindow
	{
		#region Configuration

		private const int RefreshIntervalMilliseconds = 250;
		private const int RecordedFrameCount = 300;

		/// <summary>Callbacks that take at least this long per frame on average are highlighted.</summary>
		private const double HeavyCallbackMilliseconds = 1.0;

		#endregion

		#region Initialization

		[MenuItem(ExtenityMenu.Application + "Loop", priority = ExtenityMenu.ApplicationPriority + 1)]
		private static void Init()
		{
			EditorWindowTools.ToggleWindow<LoopWindow>();
		}

		private void OnEnable()
		{
			titleContent = new GUIContent("Loop");
			minSize = new Vector2(760f, 300f);
		}

		private void OnDisable()
		{
			DisposeAllRecorders();
		}

		#endregion

		#region State

		[SerializeField] private string SearchText = "";
		[SerializeField] private bool HideEmptyLists;
		[SerializeField] private bool IsMeasuring = true;

		#endregion

		#region GUI Elements

		private Label StatusLabel;
		private Label NotRunningLabel;
		private MultiColumnTreeView Tree;
		private Label FooterLabel;

		private Label FrameValue;
		private Label FixedUpdateCountValue;
		private Label TimeValue;
		private Label UnscaledTimeValue;
		private Label DeltaTimeValue;
		private Label TimeScaleValue;
		private Label ExceptionsValue;
		private Label CallbackCountValue;
		private Label LoopTimeValue;

		private static readonly Color BarColor = new Color(0.35f, 0.6f, 1f, 0.55f);
		private static readonly Color BarTrackColor = new Color(0.5f, 0.5f, 0.5f, 0.15f);
		private static readonly Color HeavyColor = new Color(1f, 0.62f, 0.25f);
		private static readonly Color DestroyedColor = new Color(1f, 0.42f, 0.38f);

		private void CreateGUI()
		{
			var root = rootVisualElement;
			root.style.flexDirection = FlexDirection.Column;

			root.Add(CreateToolbar());
			root.Add(CreateSummary());

			NotRunningLabel = new Label("Loop is not running. It starts when entering Play Mode.");
			NotRunningLabel.style.paddingLeft = 8;
			NotRunningLabel.style.paddingTop = 8;
			NotRunningLabel.style.display = DisplayStyle.None;
			root.Add(NotRunningLabel);

			Tree = CreateTree();
			root.Add(Tree);

			FooterLabel = new Label();
			FooterLabel.style.paddingLeft = 6;
			FooterLabel.style.paddingRight = 6;
			FooterLabel.style.paddingTop = 3;
			FooterLabel.style.paddingBottom = 3;
			FooterLabel.style.opacity = 0.7f;
			FooterLabel.style.whiteSpace = WhiteSpace.Normal;
			root.Add(FooterLabel);

			IsTreeBuilt = false;
			Refresh();
			root.schedule.Execute(Refresh).Every(RefreshIntervalMilliseconds);
		}

		private VisualElement CreateToolbar()
		{
			var toolbar = new Toolbar();

			StatusLabel = new Label();
			StatusLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
			StatusLabel.style.paddingLeft = 4;
			StatusLabel.style.paddingRight = 8;
			StatusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			toolbar.Add(StatusLabel);

			var measureToggle = new ToolbarToggle { text = "Measure", value = IsMeasuring, tooltip = "Measure how long each callback takes, in Play Mode." };
			measureToggle.RegisterValueChangedCallback(change =>
			{
				IsMeasuring = change.newValue;
				Refresh();
			});
			toolbar.Add(measureToggle);

			var resetButton = new ToolbarButton(ResetStats) { text = "Reset Stats", tooltip = "Forget the frames measured so far." };
			toolbar.Add(resetButton);

			var hideEmptyToggle = new ToolbarToggle { text = "Hide Empty Lists", value = HideEmptyLists };
			hideEmptyToggle.RegisterValueChangedCallback(change =>
			{
				HideEmptyLists = change.newValue;
				IsTreeStructureDirty = true;
				Refresh();
			});
			toolbar.Add(hideEmptyToggle);

			var spacer = new ToolbarSpacer();
			spacer.style.flexGrow = 1;
			toolbar.Add(spacer);

			var searchField = new ToolbarSearchField { value = SearchText, tooltip = "Filter callbacks by list, method, type or target name." };
			searchField.RegisterValueChangedCallback(change =>
			{
				SearchText = change.newValue;
				IsTreeStructureDirty = true;
				Refresh();
			});
			toolbar.Add(searchField);

			return toolbar;
		}

		private VisualElement CreateSummary()
		{
			var summary = new VisualElement();
			summary.style.flexDirection = FlexDirection.Row;
			summary.style.flexWrap = Wrap.Wrap;
			summary.style.paddingLeft = 4;
			summary.style.paddingRight = 4;
			summary.style.paddingTop = 4;
			summary.style.paddingBottom = 4;

			FrameValue = AddStat(summary, "Frame", "Time.frameCount");
			FixedUpdateCountValue = AddStat(summary, "Fixed Updates", "How many times Loop's FixedUpdate callbacks ran.");
			TimeValue = AddStat(summary, "Time", "Time.time");
			UnscaledTimeValue = AddStat(summary, "Unscaled Time", "Time.unscaledTime");
			DeltaTimeValue = AddStat(summary, "Delta Time", "Time.deltaTime");
			TimeScaleValue = AddStat(summary, "Time Scale", "Time.timeScale");
			ExceptionsValue = AddStat(summary, "Exceptions", $"Whether exceptions thrown in callbacks are caught and logged, so the following callbacks still run, or thrown out of the list. See {nameof(Loop)}.{nameof(Loop.EnableCatchingExceptionsInUpdateCallbacks)}.");
			CallbackCountValue = AddStat(summary, "Callbacks", "Callbacks registered in all lists.");
			LoopTimeValue = AddStat(summary, "Loop Time", "Average time Loop's player loop steps take per frame, and the heaviest frame.");

			return summary;
		}

		private static Label AddStat(VisualElement container, string title, string tooltip)
		{
			var box = new VisualElement { tooltip = tooltip };
			box.style.marginRight = 4;
			box.style.marginBottom = 4;
			box.style.paddingLeft = 6;
			box.style.paddingRight = 6;
			box.style.paddingTop = 2;
			box.style.paddingBottom = 2;
			box.style.minWidth = 70;
			box.style.backgroundColor = BarTrackColor;
			box.style.borderTopLeftRadius = 3;
			box.style.borderTopRightRadius = 3;
			box.style.borderBottomLeftRadius = 3;
			box.style.borderBottomRightRadius = 3;

			var titleLabel = new Label(title);
			titleLabel.style.fontSize = 10;
			titleLabel.style.opacity = 0.7f;
			box.Add(titleLabel);

			var valueLabel = new Label();
			valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			box.Add(valueLabel);

			container.Add(box);
			return valueLabel;
		}

		private MultiColumnTreeView CreateTree()
		{
			var tree = new MultiColumnTreeView
			{
				fixedItemHeight = 20,
				showAlternatingRowBackgrounds = AlternatingRowBackground.All,
				selectionType = SelectionType.Single,
			};
			tree.style.flexGrow = 1;

			tree.columns.Add(CreateLabelColumn("Callback", "Callback list, or a callback's type and method.", 260, true, TextAnchor.MiddleLeft, BindNameCell));
			tree.columns.Add(CreateLabelColumn("Order", "Registration order. Lesser ordered callbacks run earlier.", 50, false, TextAnchor.MiddleRight, BindOrderCell));
			tree.columns.Add(CreateLabelColumn("Target", "The object the callback is called on, and its life span.", 200, true, TextAnchor.MiddleLeft, BindTargetCell));
			tree.columns.Add(CreateLabelColumn("Calls/Frame", "Average calls per frame.", 80, false, TextAnchor.MiddleRight, (label, row) => BindStatCell(label, row, row.CallsPerFrame, "0.##")));
			tree.columns.Add(CreateLabelColumn("ms/Frame", "Average milliseconds per frame.", 80, false, TextAnchor.MiddleRight, (label, row) => BindStatCell(label, row, row.MillisecondsPerFrame, "0.000")));
			tree.columns.Add(CreateLabelColumn("ms/Call", "Average milliseconds per call.", 80, false, TextAnchor.MiddleRight, (label, row) => BindStatCell(label, row, row.MillisecondsPerCall, "0.000")));
			tree.columns.Add(CreateLabelColumn("Peak ms", "Milliseconds in the heaviest frame.", 80, false, TextAnchor.MiddleRight, (label, row) => BindStatCell(label, row, row.PeakMilliseconds, "0.000")));
			tree.columns.Add(new Column
			{
				name = "Share",
				title = "Share",
				width = 140,
				makeCell = MakeShareCell,
				bindCell = (element, index) => BindShareCell(element, tree.GetItemDataForIndex<RowData>(index)),
			});

			tree.itemsChosen += OnItemsChosen;
			return tree;
		}

		private Column CreateLabelColumn(string title, string tooltip, float width, bool stretchable, TextAnchor alignment, Action<Label, RowData> bind)
		{
			return new Column
			{
				name = title,
				title = title,
				width = width,
				stretchable = stretchable,
				makeHeader = () =>
				{
					var header = new Label(title) { tooltip = tooltip };
					header.style.unityTextAlign = alignment;
					header.style.paddingLeft = 4;
					header.style.paddingRight = 4;
					return header;
				},
				makeCell = () =>
				{
					var label = new Label();
					label.style.unityTextAlign = alignment;
					label.style.paddingLeft = 4;
					label.style.paddingRight = 4;
					label.style.overflow = Overflow.Hidden;
					label.style.whiteSpace = WhiteSpace.NoWrap;
					label.style.textOverflow = TextOverflow.Ellipsis;
					return label;
				},
				bindCell = (element, index) => bind((Label)element, Tree.GetItemDataForIndex<RowData>(index)),
			};
		}

		private static void BindNameCell(Label label, RowData row)
		{
			label.text = row.IsList ? $"{row.Name} ({row.ListenerCount})" : row.Name;
			label.tooltip = row.NameTooltip;
			label.style.unityFontStyleAndWeight = row.IsList ? FontStyle.Bold : FontStyle.Normal;
			label.style.opacity = row.IsList && row.ListenerCount == 0 ? 0.5f : 1f;
			label.style.color = row.IsDestroyed ? DestroyedColor : StyleKeyword.Null;
		}

		private static void BindOrderCell(Label label, RowData row)
		{
			label.text = row.IsList ? "" : row.Listener.Order.ToString();
			label.tooltip = null;
		}

		private static void BindTargetCell(Label label, RowData row)
		{
			label.text = row.Target;
			label.tooltip = row.TargetTooltip;
			label.style.color = row.IsDestroyed ? DestroyedColor : StyleKeyword.Null;
		}

		private static void BindStatCell(Label label, RowData row, double value, string format)
		{
			if (!row.HasStats)
			{
				label.text = row.IsList && row.ListenerCount == 0 && !row.HasOwnMarker ? "" : "–";
				label.tooltip = null;
				label.style.color = StyleKeyword.Null;
				return;
			}
			label.text = (row.IsApproximate ? "≈ " : "") + value.ToString(format);
			label.tooltip = row.StatsTooltip;
			label.style.color = !row.IsList && row.MillisecondsPerFrame >= HeavyCallbackMilliseconds ? HeavyColor : StyleKeyword.Null;
		}

		private static VisualElement MakeShareCell()
		{
			var cell = new VisualElement();
			cell.style.flexDirection = FlexDirection.Row;
			cell.style.alignItems = Align.Center;
			cell.style.paddingLeft = 4;
			cell.style.paddingRight = 4;

			var track = new VisualElement { name = "track" };
			track.style.flexGrow = 1;
			track.style.height = 10;
			track.style.backgroundColor = BarTrackColor;
			cell.Add(track);

			var fill = new VisualElement { name = "fill" };
			fill.style.height = Length.Percent(100);
			fill.style.backgroundColor = BarColor;
			track.Add(fill);

			var label = new Label { name = "percentage" };
			label.style.width = 56;
			label.style.unityTextAlign = TextAnchor.MiddleRight;
			cell.Add(label);

			return cell;
		}

		private static void BindShareCell(VisualElement cell, RowData row)
		{
			var fill = cell.Q("fill");
			var label = cell.Q<Label>("percentage");
			var hasShare = row.HasStats && row.Share > 0;
			fill.style.width = Length.Percent(hasShare ? (float)Math.Min(100.0, row.Share * 100.0) : 0f);
			label.text = hasShare ? (row.IsApproximate ? "≈ " : "") + (row.Share * 100.0).ToString("0.0") + "%" : "";
			cell.tooltip = hasShare ? "Share of Loop Time." + (row.IsApproximate ? " " + row.StatsTooltip : "") : null;
		}

		#endregion

		#region Refresh

		private void Refresh()
		{
			if (Tree == null)
				return;

			var instance = Loop.Instance;
			var isRunning = instance != null;
			StatusLabel.text = isRunning
				? (EditorApplication.isPlaying ? "Running in Play Mode" : "Running in Edit Mode")
				: "Not running";
			NotRunningLabel.style.display = isRunning ? DisplayStyle.None : DisplayStyle.Flex;
			Tree.style.display = isRunning ? DisplayStyle.Flex : DisplayStyle.None;

			if (!isRunning)
			{
				DisposeAllRecorders();
				CallbackLists.Clear();
				IsTreeBuilt = false;
				UpdateSummary(null);
				FooterLabel.text = "";
				return;
			}

			if (UpdateCallbackLists(instance))
			{
				IsTreeStructureDirty = true;
			}

			var isMeasuringNow = IsMeasuring && EditorApplication.isPlaying;
			SyncRecorders(isMeasuringNow);
			ReadRecorders();

			if (IsTreeStructureDirty || !IsTreeBuilt)
			{
				RebuildTree();
			}
			UpdateRowStats();
			Tree.RefreshItems();

			UpdateSummary(instance);
			UpdateFooter(isMeasuringNow);
		}

		private void UpdateSummary(LoopCallbacks instance)
		{
			FrameValue.text = Time.frameCount.ToString();
			FixedUpdateCountValue.text = Loop.FixedUpdateCount.ToString();
			TimeValue.text = Time.time.ToString("0.00") + " s";
			UnscaledTimeValue.text = Time.unscaledTime.ToString("0.00") + " s";
			DeltaTimeValue.text = (Time.deltaTime * 1000f).ToString("0.0") + " ms";
			TimeScaleValue.text = Time.timeScale.ToString("0.###");
			ExceptionsValue.text = Loop.EnableCatchingExceptionsInUpdateCallbacks ? "Caught" : "Thrown";
			CallbackCountValue.text = instance != null ? CallbackLists.Sum(list => list.Listeners.Length).ToString() : "–";
			LoopTimeValue.text = LoopTimeFrames > 0
				? $"{LoopTimeMillisecondsPerFrame:0.000} ms (peak {LoopTimePeakMilliseconds:0.000})"
				: "–";
		}

		private void UpdateFooter(bool isMeasuringNow)
		{
			if (!EditorApplication.isPlaying)
			{
				FooterLabel.text = "Timings are measured in Play Mode.";
			}
			else if (!isMeasuringNow)
			{
				FooterLabel.text = "Measuring is off.";
			}
			else
			{
				FooterLabel.text = $"Timings over the last {LoopTimeFrames} frame(s). Lists are measured by their player loop step, including Loop's own overhead. " +
				                   "≈ marks a callback whose method is also registered elsewhere, so its profiler marker sums all of them. " +
				                   "Double-click a callback to ping its object or open its script.";
			}
		}

		#endregion

		#region Callback Lists

		private class CallbackList
		{
			public string Name;
			public ExtenityEvent.Listener[] Listeners;
			/// <summary>The profiler marker of the list's own player loop step, or null if it shares a step with other lists.</summary>
			public string RunnerMarkerName;
		}

		private readonly List<CallbackList> CallbackLists = new List<CallbackList>();

		/// <summary>How many listeners use each profiler marker, across all lists.</summary>
		private readonly Dictionary<string, int> MarkerUserCounts = new Dictionary<string, int>();

		/// <summary>
		/// Takes a copy of every callback list's listeners. Returns whether anything changed since the last call.
		/// </summary>
		private bool UpdateCallbackLists(LoopCallbacks instance)
		{
			var allCallbackLists = instance.AllCallbackLists;
			var isChanged = CallbackLists.Count != allCallbackLists.Length;
			if (!isChanged)
			{
				for (var i = 0; i < allCallbackLists.Length && !isChanged; i++)
				{
					var listeners = allCallbackLists[i].Callbacks._Listeners;
					var copy = CallbackLists[i].Listeners;
					if (CallbackLists[i].Name != allCallbackLists[i].Name || copy.Length != listeners.Count)
					{
						isChanged = true;
						break;
					}
					for (var j = 0; j < copy.Length; j++)
					{
						if (copy[j].Callback != listeners[j].Callback || copy[j].Order != listeners[j].Order)
						{
							isChanged = true;
							break;
						}
					}
				}
			}
			if (!isChanged)
				return false;

			CallbackLists.Clear();
			MarkerUserCounts.Clear();
			foreach (var (name, callbacks) in allCallbackLists)
			{
				var listeners = callbacks._Listeners.ToArray();
				CallbackLists.Add(new CallbackList
				{
					Name = name,
					Listeners = listeners,
					RunnerMarkerName = GetRunnerMarkerName(name),
				});
				foreach (var listener in listeners)
				{
					if (listener.Callback == null)
						continue;
					var markerName = ExtenityEventTools.GetProfilerMarkerName(listener.Callback.Method);
					MarkerUserCounts.TryGetValue(markerName, out var count);
					MarkerUserCounts[markerName] = count + 1;
				}
			}
			return true;
		}

		#endregion

		#region Player Loop Steps

		private static string[] _AllRunnerMarkerNames;

		/// <summary>
		/// Unity records each of Loop's player loop steps with a profiler marker named after the step's type.
		/// </summary>
		private static string[] AllRunnerMarkerNames => _AllRunnerMarkerNames ??=
			typeof(Loop).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
			            .Where(type => type.IsValueType && type.Name.EndsWith("Runner", StringComparison.Ordinal))
			            .Select(type => type.Name)
			            .ToArray();

		/// <summary>
		/// Each callback list runs in its own player loop step named after it, except the infrequent update lists
		/// that all run in the same step.
		/// </summary>
		private static string GetRunnerMarkerName(string callbackListName)
		{
			var runnerName = callbackListName + "Runner";
			return AllRunnerMarkerNames.Contains(runnerName) ? runnerName : null;
		}

		#endregion

		#region Recorders

		private struct MarkerStats
		{
			public int Frames;
			public long Calls;
			public long TotalNanoseconds;
			public long PeakNanoseconds;
		}

		private readonly Dictionary<string, ProfilerRecorder> Recorders = new Dictionary<string, ProfilerRecorder>();
		private readonly Dictionary<string, MarkerStats> StatsByMarker = new Dictionary<string, MarkerStats>();
		private readonly long[] LoopTimePerFrame = new long[RecordedFrameCount];

		private int LoopTimeFrames;
		private double LoopTimeMillisecondsPerFrame;
		private double LoopTimePeakMilliseconds;

		private void SyncRecorders(bool isMeasuring)
		{
			if (!isMeasuring)
			{
				DisposeAllRecorders();
				return;
			}

			var neededMarkers = new HashSet<string>(AllRunnerMarkerNames);
			neededMarkers.UnionWith(MarkerUserCounts.Keys);

			foreach (var markerName in Recorders.Keys.ToList())
			{
				if (!neededMarkers.Contains(markerName))
				{
					Recorders[markerName].Dispose();
					Recorders.Remove(markerName);
				}
			}
			foreach (var markerName in neededMarkers)
			{
				if (!Recorders.ContainsKey(markerName))
				{
					Recorders.Add(markerName, ProfilerRecorder.StartNew(ProfilerCategory.Scripts, markerName, RecordedFrameCount,
						ProfilerRecorderOptions.Default | ProfilerRecorderOptions.WrapAroundWhenCapacityReached));
				}
			}
		}

		private void DisposeAllRecorders()
		{
			foreach (var recorder in Recorders.Values)
			{
				recorder.Dispose();
			}
			Recorders.Clear();
			StatsByMarker.Clear();
			LoopTimeFrames = 0;
		}

		private void ResetStats()
		{
			// Recreated at the next refresh with no samples.
			DisposeAllRecorders();
			Refresh();
		}

		private void ReadRecorders()
		{
			StatsByMarker.Clear();
			foreach (var (markerName, recorder) in Recorders)
			{
				var stats = new MarkerStats { Frames = recorder.Count };
				for (var i = 0; i < stats.Frames; i++)
				{
					var sample = recorder.GetSample(i);
					stats.Calls += sample.Count;
					stats.TotalNanoseconds += sample.Value;
					if (stats.PeakNanoseconds < sample.Value)
						stats.PeakNanoseconds = sample.Value;
				}
				StatsByMarker[markerName] = stats;
			}

			// Loop Time is the sum of all of Loop's player loop steps. Their samples are lined up from the latest
			// frame backwards to find the heaviest frame.
			Array.Clear(LoopTimePerFrame, 0, LoopTimePerFrame.Length);
			LoopTimeFrames = 0;
			long loopTimeTotal = 0;
			foreach (var runnerMarkerName in AllRunnerMarkerNames)
			{
				if (!Recorders.TryGetValue(runnerMarkerName, out var recorder))
					continue;
				var count = recorder.Count;
				LoopTimeFrames = Math.Max(LoopTimeFrames, count);
				for (var i = 0; i < count; i++)
				{
					var value = recorder.GetSample(i).Value;
					LoopTimePerFrame[RecordedFrameCount - count + i] += value;
					loopTimeTotal += value;
				}
			}
			LoopTimeMillisecondsPerFrame = LoopTimeFrames > 0 ? loopTimeTotal / 1e6 / LoopTimeFrames : 0;
			LoopTimePeakMilliseconds = LoopTimePerFrame.Max() / 1e6;
		}

		#endregion

		#region Rows

		private class RowData
		{
			public int Id;
			public bool IsList;
			public CallbackList List;
			public ExtenityEvent.Listener Listener;
			public int ListenerCount;

			public string Name;
			public string NameTooltip;
			public string Target;
			public string TargetTooltip;
			public bool IsDestroyed;

			/// <summary>The callback's marker, or the list's own player loop step marker.</summary>
			public string MarkerName;
			public bool HasOwnMarker => MarkerName != null;

			public bool HasStats;
			public bool IsApproximate;
			public string StatsTooltip;
			public double CallsPerFrame;
			public double MillisecondsPerFrame;
			public double MillisecondsPerCall;
			public double PeakMilliseconds;
			public double Share;

			public List<RowData> Children;
		}

		private bool IsTreeBuilt;
		private bool IsTreeStructureDirty;
		private string PreviousSearchText;
		private readonly List<RowData> ListRows = new List<RowData>();

		/// <summary>Keeps callback rows' ids stable between rebuilds, so the tree keeps its selection and expansion.</summary>
		private readonly Dictionary<(string ListName, Action Callback), int> CallbackRowIds = new Dictionary<(string, Action), int>();
		private int NextCallbackRowId = 1000;

		/// <summary>
		/// Lists the user collapsed. Rebuilding the tree forgets the expansion state, so it is restored from here.
		/// Lists are expanded otherwise.
		/// </summary>
		private readonly HashSet<string> CollapsedListNames = new HashSet<string>();

		private void RebuildTree()
		{
			// Remember the expansion state of the lists that can be expanded, before the tree forgets it.
			if (IsTreeBuilt && string.IsNullOrWhiteSpace(PreviousSearchText))
			{
				foreach (var previousListRow in ListRows)
				{
					if (previousListRow.Children.Count == 0)
						continue;
					if (Tree.IsExpanded(previousListRow.Id))
						CollapsedListNames.Remove(previousListRow.Name);
					else
						CollapsedListNames.Add(previousListRow.Name);
				}
			}
			PreviousSearchText = SearchText;

			ListRows.Clear();
			var rootItems = new List<TreeViewItemData<RowData>>();
			var hasSearch = !string.IsNullOrWhiteSpace(SearchText);

			for (var listIndex = 0; listIndex < CallbackLists.Count; listIndex++)
			{
				var list = CallbackLists[listIndex];
				if (HideEmptyLists && list.Listeners.Length == 0)
					continue;

				var listRow = new RowData
				{
					Id = listIndex + 1,
					IsList = true,
					List = list,
					ListenerCount = list.Listeners.Length,
					Name = list.Name,
					NameTooltip = list.RunnerMarkerName != null
						? $"Callback list '{list.Name}', run by Loop's '{list.RunnerMarkerName}' player loop step."
						: $"Callback list '{list.Name}', run by Loop's infrequent updates player loop step together with the other infrequent update lists.",
					Target = "",
					MarkerName = list.RunnerMarkerName,
					Children = new List<RowData>(),
				};
				var isListNameMatching = hasSearch && Contains(list.Name, SearchText);

				var childItems = new List<TreeViewItemData<RowData>>();
				foreach (var listener in list.Listeners)
				{
					var row = CreateCallbackRow(list, listener);
					if (hasSearch && !isListNameMatching && !Contains(row.Name, SearchText) && !Contains(row.NameTooltip, SearchText) && !Contains(row.Target, SearchText))
						continue;
					listRow.Children.Add(row);
					childItems.Add(new TreeViewItemData<RowData>(GetCallbackRowId(list.Name, listener.Callback), row));
				}

				if (hasSearch && !isListNameMatching && childItems.Count == 0)
					continue;

				ListRows.Add(listRow);
				rootItems.Add(new TreeViewItemData<RowData>(listRow.Id, listRow, childItems));
			}

			Tree.SetRootItems(rootItems);
			Tree.Rebuild();
			foreach (var listRow in ListRows)
			{
				// Search results are always shown expanded, without changing what the user collapsed.
				if (listRow.Children.Count > 0 && (hasSearch || !CollapsedListNames.Contains(listRow.Name)))
				{
					Tree.ExpandItem(listRow.Id);
				}
			}
			IsTreeBuilt = true;
			IsTreeStructureDirty = false;
		}

		private int GetCallbackRowId(string listName, Action callback)
		{
			var key = (listName, callback);
			if (!CallbackRowIds.TryGetValue(key, out var id))
			{
				id = NextCallbackRowId++;
				CallbackRowIds.Add(key, id);
			}
			return id;
		}

		private RowData CreateCallbackRow(CallbackList list, ExtenityEvent.Listener listener)
		{
			var row = new RowData { List = list, Listener = listener };
			var callback = listener.Callback;
			if (callback == null)
			{
				row.Name = "(null)";
				row.Target = "";
				return row;
			}

			var method = callback.Method;
			row.Name = $"{method.DeclaringType?.Name ?? "Unknown"}.{method.Name}";
			row.NameTooltip = $"{method.DeclaringType?.FullName ?? "Unknown"}.{method.Name}\nAssembly: {method.DeclaringType?.Assembly.GetName().Name}";
			row.MarkerName = ExtenityEventTools.GetProfilerMarkerName(method);
			row.IsDestroyed = listener.IsObjectDestroyed;

			var target = callback.Target;
			if (target == null)
			{
				row.Target = "static";
			}
			else if (target is Object unityObject)
			{
				row.Target = unityObject ? $"{unityObject.name} ({unityObject.GetType().Name})" : "(destroyed)";
			}
			else
			{
				row.Target = target.GetType().Name;
			}

			var targetTooltip = target == null ? "Static method." : $"Called on {target.GetType().FullName}.";
			if (listener.LifeSpan == ListenerLifeSpan.RemovedAtFirstEmit)
			{
				row.Target += " · once";
				targetTooltip += "\nRemoved after its first call.";
			}
			if (listener.IsLifeSpanTargetAssigned)
			{
				var lifeSpanTargetName = listener.LifeSpanTarget ? listener.LifeSpanTarget.name : "(destroyed)";
				row.Target += $" · while '{lifeSpanTargetName}' lives";
				targetTooltip += $"\nRemoved when '{lifeSpanTargetName}' is destroyed.";
			}
			if (row.IsDestroyed)
			{
				targetTooltip += "\nIts object is destroyed. It will be removed at the next call of the list.";
			}
			row.TargetTooltip = targetTooltip;
			return row;
		}

		private void UpdateRowStats()
		{
			var loopTime = LoopTimeMillisecondsPerFrame;
			foreach (var listRow in ListRows)
			{
				foreach (var row in listRow.Children)
				{
					ApplyMarkerStats(row, row.MarkerName, loopTime);
					var userCount = row.MarkerName != null && MarkerUserCounts.TryGetValue(row.MarkerName, out var count) ? count : 0;
					row.IsApproximate = row.HasStats && userCount > 1;
					if (row.HasStats)
					{
						row.StatsTooltip = row.IsApproximate
							? $"Profiler marker '{row.MarkerName}' is shared by {userCount} callbacks of this method, so this is their total."
							: $"Profiler marker '{row.MarkerName}'.";
					}
				}

				if (listRow.List.RunnerMarkerName != null)
				{
					ApplyMarkerStats(listRow, listRow.List.RunnerMarkerName, loopTime);
					listRow.StatsTooltip = $"Profiler marker '{listRow.List.RunnerMarkerName}' of Loop's player loop step, including Loop's own overhead.";
				}
				else
				{
					ApplySumOfCallbacks(listRow, loopTime);
				}
			}
		}

		private void ApplyMarkerStats(RowData row, string markerName, double loopTime)
		{
			if (markerName == null || !StatsByMarker.TryGetValue(markerName, out var stats) || stats.Frames == 0)
			{
				row.HasStats = false;
				return;
			}
			row.HasStats = true;
			row.IsApproximate = false;
			row.CallsPerFrame = (double)stats.Calls / stats.Frames;
			row.MillisecondsPerFrame = stats.TotalNanoseconds / 1e6 / stats.Frames;
			row.MillisecondsPerCall = stats.Calls > 0 ? stats.TotalNanoseconds / 1e6 / stats.Calls : 0;
			row.PeakMilliseconds = stats.PeakNanoseconds / 1e6;
			row.Share = loopTime > 0 ? row.MillisecondsPerFrame / loopTime : 0;
		}

		/// <summary>
		/// The infrequent update lists share a player loop step, so a list's time is the sum of its callbacks' markers.
		/// Each of the list's listeners is counted, also the ones hidden by the search.
		/// </summary>
		private void ApplySumOfCallbacks(RowData listRow, double loopTime)
		{
			var markers = new HashSet<string>();
			var isApproximate = false;
			foreach (var listener in listRow.List.Listeners)
			{
				if (listener.Callback == null)
					continue;
				var markerName = ExtenityEventTools.GetProfilerMarkerName(listener.Callback.Method);
				markers.Add(markerName);
				if (MarkerUserCounts.TryGetValue(markerName, out var count) && count > 1)
					isApproximate = true;
			}

			var frames = 0;
			long calls = 0, total = 0, peak = 0;
			foreach (var markerName in markers)
			{
				if (!StatsByMarker.TryGetValue(markerName, out var stats))
					continue;
				frames = Math.Max(frames, stats.Frames);
				calls += stats.Calls;
				total += stats.TotalNanoseconds;
				peak += stats.PeakNanoseconds;
			}

			listRow.HasStats = frames > 0;
			if (!listRow.HasStats)
				return;
			listRow.IsApproximate = isApproximate;
			listRow.CallsPerFrame = (double)calls / frames;
			listRow.MillisecondsPerFrame = total / 1e6 / frames;
			listRow.MillisecondsPerCall = calls > 0 ? total / 1e6 / calls : 0;
			listRow.PeakMilliseconds = peak / 1e6;
			listRow.Share = loopTime > 0 ? listRow.MillisecondsPerFrame / loopTime : 0;
			listRow.StatsTooltip = "Sum of the callbacks' profiler markers, since this list shares its player loop step with the other infrequent update lists. Peak is the sum of the callbacks' peaks." +
			                       (isApproximate ? " Some of the markers are shared with callbacks in other places." : "");
		}

		private static bool Contains(string text, string search)
		{
			return text != null && text.IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
		}

		#endregion

		#region Double Click

		private void OnItemsChosen(IEnumerable<object> items)
		{
			var row = items.OfType<RowData>().FirstOrDefault();
			if (row == null || row.IsList || row.Listener.Callback == null)
				return;

			if (row.Listener.Callback.Target is Object unityObject && unityObject)
			{
				EditorGUIUtility.PingObject(unityObject);
				return;
			}

			var type = GetUserType(row.Listener.Callback.Method.DeclaringType);
			var script = type != null ? FindScript(type) : null;
			if (script != null)
			{
				AssetDatabase.OpenAsset(script);
			}
			else
			{
				ShowNotification(new GUIContent($"Could not find the script of '{type?.Name ?? row.Name}'."));
			}
		}

		/// <summary>
		/// Lambdas and local functions live in compiler-generated nested types. Their script is the one of the type
		/// that declares them.
		/// </summary>
		private static Type GetUserType(Type type)
		{
			while (type != null && (type.IsDefined(typeof(CompilerGeneratedAttribute), false) || type.Name.StartsWith("<", StringComparison.Ordinal)))
			{
				type = type.DeclaringType;
			}
			return type;
		}

		private static MonoScript FindScript(Type type)
		{
			// A nested type lives in the script of its outermost type.
			var outermostType = type;
			while (outermostType.DeclaringType != null)
			{
				outermostType = outermostType.DeclaringType;
			}

			var typeName = outermostType.Name;
			var genericMarkIndex = typeName.IndexOf('`');
			if (genericMarkIndex >= 0)
				typeName = typeName.Substring(0, genericMarkIndex);

			MonoScript nameMatch = null;
			foreach (var guid in AssetDatabase.FindAssets($"t:MonoScript {typeName}"))
			{
				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
				if (script == null)
					continue;
				if (script.GetClass() == outermostType)
					return script;
				if (nameMatch == null && script.name == typeName)
					nameMatch = script;
			}
			return nameMatch;
		}

		#endregion
	}

}
