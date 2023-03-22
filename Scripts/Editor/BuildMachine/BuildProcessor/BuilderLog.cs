namespace Extenity.BuildMachine.Editor
{

	public static class BuilderLog
	{
		public static readonly string Prefix = "[Builder] ";

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
