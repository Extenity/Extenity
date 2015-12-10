using UnityEngine;
using System.Collections;

public class WWWExt : MonoBehaviour
{
	#region Singleton

	private static WWWExt _Instance;
	private static WWWExt Instance
	{
		get
		{
			if (_Instance == null)
			{
				var go = new GameObject("_WWWExt");
				go.hideFlags = HideFlags.HideAndDontSave;
				GameObject.DontDestroyOnLoad(go);
				_Instance = go.AddComponent<WWWExt>();
			}
			return _Instance;
		}
	}

	#endregion

	#region Query

	public delegate void QueryResultDelegate(object queryTag, WWW www);

	public static void Query(string url, QueryResultDelegate onQuerySucceeded, QueryResultDelegate onQueryFailed = null, object queryTag = null)
	{
		Instance.StartCoroutine(DoQuery(url, onQuerySucceeded, onQueryFailed, queryTag));
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
