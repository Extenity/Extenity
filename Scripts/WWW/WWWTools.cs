using UnityEngine;
using System.Collections;

namespace Extenity.WWWToolbox
{

	public static class WWWTools
	{
		#region Singleton

		internal class WWWToolsHelper : MonoBehaviour
		{
			private static WWWToolsHelper _Instance;
			internal static WWWToolsHelper Instance
			{
				get
				{
					if (_Instance == null)
					{
						var go = new GameObject("_WWWToolsHelper");
						go.hideFlags = HideFlags.HideAndDontSave;
						GameObject.DontDestroyOnLoad(go);
						_Instance = go.AddComponent<WWWToolsHelper>();
					}
					return _Instance;
				}
			}
		}

		#endregion

		#region Query

		public delegate void QueryResultDelegate(object queryTag, WWW www);

		public static void Query(string url, QueryResultDelegate onQuerySucceeded, QueryResultDelegate onQueryFailed = null, object queryTag = null)
		{
			WWWToolsHelper.Instance.StartCoroutine(DoQuery(url, onQuerySucceeded, onQueryFailed, queryTag));
		}

		public static IEnumerator DoQuery(string url, QueryResultDelegate onQuerySucceeded, QueryResultDelegate onQueryFailed, object queryTag)
		{
			if (string.IsNullOrEmpty(url))
				yield break;

			using (var request = new WWW(url))
			{
				yield return request;

				if (!string.IsNullOrEmpty(request.error))
				{
					if (onQueryFailed != null)
					{
						onQueryFailed(queryTag, request);
					}
				}
				else
				{
					if (onQuerySucceeded != null)
					{
						onQuerySucceeded(queryTag, request);
					}
				}
			}
		}

		#endregion
	}

}
