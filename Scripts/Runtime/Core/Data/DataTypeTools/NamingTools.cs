using UnityEngine;

namespace Extenity.DataToolbox
{

	public static class NamingTools
	{
		#region Default Names

		public const string NullGameObjectName = "[NA/GO]";
		public const string NullComponentName = "[NA/COM]";
		public const string NullObjectName = "[NA/OBJ]";
		public const string NullDelegateName = "[NA/DEL]";
		public static string NullDelegateNameWithMethod(string methodName) => "[NA/DEL/" + methodName + "]";
		public const string NullName = "[NA]";

		#endregion

		#region Name In Hierarchy

		public const int DefaultMaxHierarchyLevels = 100;

		#endregion
	}

}
