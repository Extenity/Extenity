using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.Kernel.UnityInterface
{

	// TODO: Implement this.
	// [EnsureDerivedTypesWontUseMethod(nameof(Awake), nameof(AwakeDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDestroy), nameof(OnDestroyDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnEnable), nameof(OnEnableDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDisable), nameof(OnDisableDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnValidate), nameof(OnValidateDerived))]
	public abstract class ViewBehaviour : MonoBehaviour
	{
		#region Initialization

		protected virtual void AwakeDerived() { }
		protected virtual void OnEnableDerived() { }

		protected void Awake()
		{
			Log.Verbose($"Awake | enabled: {enabled} | {gameObject.FullName()}");

			AllViewBehaviours.Add(this);
			InitializeDataLinkIfRequired();
			AwakeDerived();
		}

		protected void OnEnable()
		{
			Log.Verbose($"OnEnable | enabled: {enabled} | {gameObject.FullName()}");

			// if (enabled) // Ensure 'enabled' is set to true by Unity at the time OnEnable is called. RefreshDataLink depends on it to work correctly.
			// {
			// 	throw new InternalException(118427123);
			// }

			AllActiveViewBehaviours.Add(this);
			OnEnableDerived();
			RefreshDataLink(true); // Call this after OnEnableDerived so that the object can initialize itself before getting the data of linked Kernel object.
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroyDerived() { }
		protected virtual void OnDisableDerived() { }

		protected void OnDestroy()
		{
			Log.Verbose($"OnDestroy | enabled: {enabled} | {gameObject.FullName()}");

			AllViewBehaviours.Remove(this);
			OnDestroyDerived();
		}

		protected void OnDisable()
		{
			Log.Verbose($"OnDisable | enabled: {enabled} | {gameObject.FullName()}");

			if (enabled) // Ensure 'enabled' is set to false by Unity at the time OnDisable is called. RefreshDataLink depends on it to work correctly.
			{
				throw new InternalException(118427123);
			}

			AllActiveViewBehaviours.Remove(this);
			RefreshDataLink(false); // Call this before OnDisableDerived so that the data callback will be called before OnDisable operations. Otherwise the data callback will be called on a disabled object.
			OnDisableDerived();
		}

		#endregion

		#region All ViewBehaviours

		public static readonly List<ViewBehaviour> AllViewBehaviours = new List<ViewBehaviour>(1000);
		public static readonly List<ViewBehaviour> AllActiveViewBehaviours = new List<ViewBehaviour>(1000);

		#endregion

		#region Data Link and Invalidation

		public DataLink DataLink;

		public Ref ID
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => DataLink.ID;
		}

		protected abstract void OnDataInvalidated();

		private void InitializeDataLinkIfRequired()
		{
			DataLink.Component = this;
			DataLink.DataInvalidationCallback = OnDataInvalidated;
		}

		public void RefreshDataLink(bool isComponentEnabled)
		{
			Log.Verbose($"RefreshDataLink | enabled: {enabled} | isComponentEnabled: {isComponentEnabled} | {gameObject.FullName()}");

			DataLink.RefreshDataLink(isComponentEnabled);
		}

		#endregion

		#region Editor

		protected virtual void OnValidateDerived() { }

		protected void OnValidate()
		{
			if (!Application.isPlaying) // Only run validate in edit mode.
			{
				// Setup the data link before calling validation codes. Note that validation codes should not cause
				// triggering events of the previously registered data link.
				InitializeDataLinkIfRequired();
				RefreshDataLink(enabled);

				OnValidateDerived();
			}
		}

		#endregion
	}

}
