using System;
using UnityEngine;

namespace SmartData.Test
{

	public class SmartVarTestInstantiator : MonoBehaviour
	{
		public GameObject SmartVarTestPrefab;

		public bool RegisterBallPositionFromOutside = true;

		private static int ballId = 0;

		protected void Update()
		{
			if (Input.GetKeyDown("e"))
			{
				Debug.Log("Instantiating new test object");
				var go = Instantiate(SmartVarTestPrefab);
				go.name = "Ball-" + (++ballId);
				if (RegisterBallPositionFromOutside)
				{
					var smartVarTest = go.GetComponent<SmartVarTest>();
					smartVarTest.Ball.OnPositionChanged.AddListener(OnBallPositionChanged);
				}
			}
			if (Input.GetKeyDown("g"))
			{
				Debug.Log("Running garbage collector");
				GC.Collect();
			}
		}

		private void OnBallPositionChanged(Transform ball, object tag)
		{
			Debug.LogFormat("Outside: Ball position changed for {0} : {1}", ball.name, ball.position);
		}
	}

}
