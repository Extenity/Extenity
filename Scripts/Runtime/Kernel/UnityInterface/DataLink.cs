using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.Kernel.UnityInterface
{

	public struct DataLink
	{
		#region Interface Object

		[NonSerialized]
		internal MonoBehaviour Component;
		[NonSerialized]
		public Action DataInvalidationCallback;

		#endregion

		#region ID

		/// <summary>
		/// The ID of an object in Kernel to catch related data modification events for that object.
		///
		/// Don't forget to call <see cref="RefreshDataLink"/> after modifying the ID.
		/// </summary>
		[Tooltip("The ID of an object in Kernel to catch related data modification events for that object.")]
		[BoxGroup("View Behaviour")]
		public Ref ID; // Default value is Ref.Invalid, which is default(Ref).

		#endregion

		#region Data Invalidation

		/// <summary>
		/// The priority of data modification event. Lesser ordered callback gets called earlier. Callbacks that have
		/// the same order gets called in the order of AddListener calls. Negative values are allowed.
		///
		/// Don't forget to call <see cref="RefreshDataLink"/> after modifying the event order.
		/// </summary>
		[Tooltip("The priority of data modification event. Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.")]
		[BoxGroup("View Behaviour")]
		public int DataInvalidationEventOrder; // Default is 0.

		[NonSerialized]
		private Ref RegisteredID; // Default value is Ref.Invalid, which is default(Ref).
		[NonSerialized]
		private int RegisteredDataInvalidationEventOrder; // Default is 0.

		public void RefreshDataLink(bool isComponentEnabled)
		{
			// Use the ID if the object is active.
			// Use Ref.Invalid if the object is inactive.
			var active = Component &&
			             Component.gameObject.activeInHierarchy &&
			             // Component.enabled; Unfortunately Unity won't properly change Component.enabled at the time of OnEnable and OnDisable callbacks. So we have to override it.
			             isComponentEnabled;
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
				Versioning.DeregisterForVersionChanges(RegisteredID, DataInvalidationCallback);
			}

			RegisteredID = targetID;
			RegisteredDataInvalidationEventOrder = DataInvalidationEventOrder;

			if (RegisteredID.IsValid)
			{
				Versioning.RegisterForVersionChanges(RegisteredID, DataInvalidationCallback, RegisteredDataInvalidationEventOrder);
				Versioning.Invalidate(RegisteredID);
			}
			else
			{
				// Immediately call the callback. That allows view to reset itself.
				DataInvalidationCallback();
			}
		}

		#endregion
	}

}
