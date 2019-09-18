using System;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.AssetToolbox.Editor
{

	public class Test_AssetTools : ExtenityTestBase
	{
		#region Script Asset

		[Test]
		public static void DeleteCommentsInScriptAssetContents()
		{
			const string ln = "\r\n";

			// Non-comment
			TestDeleteCommentsInScriptAssetContents(null, null);
			TestDeleteCommentsInScriptAssetContents("", "");
			TestDeleteCommentsInScriptAssetContents("out", "out");
			TestDeleteCommentsInScriptAssetContents("out/out", "out/out"); // Single slash should not confuse.
			TestDeleteCommentsInScriptAssetContents("out" + ln + "out" + ln + "out", "out" + ln + "out" + ln + "out");

			// Single-line comments in single-line content
			TestDeleteCommentsInScriptAssetContents("out//in", "out");
			TestDeleteCommentsInScriptAssetContents("out///in", "out");

			// Multi-line comments in single-line content
			TestDeleteCommentsInScriptAssetContents("out/*in*/out", "outout");
			TestDeleteCommentsInScriptAssetContents("out/*in*/out", "outout");
			TestDeleteCommentsInScriptAssetContents("out/*in*/out/*in*/out", "outoutout");
			TestDeleteCommentsInScriptAssetContents("out/*in*/out/*in*/out/*in*/out", "outoutoutout");
			TestDeleteCommentsInScriptAssetContentsThrows<Exception>("out/*in");
			TestDeleteCommentsInScriptAssetContentsThrows<Exception>("out/*in*/out/*in");

			// Mixed comments in single-line content
			TestDeleteCommentsInScriptAssetContents("out//*in", "out");

			TestDeleteCommentsInScriptAssetContents("out//in/*in", "out");
			TestDeleteCommentsInScriptAssetContents("out//in*/in", "out");
			TestDeleteCommentsInScriptAssetContents("out//in/*in*/in", "out");
			TestDeleteCommentsInScriptAssetContents("out//in/*in/*in", "out");
			TestDeleteCommentsInScriptAssetContents("out//in/*in/*in*/in", "out");
			TestDeleteCommentsInScriptAssetContents("out//in/*in/*in*/in*/in", "out");

			TestDeleteCommentsInScriptAssetContentsThrows<Exception>("out/*in//in/*in");
			TestDeleteCommentsInScriptAssetContents("out/*in//in*/out", "outout");
			TestDeleteCommentsInScriptAssetContents("out/*in//in*/out//in", "outout");

			// Single-line comments in multi-line content
			TestDeleteCommentsInScriptAssetContents("out//in" + ln + "out" + ln + "out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out" + ln + "out//in" + ln + "out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out" + ln + "out" + ln + "out//in", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out//in" + ln + "out//in" + ln + "out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out//in" + ln + "out" + ln + "out//in", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out" + ln + "out//in" + ln + "out//in", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out//in" + ln + "out//in" + ln + "out//in", "out" + ln + "out" + ln + "out");

			// Multi-line comments in multi-line content
			TestDeleteCommentsInScriptAssetContents("out/*in" + ln + "in*/out" + ln + "out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out" + ln + "out/*in" + ln + "in*/out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out/*in" + ln + "in" + ln + "in*/out", "out" + ln + "" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out/*in" + ln + "in*/out/*in" + ln + "in*/out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out/*in" + ln + "in*/out/*in*/out/*in" + ln + "in*/out", "out" + ln + "outout" + ln + "out");

			// Mixed comments in multi-line content
			TestDeleteCommentsInScriptAssetContents("out/*in//in" + ln + "in//in*/out" + ln + "out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out" + ln + "out/*in//in" + ln + "in//in*/out", "out" + ln + "out" + ln + "out");
			TestDeleteCommentsInScriptAssetContents("out/*in//in" + ln + "in//in" + ln + "in//in*/out", "out" + ln + "" + ln + "out");

			// Real world scenario
			TestDeleteCommentsInScriptAssetContents(
@"
		private void OnMouseDown1_ShouldDisplay()
		{
		}
		/*
		private void OnMouseDown2_1()
		{
		}
		*/
		/*/*
		private void OnMouseDown2_2()
		{
		}
		*/
		/*//
		private void OnMouseDown2_3()
		{
		}
		*/
		/*private void OnMouseDown3()*/
		/*some comment*/private void OnMouseDown4_ShouldDisplay()
		{
		}
		/**/private void OnMouseDown5_ShouldDisplay()
		{
		}
		//private void OnMouseDown6()
		private void OnMouseDown7() // Ignored by Code Correct
		{
		}
		/*private void*/// OnMouseDown8()
		/**//*private void*/// OnMouseDown9()
		/*
		*/private void OnMouseDown10_ShouldDisplay()
		{
		}
		/*
		private void OnMouseDown11()
		*/private void OnMouseDown12_ShouldDisplay()
		{
		}
		/*
		private void OnMouseDown13()*/
		/*
		private void OnMouseDown14() // Ignored by Code Correct*/
		/**//*
		private void OnMouseDown15()*/
		/**/
		///*
		private void OnMouseDown16_ShouldDisplay()
		{
		}
		// /*
		private void OnMouseDown17_ShouldDisplay()
		{
		}
		///*
		private void OnMouseDown18_ShouldDisplay()//*/
		{
		}
		// /*
		private void OnMouseDown19_ShouldDisplay()//*/
		{
		}
		private void OnMouseDown20()//*/ // Ignored by Code Correct
		{
		}
		private void OnMouseDown21() // Ignored by Code Correct*/
		{
		}
		private void OnMouseDown22() // Ignored by Code Correct //*/
		{
		}
		private void OnMouseDown23()//*/ Ignored by Code Correct
		{
		}
",
@"
		private void OnMouseDown1_ShouldDisplay()
		{
		}
		




		




		




		
		private void OnMouseDown4_ShouldDisplay()
		{
		}
		private void OnMouseDown5_ShouldDisplay()
		{
		}
		
		private void OnMouseDown7() 
		{
		}
		
		
		
private void OnMouseDown10_ShouldDisplay()
		{
		}
		

private void OnMouseDown12_ShouldDisplay()
		{
		}
		

		

		

		
		
		private void OnMouseDown16_ShouldDisplay()
		{
		}
		
		private void OnMouseDown17_ShouldDisplay()
		{
		}
		
		private void OnMouseDown18_ShouldDisplay()
		{
		}
		
		private void OnMouseDown19_ShouldDisplay()
		{
		}
		private void OnMouseDown20()
		{
		}
		private void OnMouseDown21() 
		{
		}
		private void OnMouseDown22() 
		{
		}
		private void OnMouseDown23()
		{
		}
");
		}

		private static void TestDeleteCommentsInScriptAssetContents(string input, string expected)
		{
			InternalTestDeleteCommentsInScriptAssetContents(input, expected, null);
		}

		private static void TestDeleteCommentsInScriptAssetContentsThrows<T>(string input) where T : Exception
		{
			InternalTestDeleteCommentsInScriptAssetContents(input, null, typeof(T));
		}

		private static void InternalTestDeleteCommentsInScriptAssetContents(string input, string expected, Type expectedExceptionType)
		{
			const string whitespace = " \t\r\n \t";
			var inputOutCount = input.CountSubstrings("out");
			var inputInCount = input.CountSubstrings("in");
			if (expectedExceptionType == null)
				Assert.AreEqual(inputOutCount, expected.CountSubstrings("out")); // Just in case.

			// Original check
			InternalTestDeleteCommentsInScriptAssetContents(input, null, null, expected, null, null, expectedExceptionType);

			// The same should work without "in" keywords
			for (int i = 0; i < inputInCount; i++)
				foreach (var sequence in CollectionTools.Combinations(inputOutCount, i))
					InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whole?.ReplaceSelective("in", "", sequence), expected, null, whole => whole, expectedExceptionType);
			// The same should work without "out" keywords
			for (int i = 0; i < inputOutCount; i++)
				foreach (var sequence in CollectionTools.Combinations(inputOutCount, i))
					InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whole?.ReplaceSelective("out", "", sequence), expected, null, whole => whole?.ReplaceSelective("out", "", sequence), expectedExceptionType);
			// The same should work without "in" and "out" keywords
			InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whole?.Replace("in", "").Replace("out", ""), expected, null, whole => whole?.Replace("out", ""), expectedExceptionType);

			// The same should work with whitespace at the very beginning
			InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whitespace + whole, expected, null, whole => whitespace + whole, expectedExceptionType);
			// The same should work without "in" keywords and with whitespace at the very beginning
			for (int i = 0; i < inputInCount; i++)
				foreach (var sequence in CollectionTools.Combinations(inputOutCount, i))
					InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whitespace + whole?.ReplaceSelective("in", "", sequence), expected, null, whole => whitespace + whole, expectedExceptionType);
			// The same should work without "out" keywords and with whitespace at the very beginning
			for (int i = 0; i < inputOutCount; i++)
				foreach (var sequence in CollectionTools.Combinations(inputOutCount, i))
					InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whitespace + whole?.ReplaceSelective("out", "", sequence), expected, null, whole => whitespace + whole?.ReplaceSelective("out", "", sequence), expectedExceptionType);
			// The same should work without "in" and "out" keywords and with whitespace at the very beginning
			InternalTestDeleteCommentsInScriptAssetContents(input, null, whole => whitespace + whole?.Replace("in", "").Replace("out", ""), expected, null, whole => whitespace + whole?.Replace("out", ""), expectedExceptionType);
		}

		private static void InternalTestDeleteCommentsInScriptAssetContents(
			string input, Func<string, string> operationOnInputLines, Func<string, string> operationOnWholeInput,
			string expected, Func<string, string> operationOnExpectedLines, Func<string, string> operationOnWholeExpected,
			Type expectedExceptionType)
		{
			Process_DeleteCommentsInScriptAssetContents(
				input, operationOnInputLines, operationOnWholeInput,
				expected, operationOnExpectedLines, operationOnWholeExpected,
				out string[] processedInputLines,
				out string processedExpected);

			if (expectedExceptionType != null)
			{
				Assert.Throws(expectedExceptionType, () => AssetTools.DeleteCommentsInScriptAssetContents(processedInputLines));
			}
			else
			{
				AssetTools.DeleteCommentsInScriptAssetContents(processedInputLines);
				var result = processedInputLines == null ? null : string.Join("\r\n", processedInputLines);
				Assert.AreEqual(processedExpected, result);
			}
		}

		private static void Process_DeleteCommentsInScriptAssetContents(
			string input, Func<string, string> operationOnInputLines, Func<string, string> operationOnWholeInput,
			string expected, Func<string, string> operationOnExpectedLines, Func<string, string> operationOnWholeExpected,
			out string[] processedInputLines,
			out string processedExpected)
		{
			// Process 'input'
			{
				input = input.NormalizeLineEndingsCRLF();

				if (operationOnWholeInput != null)
				{
					input = operationOnWholeInput(input);
				}

				var inputLines = input?.Split(new[] { "\r\n" }, StringSplitOptions.None) ?? null;
				Assert.AreEqual(input, inputLines == null ? null : string.Join("\r\n", inputLines)); // Just to make sure nothing gets broken when we split the lines.

				if (operationOnInputLines != null)
				{
					for (var i = 0; i < inputLines.Length; i++)
						inputLines[i] = operationOnInputLines(inputLines[i]);
				}

				// Apply operations on 'input'
				//input = string.Join("\r\n", inputLines); Not used anywhere. But keep this commented out.
				processedInputLines = inputLines;
			}

			// Process 'expected'
			{
				expected = expected.NormalizeLineEndingsCRLF();

				if (operationOnWholeExpected != null)
				{
					expected = operationOnWholeExpected(expected);
				}

				var expectedLines = expected?.Split(new[] { "\r\n" }, StringSplitOptions.None) ?? null;
				Assert.AreEqual(expected, expectedLines == null ? null : string.Join("\r\n", expectedLines)); // Just to make sure nothing gets broken when we split the lines.

				if (operationOnExpectedLines != null)
				{
					for (var i = 0; i < expectedLines.Length; i++)
						expectedLines[i] = operationOnExpectedLines(expectedLines[i]);
				}

				// Apply operations on 'expected'
				processedExpected = expectedLines == null ? null : string.Join("\r\n", expectedLines);
			}
		}

		#endregion
	}

}
