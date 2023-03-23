namespace Extenity.BuildMachine.Editor
{

	// TODO-Log: This is a temporary solution. It should be replaced with Logger.
	public static class BuilderLog
	{
		private static readonly string Prefix = "[Builder] ";

		public static void Info(string message)
		{
			Log.Info(Prefix + message);
		}

		public static void Warning(string message)
		{
			Log.Warning(Prefix + message);
		}

		public static void Error(string message)
		{
			Log.Error(Prefix + message);
		}

		public static void Fatal(string message)
		{
			Log.Fatal(Prefix + message);
		}
	}

}
