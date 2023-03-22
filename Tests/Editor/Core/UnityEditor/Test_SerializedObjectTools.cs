using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Extenity.Testing;
using Extenity.UnityEditorToolbox;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Logger = Extenity.Logger;
using Object = UnityEngine.Object;

namespace ExtenityTests.UnityEditorToolbox
{

	public class Test_SerializedObjectTools : ExtenityTestBase
	{

		public class TestSerializedObject : ScriptableObject
		{
			public string TheStringValue = "1111.23";
			public float TheFloatValue = 1111.23f;
			public float[] TheFloatArray = new float[] { 1111.1f, 1112.2f, 1113.3f };
			public List<float> TheFloatList = new List<float> { 1114.1f, 1115.2f, 1116.3f };

			public TestClassOne TheClassOne;
			public TestClassOne[] TheClassOneArray =
			{
				new TestClassOne(),
				new TestClassOne()
			};
			public List<TestClassOne> TheClassOneList = new List<TestClassOne>
			{
				new TestClassOne(),
				new TestClassOne()
			};

			public TestClassTwo TheClassTwo;
			public TestClassTwo[] TheClassTwoArray =
			{
				new TestClassTwo(),
				new TestClassTwo()
			};
			public List<TestClassTwo> TheClassTwoList = new List<TestClassTwo>
			{
				new TestClassTwo(),
				new TestClassTwo()
			};
		}

		[Serializable]
		public class TestClassOne
		{
			public string TheStringValue = "1.23";
			public float TheFloatValue = 1.23f;
			public float[] TheFloatArray = new float[] { 1.1f, 2.2f, 3.3f };
			public List<float> TheFloatList = new List<float> { 4.1f, 5.2f, 6.3f };

			public TestClassTwo TheClassTwo = new TestClassTwo
			{
				TheStringValue = "1340",
				TheIntValue = 1340,
				TheIntArray = new int[] { 1110, 1220, 1330 },
				TheIntList = new List<int> { 1410, 1520, 1630 }
			};

			public TestClassTwo[] TheClassTwoArray =
			{
				new TestClassTwo
				{
					TheStringValue = "2340",
					TheIntValue = 2340,
					TheIntArray = new int[] { 2110, 2220, 2330 },
					TheIntList = new List<int> { 2410, 2520, 2630 }
				},
				new TestClassTwo
				{
					TheStringValue = "3450",
					TheIntValue = 3450,
					TheIntArray = new int[] { 3110, 3220, 3330 },
					TheIntList = new List<int> { 3410, 3520, 3630 }
				}
			};

			public List<TestClassTwo> TheClassTwoList = new List<TestClassTwo>
			{
				new TestClassTwo
				{
					TheStringValue = "4340",
					TheIntValue = 4340,
					TheIntArray = new int[] { 4110, 4220, 4330 },
					TheIntList = new List<int> { 4410, 4520, 4630 }
				},
				new TestClassTwo
				{
					TheStringValue = "5450",
					TheIntValue = 5450,
					TheIntArray = new int[] { 5110, 5220, 5330 },
					TheIntList = new List<int> { 5410, 5520, 5630 }
				}
			};
		}

		[Serializable]
		public class TestClassTwo
		{
			public string TheStringValue = "123";
			public int TheIntValue = 123;
			public int[] TheIntArray = new int[] { 11, 22, 33 };
			public List<int> TheIntList = new List<int> { 41, 52, 63 };
		}

		#region SerializedProperty Type Getters

		[Test]
		public static void GetFieldInfo()
		{
			InternalCreateSerializedObject();

			AssertGetFieldInfo("TheStringValue", typeof(TestSerializedObject).GetField("TheStringValue"));
			AssertGetFieldInfo("TheStringValue.Array", typeof(TestSerializedObject).GetField("TheStringValue"));
			AssertGetFieldInfo("TheStringValue.Array.size", typeof(TestSerializedObject).GetField("TheStringValue"));
			AssertGetFieldInfo("TheStringValue.Array.data[0]", typeof(TestSerializedObject).GetField("TheStringValue"));
			AssertGetFieldInfo("TheFloatValue", typeof(TestSerializedObject).GetField("TheFloatValue"));
			AssertGetFieldInfo("TheFloatArray", typeof(TestSerializedObject).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheFloatArray.Array", typeof(TestSerializedObject).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheFloatArray.Array.size", typeof(TestSerializedObject).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheFloatArray.Array.data[0]", typeof(TestSerializedObject).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheFloatList", typeof(TestSerializedObject).GetField("TheFloatList"));
			AssertGetFieldInfo("TheFloatList.Array", typeof(TestSerializedObject).GetField("TheFloatList"));
			AssertGetFieldInfo("TheFloatList.Array.size", typeof(TestSerializedObject).GetField("TheFloatList"));
			AssertGetFieldInfo("TheFloatList.Array.data[0]", typeof(TestSerializedObject).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOne", typeof(TestSerializedObject).GetField("TheClassOne"));
			AssertGetFieldInfo("TheClassOne.TheStringValue", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheStringValue.Array", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheStringValue.Array.size", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheStringValue.Array.data[0]", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheFloatValue", typeof(TestClassOne).GetField("TheFloatValue"));
			AssertGetFieldInfo("TheClassOne.TheFloatArray", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOne.TheFloatArray.Array", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOne.TheFloatArray.Array.size", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOne.TheFloatArray.Array.data[0]", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOne.TheFloatList", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOne.TheFloatList.Array", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOne.TheFloatList.Array.size", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOne.TheFloatList.Array.data[0]", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo", typeof(TestClassOne).GetField("TheClassTwo"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.size", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0]", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.size", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0]", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray", typeof(TestSerializedObject).GetField("TheClassOneArray"));
			AssertGetFieldInfo("TheClassOneArray.Array", typeof(TestSerializedObject).GetField("TheClassOneArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.size", typeof(TestSerializedObject).GetField("TheClassOneArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0]", typeof(TestSerializedObject).GetField("TheClassOneArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheStringValue", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheStringValue.Array", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatValue", typeof(TestClassOne).GetField("TheFloatValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatArray", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatArray.Array", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatArray.Array.size", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatArray.Array.data[0]", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatList", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatList.Array", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatList.Array.size", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheFloatList.Array.data[0]", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo", typeof(TestClassOne).GetField("TheClassTwo"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.size", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0]", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.size", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0]", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList", typeof(TestSerializedObject).GetField("TheClassOneList"));
			AssertGetFieldInfo("TheClassOneList.Array", typeof(TestSerializedObject).GetField("TheClassOneList"));
			AssertGetFieldInfo("TheClassOneList.Array.size", typeof(TestSerializedObject).GetField("TheClassOneList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0]", typeof(TestSerializedObject).GetField("TheClassOneList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheStringValue", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheStringValue.Array", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheStringValue.Array.size", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassOne).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatValue", typeof(TestClassOne).GetField("TheFloatValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatArray", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatArray.Array", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatArray.Array.size", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatArray.Array.data[0]", typeof(TestClassOne).GetField("TheFloatArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatList", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatList.Array", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatList.Array.size", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheFloatList.Array.data[0]", typeof(TestClassOne).GetField("TheFloatList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo", typeof(TestClassOne).GetField("TheClassTwo"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.size", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0]", typeof(TestClassOne).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.size", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0]", typeof(TestClassOne).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwo", typeof(TestSerializedObject).GetField("TheClassTwo"));
			AssertGetFieldInfo("TheClassTwo.TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwo.TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwo.TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassTwo.TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwo.TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwo.TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwo.TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoArray", typeof(TestSerializedObject).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array", typeof(TestSerializedObject).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.size", typeof(TestSerializedObject).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0]", typeof(TestSerializedObject).GetField("TheClassTwoArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoList", typeof(TestSerializedObject).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassTwoList.Array", typeof(TestSerializedObject).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassTwoList.Array.size", typeof(TestSerializedObject).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0]", typeof(TestSerializedObject).GetField("TheClassTwoList"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo).GetField("TheStringValue"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo).GetField("TheIntValue"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo).GetField("TheIntArray"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo).GetField("TheIntList"));
			AssertGetFieldInfo("TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo).GetField("TheIntList"));

			InternalDestroySerializedObject();
		}

		[Test]
		public static void GetDeclaringType()
		{
			InternalCreateSerializedObject();

			AssertGetDeclaringType("TheStringValue", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheStringValue.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheStringValue.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheStringValue.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatValue", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatArray", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatArray.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatArray.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatArray.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatList", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatList.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatList.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheFloatList.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOne", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOne.TheStringValue", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheStringValue.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheStringValue.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheStringValue.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatValue", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatArray", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatArray.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatArray.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatArray.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatList", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatList.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatList.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheFloatList.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwo", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneArray.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneArray.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheStringValue", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheStringValue.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatValue", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatArray", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatArray.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatArray.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatArray.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatList", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatList.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatList.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheFloatList.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneList.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneList.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneList.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheStringValue", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheStringValue.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheStringValue.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatValue", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatArray", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatArray.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatArray.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatArray.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatList", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatList.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatList.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheFloatList.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.size", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0]", typeof(TestClassOne));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwo.TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoArray.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoArray.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoList.Array", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoList.Array.size", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0]", typeof(TestSerializedObject));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo));
			AssertGetDeclaringType("TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo));

			InternalDestroySerializedObject();
		}

		[Test]
		public static void GetDeclaringTypeAndObject()
		{
			InternalCreateSerializedObject();

			AssertGetDeclaringTypeAndObject("TheStringValue", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheStringValue.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheStringValue.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheStringValue.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatValue", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatArray", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatArray.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatArray.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatArray.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatList", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatList.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatList.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheFloatList.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOne", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheStringValue", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheStringValue.Array", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheStringValue.Array.size", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheStringValue.Array.data[0]", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatValue", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatArray", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatArray.Array", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatArray.Array.size", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatArray.Array.data[0]", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatList", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatList.Array", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatList.Array.size", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheFloatList.Array.data[0]", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheStringValue", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntValue", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntArray", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntList", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.size", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0]", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.size", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0]", typeof(TestClassOne), Instance.TheClassOne);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOne.TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOne.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheStringValue", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheStringValue.Array", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatValue", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatArray", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatArray.Array", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatArray.Array.size", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatArray.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatList", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatList.Array", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatList.Array.size", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheFloatList.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntValue", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.size", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.size", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneArray.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneArray[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheStringValue", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheStringValue.Array", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheStringValue.Array.size", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatValue", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatArray", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatArray.Array", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatArray.Array.size", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatArray.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatList", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatList.Array", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatList.Array.size", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheFloatList.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntValue", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntList", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.size", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.size", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0]", typeof(TestClassOne), Instance.TheClassOneList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassOneList.Array.data[0].TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassOneList[0].TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwo", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheStringValue", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntValue", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntArray", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntList", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntList.Array", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwo.TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwo);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoArray.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwoArray[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.size", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0]", typeof(TestSerializedObject), Instance);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheStringValue", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheStringValue.Array", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheStringValue.Array.size", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheStringValue.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntValue", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntArray", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntArray.Array", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntArray.Array.size", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntArray.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntList", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntList.Array", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntList.Array.size", typeof(TestClassTwo), Instance.TheClassTwoList[0]);
			AssertGetDeclaringTypeAndObject("TheClassTwoList.Array.data[0].TheIntList.Array.data[0]", typeof(TestClassTwo), Instance.TheClassTwoList[0]);

			InternalDestroySerializedObject();
		}

		private static void AssertGetFieldInfo(string propertyPath, FieldInfo expectedFieldInfo)
		{
			var fieldInfo = SerializedObject.FindProperty(propertyPath).GetFieldInfo();
			Assert.AreEqual(expectedFieldInfo, fieldInfo, "Got wrong FieldInfo for property at path '{0}'.", propertyPath);
		}

		private static void AssertGetDeclaringType(string propertyPath, Type expectedType)
		{
			var type = SerializedObject.FindProperty(propertyPath).GetDeclaringType();
			Assert.AreEqual(expectedType, type, "Got wrong Declaring Type for property at path '{0}'.", propertyPath);
		}

		private static void AssertGetDeclaringTypeAndObject(string propertyPath, Type expectedType, object expectedObject)
		{
			SerializedObject.FindProperty(propertyPath).GetDeclaringTypeAndObject(out var type, out var obj);
			Assert.AreEqual(expectedType, type, "Got wrong Declaring Type for property at path '{0}'.", propertyPath);
			Assert.AreEqual(expectedObject, obj, "Got wrong Declaring Object for property at path '{0}'.", propertyPath);
		}

		#endregion

		#region Tools

		private static TestSerializedObject Instance;
		private static SerializedObject SerializedObject;

		private static void InternalCreateSerializedObject()
		{
			if (Instance != null || SerializedObject != null)
			{
				InternalDestroySerializedObject();
			}

			Instance = ScriptableObject.CreateInstance<TestSerializedObject>();
			SerializedObject = new SerializedObject(Instance);
			//SerializedObject.LogAllPropertyPaths();
		}

		private static void InternalDestroySerializedObject()
		{
			Object.DestroyImmediate(Instance);
			Instance = null;
			SerializedObject.Dispose();
			SerializedObject = null;
		}

		private static void InternalLogAllFieldsInTestSerializedObject()
		{
			var stringBuilder = new StringBuilder();
			var serializedObject = new SerializedObject(ScriptableObject.CreateInstance<TestSerializedObject>());
			var iterator = serializedObject.GetIterator();
			do
			{
				var path = ((SerializedProperty)iterator).propertyPath;
				if (!path.Contains("[1]") &&
				    !path.Contains("[2]") &&
				    !path.Contains("[3]") &&
				    !path.Contains("[4]") &&
				    !path.Contains("[5]") &&
				    !path.Contains("[6]") &&
				    !path.Contains("[7]") &&
				    path.StartsWith("The"))
				{
					stringBuilder.AppendLine(path);
				}
			} while (iterator.Next(true));
			Log.Info(stringBuilder.ToString());
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Test_SerializedObjectTools));

		#endregion
	}

}
