using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.FlowToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace ExtenityTests.FlowToolbox
{

	public class Test_LoopPlayerLoopInjection : ExtenityTestBase
	{
		#region Injection Into Unity's Player Loop

		[Test]
		public void Inject_PlacesEveryRunnerAtItsDocumentedPosition()
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			Loop.InjectIntoPlayerLoop(ref playerLoop);

			AssertConsecutive<TimeUpdate>(playerLoop,
				typeof(TimeUpdate.WaitForLastPresentationAndUpdateTime),
				typeof(Loop.TimeRunner),
				typeof(Loop.NetworkingRunner),
				typeof(Loop.InputUpdateRunner));

			AssertConsecutive<FixedUpdate>(playerLoop,
				typeof(Loop.PreFixedUpdateRunner),
				typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
				typeof(Loop.FixedUpdateRunner),
				typeof(Loop.PostFixedUpdateRunner));

			AssertConsecutive<Update>(playerLoop,
				typeof(Loop.PreUpdateRunner),
				typeof(Update.ScriptRunBehaviourUpdate),
				typeof(Loop.UpdateRunner),
				typeof(Loop.PostUpdateRunner),
				typeof(Loop.InfrequentUpdatesRunner));

			AssertConsecutive<PreLateUpdate>(playerLoop,
				typeof(Loop.PreLateUpdateRunner),
				typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
				typeof(Loop.LateUpdateRunner),
				typeof(Loop.PostLateUpdateRunner),
				typeof(Loop.CameraPlacementUpdateRunner),
				typeof(Loop.PreRenderRunner),
				typeof(Loop.PreUIRunner));

			AssertConsecutive<PostLateUpdate>(playerLoop,
				typeof(PostLateUpdate).GetNestedType("UIElementsRepaintPanels", BindingFlags.Public | BindingFlags.NonPublic),
				typeof(Loop.PostUIToolkitRepaintRunner));
		}

		[Test]
		public void Inject_InjectsEveryRunnerExactlyOnce()
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			Loop.InjectIntoPlayerLoop(ref playerLoop);

			AssertEveryRunnerExactlyOnce(playerLoop);
		}

		[Test]
		public void Inject_GivesEveryRunnerAnUpdateDelegate()
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			Loop.InjectIntoPlayerLoop(ref playerLoop);

			foreach (var runner in GetAllSystems(playerLoop).Where(system => IsRunnerType(system.type)))
			{
				Assert.IsNotNull(runner.updateDelegate, $"Runner '{runner.type.Name}' has no update delegate.");
			}
		}

		[Test]
		public void InjectThenRemove_RestoresTheOriginalPlayerLoop()
		{
			var original = PlayerLoop.GetDefaultPlayerLoop();
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			Loop.InjectIntoPlayerLoop(ref playerLoop);
			Loop.RemoveFromPlayerLoop(ref playerLoop);

			Assert.AreEqual(DescribeTree(original), DescribeTree(playerLoop));
		}

		[Test]
		public void Inject_ThrowsNamingTheMissingAnchor()
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			Loop.RemoveLoopSystem<Update>(ref playerLoop, typeof(Update.ScriptRunBehaviourUpdate));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.InjectIntoPlayerLoop(ref playerLoop));
			StringAssert.Contains($"'{nameof(Update)}.{nameof(Update.ScriptRunBehaviourUpdate)}'", exception.Message);
		}

		[Test]
		public void Inject_ThrowsNamingTheMissingUIToolkitRepaintStep()
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			var repaintType = typeof(PostLateUpdate).GetNestedType("UIElementsRepaintPanels", BindingFlags.Public | BindingFlags.NonPublic);
			Loop.RemoveLoopSystem<PostLateUpdate>(ref playerLoop, repaintType);

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.InjectIntoPlayerLoop(ref playerLoop));
			StringAssert.Contains($"'{nameof(PostLateUpdate)}.UIElementsRepaintPanels'", exception.Message);
		}

		[Test]
		public void Remove_ThrowsWhenARunnerIsMissing()
		{
			var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
			Loop.InjectIntoPlayerLoop(ref playerLoop);
			Loop.RemoveLoopSystem<Update>(ref playerLoop, typeof(Loop.UpdateRunner));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.RemoveFromPlayerLoop(ref playerLoop));
			StringAssert.Contains($"'{nameof(Update)}.{nameof(Loop.UpdateRunner)}'", exception.Message);
		}

		#endregion

		#region Current Player Loop

		[Test]
		public void CurrentPlayerLoop_HasEveryRunnerExactlyOnce()
		{
			Assert.IsNotNull(Loop.Instance, "The test base should have initialized Loop.");

			AssertEveryRunnerExactlyOnce(PlayerLoop.GetCurrentPlayerLoop());
		}

		[Test]
		public void InitializeSystem_CalledAgain_DoesNotInjectAgain()
		{
			Loop.InitializeSystem();

			AssertEveryRunnerExactlyOnce(PlayerLoop.GetCurrentPlayerLoop());
		}

		[Test]
		public void DeinitializeSystem_RemovesEveryRunner_AndInitializeSystemInjectsThemAgain()
		{
			try
			{
				Loop.DeinitializeSystem();
				Assert.IsNull(Loop.Instance);

				var leftoverRunners = GetAllSystems(PlayerLoop.GetCurrentPlayerLoop())
				                      .Where(system => IsRunnerType(system.type))
				                      .Select(system => system.type.Name)
				                      .ToList();
				Assert.IsEmpty(leftoverRunners, "Runners left in the player loop after deinitialization: " + string.Join(", ", leftoverRunners));
			}
			finally
			{
				Loop.InitializeSystem();
			}

			Assert.IsNotNull(Loop.Instance);
			AssertEveryRunnerExactlyOnce(PlayerLoop.GetCurrentPlayerLoop());
		}

		#endregion

		#region Insert And Remove Helpers

		[Test]
		public void InsertBefore_InsertsRightBeforeTheAnchor([Values(0, 1, 2)] int anchorIndex)
		{
			var anchors = new[] { typeof(StepA), typeof(StepB), typeof(StepC) };
			var playerLoop = CreatePlayerLoop<Update>(anchors);

			Loop.InsertLoopSystemBefore<Update>(ref playerLoop, anchors[anchorIndex], InsertedSystem);

			var expected = anchors.ToList();
			expected.Insert(anchorIndex, typeof(InsertedStep));
			CollectionAssert.AreEqual(expected, GetPhaseTypes<Update>(playerLoop));
		}

		[Test]
		public void InsertAfter_InsertsRightAfterTheAnchor([Values(0, 1, 2)] int anchorIndex)
		{
			var anchors = new[] { typeof(StepA), typeof(StepB), typeof(StepC) };
			var playerLoop = CreatePlayerLoop<Update>(anchors);

			Loop.InsertLoopSystemAfter<Update>(ref playerLoop, anchors[anchorIndex], InsertedSystem);

			var expected = anchors.ToList();
			expected.Insert(anchorIndex + 1, typeof(InsertedStep));
			CollectionAssert.AreEqual(expected, GetPhaseTypes<Update>(playerLoop));
		}

		[Test]
		public void Remove_RemovesOnlyThatStep([Values(0, 1, 2)] int index)
		{
			var steps = new[] { typeof(StepA), typeof(StepB), typeof(StepC) };
			var playerLoop = CreatePlayerLoop<Update>(steps);

			Loop.RemoveLoopSystem<Update>(ref playerLoop, steps[index]);

			var expected = steps.ToList();
			expected.RemoveAt(index);
			CollectionAssert.AreEqual(expected, GetPhaseTypes<Update>(playerLoop));
		}

		[Test]
		public void InsertBefore_ThrowsWhenThePhaseIsMissing()
		{
			var playerLoop = CreatePlayerLoop<FixedUpdate>(typeof(StepA));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemBefore<Update>(ref playerLoop, typeof(StepA), InsertedSystem));
			StringAssert.Contains($"'{nameof(Update)}' phase", exception.Message);
			StringAssert.Contains(nameof(InsertedStep), exception.Message);
		}

		[Test]
		public void InsertAfter_ThrowsWhenThePhaseIsMissing()
		{
			var playerLoop = CreatePlayerLoop<FixedUpdate>(typeof(StepA));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemAfter<Update>(ref playerLoop, typeof(StepA), InsertedSystem));
			StringAssert.Contains($"'{nameof(Update)}' phase", exception.Message);
			StringAssert.Contains(nameof(InsertedStep), exception.Message);
		}

		[Test]
		public void Remove_ThrowsWhenThePhaseIsMissing()
		{
			var playerLoop = CreatePlayerLoop<FixedUpdate>(typeof(StepA));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.RemoveLoopSystem<Update>(ref playerLoop, typeof(StepA)));
			StringAssert.Contains($"'{nameof(Update)}' phase", exception.Message);
			StringAssert.Contains(nameof(StepA), exception.Message);
		}

		[Test]
		public void InsertBefore_ThrowsWhenTheAnchorIsMissing()
		{
			var playerLoop = CreatePlayerLoop<Update>(typeof(StepA));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemBefore<Update>(ref playerLoop, typeof(StepB), InsertedSystem));
			StringAssert.Contains($"'{nameof(Update)}.{nameof(StepB)}'", exception.Message);
			StringAssert.Contains(nameof(InsertedStep), exception.Message);
		}

		[Test]
		public void InsertAfter_ThrowsWhenTheAnchorIsMissing()
		{
			var playerLoop = CreatePlayerLoop<Update>(typeof(StepA));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemAfter<Update>(ref playerLoop, typeof(StepB), InsertedSystem));
			StringAssert.Contains($"'{nameof(Update)}.{nameof(StepB)}'", exception.Message);
			StringAssert.Contains(nameof(InsertedStep), exception.Message);
		}

		[Test]
		public void Remove_ThrowsWhenTheStepIsMissing()
		{
			var playerLoop = CreatePlayerLoop<Update>(typeof(StepA));

			var exception = Assert.Throws<InvalidOperationException>(() => Loop.RemoveLoopSystem<Update>(ref playerLoop, typeof(StepB)));
			StringAssert.Contains($"'{nameof(Update)}.{nameof(StepB)}'", exception.Message);
		}

		[Test]
		public void Insert_ThrowsWhenThePhaseHasNoSteps()
		{
			var playerLoop = new PlayerLoopSystem
			{
				subSystemList = new[] { new PlayerLoopSystem { type = typeof(Update), subSystemList = null } }
			};

			Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemAfter<Update>(ref playerLoop, typeof(StepA), InsertedSystem));
		}

		[Test]
		public void Insert_ThrowsWhenThePlayerLoopHasNoPhases()
		{
			var playerLoop = new PlayerLoopSystem { subSystemList = null };

			Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemAfter<Update>(ref playerLoop, typeof(StepA), InsertedSystem));
		}

		[Test]
		public void FailedInsert_LeavesThePlayerLoopUnchanged()
		{
			var playerLoop = CreatePlayerLoop<Update>(typeof(StepA), typeof(StepB));
			var before = DescribeTree(playerLoop);

			Assert.Throws<InvalidOperationException>(() => Loop.InsertLoopSystemAfter<Update>(ref playerLoop, typeof(StepC), InsertedSystem));

			Assert.AreEqual(before, DescribeTree(playerLoop));
		}

		// Placeholder step types for the synthetic player loops above. Loop only needs their identity.
		private struct StepA { }
		private struct StepB { }
		private struct StepC { }

		private struct InsertedStep { }

		private static readonly PlayerLoopSystem InsertedSystem = new PlayerLoopSystem { type = typeof(InsertedStep) };

		#endregion

		#region Tools

		/// <summary>
		/// Every marker type Loop declares for the player loop. Gathered by reflection, so a runner added later is
		/// covered by the tests above without changing them.
		/// </summary>
		private static Type[] GetAllRunnerTypes()
		{
			return typeof(Loop).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
			                   .Where(IsRunnerType)
			                   .ToArray();
		}

		private static bool IsRunnerType(Type type)
		{
			return type != null &&
			       type.IsValueType &&
			       type.DeclaringType == typeof(Loop) &&
			       type.Name.EndsWith("Runner", StringComparison.Ordinal);
		}

		private static void AssertEveryRunnerExactlyOnce(PlayerLoopSystem playerLoop)
		{
			var runnerTypes = GetAllRunnerTypes();
			Assert.AreEqual(17, runnerTypes.Length, "Loop's runner count changed. Update this test and the documented positions test.");

			// Compared by name rather than by Type, so that runners left over from a previous domain (whose types
			// are not the same Type objects anymore) are caught as duplicates too.
			var countsByName = GetAllSystems(playerLoop)
			                   .Where(system => system.type != null && system.type.Name.EndsWith("Runner", StringComparison.Ordinal))
			                   .GroupBy(system => system.type.FullName)
			                   .ToDictionary(group => group.Key, group => group.Count());

			var problems = new List<string>();
			foreach (var runnerType in runnerTypes)
			{
				countsByName.TryGetValue(runnerType.FullName, out var count);
				if (count != 1)
				{
					problems.Add($"{runnerType.Name} appears {count} time(s)");
				}
			}
			Assert.IsEmpty(problems, string.Join("\n", problems) + "\n\nPlayer loop:\n" + DescribeTree(playerLoop));
		}

		private static void AssertConsecutive<TPhase>(PlayerLoopSystem playerLoop, params Type[] expectedSequence)
		{
			var phaseTypes = GetPhaseTypes<TPhase>(playerLoop);
			var startIndex = phaseTypes.IndexOf(expectedSequence[0]);
			Assert.GreaterOrEqual(startIndex, 0, $"'{expectedSequence[0]?.Name}' is not in '{typeof(TPhase).Name}'.\n\nPlayer loop:\n" + DescribeTree(playerLoop));

			var actualSequence = phaseTypes.Skip(startIndex).Take(expectedSequence.Length).ToList();
			CollectionAssert.AreEqual(expectedSequence, actualSequence,
				$"Unexpected order in '{typeof(TPhase).Name}'.\nExpected: {string.Join(", ", expectedSequence.Select(type => type.Name))}\nActual: {string.Join(", ", actualSequence.Select(type => type.Name))}");
		}

		private static List<Type> GetPhaseTypes<TPhase>(PlayerLoopSystem playerLoop)
		{
			var phase = playerLoop.subSystemList.Single(system => system.type == typeof(TPhase));
			return phase.subSystemList.Select(system => system.type).ToList();
		}

		private static IEnumerable<PlayerLoopSystem> GetAllSystems(PlayerLoopSystem system)
		{
			if (system.subSystemList == null)
				yield break;
			foreach (var subsystem in system.subSystemList)
			{
				yield return subsystem;
				foreach (var nested in GetAllSystems(subsystem))
				{
					yield return nested;
				}
			}
		}

		private static string DescribeTree(PlayerLoopSystem playerLoop)
		{
			var stringBuilder = new StringBuilder();
			DescribeTree(stringBuilder, playerLoop, 0);
			return stringBuilder.ToString();
		}

		private static void DescribeTree(StringBuilder stringBuilder, PlayerLoopSystem system, int depth)
		{
			stringBuilder.Append(' ', depth * 2).AppendLine(system.type?.FullName ?? "(root)");
			if (system.subSystemList == null)
				return;
			foreach (var subsystem in system.subSystemList)
			{
				DescribeTree(stringBuilder, subsystem, depth + 1);
			}
		}

		private static PlayerLoopSystem CreatePlayerLoop<TPhase>(params Type[] steps)
		{
			return new PlayerLoopSystem
			{
				subSystemList = new[]
				{
					new PlayerLoopSystem
					{
						type = typeof(TPhase),
						subSystemList = steps.Select(step => new PlayerLoopSystem { type = step }).ToArray(),
					},
				},
			};
		}

		#endregion
	}

}
