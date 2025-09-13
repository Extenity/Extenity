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

		#region Copy

		/// <summary>
		/// Copy of <see cref="Stream.CopyTo"/> internals.
		/// </summary>
		public static void CopyTo(Stream source, Stream destination)
		{
			// TODO: Optimization. Search for all "new byte[" in code and see if making a ThreadSafe general buffer is more memory friendly or not. 
			var bytes = new byte[4096];
			int count;
			while ((count = source.Read(bytes, 0, bytes.Length)) != 0)
			{
				destination.Write(bytes, 0, count);
			}
		}

		#endregion

		#region Compare

		public static bool CompareStreamContents(this Stream stream1, Stream stream2)
		{
			const int BufferSize = 1024 * sizeof(Int64);
			var buffer1 = new byte[BufferSize];
			var buffer2 = new byte[BufferSize];

			while (true)
			{
				var count1 = stream1.Read(buffer1, 0, BufferSize);
				var count2 = stream2.Read(buffer2, 0, BufferSize);

				if (count1 != count2)
					return false;

				if (count1 == 0)
					return true;

				var iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
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
