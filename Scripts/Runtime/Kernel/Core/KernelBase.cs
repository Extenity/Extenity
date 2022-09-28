#if ExtenityKernel

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using Extenity.FlowToolbox;
using Extenity.JsonToolbox;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// TODO: Implement a build time tool to check and ensure there are no fields with KernelObject derived type. All references to kernel objects should use Ref instead.
	// TODO: Think of a way to provide behaviours for UI and Panels, just like ViewBehaviours.
	// TODO: Mark the kernel as RequiresCleanup (or something like it) whenever encountered a destroyed KernelObject and remove these from lists at the start of the application loop.

	public abstract class KernelBase<TKernel> : KernelBase
		where TKernel : KernelBase<TKernel>
	{
		#region Singleton

		private static TKernel _Instance;
		public static TKernel Instance => _Instance;
		public static bool IsInstanceAvailable => _Instance != null;

		#endregion

		#region Initialization

		public bool IsActive => _Instance == this;

		// TODO: Ensure initialization is called in all cases. (Deserialization, having another constructor in derived class, etc.)
		public void Activate()
		{
			if (IsActive)
			{
				throw new Exception($"Tried to initialize '{GetType().Name}' more than once.");
			}
			if (IsInstanceAvailable)
			{
				throw new Exception($"Tried to initialize '{GetType().Name}' while there was already an instance in use.");
			}

			_Instance = (TKernel)this;

			RegisterAllKernelObjectFieldsInKernel();
		}

		#endregion

		#region Deinitialization

		public void Deactivate()
		{
			if (!IsInstanceAvailable)
			{
				throw new Exception($"Tried to deinitialize '{GetType().Name}' but it was not active.");
			}
			if (!IsActive)
			{
				throw new Exception($"Tried to deinitialize '{GetType().Name}' but there was another singleton.");
			}

			_Instance = null;
		}

		#endregion

		#region Instantiate/Destroy KernelObject

		public TKernelObject Instantiate<TKernelObject>()
			where TKernelObject : KernelObject<TKernel>, new()
		{
			var instance = new TKernelObject();
			instance.SetID(IDGenerator.CreateID());
			Block.Register<TKernelObject>(instance);
			return instance;
		}

		public void Destroy<TKernelObject>([CanBeNull] TKernelObject instance)
			where TKernelObject : KernelObject<TKernel>
		{
			if (instance == null || instance.IsDestroyed)
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

		[ShowInInspector, ReadOnly]
		[PropertyOrder(70_0)] // Show it at the end
		public Block<TKernel> Block = new Block<TKernel>();

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

		public TKernelObject Get<TKernelObject>(Ref<TKernelObject, TKernel> instanceID, bool skipQuietlyIfDestroyed = false)
			where TKernelObject : KernelObject<TKernel>
		{
			return Block.Get(instanceID, skipQuietlyIfDestroyed);
		}

		public KernelObject Get(UInt32 instanceID, Type instanceType, bool skipQuietlyIfDestroyed = false)
		{
			return Block.Get(instanceID, instanceType, skipQuietlyIfDestroyed);
		}

		public bool Exists<TKernelObject>(Ref<TKernelObject, TKernel> instanceID)
			where TKernelObject : KernelObject<TKernel>
		{
			return Block.Exists(instanceID);
		}

		public TKernelObject FindSingleObjectOfType<TKernelObject>()
			where TKernelObject : KernelObject<TKernel>
		{
			return Block.GetSingle<TKernelObject>();
		}

		#endregion

		#region Serialization

		public string SerializeJson()
		{
			return JsonTools.SerializeCrosschecked(this, JsonTools.SerializerSettings);
		}

		public static TKernel DeserializeJson(string json, Action<JObject> upgradeMethod)
		{
			return JsonTools.DeserializeAndUpgradeCrosschecked<TKernel>(json, upgradeMethod, JsonTools.SerializerSettings);
		}

		public string SerializeJsonWithoutCrosscheck()
		{
			return JsonTools.SerializeObject(this, Formatting.Indented, JsonTools.SerializerSettings);
		}

		public static TKernel DeserializeJsonWithoutCrosscheck(string json, Action<JObject> upgradeMethod)
		{
			return JsonTools.DeserializeAndUpgradeObject<TKernel>(json, upgradeMethod, JsonTools.SerializerSettings);
		}

		#endregion

		#region Editor - Json Debug

#if UNITY_EDITOR

		[JsonIgnore]
		private Int64 _SerializedJsonLineCount = 0;

		[JsonIgnore]
		private string _SerializedJson
		{
			get
			{
				if (_SerializedJsonCache.IsTimeToProcess)
				{
					_SerializedJsonCache.CachedResult = SerializeJsonWithoutCrosscheck();
					_SerializedJsonLineCount = _SerializedJsonCache.CachedResult.CountLines();
				}
				return _SerializedJsonCache.CachedResult;
			}
		}

		[JsonIgnore]
		private readonly CachedPoller<string> _SerializedJsonCache = new CachedPoller<string>(1f, () => Time.realtimeSinceStartup);

		// private Vector2 _JsonScrollPosition;

		[OnInspectorGUI, FoldoutGroup("Debug - Json Serialized", false), HideLabel]
		private void _DrawSerializedJson()
		{
			var json = _SerializedJson;
			var lineCount = _SerializedJsonLineCount;
			var size = json.Length;

			GUILayout.Label($"Lines: {lineCount} \tSize: {size:N0} bytes");

			// _JsonScrollPosition = GUILayout.BeginScrollView(_JsonScrollPosition, GUILayout.MaxHeight(Screen.height * 0.6f));
			GUILayout.TextArea(json);
			// GUILayout.EndScrollView();
		}

#endif

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

#endif
