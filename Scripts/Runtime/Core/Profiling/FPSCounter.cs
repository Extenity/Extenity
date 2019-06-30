using UnityEngine;
using Extenity.DesignPatternsToolbox;

namespace Extenity.ProfilingToolbox
{

	public class FPSCounter : SingletonUnity<FPSCounter>
	{
		private const int qualityLowFPS = 25;
		private const int qualityHighFPS = 45;
		private const float qualityInvFPSDiff = 1f / (qualityHighFPS - qualityLowFPS);

		private float lastUpdateTime;
		private int counter;
		private int lastFPS;
		private float qualityRatio;

		public delegate void OnFPSUpdate(int fps);

		public event OnFPSUpdate onFPSUpdate;

		void Awake()
		{
			InitializeSingleton(true);

			lastUpdateTime = Time.realtimeSinceStartup;
		}

		void Update()
		{
			counter++;

			if (Time.realtimeSinceStartup - lastUpdateTime >= 1f)
			{
				lastFPS = counter;
				qualityRatio = (lastFPS - qualityLowFPS) * qualityInvFPSDiff;
				lastUpdateTime = Time.realtimeSinceStartup;
				counter = 0;

				if (onFPSUpdate != null)
				{
					onFPSUpdate(lastFPS);
				}
			}
		}

		public int FPS
		{
			get { return lastFPS; }
		}
	}

}
