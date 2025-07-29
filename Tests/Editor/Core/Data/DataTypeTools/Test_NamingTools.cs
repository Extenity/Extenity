using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using Extenity.TextureToolbox;
using NUnit.Framework;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

// ReSharper disable ExpressionIsAlwaysNull

namespace ExtenityTests.DataToolbox
{

	public class Test_NamingTools : ExtenityTestBase
	{
		[Test]
		public void FullObjectName_UnfortunatelyFailsToDistinguishTypesOfNullDelegates()
		{
			Action nullDelegateAsAction = (Action)null;
			Delegate nullDelegateAsDelegate = (Delegate)null;

			Assert.AreNotEqual(NamingTools.NullDelegateName, nullDelegateAsAction.FullObjectName());
			Assert.AreNotEqual(NamingTools.NullDelegateName, nullDelegateAsDelegate.FullObjectName());
			Assert.AreEqual(NamingTools.NullName, nullDelegateAsAction.FullObjectName());
			Assert.AreEqual(NamingTools.NullName, nullDelegateAsDelegate.FullObjectName());

			// Though directly calling the name methods for Delegates works alright.
			Assert.AreEqual(NamingTools.NullDelegateName, nullDelegateAsAction.FullNameOfTargetAndMethod());
			Assert.AreEqual(NamingTools.NullDelegateName, nullDelegateAsDelegate.FullNameOfTargetAndMethod());
		}

		[Test]
		public void FullObjectName_UnfortunatelyFailsToDistinguishTypesOfNullSystemObjects()
		{
			Assert.AreEqual(NamingTools.NullName, ((System.Object)null).FullObjectName()); // There were never a class instance. So we don't know the type of the object.
		}

#if UNITY_5_3_OR_NEWER
		[Test]
		public void FullObjectName_UnfortunatelyFailsToDistinguishTypesOfNullUnityObjects()
		{
			var go = new GameObject();
			var component = go.AddComponent<Light>();
			var texture = TextureTools.CreateSimpleTexture(Color.black);
			GameObject.DestroyImmediate(go);
			GameObject.DestroyImmediate(texture);

			Assert.AreEqual(NamingTools.NullGameObjectName, go.FullObjectName()); // The GameObject as in the eyes of Unity is destroyed. But there is still a GameObject class instance as a C# object. So we know the type of the object.
			Assert.AreEqual(NamingTools.NullName, ((GameObject)null).FullObjectName()); // There were never a GameObject class instance. So we don't know the type of the object.

			Assert.AreEqual(NamingTools.NullComponentName, component.FullObjectName()); // The same applies for Component. See GameObject comments above.
			Assert.AreEqual(NamingTools.NullName, ((Component)null).FullObjectName());

			Assert.AreEqual(NamingTools.NullObjectName, texture.FullObjectName()); // The same applies for UnityEngine.Object. See GameObject comments above.
			Assert.AreEqual(NamingTools.NullName, ((UnityEngine.Object)null).FullObjectName());
		}
#endif

#if UNITY_5_3_OR_NEWER
		[Test]
		public void FullObjectName_ReturnsDefaultNamesAfterObjectsAreDestroyed()
		{
			var go = new GameObject("NamingToolsTestObject");
			var component = go.AddComponent<Test_NamingToolsTestComponent>();
			component.SomeDelegate = component.SomeMethod;
			var texture = TextureTools.CreateSimpleTexture(Color.black);
			Action delegateAsAction = component.SomeDelegate;
			Delegate delegateAsDelegate = component.SomeDelegate;

			Assert.AreEqual("NamingToolsTestObject", go.FullObjectName());
			Assert.AreEqual("NamingToolsTestObject|Test_NamingToolsTestComponent", component.FullObjectName());
			Assert.AreEqual(" (UnityEngine.Texture2D)", texture.FullObjectName());
			Assert.AreEqual("SomeMethod in NamingToolsTestObject|Test_NamingToolsTestComponent", delegateAsAction.FullObjectName());
			Assert.AreEqual("SomeMethod in NamingToolsTestObject|Test_NamingToolsTestComponent", delegateAsDelegate.FullObjectName());

			GameObject.DestroyImmediate(go);
			GameObject.DestroyImmediate(texture);

			// Just to be sure about delegate target, which is a Unity object, is reported as null.
			Assert.False(delegateAsAction.Target as Component);
			Assert.False(delegateAsAction.Target as UnityEngine.Object);
			Assert.False(delegateAsDelegate.Target as Component);
			Assert.False(delegateAsDelegate.Target as UnityEngine.Object);

			Assert.AreEqual(NamingTools.NullGameObjectName, go.FullObjectName());
			Assert.AreEqual(NamingTools.NullComponentName, component.FullObjectName());
			Assert.AreEqual(NamingTools.NullObjectName, texture.FullObjectName());
			Assert.AreEqual(NamingTools.NullDelegateNameWithMethod_Start + delegateAsDelegate.Method.Name + NamingTools.NullDelegateNameWithMethod_End, delegateAsAction.FullObjectName());
			Assert.AreEqual(NamingTools.NullDelegateNameWithMethod_Start + delegateAsDelegate.Method.Name + NamingTools.NullDelegateNameWithMethod_End, delegateAsDelegate.FullObjectName());
		}
#endif

#if UNITY_5_3_OR_NEWER
		[Test]
		public void FullObjectName_SomeGameObjectPathExamples()
		{
			Assert.AreEqual("A", CreateGameObjectHierarchy("A").FullObjectName());
			Assert.AreEqual("A/B", CreateGameObjectHierarchy("A", "B").FullObjectName());
			Assert.AreEqual("A/B/C", CreateGameObjectHierarchy("A", "B", "C").FullObjectName());
			Assert.AreEqual("A/B/C/D", CreateGameObjectHierarchy("A", "B", "C", "D").FullObjectName());

			Assert.AreEqual("A", CreateGameObjectHierarchy("A").FullObjectName(1));
			Assert.AreEqual(".../B", CreateGameObjectHierarchy("A", "B").FullObjectName(1));
			Assert.AreEqual(".../C", CreateGameObjectHierarchy("A", "B", "C").FullObjectName(1));
			Assert.AreEqual(".../D", CreateGameObjectHierarchy("A", "B", "C", "D").FullObjectName(1));

			Assert.AreEqual("A", CreateGameObjectHierarchy("A").FullObjectName(2));
			Assert.AreEqual("A/B", CreateGameObjectHierarchy("A", "B").FullObjectName(2));
			Assert.AreEqual(".../B/C", CreateGameObjectHierarchy("A", "B", "C").FullObjectName(2));
			Assert.AreEqual(".../C/D", CreateGameObjectHierarchy("A", "B", "C", "D").FullObjectName(2));

			Assert.AreEqual("A", CreateGameObjectHierarchy("A").FullObjectName(3));
			Assert.AreEqual("A/B", CreateGameObjectHierarchy("A", "B").FullObjectName(3));
			Assert.AreEqual("A/B/C", CreateGameObjectHierarchy("A", "B", "C").FullObjectName(3));
			Assert.AreEqual(".../B/C/D", CreateGameObjectHierarchy("A", "B", "C", "D").FullObjectName(3));

			Assert.AreEqual("A", CreateGameObjectHierarchy("A").FullObjectName(4));
			Assert.AreEqual("A/B", CreateGameObjectHierarchy("A", "B").FullObjectName(4));
			Assert.AreEqual("A/B/C", CreateGameObjectHierarchy("A", "B", "C").FullObjectName(4));
			Assert.AreEqual("A/B/C/D", CreateGameObjectHierarchy("A", "B", "C", "D").FullObjectName(4));

			Assert.AreEqual("A", CreateGameObjectHierarchy("A").FullObjectName(5));
			Assert.AreEqual("A/B", CreateGameObjectHierarchy("A", "B").FullObjectName(5));
			Assert.AreEqual("A/B/C", CreateGameObjectHierarchy("A", "B", "C").FullObjectName(5));
			Assert.AreEqual("A/B/C/D", CreateGameObjectHierarchy("A", "B", "C", "D").FullObjectName(5));
		}

		private static GameObject CreateGameObjectHierarchy(params string[] gameObjects)
		{
			var parent = new GameObject(gameObjects[0]);
			for (int i = 1; i < gameObjects.Length; i++)
			{
				var child = new GameObject(gameObjects[i]);
				child.transform.SetParent(parent.transform);
				parent = child;
			}

			return parent; // Return the last child. Don't mind the variable name 'parent' here, as it's actually the last child at this point.
		}
#endif
	}

}