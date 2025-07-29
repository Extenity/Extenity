#if UNITY_5_3_OR_NEWER

using System.Collections.Generic;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class ComponentBag<T> where T : Component
	{
		public readonly Dictionary<int, T> InstanceMap;

		public ComponentBag()
		{
			InstanceMap = new Dictionary<int, T>();
		}

		public ComponentBag(int capacity)
		{
			InstanceMap = new Dictionary<int, T>(capacity);
		}

		public void Add(T component)
		{
			if (!component)
				return;
			var instanceID = component.GetInstanceID();
			if (InstanceMap.ContainsKey(instanceID))
				return;
			InstanceMap.Add(instanceID, component);
		}

		public void Add(int customID, T component)
		{
			if (!component)
				return;
			if (InstanceMap.ContainsKey(customID))
				return;
			InstanceMap.Add(customID, component);
		}

		public void AddAllComponents()
		{
			var components = Resources.FindObjectsOfTypeAll(typeof(Component));
			foreach (Component component in components)
			{
				var instanceID = component.GetInstanceID();
				if (InstanceMap.ContainsKey(instanceID))
					continue;
				InstanceMap.Add(instanceID, (T)component);
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

#endif
