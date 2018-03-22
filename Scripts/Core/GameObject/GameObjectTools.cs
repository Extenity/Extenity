using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Extenity.MathToolbox;
using Extenity.SceneManagementToolbox;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.GameObjectToolbox
{

	public enum SnapToGroundRotationOption
	{
		DontRotate,
		X = 1,
		Z = 2,
		ZX = 4,
	}

	public static class GameObjectTools
	{
		#region Create

		public static GameObject CreateOrGetGameObject(string name)
		{
			var go = GameObject.Find(name);
			if (go != null)
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

		public static void CreateOrModifyGameObject(ref Transform transform, string name, Transform parent = null)
		{
			if (!transform)
			{
				transform = new GameObject(name).transform;
			}
			else
			{
				transform.gameObject.name = name;
			}
			transform.SetParent(parent, false);
			transform.ResetTransformToLocalZero();
		}

		public static void CreateOrModifyGameObject(ref Transform transform, string name, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (!transform)
			{
				transform = new GameObject(name).transform;
			}
			else
			{
				transform.gameObject.name = name;
			}
			transform.SetParent(parent, false);
			transform.position = position;
			transform.rotation = rotation;
			transform.localScale = Vector3.one;
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
			if (component == null)
				return;

			var gameObject = component.gameObject;
			var hasChildren = gameObject.transform.childCount > 0;
			if (!hasChildren) // Make sure the gameobject has no sub gameobjects
			{
				var componentCount = gameObject.GetComponents<Component>().Length;
				if (componentCount == 2) // 1 for Transform and 1 for the 'component'
				{
					Destroy(gameObject, historySaveType);
					return;
				}
			}

			Destroy(component, historySaveType);
		}

		public static void DestroyImmediateComponentThenGameObjectIfNoneLeft(Component component, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			if (component == null)
				return;

			var gameObject = component.gameObject;
			var hasChildren = gameObject.transform.childCount > 0;
			if (!hasChildren) // Make sure the gameobject has no sub gameobjects
			{
				var componentCount = gameObject.GetComponents<Component>().Length;
				if (componentCount == 2) // 1 for Transform and 1 for the 'component'
				{
					DestroyImmediate(gameObject, historySaveType);
					return;
				}
			}

			DestroyImmediate(component, historySaveType);
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

		public static void TrackedDestroy(Object obj, HistorySaveType historySaveType = HistorySaveType.Save) { Destroy(obj, historySaveType); }
		public static void Destroy(Object obj, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			const float delay = 0f;
			Object.Destroy(obj);

			if (historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, false, false, delay, historySaveType);
		}

		public static void TrackedDestroy(Object obj, float delay, HistorySaveType historySaveType = HistorySaveType.Save) { Destroy(obj, delay, historySaveType); }
		public static void Destroy(Object obj, float delay, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			Object.Destroy(obj, delay);

			if (historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, false, false, delay, historySaveType);
		}

		public static void TrackedDestroyImmediate(Object obj, HistorySaveType historySaveType = HistorySaveType.Save) { DestroyImmediate(obj, historySaveType); }
		public static void DestroyImmediate(Object obj, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			const bool allowDestroyingAssets = false;
			Object.DestroyImmediate(obj);

			if (historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, true, allowDestroyingAssets, 0f, historySaveType);
		}

		public static void TrackedDestroyImmediate(Object obj, bool allowDestroyingAssets, HistorySaveType historySaveType = HistorySaveType.Save) { DestroyImmediate(obj, allowDestroyingAssets, historySaveType); }
		public static void DestroyImmediate(Object obj, bool allowDestroyingAssets, HistorySaveType historySaveType = HistorySaveType.Save)
		{
			Object.DestroyImmediate(obj, allowDestroyingAssets);

			if (historySaveType != HistorySaveType.DontSave)
				_CreateDestroyHistoryItem(obj, true, allowDestroyingAssets, 0f, historySaveType);
		}

		private static void _CreateDestroyHistoryItem(Object obj, bool isImmediate, bool allowDestroyingAssets, float destroyDelay, HistorySaveType historySaveType)
		{
			if (!IsDestroyHistoryEnabled)
				return;

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
			if (parent != null)
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
			foreach (Transform child in gameObject.transform)
				ChangeLayersRecursively(child.gameObject, layer);
		}

		#endregion

		#region Recursive Changes - Renderers

		public static void EnableRenderersRecursively(this GameObject gameObject)
		{
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer != null)
				renderer.enabled = true;
			foreach (Transform child in gameObject.transform)
				EnableRenderersRecursively(child.gameObject);
		}

		public static void DisableRenderersRecursively(this GameObject gameObject)
		{
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer != null)
				renderer.enabled = false;
			foreach (Transform child in gameObject.transform)
				DisableRenderersRecursively(child.gameObject);
		}

		public static void SetRendererColorsRecursively(this GameObject gameObject, Color color)
		{
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer != null)
				renderer.material.color = color;
			foreach (Transform child in gameObject.transform)
				SetRendererColorsRecursively(child.gameObject, color);
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

			var initialization = true;
			for (int i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];
				if (renderer != null)
				{
					if (initialization)
					{
						bounds = renderer.bounds;
						initialization = false;
					}
					else
					{
						bounds.Encapsulate(renderer.bounds);
					}
				}
			}
			return initialization; // false if never initialized
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

			var initialization = true;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				var meshFilter = meshFilters[i];
				var sharedMesh = meshFilter != null ? meshFilter.sharedMesh : null;
				if (sharedMesh != null)
				{
					if (initialization)
					{
						bounds = meshFilter.transform.TransformBounds(sharedMesh.bounds, baseTransform);
						initialization = false;
					}
					else
					{
						bounds.Encapsulate(meshFilter.transform.TransformBounds(sharedMesh.bounds, baseTransform));
					}
				}
			}
			return initialization; // false if never initialized
		}

		public static bool CalculateMeshWorldBoundsRecursively(this GameObject go, out Bounds bounds)
		{
			bounds = new Bounds();
			var meshFilters = go.GetComponentsInChildren<MeshFilter>();
			if (meshFilters == null || meshFilters.Length == 0)
			{
				return false;
			}

			var initialization = true;
			for (int i = 0; i < meshFilters.Length; i++)
			{
				var meshFilter = meshFilters[i];
				var sharedMesh = meshFilter != null ? meshFilter.sharedMesh : null;
				if (sharedMesh != null)
				{
					if (initialization)
					{
						bounds = meshFilter.transform.TransformBounds(sharedMesh.bounds);
						initialization = false;
					}
					else
					{
						bounds.Encapsulate(meshFilter.transform.TransformBounds(sharedMesh.bounds));
					}
				}
			}
			return initialization; // false if never initialized
		}

		public static bool CalculateColliderWorldBoundsRecursively(this GameObject go, out Bounds bounds)
		{
			bounds = new Bounds();
			var colliders = go.GetComponentsInChildren<Collider>();
			if (colliders == null || colliders.Length == 0)
			{
				return false;
			}

			var initialization = true;
			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];
				if (collider != null)
				{
					if (initialization)
					{
						bounds = collider.bounds;
						initialization = false;
					}
					else
					{
						bounds.Encapsulate(collider.bounds);
					}
				}
			}
			return initialization; // false if never initialized
		}

		#endregion

		#region IsChildOf / IsParentOf / HasComponent / IsEmpty

		/// <summary>
		/// CAUTION! This is a performance heavy method because it uses GetComponents. Use it wisely.
		/// </summary>
		public static bool IsChildOf(this Component me, Component suspectedParent, bool checkContainingObject = true)
		{
			if (me == null)
				throw new ArgumentNullException("me");
			if (suspectedParent == null)
				throw new ArgumentNullException("suspectedParent");

			if (checkContainingObject)
			{
				if (me.transform.HasSiblingComponent(suspectedParent))
					return true;
			}

			var transform = me.transform.parent;

			while (transform != null)
			{
				if (transform.HasSiblingComponent(suspectedParent))
					return true;
				transform = transform.parent;
			}

			return false;
		}

		/// <summary>
		/// CAUTION! This is a performance heavy method because it uses GetComponents. Use it wisely.
		/// </summary>
		public static bool IsParentOf(this Component me, Component suspectedChild, bool checkContainingObject = true)
		{
			if (me == null)
				throw new ArgumentNullException("me");
			if (suspectedChild == null)
				throw new ArgumentNullException("suspectedChild");

			return suspectedChild.IsChildOf(me, checkContainingObject);
		}

		/// <summary>
		/// CAUTION! This is a performance heavy method because it uses GetComponents. Use it wisely.
		/// </summary>
		public static bool HasSiblingComponent(this Component me, Component other)
		{
			if (me == null)
				throw new ArgumentNullException("me");
			if (other == null)
				return false;

			var components = me.transform.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] == other)
					return true;
			}
			return false;
		}

		/// <summary>
		/// CAUTION! This is a performance heavy method because it uses GetComponents. Use it wisely.
		/// </summary>
		public static bool HasComponent(this GameObject gameObject, Component component)
		{
			if (gameObject == null)
				throw new ArgumentNullException("gameObject");
			if (component == null)
				return false;

			var components = gameObject.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] == component)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Check if component is inside objects list. Also check if any gameobject in objects list has the component attached to itself.
		/// CAUTION! This is a performance heavy method because it uses HasComponent. Use it wisely.
		/// </summary>
		public static bool ContainsComponentAsIsOrAttachedToGameObject(this IEnumerable<UnityEngine.Object> objects, Component component)
		{
			if (objects == null)
				return false;
			if (component == null)
				return false;

			foreach (var obj in objects)
			{
				// Check if the component is in the list as it is.
				if (obj == component)
					return true;

				// Check if the component is attached to a gameobject.
				var gameObject = obj as GameObject;
				if (gameObject != null) // Ignore if not a gameobject
				{
					if (gameObject.HasComponent(component))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool IsEmpty(this Transform transform)
		{
			if (!transform)
				return false;
			return
				transform.childCount == 0 && // No children
				transform.GetComponentCount<Component>() == 1; // Only contains Transform component, which is bare minimum
		}

		public static bool IsEmpty(this GameObject gameObject)
		{
			if (!gameObject)
				return false;
			return
				gameObject.transform.childCount == 0 && // No children
				gameObject.GetComponentCount<Component>() == 1; // Only contains Transform component, which is bare minimum
		}

		#endregion

		#region FindComponentInParents

		public static T GetComponentInParent<T>(this Transform me, bool includingSelf, bool includingInactive) where T : Component
		{
			return me.gameObject.GetComponentInParent<T>(includingSelf, includingInactive);
		}

		public static T GetComponentInParent<T>(this GameObject me, bool includingSelf, bool includingInactive) where T : Component
		{
			if (!me)
				return null;

			if (!includingInactive && !me.activeInHierarchy)
				return null;

			if (includingSelf)
			{
				var component = me.GetComponent<T>();
				if (component)
					return component;
			}

			var parent = me.transform.parent;

			while (parent)
			{
				var component = parent.GetComponent<T>();
				if (component)
					return component;

				parent = parent.parent;
			}
			return null;
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
				if (found != null)
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
				if (found != null)
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
				delegate (Transform p1, Transform p2)
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

		#region Get component in children without active check

		public static T GetComponentInChildrenRecursiveWithoutActiveCheckExcludingThis<T>(this Transform me) where T : Component
		{
			if (me == null)
				return null;

			foreach (Transform child in me)
			{
				T componentInChildren = child.GetComponentInChildrenRecursiveWithoutActiveCheck<T>();
				if (componentInChildren != null)
					return componentInChildren;
			}
			return null;
		}

		public static T GetComponentInChildrenRecursiveWithoutActiveCheck<T>(this Transform me) where T : Component
		{
			if (me == null)
				return null;

			//if (this.activeInHierarchy)
			{
				T component = me.GetComponent<T>();
				if (component != null)
					return component;
			}

			foreach (Transform child in me)
			{
				T componentInChildren = child.GetComponentInChildrenRecursiveWithoutActiveCheck<T>();
				if (componentInChildren != null)
					return componentInChildren;
			}
			return null;
		}

		#endregion

		#region Get component in parent without active check

		public static T GetComponentInParentRecursiveWithoutActiveCheckExcludingThis<T>(this Transform me) where T : Component
		{
			if (me == null)
				return null;
			for (var current = me.parent; current != null; current = current.parent)
			{
				var test = current.GetComponent<T>();
				if (test != null)
					return test;
			}
			return null;
		}

		public static T GetComponentInParentRecursiveWithoutActiveCheck<T>(this Transform me) where T : Component
		{
			for (var current = me; current != null; current = current.parent)
			{
				var test = current.GetComponent<T>();
				if (test != null)
					return test;
			}
			return null;
		}

		#endregion

		#region FindObjectsOfTypeAll in Scene

		public static List<T> FindObjectsOfTypeAllInActiveScene<T>()
		{
			return SceneManager.GetActiveScene().FindObjectsOfTypeAll<T>();
		}

		public static List<T> FindObjectsOfTypeAllInLoadedScenes<T>()
		{
			return SceneManagerTools.GetLoadedScenes().FindObjectsOfTypeAll<T>();
		}

		public static List<T> FindObjectsOfTypeAll<T>(this Scene scene)
		{
			var temp = new List<T>();
			var results = new List<T>();
			var rootGameObjects = scene.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				rootGameObjects[i].GetComponentsInChildren(true, temp);
				results.AddRange(temp);
				temp.Clear();
			}
			return results;
		}

		public static List<T> FindObjectsOfTypeAll<T>(this IList<Scene> scenes)
		{
			var results = new List<T>();
			foreach (var scene in scenes)
			{
				var list = scene.FindObjectsOfTypeAll<T>();
				results.AddRange(list);
			}
			return results;
		}

		#endregion

		#region FindObjectOfTypeEnsured and FindSingleObjectOfTypeEnsured

		public static object FindObjectOfTypeEnsured(Type type)
		{
			var obj = UnityEngine.Object.FindObjectOfType(type);
			if (obj == null)
				throw new Exception("Could not find object of type '" + type.Name + "'");
			return obj;
		}

		public static T FindObjectOfTypeEnsured<T>() where T : class
		{
			var type = typeof(T);
			var obj = UnityEngine.Object.FindObjectOfType(type);
			if (obj == null)
				throw new Exception("Could not find object of type '" + type.Name + "'");
			return obj as T;
		}

		public static object FindSingleObjectOfTypeEnsured(Type type)
		{
			var objs = UnityEngine.Object.FindObjectsOfType(type);
			if (objs == null || objs.Length == 0)
				throw new Exception("Could not find object of type '" + type.Name + "'");
			else if (objs.Length > 1)
				throw new Exception("There are multiple instances for object of type '" + type.Name + "'");
			return objs[0];
		}

		public static T FindSingleObjectOfTypeEnsured<T>() where T : class
		{
			var type = typeof(T);
			var objs = UnityEngine.Object.FindObjectsOfType(type);
			if (objs == null || objs.Length == 0)
				throw new Exception("Could not find object of type '" + type.Name + "'");
			else if (objs.Length > 1)
				throw new Exception("There are multiple instances for object of type '" + type.Name + "'");
			return objs[0] as T;
		}

		#endregion

		#region FindChildEnsured

		public static Transform FindChildEnsured(this Transform me, string name)
		{
			var component = me.Find(name);
			if (component == null)
				throw new Exception("Could not find child '" + name + "' of '" + me.name + "'");
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

		public delegate void ChildHandler(GameObject child);

		public static void ForeachChildren(this GameObject gameObject, ChildHandler childHandler, bool recursive)
		{
			if (!gameObject)
				throw new ArgumentNullException("gameObject");
			InternalIterateChildren(gameObject, childHandler, recursive);
		}

		private static void InternalIterateChildren(GameObject gameObject, ChildHandler childHandler, bool recursive)
		{
			if (!gameObject)
				throw new ArgumentNullException("gameObject");
			foreach (Transform child in gameObject.transform)
			{
				childHandler(child.gameObject);
				if (recursive)
					InternalIterateChildren(child.gameObject, childHandler, true);
			}
		}

		#endregion

		#region GetParentEnsured

		public static Transform GetParentEnsured(this Transform me)
		{
			var parent = me.parent;
			if (parent == null)
			{
				throw new Exception("Could not get parent of '" + me.name + "'");
			}
			return parent;
		}

		#endregion

		#region SetParent

		public static void SetParentOfAllObjectsContainingComponentInActiveScene<T>(Transform parent, bool onlyStaticObjects) where T : Component
		{
			SceneManager.GetActiveScene().SetParentOfAllObjectsContainingComponent<T>(parent, onlyStaticObjects);
		}

		public static void SetParentOfAllObjectsContainingComponentInLoadedScenes<T>(Transform parent, bool onlyStaticObjects) where T : Component
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.SetParentOfAllObjectsContainingComponent<T>(parent, onlyStaticObjects));
		}

		public static void SetParentOfAllObjectsContainingComponent<T>(this Scene scene, Transform parent, bool onlyStaticObjects) where T : Component
		{
			IEnumerable<Transform> componentTransforms;
			if (onlyStaticObjects)
			{
				componentTransforms = scene.FindObjectsOfTypeAll<T>()
					.Where(item => item.gameObject.isStatic)
					.Select(item => item.transform);
			}
			else
			{
				componentTransforms = scene.FindObjectsOfTypeAll<T>()
					.Select(item => item.transform);
			}

			foreach (var transform in componentTransforms)
			{
				transform.SetParent(parent, true);
				transform.SetAsLastSibling();
			}
		}

		#endregion

		#region GetComponentEnsured

		public static T GetSingleComponentEnsured<T>(this Component me) where T : Component
		{
			if (me == null)
			{
				throw new Exception("Tried to get component '" + typeof(T).Name + "' of a null object");
			}
			return me.gameObject.GetSingleComponentEnsured<T>();
		}

		public static T GetSingleComponentEnsured<T>(this GameObject me) where T : Component
		{
			if (me == null)
			{
				throw new Exception("Tried to get component '" + typeof(T).Name + "' of a null object");
			}
			var components = me.GetComponents<T>();
			if (components == null || components.Length == 0)
			{
				throw new Exception("Could not get component '" + typeof(T).Name + "'");
			}
			if (components.Length != 1)
			{
				throw new Exception("There are more than 1 '" + typeof(T).Name + "' components in object '" + me.name + "'");
			}
			return components[0];
		}

		#endregion

		#region GetOrAddComponent

		public static T GetSingleOrAddComponent<T>(this GameObject me) where T : Component
		{
			if (me == null)
			{
				throw new Exception("Tried to get or add component '" + typeof(T).Name + "' for a null object");
			}
			var components = me.GetComponents<T>();
			if (components == null || components.Length == 0)
			{
				return me.AddComponent<T>();
			}
			if (components.Length != 1)
			{
				throw new Exception("There are more than 1 '" + typeof(T).Name + "' components in object '" + me.name + "'");
			}
			return components[0];
		}

		public static T GetFirstOrAddComponent<T>(this GameObject me) where T : Component
		{
			if (me == null)
			{
				throw new Exception("Tried to get or add component '" + typeof(T).Name + "' for a null object");
			}
			var components = me.GetComponents<T>();
			if (components == null || components.Length == 0)
			{
				return me.AddComponent<T>();
			}
			return components[0];
		}

		#endregion

		#region InstantiateAndGetComponent

		public static GameObject Instantiate(GameObject original, Transform parent, bool setLocationToLocalZero)
		{
			var go = GameObject.Instantiate(original);
			go.transform.SetParent(parent);
			if (setLocationToLocalZero)
			{
				go.transform.ResetTransformToLocalZero();
			}
			return go;
		}

		public static T InstantiateAndGetComponent<T>(GameObject original) where T : Component
		{
			var go = UnityEngine.Object.Instantiate(original) as GameObject;
			return go.GetComponent<T>();
		}

		public static T InstantiateAndGetComponent<T>(GameObject original, Vector3 position, Quaternion rotation) where T : Component
		{
			var go = UnityEngine.Object.Instantiate(original, position, rotation) as GameObject;
			return go.GetComponent<T>();
		}

		public static T InstantiateAndGetComponent<T>(GameObject original, Transform parent, bool setLocationToLocalZero) where T : Component
		{
			var go = Instantiate(original, parent, setLocationToLocalZero);
			return go.GetComponent<T>();
		}

		#endregion

		#region Instance

		public static bool IsAnInstanceInScene(this GameObject me)
		{
			return me.scene.name != null;
		}

		#endregion

		#region Instance Count

		public static int GetComponentCount<T>(this GameObject me) where T : Component
		{
			var components = me.GetComponents<T>();
			if (components == null)
				return 0;
			return components.Length;
		}

		public static int GetComponentCount<T>(this Component me) where T : Component
		{
			var components = me.GetComponents<T>();
			if (components == null)
				return 0;
			return components.Length;
		}

		public static void EnsureOnlyOneComponentInstance<T>(this Component me) where T : Component
		{
			if (me.GetComponentCount<T>() != 1)
				throw new Exception();
		}

		public static void EnsureOnlyOneComponentInstance<T>(this GameObject me) where T : Component
		{
			if (me.GetComponentCount<T>() != 1)
				throw new Exception();
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
				throw new Exception("Transform has no children");

			var center = me.CenterOfChildren();
			var shift = me.position - center;

			me.position = center;

			foreach (Transform child in me)
			{
				child.localPosition += shift;
			}
		}

		public static Vector3 CenterOfChildren(this Transform me)
		{
			if (me.childCount == 0)
				throw new Exception("Transform has no children");

			var value = Vector3.zero;

			foreach (Transform child in me)
			{
				value += child.position;
			}

			return value / me.childCount;
		}

		#endregion

		#region Reset Transform

		public static void ResetTransformToLocalZero(this Transform me)
		{
			me.localPosition = Vector3.zero;
			me.localRotation = Quaternion.identity;
			me.localScale = Vector3.one;
		}

		public static void ResetTransformToWorldZero(this Transform me)
		{
			me.position = Vector3.zero;
			me.rotation = Quaternion.identity;
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

				RaycastHit hit;
				if (Physics.Raycast(upperRayStartPosition, direction, out hit, stepLengthWithErrorMargin, raycastLayerMask) ||
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
						throw new ArgumentOutOfRangeException("rotation", rotation, null);
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
			var positiveSideHitPoint = MathTools.Vector3NaN;
			var negativeSideHitPoint = MathTools.Vector3NaN;
			RaycastHit hit;

			// Positive side
			var castPosition = objectPosition + castLocalPosition;
			castPosition.y += castDistance * castHalfLength;
			if (Physics.Raycast(castPosition, direction, out hit, castDistance * castLength, raycastLayerMask))
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

				RaycastHit hit;
				if (Physics.Raycast(upperRayStartPosition, direction, out hit, stepLength, raycastLayerMask) ||
					Physics.Raycast(lowerRayStartPosition, direction, out hit, stepLength, raycastLayerMask))
				{
					position.y = hit.point.y + offset;
					result = position;
					return true;
				}
			}
			result = MathTools.Vector3NaN;
			return false;
		}

		#endregion

		#region GameObject Name

		public const string NullGameObjectNamePlaceholder = "[Null]";

		public static string NameSafe(this Transform transform)
		{
			if (transform == null)
				return NullGameObjectNamePlaceholder;
			return transform.gameObject.name;
		}

		public static string NameSafe(this GameObject gameObject)
		{
			if (gameObject == null)
				return NullGameObjectNamePlaceholder;
			return gameObject.name;
		}

		public static string FullName(this GameObject me, char separator = '/')
		{
			if (me == null)
				return NullGameObjectNamePlaceholder;
			var name = me.name;
			var parent = me.transform.parent;
			while (parent != null)
			{
				name = parent.name + separator + name;
				parent = parent.parent;
			}
			return name;
		}

		public static string FullName(this Component me, char gameObjectNameSeparator = '/', char componentNameSeparator = '|')
		{
			if (me == null)
				return NullGameObjectNamePlaceholder;
			return me.gameObject.FullName(gameObjectNameSeparator) + componentNameSeparator + me.GetType().Name;
		}

		/// <summary>
		/// Only sets the name if it's not the same. Prevents firing hierarchy changed events in editor.
		/// </summary>
		public static bool SetNameIfRequired(this GameObject me, string newName)
		{
			if (!me)
				throw new NullReferenceException("me");

			if (me.name != newName)
			{
				me.name = newName;
				return true;
			}
			return false;
		}

		#endregion
	}

}
