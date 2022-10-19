#if UNITY

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Extenity.SceneManagementToolbox;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extenity.GameObjectToolbox
{

	public enum ActiveCheck
	{
		ActiveOnly = 0,
		IncludingInactive = 1,
		InactiveOnly = 2,
	}

	public enum SnapToGroundRotationOption
	{
		DontRotate,
		X = 1,
		Z = 2,
		ZX = 4,
	}

	// TODO: Check all methods and make sure if they need ActiveCheck as parameter OR if they need to be renamed to state that the method only works on active objects.
	public static class GameObjectTools
	{
		#region Create

		public static GameObject GetOrCreateGameObject(string name)
		{
			var go = GameObject.Find(name);
			if (go)
				return go;
			return new GameObject(name);
		}

		public static GameObject CreateGameObject(string name, Transform parent = null)
		{
			var go = new GameObject(name);
			if (parent)
			{
				go.transform.SetParent(parent, false);
			}
			go.transform.ResetTransformToLocalZero();
			return go;
		}

		public static GameObject CreateGameObject(string name, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			var go = new GameObject(name);
			if (parent)
			{
				go.transform.SetParent(parent, false);
			}
			go.transform.position = position;
			go.transform.rotation = rotation;
			go.transform.localScale = Vector3.one;
			return go;
		}

		public static void CreateOrModifyGameObject(ref GameObject gameObject, string name, Transform parent = null)
		{
			if (!gameObject)
			{
				gameObject = new GameObject(name);
			}
			else
			{
				gameObject.name = name;
			}
			gameObject.transform.SetParent(parent, false);
			gameObject.transform.ResetTransformToLocalZero();
		}

		public static void CreateOrModifyGameObject(ref GameObject gameObject, string name, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (!gameObject)
			{
				gameObject = new GameObject(name);
			}
			else
			{
				gameObject.name = name;
			}
			gameObject.transform.SetParent(parent, false);
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			gameObject.transform.localScale = Vector3.one;
		}

		#endregion

		#region Destroy Specials

		public static void DestroyAll<T>(this IList<T> list, float delay = 0f, HistorySaveType historySaveType = HistorySaveType.Save) where T : Object
		{
			for (int i = 0; i < list.Count; i++)
			{
				Destroy(list[i], delay, historySaveType);
			}
		}

		public static void DestroyAllImmediate<T>(this IList<T> list, bool allowDestroyingAssets = false, HistorySaveType historySaveType = HistorySaveType.Save) where T : Object
		{
			for (int i = 0; i < list.Count; i++)
			{
				DestroyImmediate(list[i], allowDestroyingAssets, historySaveType);
			}
		}

		public static void DestroyComponentThenGameObjectIfNoneLeft(Component component, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			if (!component)
				return;

			var gameObject = component.gameObject;
			if (gameObject.HasNoComponentAndNoChild())
			{
				Destroy(gameObject, historySaveType);
			}
			else
			{
				Destroy(component, historySaveType);
			}
		}

		public static void DestroyImmediateComponentThenGameObjectIfNoneLeft(Component component, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			if (!component)
				return;

			var gameObject = component.gameObject;
			if (gameObject.HasNoComponentAndNoChild())
			{
				DestroyImmediate(gameObject, historySaveType);
			}
			else
			{
				DestroyImmediate(component, historySaveType);
			}
		}

		#endregion

		#region Destroy With History

		public struct DestroyHistoryItem
		{
			// Simple log
			public string ObjectName;
			public bool IsImmediate;
			public bool IsImmediateAllowsDestroyingAssets;
			public float DestroyDelay;

			// Detailed log (that's costly)
			public bool IsDetailedLog { get { return DestroyTime.Ticks != 0; } }
			public string Type;
			public string BaseType;
			public string ObjectPathInScene;
			public DateTime DestroyTime;
			public StackFrame[] StackTraceFrames;
		}

		public enum HistorySaveType
		{
			/// <summary>
			/// History won't be saved for this Destroy call.
			/// </summary>
			DontSave,
			/// <summary>
			/// History will be saved for this Destroy call if IsDestroyHistoryEnabled set. Whether if it saved with details is up to  IsDetailedDestroyHistoryEnabled.
			/// </summary>
			Save,
			/// <summary>
			/// History will be saved for this Destroy call if IsDestroyHistoryEnabled set. It will always be saved without details regardless of what IsDetailedDestroyHistoryEnabled says.
			/// </summary>
			SaveSimple,
			/// <summary>
			/// History will be saved for this Destroy call if IsDestroyHistoryEnabled set. It will always be saved with details regardless of what IsDetailedDestroyHistoryEnabled says.
			/// </summary>
			SaveDetailed,
		}

		public static List<DestroyHistoryItem> DestroyHistory = new List<DestroyHistoryItem>(1000);

		public static bool IsDestroyHistoryEnabled = false;
		public static bool IsDetailedDestroyHistoryEnabled = false;
		public static bool KeepDestroyHistoryOnSoftReset
		{
			get => true;
			set
			{
				// Might be a good idea to have the ability to reset history with application soft reset.
				throw new NotImplementedException();
			}
		}

		private static void InternalDestroy(Object obj)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) // Use DestroyImmediate in edit mode.
			{
				const bool allowDestroyingAssets = false; // Destroying assets should only be done manually calling DestroyImmediate in a controlled environment.
				Object.DestroyImmediate(obj, allowDestroyingAssets);
				return;
			}
#endif
			Object.Destroy(obj);
		}

		private static void InternalDestroy(Object obj, float delay)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) // Use DestroyImmediate in edit mode.
			{
				const bool allowDestroyingAssets = false; // Destroying assets should only be done manually calling DestroyImmediate in a controlled environment.
				Object.DestroyImmediate(obj, allowDestroyingAssets);
				return;
			}
#endif
			Object.Destroy(obj, delay);
		}

		public static void Destroy(Object obj, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			const float delay = 0f;
			InternalDestroy(obj);

			if (IsDestroyHistoryEnabled && historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, false, false, delay, historySaveType);
		}

		public static void Destroy(Object obj, float delay, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			InternalDestroy(obj, delay);

			if (IsDestroyHistoryEnabled && historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, false, false, delay, historySaveType);
		}

		public static void DestroyImmediate(Object obj, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			const bool allowDestroyingAssets = false;
			Object.DestroyImmediate(obj);

			if (IsDestroyHistoryEnabled && historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, true, allowDestroyingAssets, 0f, historySaveType);
		}

		public static void DestroyImmediate(Object obj, bool allowDestroyingAssets, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			Object.DestroyImmediate(obj, allowDestroyingAssets);

			if (IsDestroyHistoryEnabled && historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, true, allowDestroyingAssets, 0f, historySaveType);
		}

		private static void _CreateDestroyHistoryItem(Object obj, bool isImmediate, bool allowDestroyingAssets, float destroyDelay, HistorySaveType historySaveType)
		{
			// if (!IsDestroyHistoryEnabled)
			// 	return;

			var item = new DestroyHistoryItem();
			item.ObjectName = obj.name;
			item.IsImmediate = isImmediate;
			item.IsImmediateAllowsDestroyingAssets = allowDestroyingAssets;
			item.DestroyDelay = destroyDelay;

			var saveDetails = historySaveType == HistorySaveType.SaveDetailed ||
			                  (IsDetailedDestroyHistoryEnabled && historySaveType == HistorySaveType.Save);

			if (saveDetails)
			{
				item.DestroyTime = DateTime.Now;
				item.Type = obj.GetType().Name;
				item.StackTraceFrames = new StackTrace().GetFrames();

				GameObject objAsGameObject;
				Component objAsComponent;
				if ((objAsGameObject = obj as GameObject) != null)
				{
					item.BaseType = "GameObject";
					item.ObjectPathInScene = objAsGameObject.FullName();
				}
				else if ((objAsComponent = obj as Component) != null)
				{
					item.BaseType = "Component";
					item.ObjectPathInScene = objAsComponent.FullName();
				}
				else if (obj is ScriptableObject)
				{
					item.BaseType = "ScriptableObject";
					item.ObjectPathInScene = "[N/A]";
				}
				else
				{
					item.BaseType = "[Unknown]";
					item.ObjectPathInScene = "[N/A]";
				}
			}

			DestroyHistory.Add(item);
		}

		#endregion

		#region Create Primitive

		public static GameObject CreatePrimitive(PrimitiveType primitiveType, string gameObjectName, bool createCollider, Transform parent = null)
		{
			var go = GameObject.CreatePrimitive(primitiveType);
			if (!createCollider)
			{
				DestroyImmediate(go.GetComponent<Collider>());
			}
			go.name = gameObjectName;
			if (parent)
			{
				go.transform.parent = parent;
			}
			return go;
		}

		#endregion

		#region Recursive Changes - Layers

		public static void ChangeLayersRecursively(this GameObject gameObject, string layerName)
		{
			ChangeLayersRecursively(gameObject, LayerMask.NameToLayer(layerName));
		}

		public static void ChangeLayersRecursively(this GameObject gameObject, int layer)
		{
			gameObject.layer = layer;

			var transform = gameObject.transform;
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				ChangeLayersRecursively(transform.GetChild(i).gameObject, layer);
			}
		}

		#endregion

		#region Recursive Changes - Renderers

		public static void EnableRenderersRecursively(this GameObject gameObject)
		{
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer)
				renderer.enabled = true;

			var transform = gameObject.transform;
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				EnableRenderersRecursively(transform.GetChild(i).gameObject);
			}
		}

		public static void DisableRenderersRecursively(this GameObject gameObject)
		{
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer)
				renderer.enabled = false;

			var transform = gameObject.transform;
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				DisableRenderersRecursively(transform.GetChild(i).gameObject);
			}
		}

		public static void SetRendererColorsRecursively(this GameObject gameObject, Color color)
		{
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer)
				renderer.material.color = color;

			var transform = gameObject.transform;
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				SetRendererColorsRecursively(transform.GetChild(i).gameObject, color);
			}
		}

		public delegate Material AssignNewMaterialMethod(Renderer processingRenderer, int processingMaterialIndex);

		public delegate Color AssignNewColorMethod(Renderer processingRenderer, int processingMaterialIndex);

		public static int ChangeAllChildRendererMaterials(this GameObject gameObject, AssignNewMaterialMethod assignNewMaterialMethod, AssignNewColorMethod assignNewColorMethod = null)
		{
			var processedMaterialCount = 0;
			var renderers = gameObject.GetComponentsInChildren<Renderer>();
			if (renderers != null && renderers.Length > 0)
			{
				for (int iRenderer = 0; iRenderer < renderers.Length; iRenderer++)
				{
					var renderer = renderers[iRenderer];
					var materials = renderer.materials;
					for (int iMaterial = 0; iMaterial < materials.Length; iMaterial++)
					{
						var oldMaterial = materials[iMaterial];
						var newMaterial = assignNewMaterialMethod(renderer, iMaterial);

						// Assign main texture
						newMaterial.mainTexture = oldMaterial.mainTexture;

						// Assign color
						if (assignNewColorMethod != null)
						{
							newMaterial.color = assignNewColorMethod(renderer, iMaterial);
						}

						materials[iMaterial] = newMaterial;
						processedMaterialCount++;
					}
					renderer.materials = materials;
				}
			}
			return processedMaterialCount;
		}

		#endregion

		#region Recursive Calculations - Renderer / Mesh / Collider Bounds

		public static bool CalculateRendererWorldBoundsRecursively(this GameObject go, out Bounds bounds)
		{
			bounds = new Bounds();
			var renderers = go.GetComponentsInChildren<Renderer>();
			if (renderers == null || renderers.Length == 0)
			{
				return false;
			}

			var isInitialized = false;
			for (int i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];
				if (renderer)
				{
					if (!isInitialized)
					{
						bounds = renderer.bounds;
						isInitialized = true;
					}
					else
					{
						bounds.Encapsulate(renderer.bounds);
					}
				}
			}
			return isInitialized;
		}

		public static bool CalculateMeshLocalBoundsRecursively(this GameObject go, out Bounds bounds)
		{
			bounds = new Bounds();
			var baseTransform = go.transform;
			var meshFilters = go.GetComponentsInChildren<MeshFilter>();
			if (meshFilters == null || meshFilters.Length == 0)
			{
				return false;
			}

			var isInitialized = false;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				var meshFilter = meshFilters[i];
				var sharedMesh = meshFilter ? meshFilter.sharedMesh : null;
				if (sharedMesh)
				{
					if (!isInitialized)
					{
						bounds = meshFilter.transform.TransformBounds(sharedMesh.bounds, baseTransform);
						isInitialized = true;
					}
					else
					{
						bounds.Encapsulate(meshFilter.transform.TransformBounds(sharedMesh.bounds, baseTransform));
					}
				}
			}
			return isInitialized;
		}

		public static bool CalculateMeshWorldBoundsRecursively(this GameObject go, out Bounds bounds)
		{
			bounds = new Bounds();
			var meshFilters = go.GetComponentsInChildren<MeshFilter>();
			if (meshFilters == null || meshFilters.Length == 0)
			{
				return false;
			}

			var isInitialized = false;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				var meshFilter = meshFilters[i];
				var sharedMesh = meshFilter ? meshFilter.sharedMesh : null;
				if (sharedMesh)
				{
					if (!isInitialized)
					{
						bounds = meshFilter.transform.TransformBounds(sharedMesh.bounds);
						isInitialized = true;
					}
					else
					{
						bounds.Encapsulate(meshFilter.transform.TransformBounds(sharedMesh.bounds));
					}
				}
			}
			return isInitialized;
		}

		public static bool CalculateColliderWorldBoundsRecursively(this GameObject go, out Bounds bounds)
		{
			bounds = new Bounds();
			var colliders = go.GetComponentsInChildren<Collider>();
			if (colliders == null || colliders.Length == 0)
			{
				return false;
			}

			var isInitialized = false;
			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];
				if (collider)
				{
					if (!isInitialized)
					{
						bounds = collider.bounds;
						isInitialized = true;
					}
					else
					{
						bounds.Encapsulate(collider.bounds);
					}
				}
			}
			return isInitialized;
		}

		#endregion

		#region IsChildOf / IsParentOf / IsSiblingComponent / HasComponent / IsEmpty

		public static bool IsChildOf(this Transform me, Transform parent, bool checkSelf = true)
		{
			if (!me)
				throw new ArgumentNullException(nameof(me));
			if (!parent)
				throw new ArgumentNullException(nameof(parent));

			if (checkSelf)
			{
				if (me == parent)
					return true;
			}

			var realParent = me.parent;
			while (realParent)
			{
				if (realParent == parent)
				{
					return true;
				}
				realParent = realParent.parent;
			}
			return false;
		}

		/// <summary>
		/// Tells if any one of the transforms in list happens to be a child of 'parent'.
		/// </summary>
		public static bool IsAnyTransformChildOf(this Transform[] list, Transform parent, bool checkSelf = true)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (!parent)
				throw new ArgumentNullException(nameof(parent));

			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].IsChildOf(parent, checkSelf))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Tells if all transforms in list happens to be a child of 'parent'.
		/// </summary>
		public static bool IsAllTransformsChildOf(this Transform[] list, Transform parent, bool checkSelf = true)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (!parent)
				throw new ArgumentNullException(nameof(parent));

			for (int i = 0; i < list.Length; i++)
			{
				if (!list[i].IsChildOf(parent, checkSelf))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsParentOf(this Transform me, Transform child, bool checkSelf = true)
		{
			if (!me)
				throw new ArgumentNullException(nameof(me));
			if (!child)
				throw new ArgumentNullException(nameof(child));

			return child.IsChildOf(me, checkSelf);
		}

		/// <summary>
		/// CAUTION! This is a performance heavy method because it uses GetComponents. Use it wisely.
		/// </summary>
		public static bool IsSiblingComponent(this Component me, Component other)
		{
			if (!me)
				throw new ArgumentNullException(nameof(me));
			if (!other)
				return false;

			var components = me.transform.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] == other)
					return true;
			}
			return false;
		}

		public static bool HasComponent(this GameObject gameObject, Component component)
		{
			if (!gameObject)
				throw new ArgumentNullException(nameof(gameObject));
			if (!component)
				return false;

			return component.gameObject == gameObject;
		}

		public static bool HasAnyComponentOfType<TComponent>(this GameObject gameObject)
			where TComponent : Component
		{
			// if (!gameObject) Won't be doing null check in here for performance reasons. It's caller's responsibility.
			// 	throw new ArgumentNullException(nameof(gameObject));

			return gameObject.TryGetComponent<TComponent>(out _);
		}

		public static bool HasAnyComponentOfType<TComponent1, TComponent2>(this GameObject gameObject)
			where TComponent1 : Component
			where TComponent2 : Component
		{
			// if (!gameObject) Won't be doing null check in here for performance reasons. It's caller's responsibility.
			// 	throw new ArgumentNullException(nameof(gameObject));

			return gameObject.TryGetComponent<TComponent1>(out _) ||
			       gameObject.TryGetComponent<TComponent2>(out _);
		}

		public static bool HasAnyComponentOfType<TComponent1, TComponent2, TComponent3>(this GameObject gameObject)
			where TComponent1 : Component
			where TComponent2 : Component
			where TComponent3 : Component
		{
			// if (!gameObject) Won't be doing null check in here for performance reasons. It's caller's responsibility.
			// 	throw new ArgumentNullException(nameof(gameObject));

			return gameObject.TryGetComponent<TComponent1>(out _) ||
			       gameObject.TryGetComponent<TComponent2>(out _) ||
			       gameObject.TryGetComponent<TComponent3>(out _);
		}

		public static bool HasAnyComponentOfTypeInChildren<TComponent>(this GameObject gameObject, bool includeSelf, bool includeInactive)
			where TComponent : Component
		{
			// if (!gameObject) Won't be doing null check in here for performance reasons. It's caller's responsibility.
			// 	throw new ArgumentNullException(nameof(gameObject));

			if (includeSelf && gameObject.HasAnyComponentOfType<TComponent>())
			{
				return true;
			}

			return gameObject.GetComponentInChildren<TComponent>(includeInactive) != null;
		}


		public static bool HasAnyComponentOfTypeInChildren<TComponent1, TComponent2>(this GameObject gameObject, bool includeSelf, bool includeInactive)
			where TComponent1 : Component
			where TComponent2 : Component
		{
			// if (!gameObject) Won't be doing null check in here for performance reasons. It's caller's responsibility.
			// 	throw new ArgumentNullException(nameof(gameObject));

			if (includeSelf && gameObject.HasAnyComponentOfType<TComponent1, TComponent2>())
			{
				return true;
			}

			return gameObject.GetComponentInChildren<TComponent1>(includeInactive) != null ||
			       gameObject.GetComponentInChildren<TComponent2>(includeInactive) != null;
		}

		public static bool HasAnyComponentOfTypeInChildren<TComponent1, TComponent2, TComponent3>(this GameObject gameObject, bool includeSelf, bool includeInactive)
			where TComponent1 : Component
			where TComponent2 : Component
			where TComponent3 : Component
		{
			// if (!gameObject) Won't be doing null check in here for performance reasons. It's caller's responsibility.
			// 	throw new ArgumentNullException(nameof(gameObject));

			if (includeSelf && gameObject.HasAnyComponentOfType<TComponent1, TComponent2, TComponent3>())
			{
				return true;
			}

			return gameObject.GetComponentInChildren<TComponent1>(includeInactive) != null ||
			       gameObject.GetComponentInChildren<TComponent2>(includeInactive) != null ||
			       gameObject.GetComponentInChildren<TComponent3>(includeInactive) != null;
		}

		/// <summary>
		/// Check if component is inside objects list. Also check if any gameobject in objects list has the component attached to itself.
		/// </summary>
		public static bool ContainsComponentAsIsOrAttachedToGameObject(this IEnumerable<Object> objects, Component component)
		{
			if (objects == null)
				return false;
			if (!component)
				return false;

			foreach (var obj in objects)
			{
				// Check if the component is in the list as it is.
				if (obj == component)
					return true;

				// Check if the component is attached to a gameobject.
				var gameObject = obj as GameObject;
				if (gameObject) // Ignore if not a gameobject
				{
					if (gameObject.HasComponent(component))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool HasNoComponentAndNoChild(this Transform transform)
		{
			if (!transform)
				return false;
			return
				transform.childCount == 0 && // No children
				transform.GetComponentCount<Component>() == 1; // Only contains Transform component, which is bare minimum
		}

		public static bool HasNoComponentAndNoChild(this GameObject gameObject)
		{
			if (!gameObject)
				return false;
			return
				gameObject.transform.childCount == 0 && // No children
				gameObject.GetComponentCount<Component>() == 1; // Only contains Transform component, which is bare minimum
		}

		#endregion

		#region GetComponentInParent

		public static T GetComponentInParent<T>(this Transform me, bool includeSelf, bool includeInactive) where T : Component
		{
			return me.gameObject.GetComponentInParent<T>(includeSelf, includeInactive);
		}

		// TODO: Change 'includeInactive' to ActiveCheck
		// TODO: Write tests (including all ActiveCheck combinations)
		public static T GetComponentInParent<T>(this GameObject me, bool includeSelf, bool includeInactive) where T : Component
		{
			if (!me)
				return null;

			if (!includeInactive && !me.activeInHierarchy)
				return null;

			if (includeSelf)
			{
				if (me.TryGetComponent<T>(out var component))
					return component;
			}

			var parent = me.transform.parent;

			while (parent)
			{
				if (parent.TryGetComponent<T>(out var component))
					return component;

				parent = parent.parent;
			}
			return null;
		}

		#endregion

		#region GetComponentInChildren

		public static T GetComponentInChildrenRecursiveWithoutActiveCheckExcludingThis<T>(this Transform me) where T : Component
		{
			if (!me)
				return null;

			var childCount = me.childCount;
			for (int i = 0; i < childCount; i++)
			{
				T componentInChildren = me.GetChild(i).GetComponentInChildrenRecursiveWithoutActiveCheck<T>();
				if (componentInChildren)
					return componentInChildren;
			}
			return null;
		}

		public static T GetComponentInChildrenRecursiveWithoutActiveCheck<T>(this Transform me) where T : Component
		{
			if (!me)
				return null;

			//if (this.activeInHierarchy)
			{
				T component = me.GetComponent<T>();
				if (component)
					return component;
			}

			var childCount = me.childCount;
			for (int i = 0; i < childCount; i++)
			{
				T componentInChildren = me.GetChild(i).GetComponentInChildrenRecursiveWithoutActiveCheck<T>();
				if (componentInChildren)
					return componentInChildren;
			}
			return null;
		}

		#endregion

		#region Find by path

		public static T FindComponentByGameObjectPath<T>(this IEnumerable<T> components, string expectedPath) where T : Component
		{
			return components?.FirstOrDefault(component => component.gameObject.FullName() == expectedPath);
		}

		public static T FindComponentByComponentPath<T>(this IEnumerable<T> components, string expectedPath) where T : Component
		{
			return components?.FirstOrDefault(component => component.FullName() == expectedPath);
		}

		public static List<T> FindComponentsByGameObjectPath<T>(this IEnumerable<T> components, string expectedPath) where T : Component
		{
			return components?.Where(component => component.gameObject.FullName() == expectedPath).ToList();
		}

		public static List<T> FindComponentsByComponentPath<T>(this IEnumerable<T> components, string expectedPath) where T : Component
		{
			return components?.Where(component => component.FullName() == expectedPath).ToList();
		}

		public static T FindSingleComponentByGameObjectPath<T>(this IEnumerable<T> components, string expectedPath) where T : Component
		{
			var found = components.FindComponentsByGameObjectPath(expectedPath);
			if (found.Count != 1)
			{
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{found.Count}' available at path '{expectedPath}'.");
			}
			return found[0];
		}

		public static T FindSingleComponentByComponentPath<T>(this IEnumerable<T> components, string expectedPath) where T : Component
		{
			var found = components.FindComponentsByComponentPath(expectedPath);
			if (found.Count != 1)
			{
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{found.Count}' available at path '{expectedPath}'.");
			}
			return found[0];
		}

		#endregion

		#region Find child(ren) by custom rule

		public delegate bool CustomRuleDelegate(Transform transform);

		public static Transform FindChildByCustomRule(this Transform me, CustomRuleDelegate customRuleDelegate)
		{
			if (customRuleDelegate(me))
				return me;
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				var found = child.FindChildByCustomRule(customRuleDelegate);
				if (found)
					return found;
			}
			return null;
		}

		public static void FindChildrenByCustomRuleRecursive(this Transform me, CustomRuleDelegate customRuleDelegate, IList<Transform> list)
		{
			if (customRuleDelegate(me))
			{
				list.Add(me);
			}
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				child.FindChildrenByCustomRuleRecursive(customRuleDelegate, list);
			}
		}

		#endregion

		#region Find child(ren) by name recursive

		public static Transform FindChildByNameRecursive(this Transform me, string value)
		{
			if (me.name == value)
				return me;
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				var found = child.FindChildByNameRecursive(value);
				if (found)
					return found;
			}
			return null;
		}

		public static void FindChildrenByNameRecursive(this Transform me, string value, IList<Transform> list)
		{
			if (me.name == value)
			{
				list.Add(me);
			}
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				child.FindChildrenByNameRecursive(value, list);
			}
		}

		#endregion

		#region Find child(ren) by prefix (and ordered)

		public static Transform FindChildByPrefix(this Transform me, string value)
		{
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				if (child.name.StartsWith(value))
					return child;
			}
			return null;
		}

		public static List<Transform> FindChildrenByPrefix(this Transform me, string value)
		{
			var childrenList = new List<Transform>();
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				if (child.name.StartsWith(value))
					childrenList.Add(child);
			}
			return childrenList;
		}

		public static List<Transform> FindChildrenByPrefixOrdered(this Transform me, string value)
		{
			var childrenList = me.FindChildrenByPrefix(value);

			childrenList.Sort(
				delegate(Transform p1, Transform p2)
				{
					int index1 = int.Parse(p1.name.Remove(0, value.Length));
					int index2 = int.Parse(p2.name.Remove(0, value.Length));
					return index1 - index2;
				});

			return childrenList;
		}

		#endregion

		#region Find child(ren) by suffix

		public static Transform FindChildBySuffix(this Transform me, string value)
		{
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				if (child.name.EndsWith(value))
					return child;
			}
			return null;
		}

		public static List<Transform> FindChildrenBySuffix(this Transform me, string value)
		{
			var childrenList = new List<Transform>();
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				if (child.name.EndsWith(value))
					childrenList.Add(child);
			}
			return childrenList;
		}

		#endregion

		#region Find child(ren) by name-contains

		public static Transform FindChildByNameContains(this Transform me, string value)
		{
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				if (child.name.Contains(value))
					return child;
			}
			return null;
		}

		public static List<Transform> FindChildrenByNameContains(this Transform me, string value)
		{
			var childrenList = new List<Transform>();
			for (int i = 0; i < me.childCount; i++)
			{
				var child = me.GetChild(i);
				if (child.name.Contains(value))
					childrenList.Add(child);
			}
			return childrenList;
		}

		#endregion

		#region FindObjectsOfTypeAll

		public static List<T> FindObjectsOfTypeInScenes<T>(ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			return SceneManagerTools.GetScenes(sceneListFilter)
			                        .FindObjectsOfType<T>(activeCheck);
		}

		public static List<T> FindObjectsOfType<T>(this IList<Scene> scenes, ActiveCheck activeCheck) where T : Component
		{
			var results = new List<T>();
			for (var i = 0; i < scenes.Count; i++)
			{
				var list = scenes[i].FindObjectsOfType<T>(activeCheck);
				results.AddRange(list);
			}
			return results;
		}

		public static List<T> FindObjectsOfType<T>(this Scene scene, ActiveCheck activeCheck) where T : Component
		{
			var unityReportedComponents = new List<T>();
			var results = new List<T>();
			var rootGameObjects = new List<GameObject>(scene.rootCount);
			scene.GetRootGameObjects(rootGameObjects);

			switch (activeCheck)
			{
				case ActiveCheck.ActiveOnly:
				{
					for (int i = 0; i < rootGameObjects.Count; i++)
					{
						if (!rootGameObjects[i] || !rootGameObjects[i].activeSelf)
							continue;
						rootGameObjects[i].GetComponentsInChildren(false, unityReportedComponents);
						for (var iComponent = 0; iComponent < unityReportedComponents.Count; iComponent++)
						{
							var component = unityReportedComponents[iComponent];
							if (!component)
								continue;
							if (!component.GetType().IsSameOrSubclassOf(typeof(T)))
								continue;

							var componentEnabled = component.IsComponentEnabled(true);
							var gameObjectEnabled = component.gameObject.activeInHierarchy;

							if (gameObjectEnabled && componentEnabled)
							{
								results.Add(component);
							}
						}
						unityReportedComponents.Clear();
					}
				}
					break;

				case ActiveCheck.IncludingInactive:
				{
					for (int i = 0; i < rootGameObjects.Count; i++)
					{
						if (!rootGameObjects[i])
							continue;
						rootGameObjects[i].GetComponentsInChildren(true, unityReportedComponents);
						for (var iComponent = 0; iComponent < unityReportedComponents.Count; iComponent++)
						{
							var component = unityReportedComponents[iComponent];
							if (!component)
								continue;
							if (!component.GetType().IsSameOrSubclassOf(typeof(T)))
								continue;

							results.Add(component);
						}
						unityReportedComponents.Clear();
					}
				}
					break;

				case ActiveCheck.InactiveOnly:
				{
					for (int i = 0; i < rootGameObjects.Count; i++)
					{
						if (!rootGameObjects[i])
							continue;
						rootGameObjects[i].GetComponentsInChildren(true, unityReportedComponents);
						for (var iComponent = 0; iComponent < unityReportedComponents.Count; iComponent++)
						{
							var component = unityReportedComponents[iComponent];
							if (!component)
								continue;
							if (!component.GetType().IsSameOrSubclassOf(typeof(T)))
								continue;

							var componentEnabled = component.IsComponentEnabled(true);
							var gameObjectEnabled = component.gameObject.activeInHierarchy;

							if (!gameObjectEnabled || !componentEnabled)
							{
								results.Add(component);
							}
						}
						unityReportedComponents.Clear();
					}
				}
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(activeCheck), activeCheck, null);
			}

			return results;
		}

		#endregion

		#region FindObjectOfTypeEnsured and FindSingleObjectOfTypeEnsured

		public static object FindObjectOfTypeEnsured(Type type)
		{
			var obj = Object.FindObjectOfType(type);
			if (!obj)
				throw new Exception($"Could not find object of type '{type.Name}'.");
			return obj;
		}

		public static T FindObjectOfTypeEnsured<T>() where T : class
		{
			var type = typeof(T);
			var obj = Object.FindObjectOfType(type);
			if (!obj)
				throw new Exception($"Could not find object of type '{type.Name}'.");
			return obj as T;
		}

		public static object FindSingleObjectOfTypeEnsured(Type type)
		{
			var results = Object.FindObjectsOfType(type);
			var count = results?.Length ?? 0;
			if (count != 1)
				throw new Exception($"Expected single '{type.Name}' whereas '{count}' available.");
			// ReSharper disable once PossibleNullReferenceException
			return results[0];
		}

		public static T FindSingleObjectOfTypeEnsured<T>() where T : class
		{
			var type = typeof(T);
			var results = Object.FindObjectsOfType(type);
			var count = results?.Length ?? 0;
			if (count != 1)
				throw new Exception($"Expected single '{type.Name}' whereas '{count}' available.");
			return results[0] as T;
		}

		#endregion

		#region FindObjectWithTagEnsured and FindSingleObjectWithTagEnsured

		public static GameObject FindObjectWithTagEnsured(string tag)
		{
			var obj = GameObject.FindWithTag(tag);
			if (!obj)
				throw new Exception($"Could not find object with tag '{tag}'.");
			return obj;
		}

		public static GameObject FindSingleObjectWithTagEnsured(string tag)
		{
			var results = GameObject.FindGameObjectsWithTag(tag);
			var count = results?.Length ?? 0;
			if (count != 1)
				throw new Exception($"Expected single object with tag '{tag}' whereas '{count}' available.");
			// ReSharper disable once PossibleNullReferenceException
			return results[0];
		}

		#endregion

		#region FindChildEnsured

		public static Transform FindChildEnsured(this Transform me, string name)
		{
			var component = me.Find(name);
			if (!component)
				throw new Exception($"Could not find child '{name}' of '{me.FullGameObjectName()}'.");
			return component;
		}

		#endregion

		#region Get Children

		public static List<Transform> GetChildren(this Transform parent)
		{
			var list = new List<Transform>();
			for (int i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);
				list.Add(child);
			}
			return list;
		}

		#endregion

		#region ListAllChildrenGameObjects

		public static List<GameObject> ListAllGameObjectsInActiveScene()
		{
			return SceneManager.GetActiveScene().ListAllGameObjectsInScene();
		}

		public static List<GameObject> ListAllGameObjectsInScene(this Scene scene)
		{
			var list = new List<GameObject>();
			scene.GetRootGameObjects().ListAllChildrenGameObjects(list, true);
			return list;
		}

		public static void ListAllChildrenGameObjects(this IEnumerable<GameObject> parentGameObjects, List<GameObject> list, bool includingTheParent)
		{
			if (parentGameObjects == null)
				return;
			foreach (var parentGameObject in parentGameObjects)
			{
				if (includingTheParent)
				{
					list.Add(parentGameObject);
				}
				InternalListAllChildrenGameObjects(parentGameObject.transform, list);
			}
		}

		public static void ListAllChildrenGameObjects(this GameObject parentGameObject, List<GameObject> list, bool includingTheParent)
		{
			if (!parentGameObject)
				return;
			if (includingTheParent)
			{
				list.Add(parentGameObject);
			}
			InternalListAllChildrenGameObjects(parentGameObject.transform, list);
		}

		private static void InternalListAllChildrenGameObjects(Transform transform, List<GameObject> list)
		{
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				var child = transform.GetChild(i);
				list.Add(child.gameObject);
				InternalListAllChildrenGameObjects(child, list);
			}
		}

		#endregion

		#region Iterate Children

		public delegate void ChildHandler(Transform child);

		public static void ForeachChildren(this Transform transform, ChildHandler childHandler, bool recursive)
		{
			InternalIterateChildren(transform, childHandler, recursive);
		}

		private static void InternalIterateChildren(Transform transform, ChildHandler childHandler, bool recursive)
		{
			if (!transform)
				throw new ArgumentNullException(nameof(transform));
			var childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				var child = transform.GetChild(i);
				childHandler(child);
				if (recursive)
					InternalIterateChildren(child, childHandler, true);
			}
		}

		#endregion

		#region GetParentEnsured

		public static Transform GetParentEnsured(this Transform me)
		{
			var parent = me.parent;
			if (!parent)
			{
				throw new Exception($"Could not get parent of '{me.FullGameObjectName()}'.");
			}
			return parent;
		}

		#endregion

		#region DetachChildrenRecursive

		public static void DetachChildren(this Transform parent, ref int detachedObjectCount, bool worldPositionStays = true)
		{
			var childCount = parent.childCount;
			if (childCount == 0)
				return;

			for (int i = childCount - 1; i >= 0; i--)
			{
				parent.GetChild(i).SetParent(null, worldPositionStays);
				detachedObjectCount++;
			}
		}

		public static void DetachChildrenRecursive(this Transform parent, ref int detachedObjectCount, bool worldPositionStays = true)
		{
			var childCount = parent.childCount;
			if (childCount == 0)
				return;

			for (int i = childCount - 1; i >= 0; i--)
			{
				parent.GetChild(i).DetachChildrenRecursive(ref detachedObjectCount, worldPositionStays);
			}
			parent.DetachChildren(ref detachedObjectCount, worldPositionStays);
		}

		#endregion

		#region SetParentSorted

		public static void SetParentSortedByName(this Transform me, Transform parent)
		{
			if (parent.childCount == 0)
			{
				me.parent = parent;
			}
			else
			{
				var childIndex = -1;
				var childCount = parent.childCount;
				var name = me.name;
				for (int i = 0; i < childCount; i++)
				{
					var child = parent.GetChild(i);
					if (string.Compare(child.name, name, StringComparison.Ordinal) > 0)
					{
						childIndex = i;
						break;
					}
				}
				me.parent = parent;
				if (childIndex >= 0)
					me.SetSiblingIndex(childIndex);
			}
		}

		#endregion

		#region GetComponentEnsured

		public static T GetSingleComponentEnsured<T>(this Component me) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.gameObject.InternalGetSingleComponentEnsured<T>();
		}

		public static T GetSingleComponentEnsured<T>(this GameObject me) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.InternalGetSingleComponentEnsured<T>();
		}

		public static T GetSingleComponentInChildrenEnsured<T>(this Component me, bool includeInactive) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.gameObject.InternalGetSingleComponentInChildrenEnsured<T>(includeInactive);
		}

		public static T GetSingleComponentInChildrenEnsured<T>(this GameObject me, bool includeInactive) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.InternalGetSingleComponentInChildrenEnsured<T>(includeInactive);
		}

		private static T InternalGetSingleComponentEnsured<T>(this GameObject me) where T : Component
		{
			var results = New.List<T>();
			me.GetComponents(results);
			if (results.Count != 1)
			{
				Release.List(ref results);
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{results.Count}' available in '{me.FullName()}'.");
			}
			var result = results[0];
			Release.List(ref results);
			return result;
		}

		private static T InternalGetSingleComponentInChildrenEnsured<T>(this GameObject me, bool includeInactive) where T : Component
		{
			var results = New.List<T>();
			me.GetComponentsInChildren(includeInactive, results);
			if (results.Count != 1)
			{
				Release.List(ref results);
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{results.Count}' available in '{me.FullName()}'.");
			}
			var result = results[0];
			Release.List(ref results);
			return result;
		}

		#endregion

		#region GetOrAddComponent

		public static T GetSingleOrAddComponent<T>(this Component me) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.gameObject.InternalGetSingleOrAddComponent<T>();
		}

		public static T GetSingleOrAddComponent<T>(this GameObject me) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.InternalGetSingleOrAddComponent<T>();
		}

		private static T InternalGetSingleOrAddComponent<T>(this GameObject me) where T : Component
		{
			var results = New.List<T>();
			me.GetComponents(results);
			if (results.Count == 0)
			{
				Release.List(ref results);
				return me.AddComponent<T>();
			}
			if (results.Count > 1)
			{
				Release.List(ref results);
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{results.Count}' available in '{me.FullName()}'.");
			}
			var result = results[0];
			Release.List(ref results);
			return result;
		}

		public static T GetFirstOrAddComponent<T>(this Component me) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.gameObject.InternalGetFirstOrAddComponent<T>();
		}

		public static T GetFirstOrAddComponent<T>(this GameObject me) where T : Component
		{
			if (!me)
			{
				throw new Exception($"Tried to get component '{typeof(T).Name}' of a null object.");
			}
			return me.InternalGetFirstOrAddComponent<T>();
		}

		private static T InternalGetFirstOrAddComponent<T>(this GameObject me) where T : Component
		{
			var results = New.List<T>();
			me.GetComponents(results);
			if (results.Count == 0)
			{
				Release.List(ref results);
				return me.AddComponent<T>();
			}
			var result = results[0];
			Release.List(ref results);
			return result;
		}

		#endregion

		#region Instantiate GameObject

		public static GameObject Instantiate(GameObject original, Transform parent, bool setTransformToLocalZero)
		{
			var go = GameObject.Instantiate(original);
			var transform = go.transform;
			transform.SetParent(parent);
			if (setTransformToLocalZero)
			{
				transform.ResetTransformToLocalZero();
			}
			return go;
		}

		public static GameObject Instantiate(GameObject original, Transform parent, bool setPositionRotationToLocalZero, bool setScaleToOne)
		{
			var go = GameObject.Instantiate(original);
			var transform = go.transform;
			transform.SetParent(parent);
			if (setPositionRotationToLocalZero)
			{
				transform.ResetPositionRotationToLocalZero();
			}
			if (setScaleToOne)
			{
				transform.ResetScaleToOne();
			}
			return go;
		}

		public static T InstantiateAndGetComponent<T>(GameObject original) where T : Component
		{
			var go = Object.Instantiate(original) as GameObject;
			return go.GetComponent<T>();
		}

		public static T InstantiateAndGetComponent<T>(GameObject original, Vector3 position, Quaternion rotation) where T : Component
		{
			var go = Object.Instantiate(original, position, rotation) as GameObject;
			return go.GetComponent<T>();
		}

		public static T InstantiateAndGetComponent<T>(GameObject original, Transform parent, bool setTransformToLocalZero) where T : Component
		{
			var go = Instantiate(original, parent, setTransformToLocalZero);
			return go.GetComponent<T>();
		}

		public static T InstantiateAndGetComponent<T>(GameObject original, Transform parent, bool setPositionRotationToLocalZero, bool setScaleToOne) where T : Component
		{
			var go = Instantiate(original, parent, setPositionRotationToLocalZero, setScaleToOne);
			return go.GetComponent<T>();
		}

		#endregion

		#region Instance

		public static bool IsAnInstanceInScene(this GameObject me)
		{
			return me.scene.name != null;
		}

		#endregion

		#region Component Count

		public static int GetComponentCount<T>(this GameObject me) where T : Component
		{
			var results = New.List<T>();
			me.GetComponents(results);
			var count = results.Count;
			Release.List(ref results);
			return count;
		}

		public static int GetComponentCount<T>(this Component me) where T : Component
		{
			var results = New.List<T>();
			me.GetComponents(results);
			var count = results.Count;
			Release.List(ref results);
			return count;
		}

		public static void EnsureOnlyOneComponentInstance<T>(this Component me) where T : Component
		{
			var count = me.GetComponentCount<T>();
			if (count != 1)
			{
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{count}' available in '{me.FullName()}'.");
			}
		}

		public static void EnsureOnlyOneComponentInstance<T>(this GameObject me) where T : Component
		{
			var count = me.GetComponentCount<T>();
			if (count != 1)
			{
				throw new Exception($"Expected single '{typeof(T).Name}' whereas '{count}' available in '{me.FullName()}'.");
			}
		}

		#endregion

		#region Destroy All GameObjects Containing Component

		public static void DestroyAllGameObjectsContainingComponentInScenes<T>(ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyAllGameObjectsContainingComponent<T>(activeCheck));
		}

		public static void DestroyAllGameObjectsContainingComponent<T>(this Scene scene, ActiveCheck activeCheck) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck);

			foreach (var component in components)
			{
				if (!component)
					continue;
				Object.Destroy(component.gameObject);
			}
		}

		#endregion

		#region Destroy Immediate All GameObjects Containing Component

		public static void DestroyImmediateAllGameObjectsContainingComponentInScenes<T>(ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyImmediateAllGameObjectsContainingComponent<T>(activeCheck));
		}

		public static void DestroyImmediateAllGameObjectsContainingComponent<T>(this Scene scene, ActiveCheck activeCheck) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck);

			foreach (var component in components)
			{
				if (!component)
					continue;
				Object.DestroyImmediate(component.gameObject);
			}
		}

		#endregion

		#region Get Closest

		public static Transform GetClosest<T>(this Transform me, IList<T> others) where T : Component
		{
			if (others == null || others.Count == 0)
				return null;

			Transform closest = others[0].transform;
			float closestSqrDistance = (me.position - closest.position).sqrMagnitude;

			for (int i = 0; i < others.Count; i++)
			{
				var other = others[i];
				float sqrDistance = (me.position - other.transform.position).sqrMagnitude;
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closest = other.transform;
				}
			}

			return closest;
		}

		public static Transform GetClosest<T>(this Transform me, T[] others) where T : Component
		{
			if (others == null || others.Length == 0)
				return null;

			Transform closest = others[0].transform;
			float closestSqrDistance = (me.position - closest.position).sqrMagnitude;

			for (int i = 0; i < others.Length; i++)
			{
				var other = others[i];
				float sqrDistance = (me.position - other.transform.position).sqrMagnitude;
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closest = other.transform;
				}
			}

			return closest;
		}

		#endregion

		#region Get Closest (with distance)

		public static Transform GetClosest<T>(this Transform me, IList<T> others, out float closestObjectDistance) where T : Component
		{
			if (others == null || others.Count == 0)
			{
				closestObjectDistance = float.NaN;
				return null;
			}

			Transform closest = others[0].transform;
			float closestSqrDistance = (me.position - closest.position).sqrMagnitude;

			for (int i = 0; i < others.Count; i++)
			{
				var other = others[i];
				float sqrDistance = (me.position - other.transform.position).sqrMagnitude;
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closest = other.transform;
				}
			}

			closestObjectDistance = Mathf.Sqrt(closestSqrDistance);
			return closest;
		}

		public static Transform GetClosest<T>(this Transform me, T[] others, out float closestObjectDistance) where T : Component
		{
			if (others == null || others.Length == 0)
			{
				closestObjectDistance = float.NaN;
				return null;
			}

			Transform closest = others[0].transform;
			float closestSqrDistance = (me.position - closest.position).sqrMagnitude;

			for (int i = 0; i < others.Length; i++)
			{
				var other = others[i];
				float sqrDistance = (me.position - other.transform.position).sqrMagnitude;
				if (closestSqrDistance > sqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closest = other.transform;
				}
			}

			closestObjectDistance = Mathf.Sqrt(closestSqrDistance);
			return closest;
		}

		#endregion

		#region Center To Children

		public static void CenterToChildren(this Transform me)
		{
			if (me.childCount == 0)
				throw new Exception("Transform has no children.");

			var center = me.CenterOfChildren();
			var shift = me.position - center;

			me.position = center;

			var childCount = me.childCount;
			for (int i = 0; i < childCount; i++)
			{
				me.GetChild(i).localPosition += shift;
			}
		}

		public static Vector3 CenterOfChildren(this Transform me)
		{
			if (me.childCount == 0)
				throw new Exception("Transform has no children.");

			var value = Vector3.zero;

			var childCount = me.childCount;
			for (int i = 0; i < childCount; i++)
			{
				value += me.GetChild(i).position;
			}

			return value / me.childCount;
		}

		#endregion

		#region Reset Transform

		public static void ResetPositionRotationToLocalZero(this Transform me)
		{
			me.localPosition = Vector3.zero;
			me.localRotation = Quaternion.identity;
		}

		public static void ResetTransformToLocalZero(this Transform me)
		{
			me.localPosition = Vector3.zero;
			me.localRotation = Quaternion.identity;
			me.localScale = Vector3.one;
		}

		public static void ResetPositionRotationToWorldZero(this Transform me)
		{
			me.position = Vector3.zero;
			me.rotation = Quaternion.identity;
		}

		public static void ResetTransformToWorldZero(this Transform me)
		{
			me.position = Vector3.zero;
			me.rotation = Quaternion.identity;
			me.localScale = Vector3.one;
		}

		public static void ResetScaleToOne(this Transform me)
		{
			me.localScale = Vector3.one;
		}

		#endregion

		#region Ground Snapping

		public static bool SnapToGround(this Transform transform, float raycastDistance, int raycastSteps, int raycastLayerMask, float offset)
		{
			return transform.SnapToGround(raycastDistance, raycastSteps, raycastLayerMask, offset, SnapToGroundRotationOption.DontRotate, 0f, 0f);
		}

		public static bool SnapToGround(this Transform transform, float raycastDistance, int raycastSteps, int raycastLayerMask, float offset, SnapToGroundRotationOption rotation, float rotationCastDistanceX, float rotationCastDistanceZ)
		{
			//var detailed = transform.gameObject.name == "DETAILEDSNAP";

			var currentPosition = transform.position;
			var stepLength = raycastDistance / raycastSteps;
			var stepLengthWithErrorMargin = stepLength + 0.01f;
			var direction = Vector3.down;
			var upperRayStartPosition = currentPosition;
			var lowerRayStartPosition = currentPosition;

			var snapped = false;

			for (int i = 0; i < raycastSteps; i++)
			{
				upperRayStartPosition.y = currentPosition.y + stepLength * (i + 1);
				lowerRayStartPosition.y = currentPosition.y - stepLength * i;

				//if (detailed)
				//{
				//	UnityEngine.Debug.DrawLine(upperRayStartPosition, upperRayStartPosition + direction * stepLength, Color.red, 1f);
				//	UnityEngine.Debug.DrawLine(lowerRayStartPosition, lowerRayStartPosition + direction * stepLength, Color.green, 1f);
				//}

				if (Physics.Raycast(upperRayStartPosition, direction, out var hit, stepLengthWithErrorMargin, raycastLayerMask) ||
				    Physics.Raycast(lowerRayStartPosition, direction, out hit, stepLengthWithErrorMargin, raycastLayerMask))
				{
					currentPosition.y = hit.point.y + offset;
					transform.position = currentPosition;
					snapped = true;
					break;
				}
			}

			// Rotation
			if (snapped && rotation != SnapToGroundRotationOption.DontRotate)
			{
				var rotationY = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
				var rotationResult = rotationY;

				switch (rotation)
				{
					case SnapToGroundRotationOption.X:
					{
						var angleZ = InternalCalculateSnapToGroundRotationAngle(currentPosition, offset, rotationY * Vector3.right, rotationCastDistanceX, raycastLayerMask);
						if (!float.IsNaN(angleZ))
						{
							rotationResult *= Quaternion.Euler(0f, 0f, angleZ);
						}
					}
						break;

					case SnapToGroundRotationOption.Z:
					{
						var angleX = InternalCalculateSnapToGroundRotationAngle(currentPosition, offset, rotationY * Vector3.forward, rotationCastDistanceZ, raycastLayerMask);
						if (!float.IsNaN(angleX))
						{
							rotationResult *= Quaternion.Euler(-angleX, 0f, 0f);
						}
					}
						break;

					//case SnapToGroundRotationOption.XZ: That did not work for some reason and did not bother to fix it because there is the ZX option that would be enough for now.
					//	{
					//		var angleX = InternalCalculateSnapToGroundRotationAngle(currentPosition, offset, rotationY * Vector3.forward, rotationCastDistanceZ, raycastLayerMask);
					//		if (!float.IsNaN(angleX))
					//		{
					//			rotationResult *= Quaternion.Euler(-angleX, 0f, 0f);
					//		}
					//		var angleZ = InternalCalculateSnapToGroundRotationAngle(currentPosition, offset, rotationY * Vector3.right, rotationCastDistanceX, raycastLayerMask);
					//		if (!float.IsNaN(angleZ))
					//		{
					//			rotationResult *= Quaternion.Euler(0f, 0f, angleZ);
					//		}
					//	}
					//	break;
					case SnapToGroundRotationOption.ZX:
					{
						var angleX = InternalCalculateSnapToGroundRotationAngle(currentPosition, offset, rotationY * Vector3.forward, rotationCastDistanceZ, raycastLayerMask);
						if (!float.IsNaN(angleX))
						{
							rotationResult *= Quaternion.Euler(-angleX, 0f, 0f);
						}
						var angleZ = InternalCalculateSnapToGroundRotationAngle(currentPosition, offset, rotationY * Vector3.right, rotationCastDistanceX, raycastLayerMask);
						if (!float.IsNaN(angleZ))
						{
							rotationResult *= Quaternion.Euler(0f, 0f, angleZ);
						}
					}
						break;

					default:
						throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
				}

				transform.rotation = rotationResult;
			}

			return snapped;
		}

		private static float InternalCalculateSnapToGroundRotationAngle(Vector3 objectPosition, float offset, Vector3 castStartPointDirection, float castDistance, int raycastLayerMask)
		{
			const float castLength = 4f;
			const float castHalfLength = castLength / 2f;

			objectPosition.y -= offset;

			//UnityEngine.Debug.DrawLine(objectPosition, objectPosition + castStartPointDirection, Color.red);

			var castLocalPosition = castStartPointDirection * castDistance;
			var direction = Vector3.down;

			var positiveSideHit = false;
			var negativeSideHit = false;
			var positiveSideHitPoint = Vector3Tools.NaN;
			var negativeSideHitPoint = Vector3Tools.NaN;

			// Positive side
			var castPosition = objectPosition + castLocalPosition;
			castPosition.y += castDistance * castHalfLength;
			if (Physics.Raycast(castPosition, direction, out var hit, castDistance * castLength, raycastLayerMask))
			{
				//UnityEngine.Debug.DrawLine(castPosition, castPosition + direction * (castDistance * castLength), Color.red);
				//DebugToolbox.DebugDraw.Cross(hit.point, 0.05f, Color.red);
				positiveSideHitPoint = hit.point;
				positiveSideHit = true;
			}

			// Negative side
			castPosition = objectPosition - castLocalPosition;
			castPosition.y += castDistance * castHalfLength;
			if (Physics.Raycast(castPosition, direction, out hit, castDistance * castLength, raycastLayerMask))
			{
				//UnityEngine.Debug.DrawLine(castPosition, castPosition + direction * (castDistance * castLength), Color.red);
				//DebugToolbox.DebugDraw.Cross(hit.point, 0.05f, Color.red);
				negativeSideHitPoint = hit.point;
				negativeSideHit = true;
			}

			if (positiveSideHit || negativeSideHit)
			{
				if (positiveSideHit)
				{
					if (negativeSideHit)
					{
						// Both sides hit
						var positiveHitDirection = positiveSideHitPoint - objectPosition;
						//UnityEngine.Debug.DrawLine(objectPosition, objectPosition + positiveHitDirection, Color.blue, 0f, false);
						var angleBetweenPositive = castLocalPosition.AngleBetween(positiveHitDirection) * Mathf.Rad2Deg * (positiveHitDirection.y - castLocalPosition.y).Sign();
						var negativeHitDirection = negativeSideHitPoint - objectPosition;
						//UnityEngine.Debug.DrawLine(objectPosition, objectPosition + negativeHitDirection, Color.red, 0f, false);
						var angleBetweenNegative = castLocalPosition.AngleBetween(-negativeHitDirection) * Mathf.Rad2Deg * (castLocalPosition.y - negativeHitDirection.y).Sign();
						return (angleBetweenPositive + angleBetweenNegative) * 0.5f;
					}
					else
					{
						// Only positive side hit
						var positiveHitDirection = positiveSideHitPoint - objectPosition;
						//UnityEngine.Debug.DrawLine(objectPosition, objectPosition + positiveHitDirection, Color.blue, 0f, false);
						return castLocalPosition.AngleBetween(positiveHitDirection) * Mathf.Rad2Deg * (positiveHitDirection.y - castLocalPosition.y).Sign();
					}
				}
				else
				{
					// Only negative side hit
					var negativeHitDirection = negativeSideHitPoint - objectPosition;
					//UnityEngine.Debug.DrawLine(objectPosition, objectPosition + negativeHitDirection, Color.red, 0f, false);
					return castLocalPosition.AngleBetween(-negativeHitDirection) * Mathf.Rad2Deg * (castLocalPosition.y - negativeHitDirection.y).Sign();
				}
			}
			return float.NaN;
		}

		public static bool SnapToGround(this Vector3 position, out Vector3 result, float raycastDistance, int raycastSteps, int raycastLayerMask, float offset)
		{
			var stepLength = raycastDistance / raycastSteps;
			var direction = Vector3.down;
			var upperRayStartPosition = position;
			var lowerRayStartPosition = position;

			for (int i = 0; i < raycastSteps; i++)
			{
				upperRayStartPosition.y = position.y + stepLength * (i + 1);
				lowerRayStartPosition.y = position.y - stepLength * i;

				//Debug.DrawLine(upperRayStartPosition, upperRayStartPosition + direction * stepLength, Color.red, 1f);
				//Debug.DrawLine(lowerRayStartPosition, lowerRayStartPosition + direction * stepLength, Color.green, 1f);

				if (Physics.Raycast(upperRayStartPosition, direction, out var hit, stepLength, raycastLayerMask) ||
				    Physics.Raycast(lowerRayStartPosition, direction, out hit, stepLength, raycastLayerMask))
				{
					position.y = hit.point.y + offset;
					result = position;
					return true;
				}
			}
			result = Vector3Tools.NaN;
			return false;
		}

		#endregion

		#region GameObject Name

		/// <summary>
		/// Only sets the name if it's not the same. Prevents firing hierarchy changed events in Unity Editor.
		/// </summary>
		public static bool SetNameIfRequired(this GameObject me, string newName)
		{
			// Unity will check for null references as soon as we call 'me.name' below. So skip this extra check here.
			// if (!me)
			// 	throw new NullReferenceException(nameof(me));

			if (me.name != newName)
			{
				me.name = newName;
				return true;
			}
			return false;
		}

		#endregion

		#region IsComponentEnabled

		/// <summary>
		/// Checks for 'enabled' state for those components that has an 'enabled' property.
		/// For other types, 'defaultValueForUnknownTypes' value will be returned.
		/// 
		/// Note that this is a processing heavy operation.
		/// </summary>
		public static bool IsComponentEnabled(this Component component, bool defaultValueForUnknownTypes = true)
		{
			// TODO MAINTENANCE: Update that in new Unity versions.
			// These are the classes that derive from Component (Unity version 2020.2.0a11)
			// Use _FindAllComponentsThatHaveEnabledProperty below to find all Component derived types that has
			// 'enabled' property. That should also mean the component is meant to be disabled if set to false.
			// The current list of Components:
			//    Behaviour
			//    Cloth
			//    Collider
			//    LODGroup
			//    Renderer

			// We cannot use 'as' for checking if the conversion is successful, since Unity overrides == and bool operator.
			if (component is Behaviour)
				return (component as Behaviour).enabled;
			if (component is Renderer)
				return (component as Renderer).enabled;
			if (component is Collider)
				return (component as Collider).enabled;
			if (component is LODGroup)
				return (component as LODGroup).enabled;
#if !DisableUnityCloth
			if (component is Cloth)
				return (component as Cloth).enabled;
#endif

			return defaultValueForUnknownTypes;
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(UnityEditorToolbox.ExtenityMenu.Maintenance + "Find All Components That Have Enabled Property", priority = UnityEditorToolbox.ExtenityMenu.MaintenancePriority + 0)]
		private static void _FindAllComponentsThatHaveEnabledProperty()
		{
			Log.Info("This tool is for Extenity development. It helps finding the required components that should be included in the code of GameObjectTools.IsComponentEnabled method.");

			var allComponentTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			                         from module in assembly.GetModules()
			                         from type in module.GetTypes()
			                         where type.IsSubclassOf(typeof(Component))
			                         select type).ToArray();

			var result = new HashSet<Type>();

			foreach (var componentType in allComponentTypes)
			{
				if (componentType.GetMethod("get_enabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy) != null)
				{
					var type = componentType;
					while (type != typeof(Component))
					{
						if (type.GetMethod("get_enabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
						{
							result.Add(type);
						}
						type = type.BaseType;
					}
				}
				if (componentType.GetField("enabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy) != null)
				{
					Debug.LogError($"Detected a field with name 'enabled' in type '{componentType}' and don't know how to handle that.");
				}
			}

			Log.Info($"List of types that have 'enabled' method ({result.Count}):\n" + string.Join("\n", result.Select(type => type.FullName).OrderBy(fullName => fullName)));
		}
#endif

		#endregion
	}

}

#endif
