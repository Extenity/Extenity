using System;
using System.IO;

namespace Extenity.DataToolbox
{

	public static class StreamTools
	{
		#region Deinitialization

		public static void CloseAndDisposeSafe(ref Stream stream)
		{
			if (stream == null)
				return;

			try
			{
				stream.Close();
				stream.Dispose();
				stream = null;
			}
			catch
			{
				// ignored
			}
		}

		public static void CloseAndDisposeEnsured(ref Stream stream)
		{
			if (stream == null)
				return;

			stream.Close();
			stream.Dispose();
			stream = null;
		}

		#endregion

		#region Compare

		public static bool CompareStreamContents(this Stream stream1, Stream stream2)
		{
			const int bufferSize = 1024 * sizeof(Int64);
			var buffer1 = new byte[bufferSize];
			var buffer2 = new byte[bufferSize];

			while (true)
			{
				var count1 = stream1.Read(buffer1, 0, bufferSize);
				var count2 = stream2.Read(buffer2, 0, bufferSize);

				if (count1 != count2)
					return false;

				if (count1 == 0)
					return true;

				var iterations = (int)System.Math.Ceiling((double)count1 / sizeof(Int64));
				for (var i = 0; i < iterations; i++)
				{
					if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) !=
						BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
					{
						return false;
					}
				}
			}
		}

		#endregion

		#region Stream Configuration

		public static bool TrySetStreamReadTimeout(this Stream stream, int readTimeout)
		{
			try
			{
				stream.ReadTimeout = readTimeout;
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}

}
