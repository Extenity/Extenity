using System;
using System.Collections.Generic;
using Extenity;
using Extenity.DataToolbox;
using Extenity.Testing;
using Extenity.UnityTestToolbox;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_StringTools : ExtenityTestBase
	{
		#region Equals

		[Test]
		public static void IsAllZeros()
		{
			Assert.IsFalse(((string)null).IsAllZeros());
			Assert.IsFalse("".IsAllZeros());
			Assert.IsFalse(string.Empty.IsAllZeros());
			Assert.IsFalse(" ".IsAllZeros());
			Assert.IsFalse("\t".IsAllZeros());
			Assert.IsFalse(" 0".IsAllZeros());
			Assert.IsFalse(" 00".IsAllZeros());
			Assert.IsFalse("a00".IsAllZeros());
			Assert.IsFalse("0 ".IsAllZeros());
			Assert.IsFalse("00 ".IsAllZeros());
			Assert.IsFalse("00a".IsAllZeros());
			Assert.IsFalse("00a00".IsAllZeros());
			Assert.IsTrue("0".IsAllZeros());
			Assert.IsTrue("00".IsAllZeros());
			Assert.IsTrue("000".IsAllZeros());
			Assert.IsTrue("0000".IsAllZeros());
			Assert.IsTrue("00000".IsAllZeros());
			Assert.IsTrue("000000".IsAllZeros());
			Assert.IsTrue("0000000000000000000000000".IsAllZeros());
			Assert.IsTrue("0000000000000000000000000000000000000000000000000000000".IsAllZeros());
		}

		#endregion

		#region String Operations

		[Test]
		public static void IsAlphaNumericAscii()
		{
			Assert.False("".IsAlphaNumericAscii(false, false));
			Assert.False(" ".IsAlphaNumericAscii(false, false));
			Assert.False("-".IsAlphaNumericAscii(false, false));
			Assert.False(" a".IsAlphaNumericAscii(false, false));
			Assert.False(" 1".IsAlphaNumericAscii(false, false));
			Assert.False(" a1".IsAlphaNumericAscii(false, false));
			Assert.False("-a".IsAlphaNumericAscii(false, false));
			Assert.False("-1".IsAlphaNumericAscii(false, false));
			Assert.False("-a1".IsAlphaNumericAscii(false, false));
			Assert.False("a-".IsAlphaNumericAscii(false, false));
			Assert.False("1-".IsAlphaNumericAscii(false, false));
			Assert.False("a1-".IsAlphaNumericAscii(false, false));
			Assert.False("a-".IsAlphaNumericAscii(false, false));
			Assert.False("1-".IsAlphaNumericAscii(false, false));
			Assert.False("a1-".IsAlphaNumericAscii(false, false));
			Assert.False("a a".IsAlphaNumericAscii(false, false));

			Assert.True("a".IsAlphaNumericAscii(false, false));
			Assert.True("1".IsAlphaNumericAscii(false, false));
			Assert.True("a1".IsAlphaNumericAscii(false, false));
			Assert.True("a1bjgfy8723bsdk71".IsAlphaNumericAscii(false, false));

			// Allow spaces
			Assert.True("a".IsAlphaNumericAscii(true, false));
			Assert.True("1".IsAlphaNumericAscii(true, false));
			Assert.True("a1".IsAlphaNumericAscii(true, false));
			Assert.True(" a".IsAlphaNumericAscii(true, false));
			Assert.True(" 1".IsAlphaNumericAscii(true, false));
			Assert.True(" a1".IsAlphaNumericAscii(true, false));
			Assert.True("a ".IsAlphaNumericAscii(true, false));
			Assert.True("1 ".IsAlphaNumericAscii(true, false));
			Assert.True("a1 ".IsAlphaNumericAscii(true, false));
			Assert.True("a1bjgfy8723bsdk71".IsAlphaNumericAscii(true, false));
			Assert.True(" a1bjgfy8723bsdk71".IsAlphaNumericAscii(true, false));
			Assert.True("a1bjgfy8723bsdk71 ".IsAlphaNumericAscii(true, false));
			Assert.True(" a1bjgfy8723bsdk71 ".IsAlphaNumericAscii(true, false));
			Assert.False("\ta".IsAlphaNumericAscii(true, false));
			Assert.False("\t1".IsAlphaNumericAscii(true, false));
			Assert.False("\ta1".IsAlphaNumericAscii(true, false));
			Assert.False("a\t".IsAlphaNumericAscii(true, false));
			Assert.False("1\t".IsAlphaNumericAscii(true, false));
			Assert.False("a1\t".IsAlphaNumericAscii(true, false));
			Assert.False("\ta1bjgfy8723bsdk71".IsAlphaNumericAscii(true, false));
			Assert.False("a1bjgfy8723bsdk71\t".IsAlphaNumericAscii(true, false));
			Assert.False("\ta1bjgfy8723bsdk71\t".IsAlphaNumericAscii(true, false));

			// Ensure starts with alpha
			Assert.True("a".IsAlphaNumericAscii(true, true));
			Assert.False("1".IsAlphaNumericAscii(true, true));
			Assert.True("a1".IsAlphaNumericAscii(true, true));
			Assert.False(" a".IsAlphaNumericAscii(true, true));
			Assert.False(" 1".IsAlphaNumericAscii(true, true));
			Assert.False(" a1".IsAlphaNumericAscii(true, true));
			Assert.False("1a1bjgfy8723bsdk7".IsAlphaNumericAscii(true, true));
			Assert.True("a1bjgfy8723bsdk71".IsAlphaNumericAscii(true, true));
		}


		[Test]
		public static void ReplaceBetween()
		{
			using var _ = New.List<KeyValue<string, string>>(out var list);
			list.Add(new KeyValue<string, string>("OXFORD", "NOT BROGUES"));
			list.Add(new KeyValue<string, string>("INNER", "Start <tag>OXFORD</tag> End"));

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
			using var _ = New.List<KeyValue<string, string>>(out var list);
			list.Add(new KeyValue<string, string>("OXFORD", "NOT BROGUES"));
			list.Add(new KeyValue<string, string>("INNER", "Start <tag>OXFORD</tag> End"));

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
				out var result
			);
			Assert.AreEqual(expectedResult, result);
		}

		#endregion

		#region String Operations - Replace Selective

		[Test]
		public static void ReplaceSelective()
		{
			Test_ReplaceBetween(". . .", ". . .", ImmutableStack<bool>.CreateInverse(false, false, false));
			Test_ReplaceBetween(". . .", "X . .", ImmutableStack<bool>.CreateInverse(true, false, false));
			Test_ReplaceBetween(". . .", ". X .", ImmutableStack<bool>.CreateInverse(false, true, false));
			Test_ReplaceBetween(". . .", ". . X", ImmutableStack<bool>.CreateInverse(false, false, true));
			Test_ReplaceBetween(". . .", "X . X", ImmutableStack<bool>.CreateInverse(true, false, true));
			Test_ReplaceBetween(". . .", "X X X", ImmutableStack<bool>.CreateInverse(true, true, true));

			Test_ReplaceBetween(". . .", "X . .", ImmutableStack<bool>.CreateInverse(true));
		}

		private static void Test_ReplaceBetween(string input, string expected, ImmutableStack<bool> selection)
		{
			var result = input.ReplaceSelective(".", "X", selection);
			Assert.AreEqual(expected, result);
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

		#region Normalize Line Endings

		[Test]
		public void IsLineEndingNormalizationNeededCRLF()
		{
			TestIsLineEndingNormalizationNeededCRLF(null, false);
			TestIsLineEndingNormalizationNeededCRLF("", false);
			TestIsLineEndingNormalizationNeededCRLF(" ", false);
			TestIsLineEndingNormalizationNeededCRLF("\t", false);

			TestIsLineEndingNormalizationNeededCRLF("\r\n", false);
			TestIsLineEndingNormalizationNeededCRLF(" \r\n", false);
			TestIsLineEndingNormalizationNeededCRLF("\r\n ", false);
			TestIsLineEndingNormalizationNeededCRLF(" \r\n ", false);

			TestIsLineEndingNormalizationNeededCRLF("\r", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r", true);
			TestIsLineEndingNormalizationNeededCRLF("\r ", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r ", true);

			TestIsLineEndingNormalizationNeededCRLF("\n", true);
			TestIsLineEndingNormalizationNeededCRLF(" \n", true);
			TestIsLineEndingNormalizationNeededCRLF("\n ", true);
			TestIsLineEndingNormalizationNeededCRLF(" \n ", true);

			TestIsLineEndingNormalizationNeededCRLF("\r\r\n", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r\r\n", true);
			TestIsLineEndingNormalizationNeededCRLF("\r\r\n ", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r\r\n ", true);

			TestIsLineEndingNormalizationNeededCRLF("\n\r\n", true);
			TestIsLineEndingNormalizationNeededCRLF(" \n\r\n", true);
			TestIsLineEndingNormalizationNeededCRLF("\n\r\n ", true);
			TestIsLineEndingNormalizationNeededCRLF(" \n\r\n ", true);

			TestIsLineEndingNormalizationNeededCRLF("\r\n\r", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r\n\r", true);
			TestIsLineEndingNormalizationNeededCRLF("\r\n\r ", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r\n\r ", true);

			TestIsLineEndingNormalizationNeededCRLF("\r\n\n", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r\n\n", true);
			TestIsLineEndingNormalizationNeededCRLF("\r\n\n ", true);
			TestIsLineEndingNormalizationNeededCRLF(" \r\n\n ", true);
		}

		[Test]
		public void NormalizeLineEndingsCRLF()
		{
			TestNormalizeLineEndingsCRLF(null, null);
			TestNormalizeLineEndingsCRLF("", "");
			TestNormalizeLineEndingsCRLF(" ", " ");
			TestNormalizeLineEndingsCRLF("\t", "\t");

			TestNormalizeLineEndingsCRLF("\r\n", "\r\n");
			TestNormalizeLineEndingsCRLF(" \r\n", " \r\n");
			TestNormalizeLineEndingsCRLF("\r\n ", "\r\n ");
			TestNormalizeLineEndingsCRLF(" \r\n ", " \r\n ");

			TestNormalizeLineEndingsCRLF("\r", "\r\n");
			TestNormalizeLineEndingsCRLF(" \r", " \r\n");
			TestNormalizeLineEndingsCRLF("\r ", "\r\n ");
			TestNormalizeLineEndingsCRLF(" \r ", " \r\n ");

			TestNormalizeLineEndingsCRLF("\n", "\r\n");
			TestNormalizeLineEndingsCRLF(" \n", " \r\n");
			TestNormalizeLineEndingsCRLF("\n ", "\r\n ");
			TestNormalizeLineEndingsCRLF(" \n ", " \r\n ");

			TestNormalizeLineEndingsCRLF("\r\r\n", "\r\n\r\n");
			TestNormalizeLineEndingsCRLF(" \r\r\n", " \r\n\r\n");
			TestNormalizeLineEndingsCRLF("\r\r\n ", "\r\n\r\n ");
			TestNormalizeLineEndingsCRLF(" \r\r\n ", " \r\n\r\n ");

			TestNormalizeLineEndingsCRLF("\n\r\n", "\r\n\r\n");
			TestNormalizeLineEndingsCRLF(" \n\r\n", " \r\n\r\n");
			TestNormalizeLineEndingsCRLF("\n\r\n ", "\r\n\r\n ");
			TestNormalizeLineEndingsCRLF(" \n\r\n ", " \r\n\r\n ");

			TestNormalizeLineEndingsCRLF("\r\n\r", "\r\n\r\n");
			TestNormalizeLineEndingsCRLF(" \r\n\r", " \r\n\r\n");
			TestNormalizeLineEndingsCRLF("\r\n\r ", "\r\n\r\n ");
			TestNormalizeLineEndingsCRLF(" \r\n\r ", " \r\n\r\n ");

			TestNormalizeLineEndingsCRLF("\r\n\n", "\r\n\r\n");
			TestNormalizeLineEndingsCRLF(" \r\n\n", " \r\n\r\n");
			TestNormalizeLineEndingsCRLF("\r\n\n ", "\r\n\r\n ");
			TestNormalizeLineEndingsCRLF(" \r\n\n ", " \r\n\r\n ");
		}

		private static void TestIsLineEndingNormalizationNeededCRLF(string input, bool expected)
		{
			var result = input.IsLineEndingNormalizationNeededCRLF();
			Assert.AreEqual(expected, result);
		}

		private void TestNormalizeLineEndingsCRLF(string input, string expected)
		{
			var result = input.NormalizeLineEndingsCRLF();
			Assert.AreEqual(expected, result);
		}

		#endregion

		#region Conversions - Int32 ToStringAsCharArray

		private static readonly object[] TestCases_ToStringAsCharArray_Int32 =
		{
			new Action<Int32>[]{ TestValue_ToStringAsCharArray_Int32 },
			new Action<Int32>[]{ TestValue_ToStringAsCharArrayWithPrefix_Int32 },
			new Action<Int32>[]{ TestValue_ToStringAsCharArrayWithPostfix_Int32 },
			new Action<Int32>[]{ TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32 },
			new Action<Int32>[]{ TestValue_ToStringAsCharArrayWithPrefix_WithThousandsSeparator_Int32 },
			new Action<Int32>[]{ TestValue_ToStringAsCharArrayWithPostfix_WithThousandsSeparator_Int32 },
		};

		[Test]
		[Repeat(10)]
		[TestCaseSource(nameof(TestCases_ToStringAsCharArray_Int32))]
		public static void ToStringAsCharArrayWithPrefix_Int32(Action<Int32> tester)
		{
			UnityTestTools.ApplyOverValueSet_Int32(tester);
		}

		private static void TestValue_ToStringAsCharArray_Int32(Int32 value)
		{
			var chars = new char[10 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString();
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPrefix_Int32(Int32 value)
		{
			var chars = new char[10 + 1 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPrefix(chars, '#', out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = "#" + value.ToString();
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPostfix_Int32(Int32 value)
		{
			var chars = new char[10 + 1 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPostfix(chars, '#', out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString() + "#";
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArray_WithThousandsSeparator_Int32(Int32 value)
		{
			var chars = new char[10 + 1 + 3];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, StringTools.NumberGroupSeparator, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString("N0", StringTools.CurrentNumberFormatInfo);
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPrefix_WithThousandsSeparator_Int32(Int32 value)
		{
			var chars = new char[10 + 1 + 3 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPrefix(chars, '#', StringTools.NumberGroupSeparator, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = "#" + value.ToString("N0", StringTools.CurrentNumberFormatInfo);
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPostfix_WithThousandsSeparator_Int32(Int32 value)
		{
			var chars = new char[10 + 1 + 3 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPostfix(chars, '#', StringTools.NumberGroupSeparator, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString("N0", StringTools.CurrentNumberFormatInfo) + "#";
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		#endregion

		#region Conversions - Int64 ToStringAsCharArray

		private static readonly object[] TestCases_ToStringAsCharArray_Int64 =
		{
			new Action<Int64>[]{ TestValue_ToStringAsCharArray_Int64 },
			new Action<Int64>[]{ TestValue_ToStringAsCharArrayWithPrefix_Int64 },
			new Action<Int64>[]{ TestValue_ToStringAsCharArrayWithPostfix_Int64 },
			new Action<Int64>[]{ TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64 },
			new Action<Int64>[]{ TestValue_ToStringAsCharArrayWithPrefix_WithThousandsSeparator_Int64 },
			new Action<Int64>[]{ TestValue_ToStringAsCharArrayWithPostfix_WithThousandsSeparator_Int64 },
		};

		[Test]
		[Repeat(10)]
		[TestCaseSource(nameof(TestCases_ToStringAsCharArray_Int64))]
		public static void ToStringAsCharArrayWithPrefix_Int64(Action<Int64> tester)
		{
			UnityTestTools.ApplyOverValueSet_Int64(tester);
		}

		private static void TestValue_ToStringAsCharArray_Int64(Int64 value)
		{
			var chars = new char[19 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString();
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPrefix_Int64(Int64 value)
		{
			var chars = new char[19 + 1 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPrefix(chars, '#', out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = "#" + value.ToString();
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPostfix_Int64(Int64 value)
		{
			var chars = new char[19 + 1 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPostfix(chars, '#', out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString() + "#";
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArray_WithThousandsSeparator_Int64(Int64 value)
		{
			var chars = new char[19 + 1 + 6];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArray(chars, StringTools.NumberGroupSeparator, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString("N0", StringTools.CurrentNumberFormatInfo);
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPrefix_WithThousandsSeparator_Int64(Int64 value)
		{
			var chars = new char[19 + 1 + 6 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPrefix(chars, '#', StringTools.NumberGroupSeparator, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = "#" + value.ToString("N0", StringTools.CurrentNumberFormatInfo);
			Assert.AreEqual(original, chars.ConvertToString(startIndex, length));
			Assert.AreEqual(original.Length, length);
		}

		private static void TestValue_ToStringAsCharArrayWithPostfix_WithThousandsSeparator_Int64(Int64 value)
		{
			var chars = new char[19 + 1 + 6 + 1];
			UnityTestTools.BeginMemoryCheck();
			value.ToStringAsCharArrayWithPostfix(chars, '#', StringTools.NumberGroupSeparator, out var startIndex, out var length);
			if (UnityTestTools.EndMemoryCheck())
				Assert.Fail("Memory allocated while converting value '" + value + "' to string resulting '" + chars.ConvertToString(0, length) + "'.");
			var original = value.ToString("N0", StringTools.CurrentNumberFormatInfo) + "#";
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
			UnityTestTools.ApplyOverValueSet_Int64(TestValue_ToStringAsCharArray_FormattedInt);
		}

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_FormattedFloat()
		{
			WarmUpFastNumberFormatter();
			UnityTestTools.ApplyOverValueSet_Double(TestValue_ToStringAsCharArray_FormattedFloat);
		}

		[Test]
		[Repeat(10)]
		public static void ToStringAsCharArray_FormattedDouble()
		{
			WarmUpFastNumberFormatter();
			UnityTestTools.ApplyOverValueSet_Double(TestValue_ToStringAsCharArray_FormattedDouble);
		}

		private static void TestValue_ToStringAsCharArray_FormattedInt(Int64 value)
		{
			TestValue_ToStringAsCharArray_FormattedInt("", value); // Apply default formatting, which is 'G'
			TestValue_ToStringAsCharArray_FormattedInt("G", value);
			TestValue_ToStringAsCharArray_FormattedInt("C", value);
			TestValue_ToStringAsCharArray_FormattedInt("D", value);
			TestValue_ToStringAsCharArray_FormattedInt("e", value);
			TestValue_ToStringAsCharArray_FormattedInt("E", value);
			TestValue_ToStringAsCharArray_FormattedInt("F", value);
			TestValue_ToStringAsCharArray_FormattedInt("N", value);
			TestValue_ToStringAsCharArray_FormattedInt("P", value);
			TestValue_ToStringAsCharArray_FormattedInt("X", value);

			for (int iDigits = 0; iDigits <= 10; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedInt("G" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("C" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("D" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("e" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("E" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("F" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("N" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("P" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedInt("X" + iDigits, value);
			}
		}

		private static void TestValue_ToStringAsCharArray_FormattedFloat(double valueAsDouble)
		{
			var value = (float)valueAsDouble;

			TestValue_ToStringAsCharArray_FormattedFloat("", value); // Apply default formatting, which is 'G'
			TestValue_ToStringAsCharArray_FormattedFloat("G", value);
			TestValue_ToStringAsCharArray_FormattedFloat("C", value);
			TestValue_ToStringAsCharArray_FormattedFloat("e", value);
			TestValue_ToStringAsCharArray_FormattedFloat("E", value);
			TestValue_ToStringAsCharArray_FormattedFloat("F", value);
			TestValue_ToStringAsCharArray_FormattedFloat("N", value);
			TestValue_ToStringAsCharArray_FormattedFloat("P", value);
			// This one allocates memory. We probably won't be interested in using it ever, so leave it as it is.
			//TestValue_ToStringAsCharArray_FormattedFloat("R", value);

			// String formatting generates erroneous results after 7 digits for 'G' formatting, which seems fine.
			for (int iDigits = 0; iDigits < 7; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedFloat("G" + iDigits, value);
			}

			// String formatting generates erroneous results after 6 digits for 'e' and 'E' formatting, which seems fine.
			for (int iDigits = 0; iDigits < 6; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedFloat("e" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedFloat("E" + iDigits, value);
			}

			for (int iDigits = 0; iDigits <= 20; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedFloat("C" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedFloat("F" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedFloat("N" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedFloat("P" + iDigits, value);
			}
		}

		private static void TestValue_ToStringAsCharArray_FormattedDouble(double value)
		{
			// String formatting generates erroneous results for 'G' formatting at the least significant digits, which seems fine.
			// So we skip testing default formatting for 'double' type.
			// TestValue_ToStringAsCharArray_FormattedDouble("", value); // Apply default formatting, which is 'G'
			// TestValue_ToStringAsCharArray_FormattedDouble("G", value);

			TestValue_ToStringAsCharArray_FormattedDouble("C", value);
			TestValue_ToStringAsCharArray_FormattedDouble("e", value);
			TestValue_ToStringAsCharArray_FormattedDouble("E", value);
			TestValue_ToStringAsCharArray_FormattedDouble("F", value);
			TestValue_ToStringAsCharArray_FormattedDouble("N", value);
			TestValue_ToStringAsCharArray_FormattedDouble("P", value);
			// This one allocates memory. We probably won't be interested in using it ever, so leave it as it is.
			//TestValue_ToStringAsCharArray_FormattedDouble("R", value);

			// String formatting generates erroneous results for 'G' formatting at the least significant digits, which seems fine.
			// So we skip 'G0', which is the same as 'G'.
			for (int iDigits = 1; iDigits <= 14; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedDouble("G" + iDigits, value);
			}

			// String formatting generates erroneous results after 8 digits for 'P' formatting, which seems fine.
			for (int iDigits = 0; iDigits <= 8; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedDouble("P" + iDigits, value);
			}

			// String formatting generates erroneous results after 10 digits for 'C' formatting, which seems fine.
			for (int iDigits = 0; iDigits <= 10; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedDouble("C" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedDouble("F" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedDouble("N" + iDigits, value);
			}

			// String formatting generates erroneous results after 13 digits for 'e' and 'E' formatting, which seems fine.
			for (int iDigits = 0; iDigits <= 13; iDigits++)
			{
				TestValue_ToStringAsCharArray_FormattedDouble("e" + iDigits, value);
				TestValue_ToStringAsCharArray_FormattedDouble("E" + iDigits, value);
			}
		}

		private static void WarmUpFastNumberFormatter()
		{
			lock (BigBuffer)
			{
				// Warm up internal buffers of FastNumberFormatter
				for (int iDigits = 0; iDigits < 20; iDigits++)
				{
					float.MaxValue.ToStringAsCharArray("C" + iDigits, BigBuffer);
					//float.MaxValue.ToStringAsCharArray("D" + iDigits, BigBuffer); Integers only
					float.MaxValue.ToStringAsCharArray("e" + iDigits, BigBuffer);
					float.MaxValue.ToStringAsCharArray("E" + iDigits, BigBuffer);
					float.MaxValue.ToStringAsCharArray("F" + iDigits, BigBuffer);
					float.MaxValue.ToStringAsCharArray("G" + iDigits, BigBuffer);
					float.MaxValue.ToStringAsCharArray("N" + iDigits, BigBuffer);
					float.MaxValue.ToStringAsCharArray("P" + iDigits, BigBuffer);
					//float.MaxValue.ToStringAsCharArray("X" + iDigits, BigBuffer); Integers only

					double.MaxValue.ToStringAsCharArray("C" + iDigits, BigBuffer);
					//double.MaxValue.ToStringAsCharArray("D" + iDigits, BigBuffer); Integers only
					double.MaxValue.ToStringAsCharArray("e" + iDigits, BigBuffer);
					double.MaxValue.ToStringAsCharArray("E" + iDigits, BigBuffer);
					double.MaxValue.ToStringAsCharArray("F" + iDigits, BigBuffer);
					double.MaxValue.ToStringAsCharArray("G" + iDigits, BigBuffer);
					double.MaxValue.ToStringAsCharArray("N" + iDigits, BigBuffer);
					double.MaxValue.ToStringAsCharArray("P" + iDigits, BigBuffer);
					//double.MaxValue.ToStringAsCharArray("X" + iDigits, BigBuffer); Integers only

					Int32.MaxValue.ToStringAsCharArray("C" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("D" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("e" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("E" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("F" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("G" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("N" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("P" + iDigits, BigBuffer);
					Int32.MaxValue.ToStringAsCharArray("X" + iDigits, BigBuffer);

					Int64.MaxValue.ToStringAsCharArray("C" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("D" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("e" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("E" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("F" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("G" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("N" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("P" + iDigits, BigBuffer);
					Int64.MaxValue.ToStringAsCharArray("X" + iDigits, BigBuffer);
				}
			}
		}

		private static void TestValue_ToStringAsCharArray_FormattedInt(string format, Int64 value)
		{
			TestValue_ToStringAsCharArray_FormattedValue(format, value, (_value, _format) => _value.ToString(_format), (_value, _format, _chars) => _value.ToStringAsCharArray(_format, _chars));
			TestValue_ToStringAsCharArray_FormattedValue(format, (Int32)value, (_value, _format) => _value.ToString(_format), (_value, _format, _chars) => _value.ToStringAsCharArray(_format, _chars));
		}

		private static void TestValue_ToStringAsCharArray_FormattedFloat(string format, float value)
		{
			TestValue_ToStringAsCharArray_FormattedValue(format, value, (_value, _format) => _value.ToString(_format), (_value, _format, _chars) => _value.ToStringAsCharArray(_format, _chars));
		}

		private static void TestValue_ToStringAsCharArray_FormattedDouble(string format, double value)
		{
			TestValue_ToStringAsCharArray_FormattedValue(format, value, (_value, _format) => _value.ToString(_format), (_value, _format, _chars) => _value.ToStringAsCharArray(_format, _chars));
		}

		/// <summary>
		/// The trick here is that convertToExpectedString and convertToCharArray
		/// won't allocate any memory while providing the test functionality for
		/// various types of values. The allocation problems can easily be seen
		/// with ReSharper's 'Heap Allocations Viewer' plugin.
		///
		/// Though admittedly the lines where this method is called looks a bit silly.
		/// </summary>
		private static void TestValue_ToStringAsCharArray_FormattedValue<T>(string format,
		                                                                    T value,
		                                                                    Func<T, string, string> convertToExpectedString,
		                                                                    Func<T, string, char[], int> convertToCharArray)
		{
			lock (BigBuffer)
			{
				BigBuffer.Clear();
				UnityTestTools.BeginMemoryCheck();
				var length = convertToCharArray(value, format, BigBuffer);
				if (UnityTestTools.EndMemoryCheck())
					Assert.Fail($"Memory allocated while converting value '{value.ToString()}' with format '{format}' to string resulting '{BigBuffer.ConvertToString(0, length)}'.");
				var resultString = BigBuffer.ConvertToString(0, length);
				var expectedString = convertToExpectedString(value, format);
				if (!expectedString.Equals(resultString))
				{
					Log.Error($"Erroneous value generated while converting value '{value.ToString()}' with format '{format}' to string resulting '{BigBuffer.ConvertToString(0, length)}'.");
				}
				Assert.AreEqual(expectedString, resultString);
				Assert.AreEqual(expectedString.Length, length);
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

		#region Hash

		[Test]
		public static void GetHashCodeGuaranteed()
		{
			var valueBag = new HashSet<string>(); // This will allow ignoring of checking the hashes for the same generated values.
			var history = new Dictionary<int, string>(100000); // This will be used for checking if the hash is generated before. Also it will be used for logging the previously generated hash value.

			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Throws<ArgumentNullException>(() => ((string)null).GetHashCodeGuaranteed());

			TestValue_GetHashCodeGuaranteed(valueBag, history, "", 757602046);
			TestValue_GetHashCodeGuaranteed(valueBag, history, " ", -842352768);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "\t", -842352729);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "a", -842352705);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "ab", -840386625);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "abc", 536991770);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "1", -842352753);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "a1", -843466817);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "1a", -840321137);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "aa", -840321089);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "aaa", -625742108);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "11", -843466865);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "111", 1508494276);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "12", -843532401);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "123", -1623739142);
			TestValue_GetHashCodeGuaranteed(valueBag, history, "bvuYGfg823tbn181", -2132762692);

			/*
			// Just add some random values and expect them to not collide.
			// Of course it will collide randomly. So this is just for fun experimentation.
			// Seems like the collision will be rare below 5.000 items.
			// Above that, we start to see collisions.
			// Above 100.000, we rarely see the test succeeds without collision.
			RandomTools.RandomizeGenerator();
			var buffer = new char[10];
			for (int i = 0; i < 100000; i++)
			{
				RandomTools.FillRandomly(buffer);
				var value = buffer.ConvertToString(0, Random.Range(1, 10));
				if (!TestValue_GetHashCodeGuaranteed(valueBag, history, value))
					i--;
			}
			*/
		}

		//private static void CreateLog(System.Text.StringBuilder builder, string value)
		//{
		//	var hash = value.GetHashCodeGuaranteed();
		//	builder.AppendLine($@"TestValue_GetHashCodeGuaranteed(history, ""{value}"", {hash});");
		//}

		private static bool TestValue_GetHashCodeGuaranteed(HashSet<string> valueBag, Dictionary<int, string> history, string value)
		{
			if (!valueBag.Add(value))
				return false;

			var hash = value.GetHashCodeGuaranteed();

			// Just make sure there will be no collision with our basic dataset.
			// If it's not the case, something is really going sideways.
			if (history.ContainsKey(hash))
				throw new Exception($"Collision detected at iteration '{history.Count}' between values '{history[hash]}' and '{value}'");
			history.Add(hash, value);

			return true;
		}

		private static bool TestValue_GetHashCodeGuaranteed(HashSet<string> valueBag, Dictionary<int, string> history, string value, int expectedHash)
		{
			if (!valueBag.Add(value))
				return false;

			var hash = value.GetHashCodeGuaranteed();

			// Just make sure there will be no collision with our basic dataset.
			// If it's not the case, something is really going sideways.
			if (history.ContainsKey(hash))
				throw new Exception($"Collision detected at iteration '{history.Count}' between values '{history[hash]}' and '{value}'");
			history.Add(hash, value);

			Assert.AreEqual(expectedHash, hash);

			return true;
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
			//Log.Info("------------------------------------------ before   (A: input  B: expected)");
			//Log.Info("A: '" + input.ToHexStringFancy() + "\n" + "'" + input + "'");
			//Log.Info("B: '" + expected.ToHexStringFancy() + "\n" + "'" + expected + "'");
			StringTools.SmartFormat(ref input, trim, trimEndOfEachLine, normalizeLineEndings, maxAllowedConsecutiveLineEndings, maxLength);
			//Log.Info("--------------- after   (A: input  B: expected)");
			//Log.Info("A: " + input.ToHexStringFancy() + "\n" + "'" + input + "'");
			//Log.Info("B: " + expected.ToHexStringFancy() + "\n" + "'" + expected + "'");
			Assert.AreEqual(expected, input);
		}

		#endregion

		#region Tools

		private static char[] BigBuffer = new char[500];

		#endregion
	}

}
