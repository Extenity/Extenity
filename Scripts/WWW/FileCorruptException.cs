using System;

namespace Extenity.WorldWideWeb
{

	public class FileCorruptException : Exception
	{
		public FileCorruptException(string message)
			: base(message)
		{
		}

		public FileCorruptException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

}
