using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using Extenity.TextureToolbox;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtenityTests.DataToolbox
{

	public class Test_NamingTools : ExtenityTestBase
	{
		[Test]
		public void FullObjectName_UnfortunatelyFailsToDistinguishNullDelegates()
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
			Assert.False(delegateAsAction.Target as Object);
			Assert.False(delegateAsDelegate.Target as Component);
			Assert.False(delegateAsDelegate.Target as Object);

			Assert.AreEqual(NamingTools.NullGameObjectName, go.FullObjectName());
			Assert.AreEqual(NamingTools.NullComponentName, component.FullObjectName());
			Assert.AreEqual(NamingTools.NullObjectName, texture.FullObjectName());
			Assert.AreEqual(NamingTools.NullDelegateNameWithMethod(delegateAsDelegate.Method.Name), delegateAsAction.FullObjectName());
			Assert.AreEqual(NamingTools.NullDelegateNameWithMethod(delegateAsDelegate.Method.Name), delegateAsDelegate.FullObjectName());
		}
	}

}
