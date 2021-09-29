﻿using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.IMGUIToolbox.Editor;
using Extenity.Testing;
using NUnit.Framework;
using UnityEditor;

namespace ExtenityTests.IMGUIToolbox.Editor
{

	public class Test_TreeElementUtility : ExtenityTestBase
	{
		class TestElement : TreeElement
		{
			public TestElement(string name, int depth)
			{
				this.name = name;
				this.depth = depth;
			}
		}

		[Test]
		public static void TreeToListWorks()
		{
			// Arrange
			TestElement root = new TestElement("root", -1);
			root.children = new List<TreeElement>();
			root.children.Add(new TestElement("A", 0));
			root.children.Add(new TestElement("B", 0));
			root.children.Add(new TestElement("C", 0));

			root.children[1].children = new List<TreeElement>();
			root.children[1].children.Add(new TestElement("Bchild", 1));

			root.children[1].children[0].children = new List<TreeElement>();
			root.children[1].children[0].children.Add(new TestElement("Bchildchild", 2));

			// Test
			List<TestElement> result = new List<TestElement>();
			TreeElementUtility.TreeToList(root, result);

			// Assert
			string[] namesInCorrectOrder = { "root", "A", "B", "Bchild", "Bchildchild", "C" };
			Assert.AreEqual(namesInCorrectOrder.Length, result.Count, "Result count is not match");
			for (int i = 0; i < namesInCorrectOrder.Length; ++i)
			{
				Assert.AreEqual(namesInCorrectOrder[i], result[i].name);
			}
			TreeElementUtility.ValidateDepthValues(result);
		}

		[Test]
		public static void ListToTreeWorks()
		{
			// Arrange
			var list = new List<TestElement>();
			list.Add(new TestElement("root", -1));
			list.Add(new TestElement("A", 0));
			list.Add(new TestElement("B", 0));
			list.Add(new TestElement("Bchild", 1));
			list.Add(new TestElement("Bchildchild", 2));
			list.Add(new TestElement("C", 0));

			// Test
			TestElement root = TreeElementUtility.ListToTree(list);

			// Assert
			Assert.AreEqual("root", root.name);
			Assert.AreEqual(3, root.children.Count);
			Assert.AreEqual("C", root.children[2].name);
			Assert.AreEqual("Bchildchild", root.children[1].children[0].children[0].name);
		}

		[Test]
		public static void ListToTreeThrowsExceptionIfRootIsInvalidDepth()
		{
			// Arrange
			var list = new List<TestElement>();
			list.Add(new TestElement("root", 0));
			list.Add(new TestElement("A", 1));
			list.Add(new TestElement("B", 1));
			list.Add(new TestElement("Bchild", 2));

			// Test
			bool caughtException = false;
			try
			{
				TreeElementUtility.ListToTree(list);
			}
			catch (Exception)
			{
				caughtException = true;
			}

			// Assert
			Assert.IsTrue(caughtException, "We require the root.depth to be -1, here it is: " + list[0].depth);
		}

		[Test]
		public static void FindCommonAncestorsWithinListWorks()
		{
			// Arrange
			var list = new List<TestElement>();
			list.Add(new TestElement("root", -1));
			list.Add(new TestElement("A", 0));
			var b0 = new TestElement("B", 0);
			var b1 = new TestElement("Bchild", 1);
			var b2 = new TestElement("Bchildchild", 2);
			list.Add(b0);
			list.Add(b1);
			list.Add(b2);

			var c0 = new TestElement("C", 0);
			list.Add(c0);

			var f0 = new TestElement("F", 0);
			var f1 = new TestElement("Fchild", 1);
			var f2 = new TestElement("Fchildchild", 2);
			list.Add(f0);
			list.Add(f1);
			list.Add(f2);

			// Init tree structure: set children and parent properties
			TreeElementUtility.ListToTree(list);

			// Single element
			TestElement[] input = { b1 };
			TestElement[] expectedResult = { b1 };
			var result = TreeElementUtility.FindCommonAncestorsWithinList(input).ToArray();
			Assert.IsTrue(ArrayUtility.ArrayEquals(expectedResult, result), "Single input should return single output");

			// Single sub tree
			input = new[] { b1, b2 };
			expectedResult = new[] { b1 };
			result = TreeElementUtility.FindCommonAncestorsWithinList(input).ToArray();
			Assert.IsTrue(ArrayUtility.ArrayEquals(expectedResult, result), "Common ancestor should only be b1 ");

			// Multiple sub trees
			input = new[] { b0, b2, f0, f2, c0 };
			expectedResult = new[] { b0, f0, c0 };
			result = TreeElementUtility.FindCommonAncestorsWithinList(input).ToArray();
			Assert.IsTrue(ArrayUtility.ArrayEquals(expectedResult, result), "Common ancestor should only be b0, f0, c0");
		}
	}

}
