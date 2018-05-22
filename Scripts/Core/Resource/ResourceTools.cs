using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.ResourceToolbox
{

	public static class ResourceTools
	{
		#region FindObjectsOfTypeAll

		public static Object[] FindObjectsOfTypeAll(Type type)
		{
			return Resources.FindObjectsOfTypeAll(type);
		}

		public static T[] FindObjectsOfTypeAll<T>() where T : Object
		{
			return Resources.FindObjectsOfTypeAll<T>();
		}

		#endregion

		#region FindObjectOfTypeAllEnsured

		public static Object FindObjectOfTypeAllEnsured(Type type)
		{
			var objects = FindObjectsOfTypeAllEnsured(type);
			return objects[0];
		}

		public static T FindObjectOfTypeAllEnsured<T>() where T : Object
		{
			var objects = FindObjectsOfTypeAllEnsured<T>();
			return objects[0];
		}

		#endregion

		#region FindObjectsOfTypeAllEnsured

		public static Object[] FindObjectsOfTypeAllEnsured(Type type)
		{
			var objects = FindObjectsOfTypeAll(type);
			if (objects == null || objects.Length == 0)
				throw new Exception("Could not find objects of type '" + type.Name + "'");
			return objects;
		}

		public static T[] FindObjectsOfTypeAllEnsured<T>() where T : Object
		{
			var objects = FindObjectsOfTypeAll<T>();
			if (objects == null || objects.Length == 0)
				throw new Exception("Could not find object of type '" + typeof(T).Name + "'");
			return objects;
		}

		#endregion

		#region FindSingleObjectOfTypeEnsured

		public static Object FindSingleObjectOfTypeAllEnsured(Type type)
		{
			var objects = FindObjectsOfTypeAllEnsured(type);
			if (objects.Length > 1)
				throw new Exception("There are multiple instances for object of type '" + type.Name + "'");
			return objects[0];
		}

		public static T FindSingleObjectOfTypeAllEnsured<T>() where T : Object
		{
			var objects = FindObjectsOfTypeAllEnsured<T>();
			if (objects.Length > 1)
				throw new Exception("There are multiple instances for object of type '" + typeof(T).Name + "'");
			return objects[0];
		}

		#endregion
	}

}
