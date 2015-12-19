using System;

namespace Extenity.Resource
{
	public static class ResourceTools
	{
		#region FindObjectsOfType

		public static UnityEngine.Object[] FindObjectsOfType(Type type)
		{
			return UnityEngine.Resources.FindObjectsOfTypeAll(type);
		}

		public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
		{
			return UnityEngine.Resources.FindObjectsOfTypeAll<T>();
		}

		#endregion

		#region FindObjectOfTypeEnsured

		public static UnityEngine.Object FindObjectOfTypeEnsured(Type type)
		{
			var objects = FindObjectsOfTypeEnsured(type);
			return objects[0];
		}

		public static T FindObjectOfTypeEnsured<T>() where T : UnityEngine.Object
		{
			var objects = FindObjectsOfTypeEnsured<T>();
			return objects[0];
		}

		#endregion

		#region FindObjectsOfTypeEnsured

		public static UnityEngine.Object[] FindObjectsOfTypeEnsured(Type type)
		{
			var objects = FindObjectsOfType(type);
			if (objects == null || objects.Length == 0)
				throw new Exception("Could not find objects of type '" + type.Name + "'");
			return objects;
		}

		public static T[] FindObjectsOfTypeEnsured<T>() where T : UnityEngine.Object
		{
			var objects = FindObjectsOfType<T>();
			if (objects == null || objects.Length == 0)
				throw new Exception("Could not find object of type '" + typeof(T).Name + "'");
			return objects;
		}

		#endregion

		#region FindSingleObjectOfTypeEnsured

		public static UnityEngine.Object FindSingleObjectOfTypeEnsured(Type type)
		{
			var objects = FindObjectsOfTypeEnsured(type);
			if (objects.Length > 1)
				throw new Exception("There are multiple instances for object of type '" + type.Name + "'");
			return objects[0];
		}

		public static T FindSingleObjectOfTypeEnsured<T>() where T : UnityEngine.Object
		{
			var objects = FindObjectsOfTypeEnsured<T>();
			if (objects.Length > 1)
				throw new Exception("There are multiple instances for object of type '" + typeof(T).Name + "'");
			return objects[0];
		}

		#endregion
	}
}
