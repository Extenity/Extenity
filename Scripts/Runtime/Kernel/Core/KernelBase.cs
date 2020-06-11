using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// This might be needed in future.
	// public abstract class KernelBase<TKernel> : KernelBase
	// 	where TKernel : KernelBase<TKernel>
	// {
	// }

	public abstract class KernelBase
	{
		#region TEMP Singleton

		// TODO: This is a temporary solution until a reference resolver is introduced in Kernel deserialization. DataLink and SyncList keeps a reference to Kernel, but others may too. Figure out a generalized way to assign these references on deserialization.
		public static KernelBase _TempInstance;

		public KernelBase()
		{
			_TempInstance = this;
		}

		#endregion

		#region Versioning

		[NonSerialized] // Versioning is only used for callbacks and does not keep any data.
		public readonly Versioning Versioning = new Versioning();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(ID id)
		{
			Versioning.Invalidate(id);
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
			//Register(instance);
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
			//Deregister(instance);
			instance.ID = ID.Invalid;
		}

		#endregion

		#region All KernelObjects

		/* Disabled until finding a way to Register all already existing objects at deserialization.

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
			AllKernelObjects.Add(instance.ID, instance);
		}

		public void Deregister(KernelObject instance)
		{
			if (!instance.ID.IsValid)
				return; // TODO: Not sure what to do when received an invalid object.

			AllKernelObjects.Remove(instance.ID);
		}
		*/

		#endregion
	}

}
