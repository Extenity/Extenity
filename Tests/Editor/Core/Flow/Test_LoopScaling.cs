using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extenity.FlowToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace ExtenityTests.FlowToolbox
{

	/// <summary>
	/// Benchmarks that check how Loop's cost grows with the number of registered callbacks. Each one measures the
	/// same operation at a small and a large callback count and compares the two. The growth is reported as an
	/// exponent: 1 means the cost grows linearly with the callback count, 2 means quadratically. A test fails if the
	/// cost grows faster than linearly.
	///
	/// Registering and deregistering a single callback are measured at different fill levels of the list, so the
	/// exponent tells how the cost of one registration grows with the callbacks already in the list. Invoking is
	/// measured for the whole list, so the exponent tells how one frame's cost grows with the callback count.
	/// </summary>
	public class Test_LoopScaling : ExtenityTestBase
	{
		#region Configuration

		private const int SmallCount = 1000;
		private const int LargeCount = 16000;

		/// <summary>
		/// Linear growth measures around 1. Quadratic growth measures around 2. The gap between leaves room for
		/// timing noise and for cache effects that make large lists a bit slower per item.
		/// </summary>
		private const double MaxAllowedExponent = 1.4;

		/// <summary>
		/// Each measurement is repeated and the fastest run is used, which filters out the runs that were slowed
		/// down by garbage collection or other work in the Editor.
		/// </summary>
		private const int Repeats = 7;

		/// <summary>
		/// How many callbacks are registered and deregistered in one measured batch, when measuring the cost of a
		/// single registration at a given fill level.
		/// </summary>
		private const int BatchSize = 200;

		protected override void OnInitialize()
		{
			// Measured times are logged for each test.
			SetExpectedLogSeverity(LogExpectation.AllowInfoAndBelow);
		}

		#endregion

		#region Registration

		public enum OrderPattern
		{
			/// <summary>All callbacks use the default order, so each one goes to the end of the list.</summary>
			SameOrder,
			/// <summary>Each callback has a lower order than the ones before, so each one goes to the start of the list.</summary>
			DescendingOrder,
			RandomOrder,
		}

		[Test]
		public void Register_CostPerCallbackGrowsAtMostLinearly([Values] OrderPattern orderPattern)
		{
			var growth = MeasureGrowth(count =>
			{
				var filling = CreateCallbacks(count, orderPattern, 1);
				var batch = CreateCallbacks(BatchSize, orderPattern, 2);
				RegisterAll(filling);
				try
				{
					return MeasureFastest(
						setup: null,
						operation: () => RegisterAll(batch),
						teardown: () => DeregisterAll(batch));
				}
				finally
				{
					DeregisterAll(filling);
				}
			});
			AssertAtMostLinear(growth, $"Registering one callback ({orderPattern})");
		}

		[Test]
		public void Deregister_CostPerCallbackGrowsAtMostLinearly([Values] OrderPattern orderPattern)
		{
			var growth = MeasureGrowth(count =>
			{
				var filling = CreateCallbacks(count, orderPattern, 1);
				var batch = CreateCallbacks(BatchSize, orderPattern, 2);
				RegisterAll(filling);
				try
				{
					return MeasureFastest(
						setup: () => RegisterAll(batch),
						operation: () => DeregisterAll(batch),
						teardown: null);
				}
				finally
				{
					DeregisterAll(filling);
				}
			});
			AssertAtMostLinear(growth, $"Deregistering one callback ({orderPattern})");
		}

		#endregion

		#region Invocation

		[Test]
		public void Invoke_CostGrowsAtMostLinearly([Values(true, false)] bool catchExceptions)
		{
			var growth = MeasureGrowth(count =>
			{
				var callbacks = CreateCallbacks(count, OrderPattern.RandomOrder, 1);
				RegisterAll(callbacks);
				try
				{
					return MeasureFastestWithCatchingExceptions(catchExceptions,
						setup: null,
						operation: () => Loop.ManuallyRunLoopOnce(1),
						teardown: null);
				}
				finally
				{
					DeregisterAll(callbacks);
				}
			});
			AssertAtMostLinear(growth, $"Invoking callbacks (catching exceptions: {catchExceptions})");
		}

		/// <summary>
		/// Callbacks of destroyed Unity objects are removed from the list while invoking. Destroying many objects at
		/// once (like when unloading a scene) should not make the next invocation cost more than linearly.
		/// </summary>
		[Test]
		public void Invoke_CostGrowsAtMostLinearly_WhenHalfOfTheCallbackTargetsAreDestroyed([Values(true, false)] bool catchExceptions)
		{
			var growth = MeasureGrowth(count =>
			{
				var targets = new List<CallbackTarget>(count);
				var callbacks = new List<(Action Callback, int Order)>(count);
				return MeasureFastestWithCatchingExceptions(catchExceptions,
					setup: () =>
					{
						for (int i = 0; i < count; i++)
						{
							var target = ScriptableObject.CreateInstance<CallbackTarget>();
							targets.Add(target);
							callbacks.Add((target.Callback, 0));
						}
						RegisterAll(callbacks);
						for (int i = 0; i < count; i += 2)
						{
							Object.DestroyImmediate(targets[i]);
						}
					},
					operation: () => Loop.ManuallyRunLoopOnce(1),
					teardown: () =>
					{
						// Invoking removes the callbacks of destroyed targets, so deregister only the alive ones.
						for (int i = 1; i < count; i += 2)
						{
							Loop.DeregisterUpdate(callbacks[i].Callback);
							Object.DestroyImmediate(targets[i]);
						}
						targets.Clear();
						callbacks.Clear();
					});
			});
			AssertAtMostLinear(growth, $"Invoking callbacks where half of the targets are destroyed (catching exceptions: {catchExceptions})");
		}

		#endregion

		#region Measurement

		private readonly struct Growth
		{
			public readonly double Exponent;
			public readonly string Report;

			public Growth(double exponent, string report)
			{
				Exponent = exponent;
				Report = report;
			}
		}

		/// <summary>
		/// Measures at <see cref="SmallCount"/> and <see cref="LargeCount"/> callbacks and returns the exponent of
		/// the growth between them.
		/// </summary>
		private static Growth MeasureGrowth(Func<int, long> measure)
		{
			// Run the code paths once before measuring, so JIT compiling them is not measured.
			measure(SmallCount / 10);

			var smallTicks = measure(SmallCount);
			var largeTicks = measure(LargeCount);
			var exponent = Math.Log((double)Math.Max(largeTicks, 1) / Math.Max(smallTicks, 1)) / Math.Log((double)LargeCount / SmallCount);
			var report = $"{ToMilliseconds(smallTicks):F3} ms at {SmallCount} callbacks, {ToMilliseconds(largeTicks):F3} ms at {LargeCount} callbacks, growth exponent {exponent:F2}";
			Debug.Log($"{TestContext.CurrentContext.Test.Name}: {report}");
			return new Growth(exponent, report);
		}

		/// <summary>
		/// Runs setup, operation and teardown <see cref="Repeats"/> times and returns the fastest operation time in
		/// Stopwatch ticks. Only the operation is measured. Setup and teardown are optional.
		/// </summary>
		private static long MeasureFastest(Action setup, Action operation, Action teardown)
		{
			var fastest = long.MaxValue;
			var stopwatch = new Stopwatch();
			for (int i = 0; i < Repeats; i++)
			{
				setup?.Invoke();
				try
				{
					stopwatch.Restart();
					operation();
					stopwatch.Stop();
				}
				finally
				{
					teardown?.Invoke();
				}
				fastest = Math.Min(fastest, stopwatch.ElapsedTicks);
			}
			return fastest;
		}

		private static long MeasureFastestWithCatchingExceptions(bool catchExceptions, Action setup, Action operation, Action teardown)
		{
			var previousCatchExceptions = Loop.EnableCatchingExceptionsInUpdateCallbacks;
			Loop.EnableCatchingExceptionsInUpdateCallbacks = catchExceptions;
			try
			{
				return MeasureFastest(setup, operation, teardown);
			}
			finally
			{
				Loop.EnableCatchingExceptionsInUpdateCallbacks = previousCatchExceptions;
			}
		}

		private static void AssertAtMostLinear(Growth growth, string operationName)
		{
			Assert.LessOrEqual(growth.Exponent, MaxAllowedExponent,
				$"{operationName} grows faster than linearly with the callback count: {growth.Report}");
		}

		private static double ToMilliseconds(long ticks)
		{
			return ticks * 1000.0 / Stopwatch.Frequency;
		}

		#endregion

		#region Callbacks

		/// <summary>
		/// Creates callbacks of separate targets, so each one is a separate registration. The orders are generated
		/// from the seed, so measurements at different counts use the same kind of order distribution.
		/// </summary>
		private static List<(Action Callback, int Order)> CreateCallbacks(int count, OrderPattern orderPattern, int seed)
		{
			var random = new Random(seed);
			var callbacks = new List<(Action Callback, int Order)>(count);
			for (int i = 0; i < count; i++)
			{
				var order = orderPattern switch
				{
					OrderPattern.SameOrder => 0,
					OrderPattern.DescendingOrder => -i,
					OrderPattern.RandomOrder => random.Next(-1000, 1000),
					_ => throw new ArgumentOutOfRangeException(nameof(orderPattern), orderPattern, null),
				};
				callbacks.Add((new CallbackOwner().Callback, order));
			}
			return callbacks;
		}

		private static void RegisterAll(List<(Action Callback, int Order)> callbacks)
		{
			for (int i = 0; i < callbacks.Count; i++)
			{
				Loop.RegisterUpdate(callbacks[i].Callback, callbacks[i].Order);
			}
		}

		private static void DeregisterAll(List<(Action Callback, int Order)> callbacks)
		{
			for (int i = 0; i < callbacks.Count; i++)
			{
				Loop.DeregisterUpdate(callbacks[i].Callback);
			}
		}

		private class CallbackOwner
		{
			public void Callback()
			{
			}
		}

		private class CallbackTarget : ScriptableObject
		{
			public void Callback()
			{
			}
		}

		#endregion
	}

}
