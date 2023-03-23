#if ExtenityKernel

using System;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using UnityEngine;

namespace Extenity.KernelToolbox.UnityInterface
{

	[Serializable]
	public struct DataLink<TKernelObject, TKernel>
		where TKernelObject : KernelObject<TKernel>
		where TKernel : KernelBase<TKernel>
	{
		#region Interface Object

		[NonSerialized]
		internal MonoBehaviour Component;
		private GameObject GameObject => Component ? Component.gameObject : null;

		#endregion

		#region ID

		/// <summary>
		/// The ID of an object in Kernel to catch related data modification events for that object.
		///
		/// Don't forget to call <see cref="RefreshDataLink"/> after modifying the ID.
		/// </summary>
		[Tooltip("The ID of an object in Kernel to catch related data modification events for that object.")]
		public Ref<TKernelObject, TKernel> ID; // Default value is Ref.Invalid, which is default(Ref).

		public bool DisableInvalidIDErrorAtStart;

		internal void LogInvalidIDErrorAtStart()
		{
			if (!DisableInvalidIDErrorAtStart)
			{
				if (ID.IsNotSet)
				{
					Log.ErrorWithContext(Component, $"Data link ID was not set for '{Component.FullName()}'.");
				}
			}
		}

		#endregion

		#region Data

		public TKernelObject Object => Kernel?.Get<TKernelObject>(ID);

		#endregion

		#region Data Link Modification

		public delegate void DataLinkModificationDelegate(Ref<TKernelObject, TKernel> previouslyRegisteredID, Ref<TKernelObject, TKernel> recentlyRegisteredID);

		[NonSerialized]
		public DataLinkModificationDelegate DataLinkModificationCallback;

		private void SafeInvokeDataLinkModificationCallback(Ref<TKernelObject, TKernel> previouslyRegisteredID, Ref<TKernelObject, TKernel> recentlyRegisteredID)
		{
			try
			{
				if (DataLinkModificationCallback != null)
				{
					DataLinkModificationCallback(previouslyRegisteredID, recentlyRegisteredID);
				}
			}
			catch (Exception exception)
			{
				Log.ErrorWithContext(Component, exception);
			}
		}

		#endregion

		#region Data Invalidation

		/// <summary>
		/// The priority of data modification event. Lesser ordered callback gets called earlier. Callbacks that have
		/// the same order gets called in the order of AddListener calls. Negative values are allowed.
		///
		/// Don't forget to call <see cref="RefreshDataLink"/> after modifying the event order.
		/// </summary>
		[Tooltip("The priority of data modification event. Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.")]
		public int DataInvalidationEventOrder; // Default is 0.

		[NonSerialized]
		private Ref<TKernelObject, TKernel> RegisteredID; // Default value is Ref.Invalid, which is default(Ref).
		[NonSerialized]
		private int RegisteredDataInvalidationEventOrder; // Default is 0.

		[NonSerialized]
		public Action<TKernelObject> DataInvalidationCallback;

		public void RefreshDataLink(bool isComponentEnabled)
		{
			// Use the ID if the object is active.
			// Use Ref.Invalid if the object is inactive.
			var active = Component &&
			             Component.gameObject.activeInHierarchy &&
			             // Component.enabled; Unfortunately Unity won't properly change Component.enabled at the time of OnEnable and OnDisable callbacks. So we have to override it.
			             isComponentEnabled;
			var targetID = active ? ID : Ref<TKernelObject, TKernel>.Invalid;

			// Ignore consecutive calls if already registered.
			if (RegisteredID == targetID &&
			    RegisteredDataInvalidationEventOrder == DataInvalidationEventOrder)
			{
				return;
			}

			// Deregister if previously registered for version changes.
			if (RegisteredID.IsSet)
			{
				Kernel.DeregisterForVersionChanges(RegisteredID, InvokeDataInvalidationCallback);
			}

			// Change the inner values of the DataLink. Do it before calling any callbacks.
			var previouslyRegisteredID = RegisteredID;
			RegisteredID = targetID;
			RegisteredDataInvalidationEventOrder = DataInvalidationEventOrder;

			// Call the DataLink Modification callback. Do it before invalidating the Data and triggering any operations
			// in View, because View would probably modify itself to accommodate itself to the new linked data.
			SafeInvokeDataLinkModificationCallback(previouslyRegisteredID, targetID);

			// Register the new ID for invalidations. Then Invalidate so that the Views renew themselves immediately.
			if (RegisteredID.IsSet)
			{
				Kernel.RegisterForVersionChanges(RegisteredID, InvokeDataInvalidationCallback, RegisteredDataInvalidationEventOrder);
				Kernel.Invalidate(RegisteredID);
			}
			else
			{
				// Immediately call the callback. That allows View to reset itself.
				SafeInvokeDataInvalidationCallback(); // Safe invoking prevents any thrown exceptions to block RefreshDataLink execution.
			}
		}

		private void InvokeDataInvalidationCallback()
		{
			if (DataInvalidationCallback != null)
			{
				const bool skipQuietlyIfDestroyed = true;
				var instance = Kernel.Get<TKernelObject>(ID, skipQuietlyIfDestroyed);

				if (instance != null)
				{
					DataInvalidationCallback(instance);
				}
			}
		}

		private void SafeInvokeDataInvalidationCallback()
		{
			try
			{
				InvokeDataInvalidationCallback();
			}
			catch (Exception exception)
			{
				Log.ErrorWithContext(Component, exception);
			}
		}

		#endregion

		#region Kernel

		[JsonIgnore]
		private TKernel Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new("Kernel");

		#endregion
	}

}

#endif
