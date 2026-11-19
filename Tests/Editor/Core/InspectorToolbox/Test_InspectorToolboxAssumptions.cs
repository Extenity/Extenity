using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Extenity.InspectorToolbox.Editor;
using Extenity.Testing;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ExtenityTests.InspectorToolbox
{

	/// <summary>
	/// The Unity behaviours the inspector toolbox is built on. When one of these fails after a Unity update, the
	/// toolbox must change, not the test.
	/// </summary>
	public class Test_InspectorToolboxAssumptions : ExtenityTestBase
	{
		#region Fixtures

		[Serializable]
		public abstract class AssumedBase
		{
			public int Shared = 1;
		}

		[Serializable]
		public class AssumedA : AssumedBase
		{
			public float OnlyA = 2f;
		}

		[Serializable]
		public class AssumedB : AssumedBase
		{
			public string OnlyB = "b";
		}

		[Serializable]
		public struct AssumedLargeStruct
		{
			public int Number;
			public string Text;
			public Vector3 Vector;
			public AssumedInner Inner;
			public List<AssumedInner> Inners;
			public float[] Floats;
		}

		[Serializable]
		public struct AssumedInner
		{
			public int Value;
			public string Name;
			public AnimationCurve Curve;
		}

		public class AssumedHolder : ScriptableObject
		{
			[SerializeReference]
			public List<AssumedBase> References = new List<AssumedBase>();
			public AssumedInner Generic;
			public int Sibling;
			public List<AssumedLargeStruct> Large = new List<AssumedLargeStruct>();
		}

		[CustomPropertyDrawer(typeof(AssumedBase), true)]
		public class AssumedBaseDrawer : PropertyDrawer
		{
			public override VisualElement CreatePropertyGUI(SerializedProperty property)
			{
				return new VisualElement { name = "assumed-drawer-base" };
			}
		}

		[CustomPropertyDrawer(typeof(AssumedB))]
		public class AssumedBDrawer : PropertyDrawer
		{
			public override VisualElement CreatePropertyGUI(SerializedProperty property)
			{
				return new VisualElement { name = "assumed-drawer-b" };
			}
		}

		private AssumedHolder Holder;
		private HiddenInspectorPanel Panel;

		protected override void OnInitialize()
		{
			Holder = ScriptableObject.CreateInstance<AssumedHolder>();
			Panel = new HiddenInspectorPanel();
		}

		protected override void OnDeinitialize()
		{
			Panel?.Dispose();
			if (Holder != null)
				Object.DestroyImmediate(Holder);
		}

		#endregion

		#region Drawer resolution of managed references

		[Test]
		public void ManagedReferenceElement_IsDrawnByTheDrawerOfItsInstanceType()
		{
			Holder.References.Add(new AssumedA());
			Holder.References.Add(new AssumedB());
			var serializedObject = new SerializedObject(Holder);
			var list = serializedObject.FindProperty(nameof(AssumedHolder.References));

			var fieldA = new PropertyField(list.GetArrayElementAtIndex(0));
			var fieldB = new PropertyField(list.GetArrayElementAtIndex(1));
			Panel.Root.Add(fieldA);
			Panel.Root.Add(fieldB);
			// Without an editor Unity would remember the two drawers in its global cache, which outlives the test.
			using (PropertyHandlerCacheScope.SelectNew())
			{
				Panel.Root.Bind(serializedObject);
				Panel.Update();
			}

			// AssumedA has no drawer of its own: the drawer registered for the base type with useForChildren draws it.
			Assert.IsNotNull(fieldA.Q("assumed-drawer-base"), "The element of a derived type was not drawn by the base type's drawer.");
			// AssumedB has its own drawer, which must win over the base type's.
			Assert.IsNotNull(fieldB.Q("assumed-drawer-b"), "The element was not drawn by the drawer of its own instance type.");
			Assert.IsNull(fieldB.Q("assumed-drawer-base"));
		}

		#endregion

		#region TrackPropertyValue

		[Test]
		public void TrackPropertyValue_OnGenericProperty_FiresWhenAChildChanges()
		{
			var serializedObject = new SerializedObject(Holder);
			var generic = serializedObject.FindProperty(nameof(AssumedHolder.Generic));
			var fired = 0;
			var tracker = new VisualElement();
			Panel.Root.Add(tracker);
			tracker.TrackPropertyValue(generic, _ => fired++);
			PumpBindings();
			fired = 0;

			// Through the same SerializedObject.
			generic.FindPropertyRelative(nameof(AssumedInner.Value)).intValue = 5;
			serializedObject.ApplyModifiedProperties();
			PumpBindings();
			Assert.Greater(fired, 0, "A child change applied through the SerializedObject was not reported.");

			// Directly on the object, as a button or an undo does.
			fired = 0;
			Holder.Generic.Name = "changed";
			PumpBindings();
			Assert.Greater(fired, 0, "A child change made on the object itself was not reported.");

			// A deeper child.
			fired = 0;
			Holder.Generic.Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			PumpBindings();
			Assert.Greater(fired, 0, "A change of a nested child was not reported.");
		}

		[Test]
		public void TrackPropertyValue_OnGenericProperty_StaysSilentWhenASiblingChanges()
		{
			var serializedObject = new SerializedObject(Holder);
			var generic = serializedObject.FindProperty(nameof(AssumedHolder.Generic));
			var fired = 0;
			var tracker = new VisualElement();
			Panel.Root.Add(tracker);
			tracker.TrackPropertyValue(generic, _ => fired++);
			PumpBindings();
			fired = 0;

			Holder.Sibling = 12;
			PumpBindings();
			Assert.AreEqual(0, fired);
		}

		[Test]
		public void TrackSerializedObjectValue_FiresWhenAnyFieldChanges()
		{
			var serializedObject = new SerializedObject(Holder);
			var fired = 0;
			var tracker = new VisualElement();
			Panel.Root.Add(tracker);
			tracker.TrackSerializedObjectValue(serializedObject, _ => fired++);
			PumpBindings();
			fired = 0;

			Holder.Sibling = 12;
			PumpBindings();
			Assert.Greater(fired, 0);
		}

		private void PumpBindings()
		{
			// Unity polls bindings at most every 100 ms.
			for (var i = 0; i < 3; i++)
			{
				Thread.Sleep(110);
				Panel.Update();
			}
		}

		#endregion

		#region Expanded state

		[Test]
		public void ExpandedState_IsReadWhenASerializedObjectIsCreated()
		{
			// The expanded state of a property is kept per type of object and loaded into a serialized object
			// when that is created. A change made through another serialized object afterwards is not seen by an
			// existing one, not even after Update (observed in Unity 6000.3). So a foldout can follow the state
			// of its own serialized object at any time, and the state others set only when it is built.
			var first = new SerializedObject(Holder);
			first.FindProperty(nameof(AssumedHolder.Generic)).isExpanded = false;
			using (var second = new SerializedObject(Holder))
			{
				Assert.IsFalse(second.FindProperty(nameof(AssumedHolder.Generic)).isExpanded);
				second.FindProperty(nameof(AssumedHolder.Generic)).isExpanded = true;
			}
			using (var third = new SerializedObject(Holder))
			{
				Assert.IsTrue(third.FindProperty(nameof(AssumedHolder.Generic)).isExpanded, "A new serialized object did not load the expanded state set before it was created.");
				third.FindProperty(nameof(AssumedHolder.Generic)).isExpanded = false;
			}
		}

		#endregion

		#region boxedValue

		[Test]
		public void BoxedValue_OfALargeStruct_IsCheapAndRoundTrips()
		{
			for (var i = 0; i < 200; i++)
			{
				var large = new AssumedLargeStruct
				{
					Number = i,
					Text = "Element " + i,
					Vector = new Vector3(i, i, i),
					Inner = new AssumedInner { Value = i, Name = "Inner", Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f) },
					Inners = new List<AssumedInner>(),
					Floats = new float[32],
				};
				for (var k = 0; k < 8; k++)
					large.Inners.Add(new AssumedInner { Value = k, Name = "Inner " + k, Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f) });
				Holder.Large.Add(large);
			}
			var serializedObject = new SerializedObject(Holder);
			var list = serializedObject.FindProperty(nameof(AssumedHolder.Large));

			var watch = Stopwatch.StartNew();
			for (var i = 0; i < list.arraySize; i++)
			{
				var boxed = (AssumedLargeStruct)list.GetArrayElementAtIndex(i).boxedValue;
				Assert.AreEqual(i, boxed.Number);
				Assert.AreEqual(8, boxed.Inners.Count);
			}
			watch.Stop();
			var millisecondsEach = watch.Elapsed.TotalMilliseconds / list.arraySize;
			// Measured at about 0.02 ms. The limit is far above that so that a slow machine does not fail the test,
			// yet low enough that evaluating a few hundred rows per change stays unnoticeable.
			Assert.Less(millisecondsEach, 1.0, $"Reading boxedValue took {millisecondsEach:F3} ms per element.");

			// Writing a boxed copy back changes the object.
			var element = list.GetArrayElementAtIndex(3);
			var copy = (AssumedLargeStruct)element.boxedValue;
			copy.Text = "written back";
			element.boxedValue = copy;
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
			Assert.AreEqual("written back", Holder.Large[3].Text);
		}

		#endregion
	}

}
