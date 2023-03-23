//#define EnableBlackoutUILog

using System.Diagnostics;
using Extenity.DebugToolbox;
using Extenity.DesignPatternsToolbox;

namespace Extenity.UIToolbox
{

	public class BlackoutUI : SingletonUnity<BlackoutUI>
	{
		#region Initialization

		protected override void AwakeDerived()
		{
			gameObject.SetAsLogContext(ref Log);
		}

		#endregion

		#region Fade

		public UIFader Fader;

		// TODO: Decided not to go that way. Instead of using a BoolCounter, use a FlaggedBool.
		//private readonly BoolCounter RequestCounter = new BoolCounter();

		public void Blackout(bool immediate = false)
		{
			//var justSwitchedOn = RequestCounter.Increase();
			//Log.Info($"Blackout '{(immediate ? "immediate" : "non-immediate")}' activation requested ({RequestCounter.Counter.ToStringWithEnglishPluralPostfix("active request")})");

			Log.Info($"Blackout '{(immediate ? "immediate" : "non-immediate")}' activation requested");

			//if (justSwitchedOn) // Do not apply fading on consecutive calls.
			{
				if (immediate)
				{
					Fader.FadeInImmediate();
				}
				else
				{
					Fader.FadeIn();
				}
			}
		}

		public void Clear(bool immediate = false)
		{
			//var justSwitchedOff = RequestCounter.Decrease();
			//Log.Info($"Blackout '{(immediate ? "immediate" : "non-immediate")}' clear requested ({RequestCounter.Counter.ToStringWithEnglishPluralPostfix("active request")})");

			Log.Info($"Blackout '{(immediate ? "immediate" : "non-immediate")}' clear requested");

			//if (justSwitchedOff) // Do not apply fading on consecutive calls.
			{
				if (immediate)
				{
					Fader.FadeOutImmediate();
				}
				else
				{
					Fader.FadeOut();
				}
			}
		}

		#endregion

		#region Editor

#if UNITY_EDITOR
		protected void OnValidate()
		{
			if (Fader)
			{
				if (Fader.FadeInDelay != 0f || Fader.FadeOutDelay != 0f)
				{
					UnityEditor.Undo.RecordObject(Fader, "Blackout fader delay correction");
					Log.WarningWithContext(this, "Blackout UI is expected to be launched immediately. So fader delays are not allowed. Fixing...");
					Fader.FadeInDelay = 0f;
					Fader.FadeOutDelay = 0f;
				}
			}
		}
#endif

		#endregion

		#region Log

		private Logger Log = new("BlackoutUI");

		#endregion
	}

}
