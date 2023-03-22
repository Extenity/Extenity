#if (EnableDebugTimeControllerInBuilds || UNITY_EDITOR) && !ENABLE_INPUT_SYSTEM

using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.DebugToolbox
{

	public static class DebugTimeController
	{
		#region Configuration

		public static float[] TimeScales = new float[]
		{
			0.1f,
			0.25f,
			0.5f,
			0.75f,
			1f,
			1.25f,
			1.5f,
			2f,
			5f,
			15f,
		};

		#endregion

		#region Initialization

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Initialize()
		{
			DefaultFixedDeltaTime = Time.fixedDeltaTime;
			Loop.RegisterUpdate(CustomUpdate);
		}

		#endregion

		#region Input

		private static void CustomUpdate()
		{
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				if (Input.GetKeyDown(KeyCode.KeypadPlus))
				{
					IncreaseTimeScale();
				}
				if (Input.GetKeyDown(KeyCode.KeypadMinus))
				{
					DecreaseTimeScale();
				}
			}
		}

		#endregion

		#region Increase/Decrease Time Scale

		public static bool AlsoMatchFixedDeltaTimeOnSlowTimeScales = true;
		public static float DefaultFixedDeltaTime;

		public static void IncreaseTimeScale()
		{
			var currentIndex = FindClosestTimeScaleIndex(Time.timeScale);
			_SetTimeScale(currentIndex + 1);
		}

		public static void DecreaseTimeScale()
		{
			var currentIndex = FindClosestTimeScaleIndex(Time.timeScale);
			_SetTimeScale(currentIndex - 1);
		}

		private static void _SetTimeScale(int index)
		{
			index = clamp(index, 0, TimeScales.Length - 1);
			var timeScale = TimeScales[index];
			Time.timeScale = timeScale;
			if (AlsoMatchFixedDeltaTimeOnSlowTimeScales)
			{
				Time.fixedDeltaTime = timeScale < 1f ? DefaultFixedDeltaTime * timeScale : DefaultFixedDeltaTime;
				Log.Info("Time scale set to " + timeScale + " with fixedDeltaTime " + Time.fixedDeltaTime);
			}
			else
			{
				Log.Info("Time scale set to " + timeScale);
			}
#if UNITY_EDITOR
			var type = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");
			var gameView = UnityEditor.EditorWindow.GetWindow(type);
			gameView.ShowNotification(new GUIContent("Time scale " + timeScale), 0.5f);
#endif
		}

		#endregion

		#region Find Closest Time Scale Configuration

		private static int FindClosestTimeScaleIndex(float timeScale)
		{
			var closestDistance = float.MaxValue;
			var closestDistanceIndex = -1;
			for (int i = 0; i < TimeScales.Length; i++)
			{
				var distance = abs(TimeScales[i] - timeScale);
				if (distance < 0.001f)
				{
					return i;
				}
				if (closestDistance > distance)
				{
					closestDistance = distance;
					closestDistanceIndex = i;
				}
			}
			return closestDistanceIndex;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(DebugTimeController));

		#endregion
	}

}

#endif
