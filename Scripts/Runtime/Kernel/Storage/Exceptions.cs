#if ExtenityKernel

using System;

namespace Extenity.KernelToolbox
{

	public class DataNotFoundException : Exception
	{
		public DataNotFoundException(OID oid)
			: base($"Failed to find data of object '{oid.ToString()}'.")
		{
		}
	}

	public class CorruptDataException : Exception
	{
		public CorruptDataException(OID oid)
			: base($"Detected corrupt data for object '{oid.ToString()}'.")
		{
		}
	}

}

#endif
