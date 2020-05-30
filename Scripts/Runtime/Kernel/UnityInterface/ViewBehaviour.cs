using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.Kernel.UnityInterface
{

	// TODO: Implement this.
	// [EnsureDerivedTypesWontUseMethod(nameof(Awake), nameof(AwakeDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnEnable), nameof(OnEnableDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDestroy), nameof(OnDestroyDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDisable), nameof(OnDisableDerived))]
	public abstract class ViewBehaviour : MonoBehaviour
	{
		#region Initialization

		protected virtual void AwakeDerived() { }
		protected virtual void OnEnableDerived() { }

		protected void Awake()
		{
			AllViewBehaviours.Add(this);
			AwakeDerived();
		}

		protected void OnEnable()
		{
#if UNITY_EDITOR
			if (enabled) // Ensure 'enabled' is set to true by Unity at the time OnEnable is called. RefreshDataLink depends on it to work correctly.
			{
				throw new InternalException(118427123);
			}
#endif

			AllActiveViewBehaviours.Add(this);
			OnEnableDerived();
			RefreshDataLink(); // Call this after OnEnableDerived so that the object can initialize itself before getting the data of linked Kernel object.
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroyDerived() { }
		protected virtual void OnDisableDerived() { }

		protected void OnDestroy()
		{
			AllViewBehaviours.Remove(this);
			OnDestroyDerived();
		}

		protected void OnDisable()
		{
#if UNITY_EDITOR
			if (enabled) // Ensure 'enabled' is set to false by Unity at the time OnDisable is called. RefreshDataLink depends on it to work correctly.
			{
				throw new InternalException(118427123);
			}
#endif

			AllActiveViewBehaviours.Remove(this);
			RefreshDataLink(); // Call this before OnDisableDerived so that the data callback will be called before OnDisable operations. Otherwise the data callback will be called on a disabled object.
			OnDisableDerived();
		}

		#endregion

		#region All ViewBehaviours

		public static readonly List<ViewBehaviour> AllViewBehaviours = new List<ViewBehaviour>(1000);
		public static readonly List<ViewBehaviour> AllActiveViewBehaviours = new List<ViewBehaviour>(1000);

		#endregion

		#region ID

		/// <summary>
		/// The ID of an object in Kernel to catch related data modification events for that object.
		///
		/// Don't forget to call <see cref="RefreshDataLink"/> after modifying the ID.
		/// </summary>
		[Tooltip("The ID of an object in Kernel to catch related data modification events for that object.")]
		[BoxGroup("View Behaviour")]
		public Ref ID = Ref.Invalid;

		#endregion

		#region Data Invalidation

		protected abstract void OnDataInvalidated();

		/// <summary>
		/// The priority of data modification event. Lesser ordered callback gets called earlier. Callbacks that have
		/// the same order gets called in the order of AddListener calls. Negative values are allowed.
		///
		/// Don't forget to call <see cref="RefreshDataLink"/> after modifying the event order.
		/// </summary>
		[Tooltip("The priority of data modification event. Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.")]
		[BoxGroup("View Behaviour")]
		public int DataInvalidationEventOrder = 0;

		private Ref RegisteredID = Ref.Invalid;
		private int RegisteredDataInvalidationEventOrder = 0;

		public void RefreshDataLink()
		{
			// Use the ID if the object is active.
			// Use Ref.Invalid if the object is inactive.
			var active = gameObject.activeInHierarchy && enabled;
			var targetID = active ? ID : Ref.Invalid;

			// Ignore consecutive calls if already registered.
			if (RegisteredID == targetID &&
			    RegisteredDataInvalidationEventOrder == DataInvalidationEventOrder)
			{
				return;
			}

			// Deregister if previously registered for version changes.
			if (RegisteredID.IsValid)
			{
				Versioning.DeregisterForVersionChanges(RegisteredID, OnDataInvalidated);
			}

			RegisteredID = targetID;
			RegisteredDataInvalidationEventOrder = DataInvalidationEventOrder;

			if (RegisteredID.IsValid)
			{
				Versioning.RegisterForVersionChanges(RegisteredID, OnDataInvalidated, RegisteredDataInvalidationEventOrder);
				Versioning.Invalidate(RegisteredID);
			}
			else
			{
				// Immediately call the callback. That allows view to reset itself.
				OnDataInvalidated();
			}
		}

		#endregion

		#region Editor

		protected virtual void OnValidate()
		{
			RefreshDataLink();
		}

		#endregion
	}

}
