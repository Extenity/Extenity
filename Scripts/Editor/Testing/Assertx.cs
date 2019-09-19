using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Extenity.Testing
{

	public static class Assertx
	{
		public static void AreEqual<T>(IList<T> expected, int expectedLength, IList<T> actual, int actualLength, string message = null)
		{
			// Make sure input data's integrity.
			if (expectedLength > 0)
			{
				Assert.NotNull(expected, "Expected list is missing.");
				Assert.GreaterOrEqual(expected.Count, expectedLength, $"Expected list size '{expected.Count}' does not cover the specified expectedLength '{expectedLength}'.");
			}
			if (actualLength > 0)
			{
				Assert.NotNull(actual, "Actual list is missing.");
				Assert.GreaterOrEqual(actual.Count, actualLength, $"Actual list size '{actual.Count}' does not cover the specified actualLength '{actualLength}'.");
			}

			Assert.AreEqual(expectedLength, actualLength, message);
			for (int i = 0; i < expectedLength; i++)
			{
				Assert.AreEqual(expected[i], actual[i], message);
			}
		}

		#region Assert - One

		/// <summary>Asserts that an int is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(int actual)
		{
			Assert.That<int>(actual, Isx.One);
		}

		/// <summary>Asserts that an int is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(int actual, string message, params object[] args)
		{
			Assert.That<int>(actual, Isx.One, message, args);
		}

		/// <summary>Asserts that an unsigned int is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(uint actual)
		{
			Assert.That<uint>(actual, Isx.One);
		}

		/// <summary>Asserts that an unsigned int is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(uint actual, string message, params object[] args)
		{
			Assert.That<uint>(actual, Isx.One, message, args);
		}

		/// <summary>Asserts that a Long is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(long actual)
		{
			Assert.That<long>(actual, Isx.One);
		}

		/// <summary>Asserts that a Long is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(long actual, string message, params object[] args)
		{
			Assert.That<long>(actual, Isx.One, message, args);
		}

		/// <summary>Asserts that an unsigned Long is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(ulong actual)
		{
			Assert.That<ulong>(actual, Isx.One);
		}

		/// <summary>Asserts that an unsigned Long is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(ulong actual, string message, params object[] args)
		{
			Assert.That<ulong>(actual, Isx.One, message, args);
		}

		/// <summary>Asserts that a decimal is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(Decimal actual)
		{
			Assert.That<Decimal>(actual, Isx.One);
		}

		/// <summary>Asserts that a decimal is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(Decimal actual, string message, params object[] args)
		{
			Assert.That<Decimal>(actual, Isx.One, message, args);
		}

		/// <summary>Asserts that a double is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(double actual)
		{
			Assert.That<double>(actual, Isx.One);
		}

		/// <summary>Asserts that a double is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(double actual, string message, params object[] args)
		{
			Assert.That<double>(actual, Isx.One, message, args);
		}

		/// <summary>Asserts that a float is one.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void One(float actual)
		{
			Assert.That<float>(actual, Isx.One);
		}

		/// <summary>Asserts that a float is one.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void One(float actual, string message, params object[] args)
		{
			Assert.That<float>(actual, Isx.One, message, args);
		}

		#endregion

		#region Assert - Two

		/// <summary>Asserts that an int is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(int actual)
		{
			Assert.That<int>(actual, Isx.Two);
		}

		/// <summary>Asserts that an int is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(int actual, string message, params object[] args)
		{
			Assert.That<int>(actual, Isx.Two, message, args);
		}

		/// <summary>Asserts that an unsigned int is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(uint actual)
		{
			Assert.That<uint>(actual, Isx.Two);
		}

		/// <summary>Asserts that an unsigned int is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(uint actual, string message, params object[] args)
		{
			Assert.That<uint>(actual, Isx.Two, message, args);
		}

		/// <summary>Asserts that a Long is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(long actual)
		{
			Assert.That<long>(actual, Isx.Two);
		}

		/// <summary>Asserts that a Long is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(long actual, string message, params object[] args)
		{
			Assert.That<long>(actual, Isx.Two, message, args);
		}

		/// <summary>Asserts that an unsigned Long is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(ulong actual)
		{
			Assert.That<ulong>(actual, Isx.Two);
		}

		/// <summary>Asserts that an unsigned Long is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(ulong actual, string message, params object[] args)
		{
			Assert.That<ulong>(actual, Isx.Two, message, args);
		}

		/// <summary>Asserts that a decimal is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(Decimal actual)
		{
			Assert.That<Decimal>(actual, Isx.Two);
		}

		/// <summary>Asserts that a decimal is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(Decimal actual, string message, params object[] args)
		{
			Assert.That<Decimal>(actual, Isx.Two, message, args);
		}

		/// <summary>Asserts that a double is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(double actual)
		{
			Assert.That<double>(actual, Isx.Two);
		}

		/// <summary>Asserts that a double is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(double actual, string message, params object[] args)
		{
			Assert.That<double>(actual, Isx.Two, message, args);
		}

		/// <summary>Asserts that a float is two.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void Two(float actual)
		{
			Assert.That<float>(actual, Isx.Two);
		}

		/// <summary>Asserts that a float is two.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void Two(float actual, string message, params object[] args)
		{
			Assert.That<float>(actual, Isx.Two, message, args);
		}

		#endregion

		#region Assert - MinusOne

		/// <summary>Asserts that an int is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(int actual)
		{
			Assert.That<int>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that an int is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(int actual, string message, params object[] args)
		{
			Assert.That<int>(actual, Isx.MinusOne, message, args);
		}

		/// <summary>Asserts that an unsigned int is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(uint actual)
		{
			Assert.That<uint>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that an unsigned int is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(uint actual, string message, params object[] args)
		{
			Assert.That<uint>(actual, Isx.MinusOne, message, args);
		}

		/// <summary>Asserts that a Long is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(long actual)
		{
			Assert.That<long>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that a Long is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(long actual, string message, params object[] args)
		{
			Assert.That<long>(actual, Isx.MinusOne, message, args);
		}

		/// <summary>Asserts that an unsigned Long is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(ulong actual)
		{
			Assert.That<ulong>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that an unsigned Long is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(ulong actual, string message, params object[] args)
		{
			Assert.That<ulong>(actual, Isx.MinusOne, message, args);
		}

		/// <summary>Asserts that a decimal is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(Decimal actual)
		{
			Assert.That<Decimal>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that a decimal is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(Decimal actual, string message, params object[] args)
		{
			Assert.That<Decimal>(actual, Isx.MinusOne, message, args);
		}

		/// <summary>Asserts that a double is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(double actual)
		{
			Assert.That<double>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that a double is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(double actual, string message, params object[] args)
		{
			Assert.That<double>(actual, Isx.MinusOne, message, args);
		}

		/// <summary>Asserts that a float is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusOne(float actual)
		{
			Assert.That<float>(actual, Isx.MinusOne);
		}

		/// <summary>Asserts that a float is minusOne.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusOne(float actual, string message, params object[] args)
		{
			Assert.That<float>(actual, Isx.MinusOne, message, args);
		}

		#endregion

		#region Assert - MinusTwo

		/// <summary>Asserts that an int is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(int actual)
		{
			Assert.That<int>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that an int is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(int actual, string message, params object[] args)
		{
			Assert.That<int>(actual, Isx.MinusTwo, message, args);
		}

		/// <summary>Asserts that an unsigned int is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(uint actual)
		{
			Assert.That<uint>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that an unsigned int is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(uint actual, string message, params object[] args)
		{
			Assert.That<uint>(actual, Isx.MinusTwo, message, args);
		}

		/// <summary>Asserts that a Long is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(long actual)
		{
			Assert.That<long>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that a Long is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(long actual, string message, params object[] args)
		{
			Assert.That<long>(actual, Isx.MinusTwo, message, args);
		}

		/// <summary>Asserts that an unsigned Long is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(ulong actual)
		{
			Assert.That<ulong>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that an unsigned Long is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(ulong actual, string message, params object[] args)
		{
			Assert.That<ulong>(actual, Isx.MinusTwo, message, args);
		}

		/// <summary>Asserts that a decimal is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(Decimal actual)
		{
			Assert.That<Decimal>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that a decimal is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(Decimal actual, string message, params object[] args)
		{
			Assert.That<Decimal>(actual, Isx.MinusTwo, message, args);
		}

		/// <summary>Asserts that a double is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(double actual)
		{
			Assert.That<double>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that a double is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(double actual, string message, params object[] args)
		{
			Assert.That<double>(actual, Isx.MinusTwo, message, args);
		}

		/// <summary>Asserts that a float is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		public static void MinusTwo(float actual)
		{
			Assert.That<float>(actual, Isx.MinusTwo);
		}

		/// <summary>Asserts that a float is minusTwo.</summary>
		/// <param name="actual">The number to be examined</param>
		/// <param name="message">The message to display in case of failure</param>
		/// <param name="args">Array of objects to be used in formatting the message</param>
		public static void MinusTwo(float actual, string message, params object[] args)
		{
			Assert.That<float>(actual, Isx.MinusTwo, message, args);
		}

		#endregion
	}

}
