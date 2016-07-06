using UnityEngine;

namespace Extenity.Debugging
{

	public class DebugTimeController : MonoBehaviour
	{
		private float[] timeFactors;

		private void Awake()
		{
			if (!Application.isEditor)
			{
				Destroy(this);
				return;
			}

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

		private void Update()
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
			Debug.Log("Time scale set to " + scale);
		}
	}

}
