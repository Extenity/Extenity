using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Extenity.AnimationToolbox;
using Extenity.AnimationToolbox.Editor;
using Extenity.CameraToolbox;
using Extenity.DataToolbox;
using Extenity.InspectorToolbox.Editor;
using Extenity.SceneManagementToolbox;
using Extenity.Testing;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using Extenity.UnityProjectTemplateToolbox.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ExtenityTests.Inspectors
{

	/// <summary>
	/// The inspectors of Extenity's own components and data types: every one builds, is reached by a sample, and
	/// its buttons and conditions do what they say.
	/// </summary>
	public class Test_ExtenityInspectors : ExtenityTestBase
	{
		#region Setup

		private readonly List<Object> CreatedObjects = new List<Object>();
		private readonly List<IDisposable> Disposables = new List<IDisposable>();

		protected override void OnDeinitialize()
		{
			foreach (var disposable in Disposables)
				disposable.Dispose();
			Disposables.Clear();
			foreach (var created in CreatedObjects)
			{
				if (created != null)
				{
					Undo.ClearUndo(created);
					Object.DestroyImmediate(created);
				}
			}
			CreatedObjects.Clear();
		}

		private T CreateAsset<T>() where T : ScriptableObject
		{
			var instance = ScriptableObject.CreateInstance<T>();
			instance.hideFlags = HideFlags.DontSave;
			CreatedObjects.Add(instance);
			return instance;
		}

		private T CreateComponent<T>() where T : Component
		{
			return (T)CreateComponent(typeof(T));
		}

		private Component CreateComponent(Type type)
		{
			var gameObject = new GameObject("Inspector test " + type.Name) { hideFlags = HideFlags.DontSave };
			CreatedObjects.Add(gameObject);
			return gameObject.AddComponent(type);
		}

		private Transform CreateTransform(string name)
		{
			var gameObject = new GameObject(name) { hideFlags = HideFlags.DontSave };
			CreatedObjects.Add(gameObject);
			return gameObject.transform;
		}

		private BuiltInspector Build(params Object[] targets)
		{
			var keeper = InspectorTestTools.KeepExpandedStates(targets);
			var built = InspectorTestTools.Build(targets);
			Disposables.Add(built);
			Disposables.Add(keeper);
			return built;
		}

		private static Button FindButton(VisualElement root, string text, int index = 0)
		{
			var buttons = root.Query<Button>().Where(candidate => candidate.text == text).ToList();
			if (buttons.Count <= index)
				throw new InvalidOperationException($"The inspector has {buttons.Count} button(s) labelled '{text}', not {index + 1}.");
			return buttons[index];
		}

		private static VisualElement Slot(VisualElement root, string fieldName)
		{
			return root.Q(name: "ext-field-" + fieldName)
			       ?? throw new InvalidOperationException($"The inspector has no field slot for '{fieldName}'.");
		}

		private static List<VisualElement> Slots(VisualElement root, string fieldName)
		{
			return root.Query(name: "ext-field-" + fieldName).ToList();
		}

		/// <summary>Whether the element and every ancestor up to the root is displayed.</summary>
		private static bool IsShown(VisualElement element, VisualElement root)
		{
			for (var current = element; current != null; current = current.parent)
			{
				if (current.style.display == DisplayStyle.None)
					return false;
				if (current == root)
					return true;
			}
			throw new InvalidOperationException("The element is not inside the inspector.");
		}

		/// <summary>Lets the bindings notice a change. Unity polls them at most every 100 ms.</summary>
		private static void Pump(BuiltInspector built)
		{
			for (var i = 0; i < 3; i++)
			{
				System.Threading.Thread.Sleep(110);
				built.Update();
			}
		}

		private static List<string> MessagesOf(BuiltInspector built)
		{
			built.ReevaluateAll();
			return built.GetAllMessages().Select(message => message.Message).ToList();
		}

		#endregion



		#region Every inspector

		private static List<Type> GetExtenityInspectorTypes()
		{
			var testAssembly = typeof(Test_ExtenityInspectors).Assembly;
			return InspectorTestTools.GetCustomizedInspectorTypes()
			                         .Where(type => type.Assembly != testAssembly && type.Assembly.GetName().Name.StartsWith("Extenity.", StringComparison.Ordinal))
			                         .ToList();
		}

		[Test]
		public void EveryInspectorOfThePackage_Builds()
		{
			var inspectorTypes = GetExtenityInspectorTypes();
			Assert.That(inspectorTypes, Does.Contain(typeof(ChecklistInspector)), "The scan for inspector classes is broken.");
			Assert.That(inspectorTypes, Does.Contain(typeof(TransformTweenSetupInspector)), "The scan for inspector classes is broken.");

			var failures = new List<string>();
			foreach (var inspectorType in inspectorTypes)
			{
				try
				{
					InspectorTestTools.GetBlueprint(inspectorType);
				}
				catch (Exception exception)
				{
					failures.Add($"{inspectorType.FullName}: {exception.Message}");
				}
			}
			Assert.That(failures, Is.Empty, "Inspectors whose declaration is wrong:\n" + string.Join("\n", failures));
		}

		[Test]
		public void EveryInspectorOfThePackage_IsReachedByASample_AndBuildsWithoutErrors()
		{
			// Some components log what they do (the level scene list); errors are collected below.
			SetExpectedLogSeverity(LogExpectation.AllowAllLogs);

			var inspectorTypes = GetExtenityInspectorTypes();

			var samples = new List<Object>
			{
				CreateComponent<CameraHorizontalFOV>(),
				CreateComponent<TransformTweenAnimator>(),
				CreateComponent<TwoWayTransformTweenAnimator>(),
				CreateComponent<DraggableObject>(),
				CreateComponent<LevelSceneManager>(),
				CreateComponent<SelectionBaseHelper>(),
				CreateAsset<TemplateBuilderConfiguration>(),
				CreateAsset<ExtenityInspectorSamples>(),
			};
			var checklist = CreateAsset<Checklist>();
			checklist.Categories = new List<ChecklistCategory> { ExtenityInspectorSamples.CreateCategory() };
			samples.Add(checklist);

			// The inspector types of the instances that the built trees hold once everything in them is built.
			var reachedInspectorTypes = new HashSet<Type>();
			var failures = new List<string>();
			foreach (var sample in samples)
			{
				using (var errors = InspectorTestTools.CollectErrorLogs())
				{
					try
					{
						var built = Build(sample);
						built.ExpandAllLazyContent(3);
						built.ReevaluateAll();
						built.RefreshAllLiveValues();
						foreach (var instance in built.GetAllInstances())
							reachedInspectorTypes.Add(instance.Blueprint.InspectorType);
						if (built.UsesDefaultInspector && sample is not ExtenityInspectorSamples)
							failures.Add($"{sample.GetType().Name}: drawn by Unity's default inspector, not by its own.");
					}
					catch (Exception exception)
					{
						failures.Add($"{sample.GetType().Name}: {exception}");
					}
					foreach (var error in errors.Errors)
						failures.Add($"{sample.GetType().Name}: logged '{error}'");
				}
			}
			Assert.That(failures, Is.Empty, "Inspectors failed:\n" + string.Join("\n", failures));

			var unreached = inspectorTypes.Where(type => !reachedInspectorTypes.Contains(type))
			                              .Select(type => type.FullName)
			                              .ToList();
			Assert.That(unreached, Is.Empty, "No sample shows these inspectors, so they are never built by a test:\n" + string.Join("\n", unreached));
		}

		#endregion

		#region Tweens

		private TransformTweenAnimator CreateAnimatorWithLocation(out Transform location)
		{
			var animator = CreateComponent<TransformTweenAnimator>();
			animator.Transform = animator.transform;
			location = CreateTransform("Location");
			location.position = new Vector3(1f, 2f, 3f);
			location.localScale = new Vector3(2f, 2f, 2f);
			animator.Animation = new TransformTweenSetup { Location = location };
			return animator;
		}

		[Test]
		public void TweenSetup_DoNotAnimate_SwitchesThatPartOff_AndUndoRestores()
		{
			var animator = CreateAnimatorWithLocation(out _);
			var built = Build(animator);
			built.ExpandAllLazyContent(3);

			var movementButton = FindButton(built.Root, TransformTweenSetupInspector.DoNotAnimateLabel, 0);
			var scaleButton = FindButton(built.Root, TransformTweenSetupInspector.DoNotAnimateLabel, 2);
			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(movementButton);
			Undo.FlushUndoRecordObjects();
			Assert.AreEqual(Ease.Unset, animator.Animation.MovementEase);
			Assert.AreEqual(Ease.OutCubic, animator.Animation.RotationEase);
			Assert.AreEqual(Ease.OutCubic, animator.Animation.ScaleEase);

			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(scaleButton);
			Undo.FlushUndoRecordObjects();
			Assert.AreEqual(Ease.Unset, animator.Animation.ScaleEase);

			built.ReevaluateAll();
			Assert.IsFalse(movementButton.enabledSelf, "The button of a part that is already off is still enabled.");
			Assert.IsTrue(FindButton(built.Root, TransformTweenSetupInspector.DoNotAnimateLabel, 1).enabledSelf);

			Undo.PerformUndo();
			Assert.AreEqual(Ease.OutCubic, animator.Animation.ScaleEase);
			Assert.AreEqual(Ease.Unset, animator.Animation.MovementEase);
			Undo.PerformUndo();
			Assert.AreEqual(Ease.OutCubic, animator.Animation.MovementEase);
		}

		[Test]
		public void TweenSetup_WithoutLocation_SaysItIsRequired()
		{
			var animator = CreateComponent<TransformTweenAnimator>();
			animator.Animation = new TransformTweenSetup();
			var built = Build(animator);
			built.ExpandAllLazyContent(3);
			Assert.That(MessagesOf(built), Does.Contain("Location is required."));

			animator.Animation.Location = CreateTransform("Location");
			built.SerializedObject.Update();
			Assert.That(MessagesOf(built), Does.Not.Contain("Location is required."));
		}

		[Test]
		public void TweenAnimator_Jump_PutsTheTransformAtTheLocation_AndUndoRestores()
		{
			var animator = CreateAnimatorWithLocation(out var location);
			var built = Build(animator);
			var jump = FindButton(built.Root, "Jump");
			Assert.IsTrue(jump.enabledSelf, "Jump is disabled although the animation has a location.");

			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(jump);
			Undo.FlushUndoRecordObjects();
			Assert.AreEqual(location.position, animator.transform.position);
			Assert.AreEqual(location.localScale, animator.transform.localScale);

			Undo.PerformUndo();
			Assert.AreEqual(Vector3.zero, animator.transform.position);
			Assert.AreEqual(Vector3.one, animator.transform.localScale);
		}

		[Test]
		public void TweenAnimator_Play_NeedsPlayMode_AndJump_NeedsALocation()
		{
			var animator = CreateComponent<TransformTweenAnimator>();
			animator.Animation = new TransformTweenSetup();
			var built = Build(animator);
			built.ReevaluateAll();
			Assert.IsFalse(FindButton(built.Root, "Play").enabledSelf, "Play is enabled in Edit Mode.");
			Assert.IsFalse(FindButton(built.Root, "Jump").enabledSelf, "Jump is enabled without a location.");
			Assert.IsTrue(FindButton(built.Root, "Stop").enabledSelf);
		}

		[Test]
		public void TwoWayTweenAnimator_JumpToEitherEnd()
		{
			var animator = CreateComponent<TwoWayTransformTweenAnimator>();
			animator.Transform = animator.transform;
			var a = CreateTransform("A");
			a.position = new Vector3(-1f, 0f, 0f);
			var b = CreateTransform("B");
			b.position = new Vector3(5f, 0f, 0f);
			animator.AnimationToA = new TransformTweenSetup { Location = a };
			animator.AnimationToB = new TransformTweenSetup { Location = b };
			var built = Build(animator);

			InspectorTestTools.Click(FindButton(built.Root, "Jump to B"));
			Assert.AreEqual(b.position, animator.transform.position);
			InspectorTestTools.Click(FindButton(built.Root, "Jump to A"));
			Assert.AreEqual(a.position, animator.transform.position);
			Assert.IsFalse(FindButton(built.Root, "Play to A").enabledSelf, "Play is enabled in Edit Mode.");
		}

		#endregion

		#region Small components

		[Test]
		public void DraggableObject_ShowsOnlyTheSettingsOfItsDraggingType()
		{
			var draggable = CreateComponent<DraggableObject>();
			var built = Build(draggable);
			Assert.IsTrue(IsShown(Slot(built.Root, nameof(DraggableObject.DistanceFromCamera)), built.Root));
			Assert.IsFalse(IsShown(Slot(built.Root, nameof(DraggableObject.DistanceFromHitPoint)), built.Root));
			Assert.IsFalse(IsShown(Slot(built.Root, nameof(DraggableObject.RaycastMaxDistance)), built.Root));
			Assert.IsFalse(IsShown(Slot(built.Root, nameof(DraggableObject.RaycastLayerMask)), built.Root));

			draggable.DraggingType = ObjectDraggingType.SnapWithRaycast;
			built.SerializedObject.Update();
			built.ReevaluateAll();
			Assert.IsFalse(IsShown(Slot(built.Root, nameof(DraggableObject.DistanceFromCamera)), built.Root));
			Assert.IsTrue(IsShown(Slot(built.Root, nameof(DraggableObject.DistanceFromHitPoint)), built.Root));
			Assert.IsTrue(IsShown(Slot(built.Root, nameof(DraggableObject.RaycastMaxDistance)), built.Root));
			Assert.IsTrue(IsShown(Slot(built.Root, nameof(DraggableObject.RaycastLayerMask)), built.Root));
		}

		[Test]
		public void CameraHorizontalFOV_RequiresACamera_AndAChangedFieldOfViewIsAppliedAgain()
		{
			var component = CreateComponent<CameraHorizontalFOV>();
			var built = Build(component);
			Assert.That(MessagesOf(built), Does.Contain("Camera is required."));

			var lastAspectRatio = typeof(CameraHorizontalFOV).GetField("LastAspectRatio", BindingFlags.Instance | BindingFlags.NonPublic)
			                      ?? throw new MissingFieldException(nameof(CameraHorizontalFOV), "LastAspectRatio");
			lastAspectRatio.SetValue(component, 1.5f);

			Pump(built);
			built.SerializedObject.FindProperty(nameof(CameraHorizontalFOV.HorizontalFOV)).floatValue = 60f;
			built.SerializedObject.ApplyModifiedProperties();
			Pump(built);
			Assert.AreEqual(60f, component.HorizontalFOV);
			Assert.AreEqual(-1f, (float)lastAspectRatio.GetValue(component), "The change did not invalidate the applied field of view.");
		}

		[Test]
		public void LevelSceneManager_ShowsTheLevelScenesItFound()
		{
			SetExpectedLogSeverity(LogExpectation.AllowAllLogs); // The component logs the list it generates.
			var manager = CreateComponent<LevelSceneManager>();
			manager.LevelScenes = new[] { "Level-Sample-A", "Level-Sample-B" };
			var built = Build(manager);
			built.RefreshAllLiveValues();
			var texts = built.Root.Query<TextElement>().ToList().Select(element => element.text).ToList();
			Assert.That(texts, Has.Some.Contains("Level-Sample-A"));
			Assert.That(texts, Has.Some.Contains("Level-Sample-B"));
		}

		[Test]
		public void SelectionBaseHelper_ShowsNoScriptField()
		{
			var helper = CreateComponent<SelectionBaseHelper>();
			var built = Build(helper);
			Assert.IsFalse(built.UsesDefaultInspector);
			Assert.IsEmpty(built.Root.Query<PropertyField>().Where(field => field.bindingPath == "m_Script").ToList());
		}

		#endregion

		#region String filter and template builder

		[Test]
		public void StringFilter_WarnsAboutAFilterValueThatHasNoEffect()
		{
			var samples = CreateAsset<ExtenityInspectorSamples>();
			var built = Build(samples);
			built.ExpandAllLazyContent(3);
			var messages = MessagesOf(built);
			Assert.That(messages, Has.Some.Contains("Entry 1: Filter value is ignored when using Any filter type."));
			Assert.That(messages, Has.None.Contains("Entry 0"));

			samples.Filter.Filters[1].Filter = "";
			built.SerializedObject.Update();
			Assert.That(MessagesOf(built), Has.None.Contains("Filter value is ignored"));
		}

		[Test]
		public void TemplateBuilderConfiguration_ShowsTheFilterOrder_AndPicksTheOutputFolder()
		{
			var configuration = CreateAsset<TemplateBuilderConfiguration>();
			var built = Build(configuration);
			built.ExpandAllLazyContent(5);
			var texts = built.Root.Query<TextElement>().ToList().Select(element => element.text).ToList();
			Assert.That(texts, Does.Contain(TemplateBuilderConfigurationInspector.FilterOrderInfo));
			Assert.IsNotNull(Slot(built.Root, nameof(TemplateBuilderConfiguration.OutputDirectory)).Q<TextField>(className: "ext-folder-picker"), "The output directory has no folder picker.");
			Assert.AreEqual(2, built.Root.Query<ExtTableView>().ToList().Count, "The include and ignore filters are not drawn as tables.");
		}

		#endregion

		#region Checklist

		private BuiltInspector BuildFilledChecklist(out Checklist checklist)
		{
			checklist = CreateAsset<Checklist>();
			checklist.Categories = new List<ChecklistCategory> { ExtenityInspectorSamples.CreateCategory() };
			var built = Build(checklist);
			built.ExpandAllLazyContent(3);
			return built;
		}

		private static ChecklistItem ItemOf(Checklist checklist, int index)
		{
			return checklist.Categories[0].Groups[0].Items[index];
		}

		[Test]
		public void Checklist_StatusButton_MarksAnItemCompleted_OrBackToNeedingAttention_AndUndoRestores()
		{
			var built = BuildFilledChecklist(out var checklist);
			var buttons = built.Root.Query<Button>(name: ChecklistItemInspector.StatusButtonName).ToList();
			Assert.AreEqual(2, buttons.Count);

			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(buttons[1]);
			Undo.FlushUndoRecordObjects();
			Assert.AreEqual(ChecklistItemStatus.Completed, ItemOf(checklist, 1).Status);

			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(buttons[0]);
			Undo.FlushUndoRecordObjects();
			Assert.AreEqual(ChecklistItemStatus.NeedsAttention, ItemOf(checklist, 0).Status);

			Undo.PerformUndo();
			Assert.AreEqual(ChecklistItemStatus.Completed, ItemOf(checklist, 0).Status);
			Undo.PerformUndo();
			Assert.AreEqual(ChecklistItemStatus.NotStarted, ItemOf(checklist, 1).Status);
		}

		[Test]
		public void Checklist_StatusIcons_FollowTheItems()
		{
			var built = BuildFilledChecklist(out var checklist);
			var icons = built.Root.Query<Image>(name: "checklist-status-icon").ToList();
			Assert.AreEqual(2, icons.Count, "The category and the group should each have an icon.");
			Assert.AreSame(ChecklistIcons.Texture_Reject, icons[0].image);

			var buttons = built.Root.Query<Button>(name: ChecklistItemInspector.StatusButtonName).ToList();
			InspectorTestTools.Click(buttons[1]);
			built.ReevaluateAll();
			Assert.AreSame(ChecklistIcons.Texture_Accept, icons[0].image, "The category icon did not follow its completed items.");
			Assert.AreSame(ChecklistIcons.Texture_Accept, icons[1].image, "The group icon did not follow its completed items.");
			Assert.AreSame(ChecklistIcons.Texture_Accept, buttons[1].Q<Image>().image);
		}

		[Test]
		public void Checklist_ItemDetails_FollowTheFoldout_ButAnItemWithoutTitleAlwaysShowsThem()
		{
			var built = BuildFilledChecklist(out _);
			var foldouts = built.Root.Query<Foldout>(name: ChecklistItemInspector.DetailsFoldoutName).ToList();
			Assert.AreEqual(2, foldouts.Count);
			Assert.AreEqual("Done", foldouts[0].text);
			Assert.AreEqual("(No title)", foldouts[1].text);

			foldouts[0].value = false;
			foldouts[1].value = false;
			built.Update();
			built.ReevaluateAll();
			// The rows of a forced list are built aside, in a hidden container: look at the slots themselves.
			var titleSlots = Slots(built.Root, nameof(ChecklistItem.Title));
			var notesSlots = Slots(built.Root, nameof(ChecklistItem.Notes));
			Assert.AreEqual(2, titleSlots.Count);
			Assert.AreEqual(DisplayStyle.None, titleSlots[0].style.display.value, "The details of a closed item are shown.");
			Assert.AreEqual(DisplayStyle.None, notesSlots[0].style.display.value, "The details of a closed item are shown.");
			Assert.AreNotEqual(DisplayStyle.None, titleSlots[1].style.display.value, "The details of an item without a title are hidden.");

			foldouts[0].value = true;
			built.Update();
			Assert.AreNotEqual(DisplayStyle.None, titleSlots[0].style.display.value, "Opening the foldout did not show the details.");
			Assert.AreNotEqual(DisplayStyle.None, notesSlots[0].style.display.value, "Opening the foldout did not show the details.");
		}

		[Test]
		public void Checklist_Search_LooksInsideGroupsAndItems()
		{
			var category = ExtenityInspectorSamples.CreateCategory();
			Assert.IsTrue(ChecklistSearch.Matches(category, "categ"));
			Assert.IsTrue(ChecklistSearch.Matches(category, "group"));
			Assert.IsTrue(ChecklistSearch.Matches(category, "descr"), "An item's description is not searched.");
			Assert.IsFalse(ChecklistSearch.Matches(category, "nothing like this"));
			Assert.IsTrue(ChecklistSearch.Matches(category.Groups[0], "done"));
			Assert.IsFalse(ChecklistSearch.Matches(category.Groups[0], "categ"));
		}

		#endregion
	}

}
