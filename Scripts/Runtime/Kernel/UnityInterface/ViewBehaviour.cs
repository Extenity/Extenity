using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
			AllViewBehaviours.Add(this);
			InitializeDataLink();
			AwakeDerived();
		}

		protected void OnEnable()
		{
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
			AllViewBehaviours.Remove(this);
			OnDestroyDerived();
		}

		protected void OnDisable()
		{
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

		private void InitializeDataLink()
		{
			DataLink.Component = this;
			DataLink.DataInvalidationCallback = OnDataInvalidated;
		}

		public void RefreshDataLink(bool isComponentEnabled)
		{
			DataLink.RefreshDataLink(isComponentEnabled);
		}

		#endregion

		#region Editor

		protected virtual void OnValidateDerived() { }

		protected void OnValidate()
		{
			// Setup the data link before calling validation codes. So validation codes will not cause triggering events
			// of the previously registered data link.
			RefreshDataLink(enabled);

			OnValidateDerived();
		}

		#endregion
	}

}
