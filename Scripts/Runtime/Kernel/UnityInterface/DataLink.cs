using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.Kernel.UnityInterface
{

	// TODO IMMEDIATE OPTIMIZATION: Convert to struct if possible
	public class DataLink
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
		public Ref ID = Ref.Invalid;

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
		public int DataInvalidationEventOrder = 0;

		[NonSerialized]
		private Ref RegisteredID = Ref.Invalid;
		[NonSerialized]
		private int RegisteredDataInvalidationEventOrder = 0;

		public void RefreshDataLink()
		{
			// Use the ID if the object is active.
			// Use Ref.Invalid if the object is inactive.
			var active = Component &&
			             Component.gameObject.activeInHierarchy &&
			             Component.enabled;
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
