using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extenity.FlowToolbox;
using Extenity.MessagingToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.FlowToolbox
{

	/// <summary>
	/// Loop has a Register and Deregister method for each of its callback lists, all written by hand. These tests go
	/// through every list by reflection, so a list added later is covered without changing them.
	/// </summary>
	public class Test_LoopCallbackRegistration : ExtenityTestBase
	{
		#region All Callback Lists

		[Test]
		public void AllCallbackLists_ContainsEveryCallbackListExactlyOnce()
		{
			var instance = new LoopCallbacks();
			var fields = GetCallbackListFields();
			Assert.IsNotEmpty(fields);

			var problems = new List<string>();
			foreach (var field in fields)
			{
				var callbacks = (ExtenityEvent)field.GetValue(instance);
				var entries = instance.AllCallbackLists.Where(entry => entry.Callbacks == callbacks).ToList();
				if (entries.Count != 1)
				{
					problems.Add($"'{field.Name}' is in {nameof(LoopCallbacks.AllCallbackLists)} {entries.Count} time(s).");
				}
				else if (entries[0].Name + "Callbacks" != field.Name)
				{
					problems.Add($"'{field.Name}' is named '{entries[0].Name}' in {nameof(LoopCallbacks.AllCallbackLists)}.");
				}
			}
			Assert.IsEmpty(problems, string.Join("\n", problems));
			Assert.AreEqual(fields.Length, instance.AllCallbackLists.Length, $"{nameof(LoopCallbacks.AllCallbackLists)} has entries that are not callback lists of {nameof(LoopCallbacks)}.");
		}

		[Test]
		public void AllCallbackLists_HasUniqueNames()
		{
			var duplicateNames = new LoopCallbacks().AllCallbackLists
			                                        .GroupBy(entry => entry.Name)
			                                        .Where(group => group.Count() > 1)
			                                        .Select(group => group.Key)
			                                        .ToList();
			Assert.IsEmpty(duplicateNames, "Duplicate names: " + string.Join(", ", duplicateNames));
		}

		/// <summary>
		/// The Loop window lists the callbacks in <see cref="LoopCallbacks.AllCallbackLists"/> order to show the
		/// order they run in, so that order has to match the invocation order.
		/// </summary>
		[Test]
		public void AllCallbackLists_IsInInvocationOrder([Values(true, false)] bool catchExceptions)
		{
			var invokedListNames = new List<string>();
			var registered = new List<(ExtenityEvent Callbacks, Action Callback)>();
			var previousCatchExceptions = Loop.EnableCatchingExceptionsInUpdateCallbacks;

			try
			{
				foreach (var (name, callbacks) in Loop.Instance.AllCallbackLists)
				{
					Action callback = () => invokedListNames.Add(name);
					callbacks.AddListener(callback);
					registered.Add((callbacks, callback));
				}

				Loop.EnableCatchingExceptionsInUpdateCallbacks = catchExceptions;
				Loop.ManuallyRunLoopOnce(1);
			}
			finally
			{
				Loop.EnableCatchingExceptionsInUpdateCallbacks = previousCatchExceptions;
				foreach (var (callbacks, callback) in registered)
				{
					callbacks.RemoveListener(callback);
				}
			}

			CollectionAssert.AreEqual(ListNames.ToList(), invokedListNames,
				$"Expected: {string.Join(", ", ListNames)}\nInvoked: {string.Join(", ", invokedListNames)}");
		}

		#endregion

		#region Register And Deregister Methods

		[Test]
		public void EveryCallbackList_HasRegisterAndDeregisterMethods()
		{
			var problems = new List<string>();
			foreach (var listName in ListNames)
			{
				if (GetRegisterMethod(listName) == null)
					problems.Add($"Loop.Register{listName}(Action, int) is missing.");
				if (GetDeregisterMethod(listName) == null)
					problems.Add($"Loop.Deregister{listName}(Action) is missing.");
			}
			Assert.IsEmpty(problems, string.Join("\n", problems));
		}

		[Test]
		public void EveryRegisterAndDeregisterMethod_HasACallbackList()
		{
			var listNames = ListNames.ToHashSet();
			var orphans = typeof(Loop).GetMethods(BindingFlags.Public | BindingFlags.Static)
			                          .Where(method => method.Name.StartsWith("Register", StringComparison.Ordinal) ||
			                                           method.Name.StartsWith("Deregister", StringComparison.Ordinal))
			                          .Where(method => !listNames.Contains(GetListNameOf(method)))
			                          .Select(method => method.Name)
			                          .ToList();
			Assert.IsEmpty(orphans, "Methods without a callback list: " + string.Join(", ", orphans));
		}

		[Test]
		public void Register_AddsToItsOwnListOnly_WithTheGivenOrder([ValueSource(nameof(ListNames))] string listName)
		{
			const int order = 7;
			var callback = (Action)new CallbackOwner().Callback;

			try
			{
				GetRegisterMethod(listName).Invoke(null, new object[] { callback, order });

				var listsHavingTheCallback = GetListsHaving(callback);
				CollectionAssert.AreEqual(new[] { listName }, listsHavingTheCallback, $"Register{listName} added the callback to: {string.Join(", ", listsHavingTheCallback)}");
				Assert.AreEqual(order, GetList(listName).GetListenerByCallback(callback).Order);
			}
			finally
			{
				RemoveFromAllLists(callback);
			}
		}

		[Test]
		public void EveryRegisterMethod_DefaultsOrderToZero()
		{
			var problems = ListNames.Select(GetRegisterMethod)
			                        .Where(method => method != null)
			                        .Where(method => !method.GetParameters()[1].HasDefaultValue || !Equals(method.GetParameters()[1].DefaultValue, 0))
			                        .Select(method => method.Name)
			                        .ToList();
			Assert.IsEmpty(problems, "Register methods whose order doesn't default to 0: " + string.Join(", ", problems));
		}

		[Test]
		public void Deregister_LeavesTheSameCallbackInOtherLists([ValueSource(nameof(ListNames))] string listName)
		{
			var callback = (Action)new CallbackOwner().Callback;
			var allListNames = ListNames.ToList();

			try
			{
				foreach (var name in allListNames)
				{
					GetRegisterMethod(name).Invoke(null, new object[] { callback, 0 });
				}

				// Compared with what is registered right before deregistering, so that a faulty Register method is
				// reported by the Register test alone, not by this test for every list.
				var listsBefore = GetListsHaving(callback);
				Assert.Contains(listName, listsBefore, $"Register{listName} did not add the callback to '{listName}'.");

				GetDeregisterMethod(listName).Invoke(null, new object[] { callback });

				var expected = listsBefore.Where(name => name != listName).ToList();
				var actual = GetListsHaving(callback);
				CollectionAssert.AreEqual(expected, actual, $"Deregister{listName} did not remove only from '{listName}'.\nRemoved from: {string.Join(", ", listsBefore.Except(actual))}\nExpected to remove from: {listName}");
			}
			finally
			{
				RemoveFromAllLists(callback);
			}
		}

		#endregion

		#region Tools

		private static IEnumerable<string> ListNames => new LoopCallbacks().AllCallbackLists.Select(entry => entry.Name);

		private static FieldInfo[] GetCallbackListFields()
		{
			return typeof(LoopCallbacks).GetFields(BindingFlags.Public | BindingFlags.Instance)
			                            .Where(field => field.FieldType == typeof(ExtenityEvent))
			                            .ToArray();
		}

		private static MethodInfo GetRegisterMethod(string listName)
		{
			return typeof(Loop).GetMethod("Register" + listName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Action), typeof(int) }, null);
		}

		private static MethodInfo GetDeregisterMethod(string listName)
		{
			return typeof(Loop).GetMethod("Deregister" + listName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Action) }, null);
		}

		private static string GetListNameOf(MethodInfo method)
		{
			return method.Name.StartsWith("Deregister", StringComparison.Ordinal)
				? method.Name.Substring("Deregister".Length)
				: method.Name.Substring("Register".Length);
		}

		private static ExtenityEvent GetList(string listName)
		{
			return Loop.Instance.AllCallbackLists.Single(entry => entry.Name == listName).Callbacks;
		}

		private static List<string> GetListsHaving(Action callback)
		{
			return Loop.Instance.AllCallbackLists
			           .Where(entry => entry.Callbacks.IsCallbackRegistered(callback))
			           .Select(entry => entry.Name)
			           .ToList();
		}

		/// <summary>
		/// Cleans up without going through the Deregister methods under test, so a faulty one fails only its own test
		/// instead of leaving the callback behind for the test base to report.
		/// </summary>
		private static void RemoveFromAllLists(Action callback)
		{
			foreach (var entry in Loop.Instance.AllCallbackLists)
			{
				entry.Callbacks.RemoveListener(callback);
			}
		}

		/// <summary>
		/// Gives each test its own callback, so a callback left behind by one test can't be mistaken for another's.
		/// </summary>
		private class CallbackOwner
		{
			public void Callback()
			{
			}
		}

		#endregion
	}

}
