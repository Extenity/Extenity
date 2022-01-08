using System;

namespace Extenity
{

	public class InternalException : Exception
	{
		public InternalException(int errorCode)
			: base(Log.BuildInternalErrorMessage(errorCode))
		{
		}

		public InternalException(int errorCode, Exception innerException)
			: base(Log.BuildInternalErrorMessage(errorCode), innerException)
		{
		}
	}

}
