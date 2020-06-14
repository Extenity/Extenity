using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// TODO: Implement KernelData and move AllKernelObjects mechanism into there. See 119993322.

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

			RegisterAllKernelObjectFieldsInKernel();
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

		#region Instantiate/Destroy KernelObject

		public TKernelObject Instantiate<TKernelObject>()
			where TKernelObject : KernelObject<TKernel>, new()
		{
			var instance = new TKernelObject();
			instance.SetID(IDGenerator.CreateID());
			Register(instance);
			return instance;
		}

		public void Destroy<TKernelObject>([CanBeNull] TKernelObject instance)
			where TKernelObject : KernelObject<TKernel>
		{
			if (instance == null || !instance.ID.IsValid)
			{
				// TODO: Not sure what to do when received an invalid object.
				return;
			}

			instance.OnDestroy();
			Invalidate(instance.ID); // Invalidate the object one last time so any listeners can refresh themselves.
			Deregister(instance);
			instance.ResetIDOnDestroy();
		}

		#endregion
	}

	public abstract class KernelBase
	{
		#region Versioning

		[NonSerialized] // Versioning is only used for callbacks and does not keep any data.
		public readonly Versioning Versioning = new Versioning();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(int id)
		{
			Versioning.Invalidate(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InvalidateAllRegisteredIDs()
		{
			Versioning.InvalidateAllRegisteredIDs();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterForVersionChanges(int id, Action callback, int order = 0)
		{
			Versioning.RegisterForVersionChanges(id, callback, order);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool DeregisterForVersionChanges(int id, Action callback)
		{
			return Versioning.DeregisterForVersionChanges(id, callback);
		}

		#endregion

		#region ID Generator

		[SerializeField]
		public IDGenerator IDGenerator = new IDGenerator();

		#endregion

		#region All KernelObjects

		// TODO OPTIMIZATION: Use something like sparse matrices to improve access times.

		/// <summary>
		/// CAUTION! Use this as readonly.
		/// </summary>
		[NonSerialized, JsonIgnore]
		public readonly Dictionary<int, KernelObject> AllKernelObjects = new Dictionary<int, KernelObject>();

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

		#region All KernelObjects - Gather And Register All Fields

		// This is a temporary solution until KernelData is introduced. The codes below are fine for a temporary
		// solution but they actually do a terrible job at deciding whether a field should be serialized.
		//
		// When implementing the KernelData, there will be no need to Register each KernelObject into the data,
		// since the data will already be there. See 119993322.

		protected void RegisterAllKernelObjectFieldsInKernel()
		{
			IterateAllFieldsRecursively(this, "");
		}

		private void IterateAllFieldsRecursively(object obj, string basePath)
		{
			var allFields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var fieldInfo in allFields)
			{
				if (fieldInfo.GetAttribute<NonSerializedAttribute>(false) != null ||
				    fieldInfo.GetAttribute<JsonIgnoreAttribute>(false) != null)
				{
					continue;
				}

				var value = fieldInfo.GetValue(obj);
				if (value == null)
					continue;
				var fieldType = fieldInfo.FieldType;

				// See if its KernelObject
				if (fieldType.InheritsOrImplements(typeof(KernelObject)))
				{
					var kernelObject = (KernelObject)value;
					// Log.Info($"Found | {(basePath + "/" + fieldInfo.Name)} : " + kernelObject.ToTypeAndIDString());
					Register(kernelObject);
					continue;
				}
				// Log.Info($"{(basePath + "/" + fieldInfo.Name)}");

				// See if its a container
				if (fieldType.InheritsOrImplements(typeof(IEnumerable)) &&
				    fieldType != typeof(string)) // string is an exception. We don't want to iterate its characters.
				{
					var enumerable = (IEnumerable)value;
					foreach (var item in enumerable)
					{
						if (item != null)
						{
							IterateAllFieldsRecursively(item, basePath + "/" + fieldInfo.Name);
						}
					}
					continue;
				}

				// See if its another class that may keep an ID field inside
				if (fieldType.IsClassOrStruct())
				{
					IterateAllFieldsRecursively(value, basePath + "/" + fieldInfo.Name);
					continue;
				}
			}
		}

		#endregion

		#region All KernelObjects - Queries

		public TKernelObject Get<TKernelObject>(Ref instanceID, bool skipQuietlyIfDestroyed = false) where TKernelObject : KernelObject
		{
			if (AllKernelObjects.TryGetValue(instanceID.Value, out var instance))
			{
				// No need to check if instance is null. We already know any registered object does exist.
				// if (instance == null)

				// Check for type safety
				if (instance is TKernelObject cast)
				{
					if (!cast.ID.IsInvalid)
						return cast;

					if (!skipQuietlyIfDestroyed)
					{
						Log.CriticalError($"Queried a destroyed object '{cast.ToTypeAndIDString()}'.");
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

				if (!instance.ID.IsInvalid)
					return instance;

				if (!skipQuietlyIfDestroyed)
				{
					Log.CriticalError($"Queried a destroyed object '{instance.ToTypeAndIDString()}'.");
				}
				return null;
			}
			return null;
		}

		#endregion
	}

}
