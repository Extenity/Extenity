
namespace Extenity.MessagingToolbox
{

	public enum ListenerLifeSpan : byte
	{
		Permanent,
		RemovedAtFirstEmit,
	}

	public static class ExtenityEventTools
	{
		#region Log

		public static bool VerboseLogging = false; // Check this at the location of method call for performance reasons.

		#endregion
	}

}
