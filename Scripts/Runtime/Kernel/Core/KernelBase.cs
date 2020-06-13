using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// TODO: Implement a build time tool to check and ensure there are no fields with KernelObject derived type. All references to kernel objects should use Ref instead.

	public abstract class KernelBase<TKernel> : KernelBase
		where TKernel : KernelBase<TKernel>
	{
		#region Singleton

		public static TKernel Instance;

		#endregion

		#region Initialization

		public bool IsInitialized { get; private set; }

		// TODO: Ensure initialization is called in all cases. (Deserialization, having another constructor in derived class, etc.)
		public void Initialize()
		{
			if (IsInitialized)
			{
				throw new Exception($"Tried to initialize '{GetType().Name}' more than once.");
			}
			if (Instance != null)
			{
				throw new Exception($"Tried to initialize '{GetType().Name}' while there was already an instance in use.");
			}

			Instance = (TKernel)this;
			IsInitialized = true;
		}

		#endregion

		#region Deinitialization

		public void Deinitialize()
		{
			if (!IsInitialized)
			{
				throw new Exception($"Tried to deinitialize '{GetType().Name}' more than once.");
			}
			IsInitialized = false;

			if (Instance != this)
			{
				throw new Exception($"Tried to deinitialize '{GetType().Name}' but there was another singleton.");
			}

			Instance = null;
		}

		#endregion
	}

	public abstract class KernelBase
	{
		#region Versioning

		[NonSerialized] // Versioning is only used for callbacks and does not keep any data.
		public readonly Versioning Versioning = new Versioning();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(Ref id)
		{
			Versioning.Invalidate(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InvalidateAllRegisteredIDs()
		{
			Versioning.InvalidateAllRegisteredIDs();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterForVersionChanges(Ref id, Action callback, int order = 0)
		{
			Versioning.RegisterForVersionChanges(id, callback, order);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool DeregisterForVersionChanges(Ref id, Action callback)
		{
			return Versioning.DeregisterForVersionChanges(id, callback);
		}

		#endregion

		#region ID Generator

		[SerializeField]
		public IDGenerator IDGenerator = new IDGenerator();

		#endregion

		#region Instantiate/Destroy KernelObject

		public T Instantiate<T>() where T : KernelObject, new()
		{
			var instance = new T
			{
				ID = IDGenerator.CreateID()
			};
			Register(instance);
			return instance;
		}

		public void Destroy<T>([CanBeNull] T instance) where T : KernelObject
		{
			if (instance == null || !instance.ID.IsValid)
			{
				// TODO: Not sure what to do when received an invalid object.
				return;
			}

			instance.OnDestroy();
			Invalidate(instance.ID); // Invalidate the object one last time so any listeners can refresh themselves.
			Deregister(instance);
			instance.ID = ID.Invalid;
		}

		#endregion

		#region All KernelObjects

		// TODO OPTIMIZATION: Use something like sparse matrices to improve access times.

		/// <summary>
		/// CAUTION! Use this as readonly.
		/// </summary>
		[NonSerialized, JsonIgnore]
		public readonly Dictionary<int, KernelObject> AllKernelObjects = new Dictionary<int, KernelObject>();

		public T Get<T>(Ref instanceID) where T : KernelObject
		{
			if (AllKernelObjects.TryGetValue(instanceID.Value, out var instance))
			{
				// No need to check if instance is null. We already know any registered object is alive.
				// if (instance == null)

				// Check for type safety
				if (instance is T cast)
					return cast;

				Log.CriticalError($"Queried object type '{typeof(T).Name}' does not match the object '{instance.GetType().Name}' with ID '{instanceID}'.");
				return null;
			}
			return null;
		}

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
	}

}
