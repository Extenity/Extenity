using Extenity.FileSystemToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.FileSystemToolbox
{

	public class Test_FileTools : ExtenityTestBase
	{
		#region String Operations - File Size

		[Test]
		public static void ToFileSizeString()
		{

			// Positive values
			{
				Assert.AreEqual("1 B", (1L).ToFileSizeString());
				Assert.AreEqual("10 B", (10L).ToFileSizeString());
				Assert.AreEqual("999 B", (999L).ToFileSizeString());
				Assert.AreEqual("1 KB", (1000L).ToFileSizeString());
				Assert.AreEqual("1 KB", (1099L).ToFileSizeString());
				Assert.AreEqual("1.1 KB", (1100L).ToFileSizeString());
				Assert.AreEqual("1.1 KB", (1199L).ToFileSizeString());
				Assert.AreEqual("1.2 KB", (1200L).ToFileSizeString());
				Assert.AreEqual("1.9 KB", (1999L).ToFileSizeString());
				Assert.AreEqual("2 KB", (2000L).ToFileSizeString());
				Assert.AreEqual("2 KB", (2099L).ToFileSizeString());
				Assert.AreEqual("2.1 KB", (2100L).ToFileSizeString());
				Assert.AreEqual("999 KB", (999_000L).ToFileSizeString());
				Assert.AreEqual("999 KB", (999_099L).ToFileSizeString());
				Assert.AreEqual("999.1 KB", (999_100L).ToFileSizeString());
				Assert.AreEqual("999.9 KB", (999_999L).ToFileSizeString());
				Assert.AreEqual("1 MB", (1_000_000L).ToFileSizeString());
				Assert.AreEqual("1 MB", (1_099_000L).ToFileSizeString());
				Assert.AreEqual("1 MB", (1_099_999L).ToFileSizeString());
				Assert.AreEqual("1.1 MB", (1_100_000L).ToFileSizeString());

				Assert.AreEqual("999.9 MB", (999_999_999L).ToFileSizeString());
				Assert.AreEqual("1 GB", (1_000_000_000L).ToFileSizeString());
				Assert.AreEqual("1 GB", (1_099_999_999L).ToFileSizeString());
				Assert.AreEqual("1.1 GB", (1_100_000_000L).ToFileSizeString());

				Assert.AreEqual("999.9 GB", (999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("1 TB", (1_000_000_000_000L).ToFileSizeString());
				Assert.AreEqual("1 TB", (1_099_999_999_999L).ToFileSizeString());
				Assert.AreEqual("1.1 TB", (1_100_000_000_000L).ToFileSizeString());

				Assert.AreEqual("999.9 TB", (999_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("1 PB", (1_000_000_000_000_000L).ToFileSizeString());
				Assert.AreEqual("1 PB", (1_099_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("1.1 PB", (1_100_000_000_000_000L).ToFileSizeString());

				Assert.AreEqual("999.9 PB", (999_999_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("1 EB", (1_000_000_000_000_000_000L).ToFileSizeString());
				Assert.AreEqual("1 EB", (1_099_999_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("1.1 EB", (1_100_000_000_000_000_000L).ToFileSizeString());
			}

			// Negative values
			{
				Assert.AreEqual("-1 B", (-1L).ToFileSizeString());
				Assert.AreEqual("-10 B", (-10L).ToFileSizeString());
				Assert.AreEqual("-999 B", (-999L).ToFileSizeString());
				Assert.AreEqual("-1 KB", (-1000L).ToFileSizeString());
				Assert.AreEqual("-1 KB", (-1099L).ToFileSizeString());
				Assert.AreEqual("-1.1 KB", (-1100L).ToFileSizeString());
				Assert.AreEqual("-1.1 KB", (-1199L).ToFileSizeString());
				Assert.AreEqual("-1.2 KB", (-1200L).ToFileSizeString());
				Assert.AreEqual("-1.9 KB", (-1999L).ToFileSizeString());
				Assert.AreEqual("-2 KB", (-2000L).ToFileSizeString());
				Assert.AreEqual("-2 KB", (-2099L).ToFileSizeString());
				Assert.AreEqual("-2.1 KB", (-2100L).ToFileSizeString());
				Assert.AreEqual("-999 KB", (-999_000L).ToFileSizeString());
				Assert.AreEqual("-999 KB", (-999_099L).ToFileSizeString());
				Assert.AreEqual("-999.1 KB", (-999_100L).ToFileSizeString());
				Assert.AreEqual("-999.9 KB", (-999_999L).ToFileSizeString());
				Assert.AreEqual("-1 MB", (-1_000_000L).ToFileSizeString());
				Assert.AreEqual("-1 MB", (-1_099_000L).ToFileSizeString());
				Assert.AreEqual("-1 MB", (-1_099_999L).ToFileSizeString());
				Assert.AreEqual("-1.1 MB", (-1_100_000L).ToFileSizeString());

				Assert.AreEqual("-999.9 MB", (-999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1 GB", (-1_000_000_000L).ToFileSizeString());
				Assert.AreEqual("-1 GB", (-1_099_999_999L).ToFileSizeString());
				Assert.AreEqual("-1.1 GB", (-1_100_000_000L).ToFileSizeString());

				Assert.AreEqual("-999.9 GB", (-999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1 TB", (-1_000_000_000_000L).ToFileSizeString());
				Assert.AreEqual("-1 TB", (-1_099_999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1.1 TB", (-1_100_000_000_000L).ToFileSizeString());

				Assert.AreEqual("-999.9 TB", (-999_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1 PB", (-1_000_000_000_000_000L).ToFileSizeString());
				Assert.AreEqual("-1 PB", (-1_099_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1.1 PB", (-1_100_000_000_000_000L).ToFileSizeString());

				Assert.AreEqual("-999.9 PB", (-999_999_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1 EB", (-1_000_000_000_000_000_000L).ToFileSizeString());
				Assert.AreEqual("-1 EB", (-1_099_999_999_999_999_999L).ToFileSizeString());
				Assert.AreEqual("-1.1 EB", (-1_100_000_000_000_000_000L).ToFileSizeString());
			}

			// Edge cases
			{
				Assert.AreEqual("0 B", 0L.ToFileSizeString());
				Assert.AreEqual("0 B", (-0L).ToFileSizeString()); // There is no negative zero. It's just zero.

				Assert.AreEqual("9.2 EB", (long.MaxValue).ToFileSizeString());
				Assert.AreEqual("9.2 EB", (long.MaxValue - 1).ToFileSizeString());
				Assert.AreEqual("-9.2 EB", (long.MinValue + 1).ToFileSizeString());
				Assert.AreEqual("-9.2 EB", (long.MinValue).ToFileSizeString());

				// Assert.AreEqual(..., (ulong.MaxValue).ToFileSizeString()); // This is a compilation error.
				// Assert.AreEqual(..., FileTools.ToFileSizeString(ulong.MaxValue)); // This is a compilation error.

				Assert.AreEqual("2.1 GB", FileTools.ToFileSizeString(int.MaxValue));
				Assert.AreEqual("-2.1 GB", FileTools.ToFileSizeString(int.MinValue));

				Assert.AreEqual("4.2 GB", FileTools.ToFileSizeString(uint.MaxValue));
			}

			// Some random values
			{
				Assert.AreEqual("1 KB", (1_000L).ToFileSizeString());
				Assert.AreEqual("1 KB", (1_024L).ToFileSizeString());
				Assert.AreEqual("1 KB", (1_025L).ToFileSizeString());
				Assert.AreEqual("123.4 MB", (123_456_789L).ToFileSizeString());
				Assert.AreEqual("1.2 GB", (1_234_567_890L).ToFileSizeString());
			}

			// ReSharper restore InvokeAsExtensionMethod
		}

		#endregion
	}

}