using System;
using UnityEngine;
using System.Collections.Generic;

namespace Extenity.RenderingToolbox
{

	[RequireComponent(typeof(Camera))]
	public class SelectiveRenderer : MonoBehaviour
	{
		#region Camera

		private Camera _Camera;
		public Camera Camera
		{
			get
			{
				if (_Camera == null)
					_Camera = GetComponent<Camera>();
				return _Camera;
			}
		}

		#endregion

		#region Render Layer

		public int RenderLayer = 31;

		#endregion

		#region Render Events

		// TODO: These won't work in SRP anymore. Find a better approach.

		//protected void OnPreCull()
		//{
		//	ChangeAllRenderedObjectLayers();
		//	//LayerHistory.LogDump();
		//}

		//protected void OnPostRender()
		//{
		//	RestoreAllRenderedObjectLayers();
		//	//LayerHistory.LogDump();
		//}

		#endregion

		#region Layer History

		public class LayerHistoryEntry
		{
			public GameObject GameObject;
			public int Layer;
			public List<LayerHistoryEntry> ChildHistoryEntries;

			public void SetLayerAndStore(GameObject go, int newLayer)
			{
				if (GameObject != null)
				{
					Debug.LogErrorFormat(GameObject, "Tried to overwrite a noncleared history entry in selective renderer for object '{0}'. Expect unpredictable results. Make sure you clear history before starting new render.", GameObject.name);
					ClearOnly();
				}

				GameObject = go;
				Layer = go.layer;
				go.layer = newLayer;
				var trans = go.transform;
				for (int iChild = 0; iChild < trans.childCount; iChild++)
				{
					var child = trans.GetChild(iChild).gameObject;
					var childHistoryEntry = GetCleanChildHistoryEntry();
					childHistoryEntry.SetLayerAndStore(child, newLayer);
				}
			}

			public void ClearOnly()
			{
				GameObject = null;
				Layer = 0;
				if (ChildHistoryEntries != null)
				{
					for (int i = 0; i < ChildHistoryEntries.Count; i++)
					{
						ChildHistoryEntries[i].ClearOnly();
					}
				}
			}

			public void ClearAndRestore()
			{
				if (GameObject != null)
				{
					GameObject.layer = Layer;
				}
				GameObject = null;
				Layer = 0;
				if (ChildHistoryEntries != null)
				{
					for (int i = 0; i < ChildHistoryEntries.Count; i++)
					{
						ChildHistoryEntries[i].ClearAndRestore();
					}
				}
			}

			public LayerHistoryEntry GetCleanChildHistoryEntry()
			{
				if (ChildHistoryEntries == null)
				{
					ChildHistoryEntries = new List<LayerHistoryEntry>();
				}
				else
				{
					// See if there is an unallocated entry in list.
					for (int i = 0; i < ChildHistoryEntries.Count; i++)
					{
						var entry = ChildHistoryEntries[i];
						if (entry.GameObject == null)
							return entry;
					}
				}

				var newEntry = new LayerHistoryEntry();
				ChildHistoryEntries.Add(newEntry);
				return newEntry;
			}

			public void LogDump(int indentation)
			{
				Debug.Log(new string('\t', indentation) + Layer + "   " + GameObject.name, GameObject);
				if (ChildHistoryEntries != null)
				{
					for (int i = 0; i < ChildHistoryEntries.Count; i++)
					{
						var entry = ChildHistoryEntries[i];
						if (entry.GameObject != null)
							entry.LogDump(indentation + 1);
					}
				}
			}
		}

		public class LayerHistoryCollection
		{
			public List<LayerHistoryEntry> Entries = new List<LayerHistoryEntry>();

			public void ClearOnly()
			{
				for (int i = 0; i < Entries.Count; i++)
				{
					Entries[i].ClearOnly();
				}
			}

			public void ClearAndRestore()
			{
				for (int i = 0; i < Entries.Count; i++)
				{
					Entries[i].ClearAndRestore();
				}
			}

			public LayerHistoryEntry GetCleanHistoryEntry()
			{
				// See if there is an unallocated entry in list.
				for (int i = 0; i < Entries.Count; i++)
				{
					var entry = Entries[i];
					if (entry.GameObject == null)
						return entry;
				}

				var newEntry = new LayerHistoryEntry();
				Entries.Add(newEntry);
				return newEntry;
			}

			public void LogDump()
			{
				Debug.Log("Dumping all layer history:");
				for (int i = 0; i < Entries.Count; i++)
				{
					var entry = Entries[i];
					if (entry.GameObject != null)
						entry.LogDump(0);
				}
			}
		}

		[NonSerialized]
		public LayerHistoryCollection LayerHistory = new LayerHistoryCollection();

		#endregion

		#region Rendered Objects

		[NonSerialized]
		private List<GameObject> RenderedObjects = new List<GameObject>();

		public void AddRenderedObject(GameObject go)
		{
			if (RenderedObjects.Contains(go))
				return;

			var historyEntry = LayerHistory.GetCleanHistoryEntry();
			historyEntry.SetLayerAndStore(go, RenderLayer);
		}

		public void RemoveRenderedObject(GameObject go)
		{
			for (var i = 0; i < LayerHistory.Entries.Count; i++)
			{
				var historyEntry = LayerHistory.Entries[i];
				if (historyEntry.GameObject == go)
				{
					historyEntry.ClearAndRestore();
					LayerHistory.Entries.RemoveAt(i);
					i--;
				}
			}
		}

		public void RestoreAllRenderedObjectLayers()
		{
			LayerHistory.ClearAndRestore();
		}

		//public void ChangeAllRenderedObjectLayers()
		//{
		//	if (RenderedObjects == null || RenderedObjects.Count == 0)
		//		return;
		//	for (int i = 0; i < RenderedObjects.Count; i++)
		//	{
		//		var renderedObject = RenderedObjects[i];
		//		var historyEntry = LayerHistory.GetCleanHistoryEntry();
		//		historyEntry.SetLayerAndStore(renderedObject, RenderLayer);
		//	}
		//}

		//public void RestoreAllRenderedObjectLayers()
		//{
		//	LayerHistory.ClearAndRestore();
		//}

		#endregion
	}

}
