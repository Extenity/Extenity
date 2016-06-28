using AdvancedInspector;
using UnityEngine;

namespace SmartData.Test
{

	[AdvancedInspector(true, false)]
	public class SmartVarTest : MonoBehaviour
	{
		public SmartTransform Ball = new SmartTransform();
		public bool RemoveListenerOnDestroy = true;

		protected void Awake()
		{
			Ball.OnPositionChanged.AddListener(OnBallPositionChanged);
		}

		protected void OnDestroy()
		{
			if (RemoveListenerOnDestroy)
			{
				Ball.OnPositionChanged.RemoveListener(OnBallPositionChanged);
			}
		}

		private void OnBallPositionChanged(Transform ball, object tag)
		{
			Debug.Log("Ball position changed: " + ball.position);
		}

		protected void Update()
		{
			var speed = 20f * Time.deltaTime;

			if (Input.GetKeyDown("w")) Ball.position += new Vector3(0f, speed, 0f);
			if (Input.GetKeyDown("s")) Ball.position += new Vector3(0f, -speed, 0f);
			if (Input.GetKeyDown("a")) Ball.position += new Vector3(-speed, 0f, 0f);
			if (Input.GetKeyDown("d")) Ball.position += new Vector3(speed, 0f, 0f);
		}
	}

}
