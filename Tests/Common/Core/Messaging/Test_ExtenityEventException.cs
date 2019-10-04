using System;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenityEventException : Exception
	{
		public Test_ExtenityEventException(string message)
			: base(message)
		{
		}
	}

}
