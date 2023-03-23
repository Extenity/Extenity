using System;

namespace Extenity.BuildMachine.Editor
{

	public class BuildMachineException : Exception
	{
		private const string Prefix = "[Builder] ";

		public BuildMachineException(string message)
			: base(Prefix + message)
		{
		}

		public BuildMachineException(string message, Exception innerException)
			: base(Prefix + message, innerException)
		{
		}
	}

}