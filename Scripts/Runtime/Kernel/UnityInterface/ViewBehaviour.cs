using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Sirenix.OdinInspector;
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
			DataLink.LogInvalidIDErrorAtStart();
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

		[InlineProperty, HideLabel]
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

		#region Model

		/// <summary>
		/// Setting an object as a model just allows easy access to that object via this <see cref="ViewBehaviour"/>.
		/// It's now possible to easily call <see cref="DestroyModel"/> or <see cref="DetachModel"/> for that model
		/// object. Use <see cref="SetAsModel"/> to associate an object (scene object or UI object) with the view.
		/// </summary>
		/// <remarks>
		/// If you feel like assigning multiple models to a single view, you most likely want to use multiple views
		/// instead of a single view. If you really need to use a single view, then think of introducing a parent model
		/// that have the responsibility of managing these multiple models.
		/// </remarks>
		// [NonSerialized] Nope! Let Unity serialize the created views to survive assembly refreshes.
		[ShowIf("@Model"), ReadOnly]
		public GameObject Model;

		public GameObject InstantiateChildAndSetAsModel(GameObject original)
		{
			DestroyModel(); // Destroy current model if exists. Do it even the 'original' is null.

			if (!original)
			{
				Log.Warning($"Tried to instantiate a non-existing object as model for '{gameObject.FullName()}'.", this);
				return null;
			}

			var model = GameObjectTools.Instantiate(original, transform, true);
			Model = model;
			return model;
		}

		/// <summary>
		/// Associate the object to be the model of this view. See <see cref="Model"/> for more.
		/// </summary>
		public void SetAsModel(GameObject model)
		{
			DestroyModel(); // Destroy current model if exists. Do it even the 'model' is null.

			if (!Model)
			{
				Log.Warning($"Tried to set a non-existing object as model for '{gameObject.FullName()}'.", this);
				return;
			}
			Model = model;
		}

		/// <summary>
		/// Destroy the associated model object. See <see cref="Model"/> for more.
		/// </summary>
		public void DestroyModel()
		{
			if (Model) // Silently ignore if there is no model.
			{
				GameObjectTools.Destroy(Model);
			}
		}

		/// <summary>
		/// Detach the associated model object from this view. This allows destroying the object after finishing an
		/// animation or pooling the object instead of destroying it. See <see cref="Model"/> for more.
		/// </summary>
		public void DetachModel()
		{
			DetachModelAndSetParent(null); // Set as world object
		}

		/// <summary>
		/// Detach the associated model object from this view and attach it to a parent in scene. See
		/// <see cref="DetachModel"/> for more.
		/// </summary>
		public void DetachModelAndSetParent(Transform newParent)
		{
			if (!Model)
			{
				Log.Warning($"Tried to detach a non-existing model of '{gameObject.FullName()}'.", this);
				return;
			}
			const bool worldPositionStays = false; // Preserve the local position in new parent.
			Model.transform.SetParent(newParent, worldPositionStays);
			Model = null;
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
