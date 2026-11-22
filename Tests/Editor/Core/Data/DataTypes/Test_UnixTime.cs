using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_UnixTime : ExtenityTestBase
	{
		#region String Conversions

		[Test]
		public void ToStringAndParse()
		{
			// The text is the same whatever the culture of the machine, and reads back to the same value.
			AssertToStringAndParse();
			Test_StringTools.RunInCommaDecimalSeparatorCulture(AssertToStringAndParse);
		}

		private static void AssertToStringAndParse()
		{
			Assert.AreEqual("1700000000.5", new UnixTime(1700000000.5).ToString());
			Assert.AreEqual("-0.25", new UnixTime(-0.25).ToString());
			Assert.AreEqual("1700000000", new UnixTime(1700000000.5).ToStringInt());

			Assert.AreEqual(1700000000.5, UnixTime.Parse("1700000000.5").Time);
			Assert.AreEqual(-0.25, UnixTime.Parse("-0.25").Time);
			Assert.AreEqual(UnixTime.Origin.AddSeconds(86400.5), UnixTime.ConvertFrom("86400.5"));
		}

		[Test]
		public void Parse_ThrowsOnDecimalComma()
		{
			// A timestamp written with a decimal comma is not read as a number with a thousands separator.
			AssertParseThrowsOnDecimalComma();
			Test_StringTools.RunInCommaDecimalSeparatorCulture(AssertParseThrowsOnDecimalComma);
		}

		private static void AssertParseThrowsOnDecimalComma()
		{
			Assert.Throws<FormatException>(() => UnixTime.Parse("1700000000,5"));
			Assert.Throws<FormatException>(() => UnixTime.ConvertFrom("86400,5"));
		}

		#endregion
	}

}
