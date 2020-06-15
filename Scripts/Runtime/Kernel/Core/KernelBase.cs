using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
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
			where TKernelObject : KernelObject<TKernelObject, TKernel>, new()
		{
			var instance = new TKernelObject();
			instance.SetID(IDGenerator.CreateID());
			Block.Register<TKernelObject>(instance);
			return instance;
		}

		public void Destroy<TKernelObject>([CanBeNull] TKernelObject instance)
			where TKernelObject : KernelObject<TKernelObject, TKernel>
		{
			if (instance == null || instance.IsInvalid)
			{
				// TODO: Not sure what to do when received an invalid object.
				return;
			}

			instance.OnDestroy();
			Invalidate(instance.ID); // Invalidate the object one last time so any listeners can refresh themselves.
			Block.Deregister<TKernelObject>(instance);
			instance.ResetIDOnDestroy();
		}

		#endregion

		#region Data

		[NonSerialized, JsonIgnore]
		[ShowInInspector, ReadOnly]
		[PropertyOrder(70_0)] // Show it at the end
		public Block Block = new Block();

		#endregion

		#region Data - Gather And Register All Fields

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
					Block.Register(kernelObject, fieldType);
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

		#region Data - Queries

		public TKernelObject Get<TKernelObject>(Ref instanceID, bool skipQuietlyIfDestroyed = false)
			where TKernelObject : KernelObject
		{
			return Block.Get<TKernelObject>(instanceID, skipQuietlyIfDestroyed);
		}

		public KernelObject Get(Ref instanceID, Type instanceType, bool skipQuietlyIfDestroyed = false)
		{
			return Block.Get(instanceID, instanceType, skipQuietlyIfDestroyed);
		}

		public bool Exists<TKernelObject>(Ref instanceID)
			where TKernelObject : KernelObject
		{
			return Block.Exists<TKernelObject>(instanceID);
		}

		#endregion
	}

	public abstract class KernelBase
	{
		#region Versioning

		[NonSerialized] // Versioning is only used for callbacks and does not keep any data.
		public readonly Versioning Versioning = new Versioning();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(UInt32 id)
		{
			Versioning.Invalidate(id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InvalidateAllRegisteredIDs()
		{
			Versioning.InvalidateAllRegisteredIDs();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RegisterForVersionChanges(UInt32 id, Action callback, int order = 0)
		{
			Versioning.RegisterForVersionChanges(id, callback, order);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool DeregisterForVersionChanges(UInt32 id, Action callback)
		{
			return Versioning.DeregisterForVersionChanges(id, callback);
		}

		#endregion

		#region ID Generator

		[SerializeField]
		public IDGenerator IDGenerator = new IDGenerator();

		#endregion
	}

}
