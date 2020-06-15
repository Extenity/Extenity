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
		public readonly Dictionary<Type, Dictionary<UInt32, KernelObject>> KernelObjectsByTypes = new Dictionary<Type, Dictionary<uint, KernelObject>>();

		public void Register<TKernelObject>(KernelObject instance)
			where TKernelObject : KernelObject
		{
			Register(instance, typeof(TKernelObject));
		}

		public void Register(KernelObject instance, Type kernelObjectType)
		{
			if (instance == null)
			{
				throw new Exception($"Tried to register a null {kernelObjectType.Name}'.");
			}
			if (instance.IsInvalid)
			{
				// 'Object as string' at the end is there to help debugging. ToString method of the derived KernelObject
				// can be overridden to fill in more information.
				throw new Exception($"Tried to register '{instance.ToTypeAndIDStringSafe()}' but it has an invalid ID. Object as string: '{instance.ToString()}'");
			}

			var result = KernelObjectsByTypes.TryGetValue(kernelObjectType, out var kernelObjectsByIDs);
			if (!result)
			{
				kernelObjectsByIDs = new Dictionary<uint, KernelObject>();
				KernelObjectsByTypes.Add(kernelObjectType, kernelObjectsByIDs);
			}

			if (kernelObjectsByIDs.TryGetValue(instance.ID, out var existingInstance))
			{
				throw new Exception($"Tried to register '{instance.ToTypeAndIDStringSafe()}' while there was an already registered object '{existingInstance.ToTypeAndIDStringSafe()}'.");
			}

			kernelObjectsByIDs.Add(instance.ID, instance);
		}

		public void Deregister<TKernelObject>(KernelObject instance)
			where TKernelObject : KernelObject
		{
			if (instance == null)
			{
				throw new Exception($"Tried to deregister a null {typeof(TKernelObject).Name}'.");
			}
			if (instance.IsInvalid)
			{
				// 'Object as string' at the end is there to help debugging. ToString method of the derived KernelObject
				// can be overridden to fill in more information.
				throw new Exception($"Tried to deregister '{instance.ToTypeAndIDStringSafe()}' but it has an invalid ID. Object as string: '{instance.ToString()}'");
			}

			var result = KernelObjectsByTypes.TryGetValue(typeof(TKernelObject), out var kernelObjectsByIDs);
			if (!result)
			{
				throw new Exception($"Tried to deregister '{instance.ToTypeAndIDStringSafe()}' but it was not registered.");
			}

			result = kernelObjectsByIDs.Remove(instance.ID);
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
			if (KernelObjectsByTypes.TryGetValue(typeof(TKernelObject), out var kernelObjectsByIDs))
			{
				if (kernelObjectsByIDs.TryGetValue(instanceID.Value, out var instance))
				{
					// No need to check if instance is null. We already know any registered object does exist.
					// if (instance == null)

					// Check for type safety
					if (instance is TKernelObject cast)
					{
						// Ensure it's not destroyed
						if (cast.IsValid)
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
			}
			return null;
		}

		public KernelObject Get(UInt32 instanceID, Type instanceType, bool skipQuietlyIfDestroyed = false)
		{
			if (KernelObjectsByTypes.TryGetValue(instanceType, out var kernelObjectsByIDs))
			{
				if (kernelObjectsByIDs.TryGetValue(instanceID, out var instance))
				{
					// No need to check if instance is null. We already know any registered object does exist.
					// if (instance == null)

					// Check for type safety
					if (instance.GetType() == instanceType)
					{
						// Ensure it's not destroyed
						if (instance.IsValid)
							return instance;

						if (!skipQuietlyIfDestroyed)
						{
							Log.Warning($"Queried a destroyed object '{instance.ToTypeAndIDString()}'.");
						}
						return null;
					}

					Log.CriticalError($"Queried object type '{instanceType.Name}' does not match the object '{instance.GetType().Name}' with ID '{instanceID}'.");
					return null;
				}
			}
			return null;
		}

		// Old implementation that allowed typeless query.
		// public KernelObject Get(Ref instanceID, bool skipQuietlyIfDestroyed = false)
		// {
		// 	if (AllKernelObjects.TryGetValue(instanceID.Value, out var instance))
		// 	{
		// 		// No need to check if instance is null. We already know any registered object does exist.
		// 		// if (instance == null)
		//
		// 		// Ensure it's not destroyed
		// 		if (instance.ID.IsValid)
		// 			return instance;
		//
		// 		if (!skipQuietlyIfDestroyed)
		// 		{
		// 			Log.CriticalError($"Queried a destroyed object '{instance.ToTypeAndIDString()}'.");
		// 		}
		// 		return null;
		// 	}
		// 	return null;
		// }

		public bool Exists<TKernelObject>(Ref instanceID)
			where TKernelObject : KernelObject
		{
			if (KernelObjectsByTypes.TryGetValue(typeof(TKernelObject), out var kernelObjectsByIDs))
			{
				if (kernelObjectsByIDs.TryGetValue(instanceID.Value, out var instance))
				{
					// No need to check if instance is null. We already know any registered object does exist.
					// if (instance == null)

					// Ensure it's not destroyed
					if (instance.IsValid)
						return true;
				}
			}
			return false;
		}

		#endregion
	}

}
