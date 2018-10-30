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

		public void AddAllGameObjects()
		{
			var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach (GameObject gameObject in gameObjects)
			{
				var id = gameObject.GetInstanceID();
				if (InstanceMap.ContainsKey(id))
					continue;
				InstanceMap.Add(id, gameObject);
			}
		}

		public void ClearDestroyedGameObjects()
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
				foreach (var gameObjectID in bag)
				{
					InstanceMap.Remove(gameObjectID);
				}
			}
		}
	}

}
