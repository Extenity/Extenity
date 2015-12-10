using System;

public static class GuidExt
{
	public static Guid ToGuid(this byte[] bytes)
	{
		return new Guid(bytes);
	}
}
