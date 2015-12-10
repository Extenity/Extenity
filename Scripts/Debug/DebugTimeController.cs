using Extenity.Logging;
using UnityEngine;
using Logger = Extenity.Logging.Logger;

public class DebugTimeController : MonoBehaviour
{
	private float[] timeFactors;

	void Awake()
	{
		int i = 0;
		timeFactors = new float[9];
		timeFactors[i++] = 0f;
		timeFactors[i++] = 0.25f;
		timeFactors[i++] = 0.5f;
		timeFactors[i++] = 0.75f;
		timeFactors[i++] = 1f;
		timeFactors[i++] = 1.5f;
		timeFactors[i++] = 2.0f;
		timeFactors[i++] = 5.0f;
		timeFactors[i++] = 20.0f;
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			for (int i = 0; i < timeFactors.Length; i++)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + i))
				{
					SetTimeScale(timeFactors[i]);
				}
			}
		}
	}

	private void SetTimeScale(float scale)
	{
		Time.timeScale = scale;
		Logger.Log("Time scale set to " + scale);
	}
}
