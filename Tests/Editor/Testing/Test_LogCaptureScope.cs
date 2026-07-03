using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.Testing
{
	public class Test_LogCaptureScope : ExtenityTestBase
	{
		[Test]
		public void Captures_RawDebugWarning()
		{
			Debug.LogWarning("hello world");
			Assert.AreEqual(1, Logs.Count);
			Assert.AreEqual(LogType.Warning, Logs[0].Type);
			Assert.IsTrue(Logs[0].Message.Contains("hello world"));
			MarkLogsAsExpectedThatIncludes("hello world");
		}

		[Test]
		public void Captures_RawDebugError()
		{
			Debug.LogError("raw error");
			Assert.AreEqual(1, Logs.Count);
			Assert.AreEqual(LogType.Error, Logs[0].Type);
			MarkLogsAsExpectedThatIncludes("raw error");
		}

		[Test]
		public void Captures_RawDebugAssert()
		{
			Debug.LogAssertion("raw assertion");
			Assert.AreEqual(1, Logs.Count);
			Assert.AreEqual(LogType.Assert, Logs[0].Type);
			MarkLogsAsExpectedThatIncludes("raw assertion");
		}

		[Test]
		public void MarkExpectedContaining_RemovesMatchingAndReturnsCount()
		{
			Debug.LogWarning("alpha");
			Debug.LogWarning("beta");
			Debug.LogWarning("alpha again");
			Assert.AreEqual(2, MarkLogsAsExpectedThatIncludes("alpha"));
			Assert.AreEqual(1, Logs.Count);
			Assert.IsTrue(Logs[0].Message.Contains("beta"));
			MarkLogsAsExpectedThatIncludes("beta");
		}

		[Test]
		public void ShouldRecordPredicate_FiltersOutMatchingLines()
		{
			Debug.Log("#a comment line");
			Debug.LogWarning("kept");
			Assert.AreEqual(1, Logs.Count);
			Assert.IsTrue(Logs[0].Message.Contains("kept"));
			MarkLogsAsExpectedThatIncludes("kept");
		}
	}
}