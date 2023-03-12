using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.DataToolbox
{

	public class Test_DelegateTools : ExtenityTestBase
	{
		[Test]
		public void IsUnityObjectTargeted_DistinguishesBetweenUnityAndNonUnityObjects()
		{
			var component = new GameObject("DelegateToolsTestObject").AddComponent<Test_DelegateToolsTestComponent>();
			component.SomeDelegate = component.SomeMethod;
			var nonUnityObject = new Test_DelegateToolsTestNonUnityObject();
			nonUnityObject.SomeDelegate = nonUnityObject.SomeMethod;

			Action unityDelegateAsAction = component.SomeDelegate;
			Delegate unityDelegateAsDelegate = component.SomeDelegate;
			Action nonUnityDelegateAsAction = nonUnityObject.SomeDelegate;
			Delegate nonUnityDelegateAsDelegate = nonUnityObject.SomeDelegate;

			Assert.True(unityDelegateAsAction.IsUnityObjectTargeted());
			Assert.True(unityDelegateAsDelegate.IsUnityObjectTargeted());
			Assert.False(nonUnityDelegateAsAction.IsUnityObjectTargeted());
			Assert.False(nonUnityDelegateAsDelegate.IsUnityObjectTargeted());
		}

		[Test]
		public void IsUnityObjectTargeted_DistinguishesBetweenUnityAndNonUnityObjects_EvenAfterDestroyed()
		{
			var component = new GameObject("DelegateToolsTestObject").AddComponent<Test_DelegateToolsTestComponent>();
			component.SomeDelegate = component.SomeMethod;
			var nonUnityObject = new Test_DelegateToolsTestNonUnityObject();
			nonUnityObject.SomeDelegate = nonUnityObject.SomeMethod;

			Action unityDelegateAsAction = component.SomeDelegate;
			Delegate unityDelegateAsDelegate = component.SomeDelegate;
			Action nonUnityDelegateAsAction = nonUnityObject.SomeDelegate;
			Delegate nonUnityDelegateAsDelegate = nonUnityObject.SomeDelegate;

			GameObject.DestroyImmediate(component.gameObject);

			Assert.True(unityDelegateAsAction.IsUnityObjectTargeted());
			Assert.True(unityDelegateAsDelegate.IsUnityObjectTargeted());
			Assert.False(nonUnityDelegateAsAction.IsUnityObjectTargeted());
			Assert.False(nonUnityDelegateAsDelegate.IsUnityObjectTargeted());
		}

		[Test]
		public void IsUnityObjectTargetedAndAlive_DistinguishesBetweenDestroyedAndAliveUnityObjects()
		{
			var component = new GameObject("DelegateToolsTestObject").AddComponent<Test_DelegateToolsTestComponent>();
			component.SomeDelegate = component.SomeMethod;

			Action unityDelegateAsAction = component.SomeDelegate;
			Delegate unityDelegateAsDelegate = component.SomeDelegate;

			Assert.True(unityDelegateAsAction.IsUnityObjectTargetedAndAlive());
			Assert.True(unityDelegateAsDelegate.IsUnityObjectTargetedAndAlive());

			GameObject.DestroyImmediate(component.gameObject);

			Assert.False(unityDelegateAsAction.IsUnityObjectTargetedAndAlive());
			Assert.False(unityDelegateAsDelegate.IsUnityObjectTargetedAndAlive());
		}

		[Test]
		public void IsUnityObjectTargetedAndDestroyed_DistinguishesBetweenDestroyedAndAliveUnityObjects()
		{
			var component = new GameObject("DelegateToolsTestObject").AddComponent<Test_DelegateToolsTestComponent>();
			component.SomeDelegate = component.SomeMethod;

			Action unityDelegateAsAction = component.SomeDelegate;
			Delegate unityDelegateAsDelegate = component.SomeDelegate;

			Assert.False(unityDelegateAsAction.IsUnityObjectTargetedAndDestroyed());
			Assert.False(unityDelegateAsDelegate.IsUnityObjectTargetedAndDestroyed());

			GameObject.DestroyImmediate(component.gameObject);

			Assert.True(unityDelegateAsAction.IsUnityObjectTargetedAndDestroyed());
			Assert.True(unityDelegateAsDelegate.IsUnityObjectTargetedAndDestroyed());
		}
	}

}