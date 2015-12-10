using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
	public float timer = 0f;
	public bool destroyImmediate = false;
	public bool removeFromParentOnAwake = false;
	public bool removeFromParentOnStart = false;

	void Awake()
	{
		if (removeFromParentOnAwake)
			transform.parent = null;
	}

	void Start()
	{
		if (removeFromParentOnStart)
			transform.parent = null;

		Invoke("DestroyOnTimer", timer);
	}

	void DestroyOnTimer()
	{
		if (destroyImmediate)
			DestroyImmediate(gameObject);
		else
			Destroy(gameObject);
	}

	public void SetDestroyTimer(float value)
	{
		timer = value;

		if (IsInvoking("DestroyOnTimer"))
			CancelInvoke("DestroyOnTimer");
		Invoke("DestroyOnTimer", timer);
	}
}
