using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace ExtenityTests.DataToolbox
{

	public class Test_StringTools : AssertionHelper
	{
		#region String Operations

		[Test]
		public static void ReplaceBetween()
		{
			var list = new List<KeyValue<string, string>>
			{
				new KeyValue<string, string>("OXFORD", "NOT BROGUES"),
				new KeyValue<string, string>("INNER", "Start <tag>OXFORD</tag> End"),
			};
			TestValue_ReplaceBetween(
				"Some text <tag>OXFORD</tag> and some more.", "<tag>", "</tag>", list, true,
				"Some text <tag>NOT BROGUES</tag> and some more.");
			TestValue_ReplaceBetween(
				"Some text <tag>OXFORD</tag> and some more.", "<tag>", "</tag>", list, false,
				"Some text NOT BROGUES and some more.");
			TestValue_ReplaceBetween(
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.", "<tag>", "</tag>", list, true,
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.");
			TestValue_ReplaceBetween(
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.", "<tag>", "</tag>", list, false,
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.");

			// Multiple
			TestValue_ReplaceBetween(
				"Some text <tag>OXFORD</tag> with <tag>OXFORD</tag> and some more.", "<tag>", "</tag>", list, true,
				"Some text <tag>NOT BROGUES</tag> with <tag>OXFORD</tag> and some more.");

			// Inner
			TestValue_ReplaceBetween(
				"Some text <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, true,
				"Some text <tag>Start <tag>OXFORD</tag> End</tag> and some more.");
		}

		[Test]
		public static void ReplaceBetweenAll()
		{
			var list = new List<KeyValue<string, string>>
			{
				new KeyValue<string, string>("OXFORD", "NOT BROGUES"),
				new KeyValue<string, string>("INNER", "Start <tag>OXFORD</tag> End"),
			};
			TestValue_ReplaceBetweenAll(
				"Some text <tag>OXFORD</tag> and some more.", "<tag>", "</tag>", list, true, true,
				"Some text <tag>NOT BROGUES</tag> and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>OXFORD</tag> and some more.", "<tag>", "</tag>", list, false, true,
				"Some text NOT BROGUES and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.", "<tag>", "</tag>", list, true, true,
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.", "<tag>", "</tag>", list, false, true,
				"Some text <tag>NON-EXISTING-KEY</tag> and some more.");

			// Multiple
			TestValue_ReplaceBetweenAll(
				"Some text <tag>OXFORD</tag> with <tag>OXFORD</tag> and some more.", "<tag>", "</tag>", list, true, true,
				"Some text <tag>NOT BROGUES</tag> with <tag>NOT BROGUES</tag> and some more.");

			// Inner (single)
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, true, true,
				"Some text <tag>Start <tag>OXFORD</tag> End</tag> and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, false, true,
				"Some text Start <tag>OXFORD</tag> End and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, true, false,
				"Some text <tag>Start <tag>NOT BROGUES</tag> End</tag> and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, false, false,
				"Some text Start NOT BROGUES End and some more.");

			// Inner (multiple)
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> with <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, true, true,
				"Some text <tag>Start <tag>OXFORD</tag> End</tag> with <tag>Start <tag>OXFORD</tag> End</tag> and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> with <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, false, true,
				"Some text Start <tag>OXFORD</tag> End with Start <tag>OXFORD</tag> End and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> with <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, true, false,
				"Some text <tag>Start <tag>NOT BROGUES</tag> End</tag> with <tag>Start <tag>NOT BROGUES</tag> End</tag> and some more.");
			TestValue_ReplaceBetweenAll(
				"Some text <tag>INNER</tag> with <tag>INNER</tag> and some more.", "<tag>", "</tag>", list, false, false,
				"Some text Start NOT BROGUES End with Start NOT BROGUES End and some more.");
		}

		private static void TestValue_ReplaceBetween(string text, string startTag, string endTag, List<KeyValue<string, string>> list, bool keepTags, string expectedResult)
		{
			var result = text.ReplaceBetween(startTag, endTag,
				key =>
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].Key == key)
							return list[i].Value;
					}
					return null;
				},
				keepTags
			);
			Assert.AreEqual(expectedResult, result);
		}

		private static void TestValue_ReplaceBetweenAll(string text, string startTag, string endTag, List<KeyValue<string, string>> list, bool keepTags, bool skipTagsInReplacedText, string expectedResult)
		{
			string result;
			text.ReplaceBetweenAll(startTag, endTag,
				key =>
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].Key == key)
							return list[i].Value;
					}
					return null;
				},
				keepTags,
				skipTagsInReplacedText,
				out result
			);
			Assert.AreEqual(expectedResult, result);
		}

		#endregion

		#region Number At The End

		[Test]
		public static void RemoveEndingNumberedParentheses()
		{
			Assert.AreEqual("Asd", "Asd".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd", "Asd".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd ", "Asd ".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd ", "Asd ".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd", "Asd ()".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd", "Asd (3)".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd", "Asd (12)".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd()", "Asd()".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd(3)", "Asd(3)".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd(12)", "Asd(12)".RemoveEndingNumberedParentheses('(', ')', true));
			Assert.AreEqual("Asd ", "Asd ()".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd ", "Asd (3)".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd ", "Asd (12)".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd", "Asd()".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd", "Asd(3)".RemoveEndingNumberedParentheses('(', ')', false));
			Assert.AreEqual("Asd", "Asd(12)".RemoveEndingNumberedParentheses('(', ')', false));
		}

		[Test]
		public static void IsEndingWithNumberedParentheses()
		{
			Assert.AreEqual(false, "Asd".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(false, "Asd".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(false, "Asd ".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(false, "Asd ".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(true, "Asd ()".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(true, "Asd (3)".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(true, "Asd (12)".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(false, "Asd()".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(false, "Asd(3)".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(false, "Asd(12)".IsEndingWithNumberedParentheses('(', ')', true));
			Assert.AreEqual(true, "Asd ()".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(true, "Asd (3)".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(true, "Asd (12)".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(true, "Asd()".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(true, "Asd(3)".IsEndingWithNumberedParentheses('(', ')', false));
			Assert.AreEqual(true, "Asd(12)".IsEndingWithNumberedParentheses('(', ')', false));
		}

		#endregion

		#region Conversions - Int ToStringAsCharArray

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_Int32()
		{
			TestValue_ToStringAsCharArray_Int32(0);
			TestValue_ToStringAsCharArray_Int32(1);
			TestValue_ToStringAsCharArray_Int32(-1);
			for (Int32 value = -10000; value < 10000; value += Random.Range(1, 500))
			{
				TestValue_ToStringAsCharArray_Int32(value);
			}
			TestValue_ToStringAsCharArray_Int32(-99999);
			TestValue_ToStringAsCharArray_Int32(-999999);
			TestValue_ToStringAsCharArray_Int32(-9999999);
			TestValue_ToStringAsCharArray_Int32(-99999999);
			TestValue_ToStringAsCharArray_Int32(-999999999);
			TestValue_ToStringAsCharArray_Int32(-10000);
			TestValue_ToStringAsCharArray_Int32(-100000);
			TestValue_ToStringAsCharArray_Int32(-1000000);
			TestValue_ToStringAsCharArray_Int32(-10000000);
			TestValue_ToStringAsCharArray_Int32(-100000000);
			TestValue_ToStringAsCharArray_Int32(-20000);
			TestValue_ToStringAsCharArray_Int32(-200000);
			TestValue_ToStringAsCharArray_Int32(-2000000);
			TestValue_ToStringAsCharArray_Int32(-20000000);
			TestValue_ToStringAsCharArray_Int32(-200000000);
			TestValue_ToStringAsCharArray_Int32(-2000000000);
			TestValue_ToStringAsCharArray_Int32(10000);
			TestValue_ToStringAsCharArray_Int32(100000);
			TestValue_ToStringAsCharArray_Int32(1000000);
			TestValue_ToStringAsCharArray_Int32(10000000);
			TestValue_ToStringAsCharArray_Int32(100000000);
			TestValue_ToStringAsCharArray_Int32(1000000000);
			TestValue_ToStringAsCharArray_Int32(20000);
			TestValue_ToStringAsCharArray_Int32(200000);
			TestValue_ToStringAsCharArray_Int32(2000000);
			TestValue_ToStringAsCharArray_Int32(20000000);
			TestValue_ToStringAsCharArray_Int32(200000000);
			TestValue_ToStringAsCharArray_Int32(2000000000);
			TestValue_ToStringAsCharArray_Int32(99999);
			TestValue_ToStringAsCharArray_Int32(999999);
			TestValue_ToStringAsCharArray_Int32(9999999);
			TestValue_ToStringAsCharArray_Int32(99999999);
			TestValue_ToStringAsCharArray_Int32(999999999);
			TestValue_ToStringAsCharArray_Int32(123456789);
			TestValue_ToStringAsCharArray_Int32(987654321);
			TestValue_ToStringAsCharArray_Int32(Int32.MinValue);
			TestValue_ToStringAsCharArray_Int32(Int32.MinValue + 1);
			TestValue_ToStringAsCharArray_Int32(Int32.MinValue + 2);
			TestValue_ToStringAsCharArray_Int32(Int32.MaxValue);
			TestValue_ToStringAsCharArray_Int32(Int32.MaxValue - 1);
			TestValue_ToStringAsCharArray_Int32(Int32.MaxValue - 2);
		}

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_Int64()
		{
			TestValue_ToStringAsCharArray_Int64(0);
			TestValue_ToStringAsCharArray_Int64(1);
			TestValue_ToStringAsCharArray_Int64(-1);
			for (Int64 value = -10000; value < 10000; value += Random.Range(1, 500))
			{
				TestValue_ToStringAsCharArray_Int64(value);
			}
			TestValue_ToStringAsCharArray_Int64(-99999);
			TestValue_ToStringAsCharArray_Int64(-999999);
			TestValue_ToStringAsCharArray_Int64(-9999999);
			TestValue_ToStringAsCharArray_Int64(-99999999);
			TestValue_ToStringAsCharArray_Int64(-999999999);
			TestValue_ToStringAsCharArray_Int64(-9999999999);
			TestValue_ToStringAsCharArray_Int64(-99999999999);
			TestValue_ToStringAsCharArray_Int64(-999999999999);
			TestValue_ToStringAsCharArray_Int64(-9999999999999);
			TestValue_ToStringAsCharArray_Int64(-99999999999999);
			TestValue_ToStringAsCharArray_Int64(-999999999999999);
			TestValue_ToStringAsCharArray_Int64(-9999999999999999);
			TestValue_ToStringAsCharArray_Int64(-99999999999999999);
			TestValue_ToStringAsCharArray_Int64(-999999999999999999);
			TestValue_ToStringAsCharArray_Int64(-10000);
			TestValue_ToStringAsCharArray_Int64(-100000);
			TestValue_ToStringAsCharArray_Int64(-1000000);
			TestValue_ToStringAsCharArray_Int64(-10000000);
			TestValue_ToStringAsCharArray_Int64(-100000000);
			TestValue_ToStringAsCharArray_Int64(-1000000000);
			TestValue_ToStringAsCharArray_Int64(-10000000000);
			TestValue_ToStringAsCharArray_Int64(-100000000000);
			TestValue_ToStringAsCharArray_Int64(-1000000000000);
			TestValue_ToStringAsCharArray_Int64(-10000000000000);
			TestValue_ToStringAsCharArray_Int64(-100000000000000);
			TestValue_ToStringAsCharArray_Int64(-1000000000000000);
			TestValue_ToStringAsCharArray_Int64(-10000000000000000);
			TestValue_ToStringAsCharArray_Int64(-100000000000000000);
			TestValue_ToStringAsCharArray_Int64(-1000000000000000000);
			TestValue_ToStringAsCharArray_Int64(-20000);
			TestValue_ToStringAsCharArray_Int64(-200000);
			TestValue_ToStringAsCharArray_Int64(-2000000);
			TestValue_ToStringAsCharArray_Int64(-200000000);
			TestValue_ToStringAsCharArray_Int64(-2000000000);
			TestValue_ToStringAsCharArray_Int64(-20000000000);
			TestValue_ToStringAsCharArray_Int64(-200000000000);
			TestValue_ToStringAsCharArray_Int64(-2000000000000);
			TestValue_ToStringAsCharArray_Int64(-20000000000000);
			TestValue_ToStringAsCharArray_Int64(-200000000000000);
			TestValue_ToStringAsCharArray_Int64(-2000000000000000);
			TestValue_ToStringAsCharArray_Int64(-20000000000000000);
			TestValue_ToStringAsCharArray_Int64(-200000000000000000);
			TestValue_ToStringAsCharArray_Int64(-2000000000000000000);
			TestValue_ToStringAsCharArray_Int64(10000);
			TestValue_ToStringAsCharArray_Int64(100000);
			TestValue_ToStringAsCharArray_Int64(1000000);
			TestValue_ToStringAsCharArray_Int64(10000000);
			TestValue_ToStringAsCharArray_Int64(100000000);
			TestValue_ToStringAsCharArray_Int64(1000000000);
			TestValue_ToStringAsCharArray_Int64(10000000000);
			TestValue_ToStringAsCharArray_Int64(100000000000);
			TestValue_ToStringAsCharArray_Int64(1000000000000);
			TestValue_ToStringAsCharArray_Int64(10000000000000);
			TestValue_ToStringAsCharArray_Int64(100000000000000);
			TestValue_ToStringAsCharArray_Int64(1000000000000000);
			TestValue_ToStringAsCharArray_Int64(10000000000000000);
			TestValue_ToStringAsCharArray_Int64(100000000000000000);
			TestValue_ToStringAsCharArray_Int64(1000000000000000000);
			TestValue_ToStringAsCharArray_Int64(20000);
			TestValue_ToStringAsCharArray_Int64(200000);
			TestValue_ToStringAsCharArray_Int64(2000000);
			TestValue_ToStringAsCharArray_Int64(200000000);
			TestValue_ToStringAsCharArray_Int64(2000000000);
			TestValue_ToStringAsCharArray_Int64(20000000000);
			TestValue_ToStringAsCharArray_Int64(200000000000);
			TestValue_ToStringAsCharArray_Int64(2000000000000);
			TestValue_ToStringAsCharArray_Int64(20000000000000);
			TestValue_ToStringAsCharArray_Int64(200000000000000);
			TestValue_ToStringAsCharArray_Int64(2000000000000000);
			TestValue_ToStringAsCharArray_Int64(20000000000000000);
			TestValue_ToStringAsCharArray_Int64(200000000000000000);
			TestValue_ToStringAsCharArray_Int64(2000000000000000000);
			TestValue_ToStringAsCharArray_Int64(99999);
			TestValue_ToStringAsCharArray_Int64(999999);
			TestValue_ToStringAsCharArray_Int64(9999999);
			TestValue_ToStringAsCharArray_Int64(99999999);
			TestValue_ToStringAsCharArray_Int64(999999999);
			TestValue_ToStringAsCharArray_Int64(9999999999);
			TestValue_ToStringAsCharArray_Int64(99999999999);
			TestValue_ToStringAsCharArray_Int64(999999999999);
			TestValue_ToStringAsCharArray_Int64(9999999999999);
			TestValue_ToStringAsCharArray_Int64(99999999999999);
			TestValue_ToStringAsCharArray_Int64(999999999999999);
			TestValue_ToStringAsCharArray_Int64(9999999999999999);
			TestValue_ToStringAsCharArray_Int64(99999999999999999);
			TestValue_ToStringAsCharArray_Int64(999999999999999999);
			TestValue_ToStringAsCharArray_Int64(123456789);
			TestValue_ToStringAsCharArray_Int64(987654321);
			TestValue_ToStringAsCharArray_Int64(Int32.MinValue);
			TestValue_ToStringAsCharArray_Int64(Int32.MinValue + 1);
			TestValue_ToStringAsCharArray_Int64(Int32.MinValue + 2);
			TestValue_ToStringAsCharArray_Int64(Int32.MaxValue);
			TestValue_ToStringAsCharArray_Int64(Int32.MaxValue - 1);
			TestValue_ToStringAsCharArray_Int64(Int32.MaxValue - 2);
			TestValue_ToStringAsCharArray_Int64(Int64.MinValue);
			TestValue_ToStringAsCharArray_Int64(Int64.MinValue + 1);
			TestValue_ToStringAsCharArray_Int64(Int64.MinValue + 2);
			TestValue_ToStringAsCharArray_Int64(Int64.MaxValue);
			TestValue_ToStringAsCharArray_Int64(Int64.MaxValue - 1);
			TestValue_ToStringAsCharArray_Int64(Int64.MaxValue - 2);
		}

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_WithThousandsSeparator_Int32()
		{
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(0);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-1);
			for (Int32 value = -10000; value < 10000; value += Random.Range(1, 500))
			{
				TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(value);
			}
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-99999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-9999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-99999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-10000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-100000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-1000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-10000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-100000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-20000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-200000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-2000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-20000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-200000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(-2000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(10000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(100000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(1000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(10000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(100000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(1000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(20000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(200000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(2000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(20000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(200000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(2000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(99999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(9999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(99999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(123456789);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(987654321);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32.MinValue);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32.MinValue + 1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32.MinValue + 2);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32.MaxValue);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32.MaxValue - 1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32.MaxValue - 2);
		}

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_WithThousandsSeparator_Int64()
		{
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(0);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-1);
			for (Int64 value = -10000; value < 10000; value += Random.Range(1, 500))
			{
				TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(value);
			}
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-99999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-9999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-99999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-9999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-99999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-9999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-99999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-9999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-99999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-999999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-10000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-100000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-1000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-10000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-100000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-1000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-10000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-100000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-1000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-10000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-100000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-1000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-10000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-100000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-1000000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-20000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-200000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-2000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-200000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-2000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-20000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-200000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-2000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-20000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-200000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-2000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-20000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-200000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(-2000000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(10000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(100000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(1000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(10000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(100000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(1000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(10000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(100000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(1000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(10000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(100000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(1000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(10000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(100000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(1000000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(20000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(200000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(2000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(200000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(2000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(20000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(200000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(2000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(20000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(200000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(2000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(20000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(200000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(2000000000000000000);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(99999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(9999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(99999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(9999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(99999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(9999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(99999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(9999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(99999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(999999999999999999);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(123456789);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(987654321);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int32.MinValue);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int32.MinValue + 1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int32.MinValue + 2);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int32.MaxValue);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int32.MaxValue - 1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int32.MaxValue - 2);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64.MinValue);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64.MinValue + 1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64.MinValue + 2);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64.MaxValue);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64.MaxValue - 1);
			TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64.MaxValue - 2);
		}

		private static void TestValue_ToStringAsCharArray_Int32(Int32 value)
		{
			var chars = new char[10 + 1];
			int startIndex;
			int length;
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, out startIndex, out length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString();
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArray_Int64(Int64 value)
		{
			var chars = new char[19 + 1];
			int startIndex;
			int length;
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, out startIndex, out length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString();
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32 value)
		{
			var chars = new char[10 + 1 + 3];
			int startIndex;
			int length;
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, ',', out startIndex, out length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString("N0");
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64 value)
		{
			var chars = new char[19 + 1 + 6];
			int startIndex;
			int length;
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, ',', out startIndex, out length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString("N0");
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		#endregion

		#region Conversions - Formatted Value ToStringAsCharArray

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_FormattedInt()
		{
			WarmUpFastNumberFormatter();

			TestValue_ToStringAsCharArray_FormattedInt(0);
			TestValue_ToStringAsCharArray_FormattedInt(1);
			TestValue_ToStringAsCharArray_FormattedInt(-1);
			for (Int64 value = -10000; value < 10000; value += Random.Range(1, 500))
			{
				TestValue_ToStringAsCharArray_FormattedInt(value);
			}
			TestValue_ToStringAsCharArray_FormattedInt(-99999);
			TestValue_ToStringAsCharArray_FormattedInt(-999999);
			TestValue_ToStringAsCharArray_FormattedInt(-9999999);
			TestValue_ToStringAsCharArray_FormattedInt(-99999999);
			TestValue_ToStringAsCharArray_FormattedInt(-999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-9999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-99999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-9999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-99999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-9999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-99999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-999999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(-10000);
			TestValue_ToStringAsCharArray_FormattedInt(-100000);
			TestValue_ToStringAsCharArray_FormattedInt(-1000000);
			TestValue_ToStringAsCharArray_FormattedInt(-10000000);
			TestValue_ToStringAsCharArray_FormattedInt(-100000000);
			TestValue_ToStringAsCharArray_FormattedInt(-1000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-10000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-100000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-1000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-10000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-100000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-1000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-10000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-100000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-1000000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-20000);
			TestValue_ToStringAsCharArray_FormattedInt(-200000);
			TestValue_ToStringAsCharArray_FormattedInt(-2000000);
			TestValue_ToStringAsCharArray_FormattedInt(-200000000);
			TestValue_ToStringAsCharArray_FormattedInt(-2000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-20000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-200000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-2000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-20000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-200000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-2000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-20000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-200000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(-2000000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(10000);
			TestValue_ToStringAsCharArray_FormattedInt(100000);
			TestValue_ToStringAsCharArray_FormattedInt(1000000);
			TestValue_ToStringAsCharArray_FormattedInt(10000000);
			TestValue_ToStringAsCharArray_FormattedInt(100000000);
			TestValue_ToStringAsCharArray_FormattedInt(1000000000);
			TestValue_ToStringAsCharArray_FormattedInt(10000000000);
			TestValue_ToStringAsCharArray_FormattedInt(100000000000);
			TestValue_ToStringAsCharArray_FormattedInt(1000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(10000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(100000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(1000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(10000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(100000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(1000000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(20000);
			TestValue_ToStringAsCharArray_FormattedInt(200000);
			TestValue_ToStringAsCharArray_FormattedInt(2000000);
			TestValue_ToStringAsCharArray_FormattedInt(200000000);
			TestValue_ToStringAsCharArray_FormattedInt(2000000000);
			TestValue_ToStringAsCharArray_FormattedInt(20000000000);
			TestValue_ToStringAsCharArray_FormattedInt(200000000000);
			TestValue_ToStringAsCharArray_FormattedInt(2000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(20000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(200000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(2000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(20000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(200000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(2000000000000000000);
			TestValue_ToStringAsCharArray_FormattedInt(99999);
			TestValue_ToStringAsCharArray_FormattedInt(999999);
			TestValue_ToStringAsCharArray_FormattedInt(9999999);
			TestValue_ToStringAsCharArray_FormattedInt(99999999);
			TestValue_ToStringAsCharArray_FormattedInt(999999999);
			TestValue_ToStringAsCharArray_FormattedInt(9999999999);
			TestValue_ToStringAsCharArray_FormattedInt(99999999999);
			TestValue_ToStringAsCharArray_FormattedInt(999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(9999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(99999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(9999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(99999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(999999999999999999);
			TestValue_ToStringAsCharArray_FormattedInt(123456789);
			TestValue_ToStringAsCharArray_FormattedInt(987654321);
			TestValue_ToStringAsCharArray_FormattedInt(Int32.MinValue);
			TestValue_ToStringAsCharArray_FormattedInt(Int32.MinValue + 1);
			TestValue_ToStringAsCharArray_FormattedInt(Int32.MinValue + 2);
			TestValue_ToStringAsCharArray_FormattedInt(Int32.MaxValue);
			TestValue_ToStringAsCharArray_FormattedInt(Int32.MaxValue - 1);
			TestValue_ToStringAsCharArray_FormattedInt(Int32.MaxValue - 2);
			TestValue_ToStringAsCharArray_FormattedInt(Int64.MinValue);
			TestValue_ToStringAsCharArray_FormattedInt(Int64.MinValue + 1);
			TestValue_ToStringAsCharArray_FormattedInt(Int64.MinValue + 2);
			TestValue_ToStringAsCharArray_FormattedInt(Int64.MaxValue);
			TestValue_ToStringAsCharArray_FormattedInt(Int64.MaxValue - 1);
			TestValue_ToStringAsCharArray_FormattedInt(Int64.MaxValue - 2);
		}

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_FormattedDouble()
		{
			WarmUpFastNumberFormatter();

			TestValue_ToStringAsCharArray_FormattedDouble(0);
			TestValue_ToStringAsCharArray_FormattedDouble(1);
			TestValue_ToStringAsCharArray_FormattedDouble(-1);
			for (double value = -10000d; value < 10000d; value += Random.Range(0.1f, 500.0f))
			{
				TestValue_ToStringAsCharArray_FormattedDouble(value);
			}
			TestValue_ToStringAsCharArray_FormattedDouble(-99999);
			TestValue_ToStringAsCharArray_FormattedDouble(-999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-9999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-99999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-9999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-99999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-9999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-99999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-9999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-99999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-999999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(-10000);
			TestValue_ToStringAsCharArray_FormattedDouble(-100000);
			TestValue_ToStringAsCharArray_FormattedDouble(-1000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-10000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-100000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-1000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-10000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-100000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-1000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-10000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-100000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-1000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-10000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-100000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-1000000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-20000);
			TestValue_ToStringAsCharArray_FormattedDouble(-200000);
			TestValue_ToStringAsCharArray_FormattedDouble(-2000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-200000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-2000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-20000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-200000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-2000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-20000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-200000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-2000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-20000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-200000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(-2000000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(10000);
			TestValue_ToStringAsCharArray_FormattedDouble(100000);
			TestValue_ToStringAsCharArray_FormattedDouble(1000000);
			TestValue_ToStringAsCharArray_FormattedDouble(10000000);
			TestValue_ToStringAsCharArray_FormattedDouble(100000000);
			TestValue_ToStringAsCharArray_FormattedDouble(1000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(10000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(100000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(1000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(10000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(100000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(1000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(10000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(100000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(1000000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(20000);
			TestValue_ToStringAsCharArray_FormattedDouble(200000);
			TestValue_ToStringAsCharArray_FormattedDouble(2000000);
			TestValue_ToStringAsCharArray_FormattedDouble(200000000);
			TestValue_ToStringAsCharArray_FormattedDouble(2000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(20000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(200000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(2000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(20000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(200000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(2000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(20000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(200000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(2000000000000000000);
			TestValue_ToStringAsCharArray_FormattedDouble(99999);
			TestValue_ToStringAsCharArray_FormattedDouble(999999);
			TestValue_ToStringAsCharArray_FormattedDouble(9999999);
			TestValue_ToStringAsCharArray_FormattedDouble(99999999);
			TestValue_ToStringAsCharArray_FormattedDouble(999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(9999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(99999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(9999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(99999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(9999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(99999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(999999999999999999);
			TestValue_ToStringAsCharArray_FormattedDouble(123456789);
			TestValue_ToStringAsCharArray_FormattedDouble(987654321);
			TestValue_ToStringAsCharArray_FormattedDouble(float.MinValue);
			TestValue_ToStringAsCharArray_FormattedDouble(float.MinValue + 1);
			TestValue_ToStringAsCharArray_FormattedDouble(float.MinValue + 2);
			TestValue_ToStringAsCharArray_FormattedDouble(float.MaxValue);
			TestValue_ToStringAsCharArray_FormattedDouble(float.MaxValue - 1);
			TestValue_ToStringAsCharArray_FormattedDouble(float.MaxValue - 2);
			TestValue_ToStringAsCharArray_FormattedDouble(double.MinValue);
			TestValue_ToStringAsCharArray_FormattedDouble(double.MinValue + 1);
			TestValue_ToStringAsCharArray_FormattedDouble(double.MinValue + 2);
			TestValue_ToStringAsCharArray_FormattedDouble(double.MaxValue);
			TestValue_ToStringAsCharArray_FormattedDouble(double.MaxValue - 1);
			TestValue_ToStringAsCharArray_FormattedDouble(double.MaxValue - 2);
		}

		private static void TestValue_ToStringAsCharArray_FormattedInt(Int64 value)
		{
			TestValue_ToStringAsCharArray_FormattedInt("C", value);
			TestValue_ToStringAsCharArray_FormattedInt("D", value);
			TestValue_ToStringAsCharArray_FormattedInt("e", value);
			TestValue_ToStringAsCharArray_FormattedInt("E", value);
			TestValue_ToStringAsCharArray_FormattedInt("F", value);
			TestValue_ToStringAsCharArray_FormattedInt("G", value);
			TestValue_ToStringAsCharArray_FormattedInt("N", value);
			TestValue_ToStringAsCharArray_FormattedInt("P", value);
			TestValue_ToStringAsCharArray_FormattedInt("X", value);

			for (int i = 0; i <= 10; i++)
			{
				TestValue_ToStringAsCharArray_FormattedInt("C" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("D" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("e" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("E" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("F" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("G" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("N" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("P" + i, value);
				TestValue_ToStringAsCharArray_FormattedInt("X" + i, value);
			}
		}

		private static void TestValue_ToStringAsCharArray_FormattedDouble(double value)
		{
			TestValue_ToStringAsCharArray_FormattedDouble("C", value);
			TestValue_ToStringAsCharArray_FormattedDouble("e", value);
			TestValue_ToStringAsCharArray_FormattedDouble("E", value);
			TestValue_ToStringAsCharArray_FormattedDouble("F", value);
			TestValue_ToStringAsCharArray_FormattedDouble("G", value);
			TestValue_ToStringAsCharArray_FormattedDouble("N", value);
			TestValue_ToStringAsCharArray_FormattedDouble("P", value);
			// This one allocates memory. We probably won't be interested in using it ever, so leave it as it is.
			//TestValue_ToStringAsCharArray_FormattedDouble("R", value);

			for (int i = 0; i <= 10; i++)
			{
				TestValue_ToStringAsCharArray_FormattedDouble("C" + i, value);
				TestValue_ToStringAsCharArray_FormattedDouble("e" + i, value);
				TestValue_ToStringAsCharArray_FormattedDouble("E" + i, value);
				TestValue_ToStringAsCharArray_FormattedDouble("F" + i, value);
				TestValue_ToStringAsCharArray_FormattedDouble("G" + i, value);
				TestValue_ToStringAsCharArray_FormattedDouble("N" + i, value);
				TestValue_ToStringAsCharArray_FormattedDouble("P" + i, value);
			}
		}

		private static void WarmUpFastNumberFormatter()
		{
			lock (BigBuffer)
			{
				// Warm up internal buffers of FastNumberFormatter
				for (int iDecimal = 0; iDecimal < 15; iDecimal++)
				{
					double.MaxValue.ToStringAsCharArray("C" + iDecimal, BigBuffer);
					//double.MaxValue.ToStringAsCharArray("D" + iDecimal, BigBuffer); Integers only
					double.MaxValue.ToStringAsCharArray("e" + iDecimal, BigBuffer);
					double.MaxValue.ToStringAsCharArray("E" + iDecimal, BigBuffer);
					double.MaxValue.ToStringAsCharArray("F" + iDecimal, BigBuffer);
					double.MaxValue.ToStringAsCharArray("G" + iDecimal, BigBuffer);
					double.MaxValue.ToStringAsCharArray("N" + iDecimal, BigBuffer);
					double.MaxValue.ToStringAsCharArray("P" + iDecimal, BigBuffer);
					//double.MaxValue.ToStringAsCharArray("X" + iDecimal, BigBuffer); Integers only
					Int64.MaxValue.ToStringAsCharArray("C" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("D" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("e" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("E" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("F" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("G" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("N" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("P" + iDecimal, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("X" + iDecimal, BigBuffer);
				}
			}
		}

		private static void TestValue_ToStringAsCharArray_FormattedInt(string format, Int64 value)
		{
			TestValue_ToStringAsCharArray_FormattedValue(format, value, chars => value.ToStringAsCharArray(format, chars), () => value.ToString(format));
			TestValue_ToStringAsCharArray_FormattedValue(format, value, chars => ((Int32)value).ToStringAsCharArray(format, chars), () => ((Int32)value).ToString(format));
		}

		private static void TestValue_ToStringAsCharArray_FormattedDouble(string format, double value)
		{
			TestValue_ToStringAsCharArray_FormattedValue(format, value, chars => value.ToStringAsCharArray(format, chars), () => value.ToString(format));
			TestValue_ToStringAsCharArray_FormattedValue(format, value, chars => ((float)value).ToStringAsCharArray(format, chars), () => ((float)value).ToString(format));
		}

		private static void TestValue_ToStringAsCharArray_FormattedValue(string format, object value, Func<char[], int> convertToCharArray, Func<string> convertToString)
		{
			lock (BigBuffer)
			{
				BigBuffer.FillRandomly();
				UnityTestTools.BeginMemoryCheck();
				var length = convertToCharArray(BigBuffer);
				if (UnityTestTools.EndMemoryCheck())
					Assert.Fail("Memory allocated while converting value '" + value + "' with format '" + format + "' to string resulting '" + BigBuffer.ConvertToString(0, length) + "'.");
				var originalString = convertToString();
				var resultString = BigBuffer.ConvertToString(0, length);
				if (!originalString.Equals(resultString))
				{
					Debug.LogError("Erroneous value generated while converting value '" + value + "' with format '" + format + "' to string resulting '" + BigBuffer.ConvertToString(0, length) + "'.");
				}
				Assert.AreEqual(originalString, resultString);
				Assert.AreEqual(originalString.Length, length);
			}
		}

		#endregion

		#region Conversions - Time

		[Test]
		public void ToStringMinutesSecondsMillisecondsFromSeconds()
		{
			Assert.AreEqual("0:00.000", (0d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:01.000", (1d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:02.000", (2d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:09.000", (9d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:10.000", (10d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:11.000", (11d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:59.000", (59d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("1:00.000", (60d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("1:01.000", (61d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("1:02.000", (62d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("1:59.000", (119d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("2:02.000", (122d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("9:59.000", (599d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("10:00.000", (600d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("11:00.000", (660d).ToStringMinutesSecondsMillisecondsFromSeconds());

			Assert.AreEqual("0:00.001", (0.001d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:00.001", (0.0014d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:00.002", (0.0016d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:00.999", (0.9994d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:01.000", (0.9996d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:01.001", (1.0014d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:01.002", (1.0016d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:10.001", (10.0014d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:10.002", (10.0016d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:59.001", (59.0014d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:59.002", (59.0016d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("0:59.999", (59.9994d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("1:00.000", (59.9996d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("1:59.999", (119.9994d).ToStringMinutesSecondsMillisecondsFromSeconds());
			Assert.AreEqual("2:00.000", (119.9996d).ToStringMinutesSecondsMillisecondsFromSeconds());
		}

		[Test]
		public void ToStringMillisecondsFromSeconds()
		{
			Assert.AreEqual("0.000", (0d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1000.000", (1d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("2000.000", (2d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("9000.000", (9d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("10000.000", (10d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("11000.000", (11d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("59000.000", (59d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("60000.000", (60d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("61000.000", (61d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("99000.000", (99d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("100000.000", (100d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1000000.000", (1000d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("10000000.000", (10000d).ToStringMillisecondsFromSeconds());

			Assert.AreEqual("0.001", (0.000001d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1.000", (0.001d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1.001", (0.001001d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1.001", (0.0010014d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1.002", (0.0010016d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("1.999", (0.0019994d).ToStringMillisecondsFromSeconds());
			Assert.AreEqual("2.000", (0.0019996d).ToStringMillisecondsFromSeconds());
		}

		#endregion

		#region Smart Format

		[Test]
		public void SmartFormat()
		{
			TestSmartFormat(null, "", false, false, false, -1, -1);
			TestSmartFormat("", "", false, false, false, -1, -1);
			TestSmartFormat("asd", "asd", false, false, false, -1, -1);

			// trim
			TestSmartFormat(" ", " ", false, false, false, -1, -1);
			TestSmartFormat(" ", "", true, false, false, -1, -1);
			TestSmartFormat("   ", "", true, false, false, -1, -1);
			TestSmartFormat("   \n   \r\n   ", "", true, false, false, -1, -1);
			TestSmartFormat(" asd ", " asd ", false, false, false, -1, -1);
			TestSmartFormat(" asd ", "asd", true, false, false, -1, -1);
			TestSmartFormat("\nasd\n", "\nasd\n", false, false, false, -1, -1);
			TestSmartFormat("\nasd\n", "asd", true, false, false, -1, -1);
			TestSmartFormat(
				"  \r\n \r\n\n  asd  \r\n \r\n\n  ",
				"  \r\n \r\n\n  asd  \r\n \r\n\n  ",
				false, false, false, -1, -1);
			TestSmartFormat(
				"  \r\n \r\n\n  asd  \r\n \r\n\n  ",
				"asd",
				true, false, false, -1, -1);

			// maxAllowedConsecutiveLineEndings
			TestSmartFormat(
				"LINE 1\n\n\n\nLINE 2",
				"LINE 1\n\nLINE 2",
				false, false, false, 2, -1);
			TestSmartFormat(
				"LINE 1\n\n\n\nLINE 2\n\n\nLINE 3\n\nLINE 4",
				"LINE 1\n\nLINE 2\n\nLINE 3\n\nLINE 4",
				false, false, false, 2, -1);

			// trimEndOfEachLine
			TestSmartFormat(
				"  LINE 1   \n \n  \n      \nLINE 2   ",
				"  LINE 1\n\n\n\nLINE 2",
				false, true, false, -1, -1);
			TestSmartFormat(
				"  LINE 1   \n  LINE 2   ",
				"  LINE 1\n  LINE 2",
				false, true, false, -1, -1);
			TestSmartFormat(
				"  LINE 1  ",
				"  LINE 1",
				false, true, false, -1, -1);

			// trimEndOfEachLine combined with maxAllowedConsecutiveLineEndings
			TestSmartFormat(
				"  LINE 1   \n \n  \n      \nLINE 2   ",
				"  LINE 1\n\nLINE 2",
				false, true, false, 2, -1);

			// !trimEndOfEachLine combined with maxAllowedConsecutiveLineEndings
			TestSmartFormat(
				"  LINE 1   \n \n  \n      \nLINE 2   ",
				"  LINE 1   \n \n  \n      \nLINE 2   ",
				false, false, false, 2, -1);
		}

		private static void TestSmartFormat(string input, string expected, bool trim, bool trimEndOfEachLine, bool normalizeLineEndings, int maxAllowedConsecutiveLineEndings, int maxLength)
		{
			//Debug.Log("------------------------------------------ before   (A: input  B: expected)");
			//Debug.Log("A: '" + input.ToHexStringFancy() + "\n" + "'" + input + "'");
			//Debug.Log("B: '" + expected.ToHexStringFancy() + "\n" + "'" + expected + "'");
			StringTools.SmartFormat(ref input, trim, trimEndOfEachLine, normalizeLineEndings, maxAllowedConsecutiveLineEndings, maxLength);
			//Debug.Log("--------------- after   (A: input  B: expected)");
			//Debug.Log("A: " + input.ToHexStringFancy() + "\n" + "'" + input + "'");
			//Debug.Log("B: " + expected.ToHexStringFancy() + "\n" + "'" + expected + "'");
			Assert.AreEqual(expected, input);
		}

		#endregion

		#region Tools

		private static char[] BigBuffer = new char[500];

		#endregion
	}

}
