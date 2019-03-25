using Extenity.DesignPatternsToolbox;

namespace Extenity.UIToolbox
{

	public class BlackoutUI : SingletonUnity<BlackoutUI>
	{
		#region Initialization

		private void Awake()
		{
			InitializeSingleton(true);
		}

		#endregion

		#region Fade

		// TODO: Make it count the Show and Hide requests and act accordingly, i.e. hide only if all the Show requests canceled out with a respective Hide request.

		public UIFader Fader;

		public void Blackout(bool immediate = false)
		{
			//Log.Info("Activating blackout" + (immediate ? " immediately" : ""));

			if (immediate)
			{
				Fader.FadeInImmediate();
			}
			else
			{
				Fader.FadeIn();
			}
		}

		public void Clear(bool immediate = false)
		{
			//Log.Info("Clearing blackout" + (immediate ? " immediately" : ""));

			if (immediate)
			{
				Fader.FadeOutImmediate();
			}
			else
			{
				Fader.FadeOut();
			}
		}

		#endregion
	}

}
