#if ExtenityKernel

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.ReflectionToolbox;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox.UnityInterface
{

	// TODO: Inspect how ViewBehaviour works with ExecuteAlways attribute. Should we support it? If not, prevent ViewBehaviour derived classes from having that attribute.

	// TODO: Implement this.
	// [EnsureDerivedTypesWontUseMethod(nameof(Awake), nameof(AwakeDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(Start), nameof(StartDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDestroy), nameof(OnDestroyDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnEnable), nameof(OnEnableDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDisable), nameof(OnDisableDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnValidate), nameof(OnValidateDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(Update), nameof(UpdateDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(FixedUpdate), nameof(FixedUpdateDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(LateUpdate), nameof(LateUpdateDerived))]
	public abstract class ViewBehaviour<TKernelObject, TKernel> : MonoBehaviour
		where TKernelObject : KernelObject<TKernel>
		where TKernel : KernelBase<TKernel>
	{
		#region Initialization

		protected virtual void AwakeDerived() { }
		protected virtual void StartDerived() { }
		protected virtual void OnEnableDerived() { }

		protected void Awake()
		{
			_AllViewBehaviours.Add(this);
			InitializeDataLinkIfRequired();
			AwakeDerived();
		}

		protected void Start()
		{
			StartDerived();
			DataLink.LogInvalidIDErrorAtStart();
			RefreshDataLink(true); // Call this after StartDerived so that the object can initialize itself before getting the data of linked Kernel object.
		}

		protected void OnEnable()
		{
			// Ensure the object correctly shows it's enabled state. RefreshDataLink depends on it to work correctly.
			// Though the safest way is to override the result anyway. This check is not essential but will guide
			// developers in case anything changes in Unity or if these queries are not consistent in every platform.
			// But can be safely excluded from builds if performance becomes an issue.
			var isEnabled = IsEnabled;
			if (!isEnabled)
			{
				// Do not fail harshly. The result will be overridden and everything will proceed as planned.
				Log.InternalError(118427123);
				isEnabled = true;
			}

			_AllActiveViewBehaviours.Add(this);
			RegisterUpdateCallbacks(); // Should come before OnEnableDerived.
			OnEnableDerived();
			RefreshDataLink(isEnabled); // Call this after OnEnableDerived so that the object can initialize itself before getting the data of linked Kernel object.
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroyDerived() { }
		protected virtual void OnDisableDerived() { }

		protected void OnDestroy()
		{
			_AllViewBehaviours.Remove(this);
			OnDestroyDerived();
		}

		protected void OnDisable()
		{
			// Ensure the object correctly shows it's enabled state. RefreshDataLink depends on it to work correctly.
			// Though the safest way is to override the result anyway. This check is not essential but will guide
			// developers in case anything changes in Unity or if these queries are not consistent in every platform.
			// But can be safely excluded from builds if performance becomes an issue.
			var isEnabled = IsEnabled;
			if (isEnabled)
			{
				// Do not fail harshly. The result will be overridden and everything will proceed as planned.
				Log.InternalError(118427124);
				isEnabled = false;
			}

			_AllActiveViewBehaviours.Remove(this);
			RefreshDataLink(isEnabled); // Call this before OnDisableDerived so that the data callback will be called before OnDisable operations. Otherwise the data callback will be called on a disabled object.
			DeregisterUpdateCallbacks(); // Should come before OnDisableDerived.
			OnDisableDerived();
		}

		#endregion

		#region Update

		// These Unity callbacks won't be implemented in this base class. Because Unity calls them even if they are not
		// implemented by derived types and they are too costly, especially to be called for blanks.
		// protected void Update() { }
		// protected void FixedUpdate() { }
		// protected void LateUpdate() { }

		protected virtual void UpdateDerived(TKernelObject instance) { }
		protected virtual void FixedUpdateDerived(TKernelObject instance) { }
		protected virtual void LateUpdateDerived(TKernelObject instance) { }

		private static readonly Type[] UpdateCallbackParameters = new Type[] { typeof(TKernelObject) };

		private bool _UpdateRegistered;
		private bool _FixedUpdateRegistered;
		private bool _LateUpdateRegistered;

		private void InvokeUpdate()
		{
			const bool skipQuietlyIfDestroyed = true;
			var instance = Kernel.Get<TKernelObject>(ID, skipQuietlyIfDestroyed);

			if (instance != null)
			{
				UpdateDerived(instance);
			}
		}

		private void InvokeFixedUpdate()
		{
			const bool skipQuietlyIfDestroyed = true;
			var instance = Kernel.Get<TKernelObject>(ID, skipQuietlyIfDestroyed);

			if (instance != null)
			{
				FixedUpdateDerived(instance);
			}
		}

		private void InvokeLateUpdate()
		{
			const bool skipQuietlyIfDestroyed = true;
			var instance = Kernel.Get<TKernelObject>(ID, skipQuietlyIfDestroyed);

			if (instance != null)
			{
				LateUpdateDerived(instance);
			}
		}

		private void RegisterUpdateCallbacks()
		{
			if (this.IsMethodOverriden(nameof(UpdateDerived), UpdateCallbackParameters))
			{
				_UpdateRegistered = true;
				Loop.RegisterUpdate(InvokeUpdate);
			}
			if (this.IsMethodOverriden(nameof(FixedUpdateDerived), UpdateCallbackParameters))
			{
				_FixedUpdateRegistered = true;
				Loop.RegisterFixedUpdate(InvokeFixedUpdate);
			}
			if (this.IsMethodOverriden(nameof(LateUpdateDerived), UpdateCallbackParameters))
			{
				_LateUpdateRegistered = true;
				Loop.RegisterLateUpdate(InvokeLateUpdate);
			}
		}

		private void DeregisterUpdateCallbacks()
		{
			if (_UpdateRegistered)
			{
				Loop.DeregisterUpdate(InvokeUpdate);
			}
			if (_FixedUpdateRegistered)
			{
				Loop.DeregisterFixedUpdate(InvokeFixedUpdate);
			}
			if (_LateUpdateRegistered)
			{
				Loop.DeregisterLateUpdate(InvokeLateUpdate);
			}
		}

		#endregion

		#region All ViewBehaviours

		// TODO: Try to convert these into non-static fields. Maybe move into Kernel?
		private static readonly List<ViewBehaviour<TKernelObject, TKernel>> _AllViewBehaviours = New.List<ViewBehaviour<TKernelObject, TKernel>>(1000);
		private static readonly List<ViewBehaviour<TKernelObject, TKernel>> _AllActiveViewBehaviours = New.List<ViewBehaviour<TKernelObject, TKernel>>(1000);

		public static List<ViewBehaviour<TQueriedKernelObject, TKernel>> GetAllViewBehaviours<TQueriedKernelObject>()
			where TQueriedKernelObject : KernelObject<TKernel>
		{
			return ViewBehaviour<TQueriedKernelObject, TKernel>._AllViewBehaviours;
		}

		public static List<ViewBehaviour<TQueriedKernelObject, TKernel>> GetAllActiveViewBehaviours<TQueriedKernelObject>()
			where TQueriedKernelObject : KernelObject<TKernel>
		{
			return ViewBehaviour<TQueriedKernelObject, TKernel>._AllActiveViewBehaviours;
		}

		#endregion

		#region Data Link and Invalidation

		[InlineProperty, HideLabel, FoldoutGroup("View Behaviour", Order = 15000), PropertyOrder(107)]
		public DataLink<TKernelObject, TKernel> DataLink;

		[HideInInspector] // Already displayed in DataLink
		public Ref<TKernelObject, TKernel> ID
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => DataLink.ID;
		}

		[ShowInInspector, ReadOnly, FoldoutGroup("View Behaviour"), PropertyOrder(103)]
		public TKernelObject Object => DataLink.Object;

		protected virtual void OnDataLinkModified(Ref<TKernelObject, TKernel> previouslyRegisteredID, Ref<TKernelObject, TKernel> recentlyRegisteredID) { }

		protected abstract void OnDataInvalidated([NotNull] TKernelObject instance);

		private void InitializeDataLinkIfRequired()
		{
			DataLink.Component = this;
			DataLink.DataInvalidationCallback = OnDataInvalidated;
			DataLink.DataLinkModificationCallback = OnDataLinkModified;
		}

		public void RefreshDataLink(bool isComponentEnabled)
		{
			DataLink.RefreshDataLink(isComponentEnabled);
		}

		#endregion

		#region Kernel

		[JsonIgnore]
		public TKernel Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
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

		public GameObject InstantiateChildAndSetAsModel(GameObject original, bool setPositionRotationToLocalZero, bool setScaleToOne)
		{
			DestroyModel(); // Destroy current model if exists. Do it even the 'original' is null.

			if (!original)
			{
				Log.Warning($"Tried to instantiate a non-existing object as model for '{gameObject.FullName()}'.", this);
				return null;
			}

			var model = GameObjectTools.Instantiate(original, transform, setPositionRotationToLocalZero, setScaleToOne);
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

		#region Utilities

		public bool IsEnabled => enabled && // Note that 'enabled' is the MonoBehaviour's state and is not enough by itself.
		                         gameObject.activeInHierarchy;

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
				RefreshDataLink(IsEnabled);

				OnValidateDerived();
			}
		}

		#endregion
	}

}

#endif
