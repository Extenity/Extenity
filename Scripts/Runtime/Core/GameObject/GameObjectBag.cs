using System.Collections.Generic;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class GameObjectBag
	{
		public readonly Dictionary<int, GameObject> InstanceMap;

		public GameObjectBag()
		{
			InstanceMap = new Dictionary<int, GameObject>();
		}

		public GameObjectBag(int capacity)
		{
			InstanceMap = new Dictionary<int, GameObject>(capacity);
		}

		public void Add(GameObject gameObject)
		{
			if (!gameObject)
				return;
			var id = gameObject.GetInstanceID();
			if (InstanceMap.ContainsKey(id))
				return;
			InstanceMap.Add(id, gameObject);
		}

		public void Add(int customID, GameObject gameObject)
		{
			if (!gameObject)
				return;
			if (InstanceMap.ContainsKey(customID))
				return;
			InstanceMap.Add(customID, gameObject);
		}

		public void AddAllGameObjects()
		{
			var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach (GameObject gameObject in gameObjects)
			{
				var instanceID = gameObject.GetInstanceID();
				if (InstanceMap.ContainsKey(instanceID))
					continue;
				InstanceMap.Add(instanceID, gameObject);
			}
		}

		public void ClearLostReferences()
		{
			List<int> bag = null;
			foreach (var entry in InstanceMap)
			{
				if (!entry.Value)
				{
					if (bag == null)
					{
						bag = new List<int>();
					}
					bag.Add(entry.Key);
				}
			}
			if (bag != null)
			{
				foreach (var instanceID in bag)
				{
					InstanceMap.Remove(instanceID);
				}
			}
		}
	}

}
