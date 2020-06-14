using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace Extenity.KernelToolbox
{

	[Serializable]
	public class Block
	{
		#region KernelObjects

		// TODO OPTIMIZATION: Use something like sparse matrices to improve access times.

		/// <summary>
		/// CAUTION! Use this as readonly.
		/// </summary>
		[NonSerialized, JsonIgnore]
		[ShowInInspector, ReadOnly]
		public readonly Dictionary<UInt32, KernelObject> AllKernelObjects = new Dictionary<UInt32, KernelObject>();

		public void Register(KernelObject instance)
		{
			if (instance == null)
			{
				throw new Exception($"Tried to register a null {nameof(KernelObject)}'.");
			}
			if (instance.ID.IsInvalid)
			{
				// 'Object as string' at the end is there to help debugging. ToString method of the derived KernelObject
				// can be overridden to fill in more information.
				throw new Exception($"Tried to register '{instance.ToTypeAndIDStringSafe()}' but it has an invalid ID. Object as string: '{instance.ToString()}'");
			}

			if (AllKernelObjects.TryGetValue(instance.ID, out var existingInstance))
			{
				throw new Exception($"Tried to register '{instance.ToTypeAndIDStringSafe()}' while there was an already registered object '{existingInstance.ToTypeAndIDStringSafe()}'.");
			}

			AllKernelObjects.Add(instance.ID, instance);
		}

		public void Deregister(KernelObject instance)
		{
			if (instance == null)
			{
				throw new Exception($"Tried to deregister a null {nameof(KernelObject)}'.");
			}
			if (instance.ID.IsInvalid)
			{
				// 'Object as string' at the end is there to help debugging. ToString method of the derived KernelObject
				// can be overridden to fill in more information.
				throw new Exception($"Tried to deregister '{instance.ToTypeAndIDStringSafe()}' but it has an invalid ID. Object as string: '{instance.ToString()}'");
			}

			var result = AllKernelObjects.Remove(instance.ID);

			if (!result)
			{
				throw new Exception($"Tried to deregister '{instance.ToTypeAndIDStringSafe()}' but it was not registered.");
			}
		}

		#endregion

		#region Queries

		public TKernelObject Get<TKernelObject>(Ref instanceID, bool skipQuietlyIfDestroyed = false)
			where TKernelObject : KernelObject
		{
			if (AllKernelObjects.TryGetValue(instanceID.Value, out var instance))
			{
				// No need to check if instance is null. We already know any registered object does exist.
				// if (instance == null)

				// Check for type safety
				if (instance is TKernelObject cast)
				{
					// Ensure it's not destroyed
					if (cast.ID.IsValid)
						return cast;

					if (!skipQuietlyIfDestroyed)
					{
						Log.Warning($"Queried a destroyed object '{cast.ToTypeAndIDString()}'.");
					}
					return null;
				}

				Log.CriticalError($"Queried object type '{typeof(TKernelObject).Name}' does not match the object '{instance.GetType().Name}' with ID '{instanceID}'.");
				return null;
			}
			return null;
		}

		public KernelObject Get(Ref instanceID, bool skipQuietlyIfDestroyed = false)
		{
			if (AllKernelObjects.TryGetValue(instanceID.Value, out var instance))
			{
				// No need to check if instance is null. We already know any registered object does exist.
				// if (instance == null)

				// Ensure it's not destroyed
				if (instance.ID.IsValid)
					return instance;

				if (!skipQuietlyIfDestroyed)
				{
					Log.CriticalError($"Queried a destroyed object '{instance.ToTypeAndIDString()}'.");
				}
				return null;
			}
			return null;
		}

		public bool Exists(Ref instanceID)
		{
			if (AllKernelObjects.TryGetValue(instanceID.Value, out var instance))
			{
				// No need to check if instance is null. We already know any registered object does exist.
				// if (instance == null)

				// Ensure it's not destroyed
				if (instance.ID.IsValid)
					return true;
			}
			return false;
		}

		#endregion
	}

}
