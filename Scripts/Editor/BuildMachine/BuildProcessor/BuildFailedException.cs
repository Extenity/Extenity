using System;

namespace Extenity.BuildMachine.Editor
{

	public class BuildFailedException : Exception
	{
		private const string Prefix = "[Builder] ";

		public BuildFailedException(string message)
			: base(Prefix + message)
		{
		}

		public BuildFailedException(string message, Exception innerException)
			: base(Prefix + message, innerException)
		{
		}
	}

}