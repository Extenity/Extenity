using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Extenity.SceneManagement
{

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

		#endregion

		#region Destroy Specials

		public static void DestroyAll<T>(this IList<T> list, float delay = 0f, bool saveToHistory = true) where T : Object
		{
			for (int i = 0; i < list.Count; i++)
			{
				Destroy(list[i], delay, saveToHistory);
			}
		}

		public static void DestroyAllImmediate<T>(this IList<T> list, bool allowDestroyingAssets = false, bool saveToHistory = true) where T : Object
		{
			for (int i = 0; i < list.Count; i++)
			{
				DestroyImmediate(list[i], allowDestroyingAssets, saveToHistory);
			}
		}

		public static void DestroyComponentThenGameObjectIfNoneLeft(Component component, bool saveToHistory = true)
		{
			if (component == null)
				return;

			var gameObject = component.gameObject;
			var componentCount = gameObject.GetComponents<Component>().Length;

			if (componentCount == 2) // 1 for Transform and 1 for the 'component'
			{
				Destroy(gameObject, saveToHistory);
			}
			else
			{
				Destroy(component, saveToHistory);
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

		public static List<DestroyHistoryItem> DestroyHistory = new List<DestroyHistoryItem>(1000);

		public static bool IsDestroyHistoryEnabled = false;
		public static bool IsDetailedDestroyHistoryEnabled = false;

		public static void Destroy(Object obj, bool saveToHistory = true)
		{
			Destroy(obj, 0f, saveToHistory);
		}

		public static void Destroy(Object obj, float delay, bool saveToHistory = true)
		{
			Object.Destroy(obj, delay);

			if (saveToHistory)
				_CreateDestroyHistoryItem(obj, false, false, delay);
		}

		// Commented out because it causes ambiguity errors.
		//public static void DestroyImmediate(Object obj, bool saveToHistory = true)
		//{
		//	DestroyImmediate(obj, false, saveToHistory);
		//}

		public static void DestroyImmediate(Object obj, bool allowDestroyingAssets = false, bool saveToHistory = true)
		{
			Object.DestroyImmediate(obj, allowDestroyingAssets);

			if (saveToHistory)
				_CreateDestroyHistoryItem(obj, true, allowDestroyingAssets, 0f);
		}

		private static void _CreateDestroyHistoryItem(Object obj, bool isImmediate, bool allowDestroyingAssets, float destroyDelay)
		{
			if (!IsDestroyHistoryEnabled)
				return;

			var item = new DestroyHistoryItem();
			item.ObjectName = obj.name;
			item.IsImmediate = isImmediate;
			item.IsImmediateAllowsDestroyingAssets = allowDestroyingAssets;
			item.DestroyDelay = destroyDelay;

			if (IsDetailedDestroyHistoryEnabled)
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

		#region FindComponentInParents

		public static T GetComponentInParents<T>(this Component me) where T : Component
		{
			if (me == null)
				return null;

			var transform = me.transform.parent;

			while (transform != null)
			{
				var component = transform.GetComponent<T>();
				if (component != null)
					return component;

				transform = transform.parent;
			}
			return null;
		}

		public static T GetComponentInParentsIncludingSelf<T>(this Component me) where T : Component
		{
			if (me == null)
				return null;

			var component = me.GetComponent<T>();
			if (component != null)
				return component;

			var transform = me.transform.parent;

			while (transform != null)
			{
				component = transform.GetComponent<T>();
				if (component != null)
					return component;

				transform = transform.parent;
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

			var current = me.parent;
			while (current != null)
			{
				var component = current.GetComponent<T>();
				if (component != null)
					return component;
				current = current.parent;
			}

			return null;
		}

		public static T GetComponentInParentRecursiveWithoutActiveCheck<T>(this Transform me) where T : Component
		{
			if (me == null)
				return null;

			var current = me;
			while (current != null)
			{
				var component = current.GetComponent<T>();
				if (component != null)
					return component;
				current = current.parent;
			}

			return null;
		}

		#endregion

		#region FindObjectOfTypeEnsured and FindSingleObjectOfTypeEnsured

		public static T FindObjectOfType<T>() where T : class
		{
			return UnityEngine.Object.FindObjectOfType(typeof(T)) as T;
		}

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
			var component = me.FindChild(name);
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

		#region Full Name

		public static string FullName(this GameObject me, char separator = '/')
		{
			if (me == null)
				return "";
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
				return "";
			return me.gameObject.FullName(gameObjectNameSeparator) + componentNameSeparator + me.name;
		}

		#endregion
	}

}
