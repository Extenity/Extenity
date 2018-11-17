using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox;
using Extenity.UnityTestToolbox;
using ExtenityTests.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ExtenityTests.DataToolbox
{

	public class Test_GameObjectTools : AssertionHelper
	{
		private Scene Scene;

		#region Initialization and Cleanup

		public IEnumerator Setup()
		{
			if (SceneManagerTools.GetLoadedScenes().Count > 1)
			{
				// Allow some time for previously loaded scene to unload. See TearDown method below.
				Log.Info("Waiting for previous scene to unload");
				const float timeout = 2f; // 2 seconds
				var startTime = PrecisionTiming.PreciseTime;
				while (SceneManagerTools.GetLoadedScenes().Count > 1)
				{
					if (timeout < PrecisionTiming.PreciseTime - startTime)
						throw new Exception("There are more than 1 scenes loaded.");
					yield return null;
				}
			}

			SceneManager.LoadScene("Test_GameObjectTools", LoadSceneMode.Additive);
			yield return null; // Need to wait for a bit so that Unity loads the scene and set it's isLoaded state. Otherwise we can't get the scene below.
			Scene = SceneManagerTools.GetLoadedScenes().First(scene => scene.name == "Test_GameObjectTools");
		}

		[TearDown]
		public void TearDown()
		{
			SceneManager.UnloadSceneAsync("Test_GameObjectTools");
			UnityTestTools.Cleanup();
		}

		#endregion

		#region FindObjectsOfType

		private static readonly string[] ActiveOnlyList =
		{
			"Cube",
			"Container 1/ChildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 4/ChildCube",
			"Container 5/ChildCube",
		};

		private static readonly string[] IncludingInactiveList =
		{
			"Cube",
			"Cube (GO Disabled)",
			"Cube (Comp Disabled)",
			"Container 1/ChildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 5/ChildCube",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
		};

		private static readonly string[] InactiveOnlyList =
		{
			"Cube (GO Disabled)",
			"Cube (Comp Disabled)",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
		};

		private static readonly string[] BehaviourAddition =
		{
			"MultiBehaviour",
		};

		private static readonly string[] IneffectiveComponentExclusion =
		{
			"Cube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
		};

		private static readonly string[] ActiveOnlyListForAllComponents =
		{
			"Empty",
			"Cube",
			"Cube",
			"Cube",
			"Cube",
			"Cube",
			"Cube",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Container 1",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 2",
			"Container 3",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 4",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 5",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"MultiBehaviour",
			"MultiBehaviour",
		};

		private static readonly string[] IncludingInactiveListForAllComponents =
		{
			"Empty",
			"Empty (GO Disabled)",
			"Cube",
			"Cube",
			"Cube",
			"Cube",
			"Cube",
			"Cube",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Container 1",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 2",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 3",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 4",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 5",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"MultiBehaviour",
			"MultiBehaviour",
			"MultiBehaviour",
		};

		private static readonly string[] InactiveOnlyListForAllComponents =
		{
			"Empty (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (GO Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Cube (Comp Disabled)",
			"Container 1 (GO Disabled)",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)",
			"MultiBehaviour",
		};

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_BoxCollider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<BoxCollider>(ActiveCheck.ActiveOnly), ActiveOnlyList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_Collider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Collider>(ActiveCheck.ActiveOnly), ActiveOnlyList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_MeshRenderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<MeshRenderer>(ActiveCheck.ActiveOnly), ActiveOnlyList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_Component()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Component>(ActiveCheck.ActiveOnly), ActiveOnlyListForAllComponents);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_Behaviour()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Behaviour>(ActiveCheck.ActiveOnly), ActiveOnlyList.Combine(BehaviourAddition));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_CanvasGroup()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<CanvasGroup>(ActiveCheck.ActiveOnly), ActiveOnlyList.Combine(IneffectiveComponentExclusion));
		}

		// ------------------------------------------------------

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_BoxCollider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<BoxCollider>(ActiveCheck.IncludingInactive), IncludingInactiveList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_Collider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Collider>(ActiveCheck.IncludingInactive), IncludingInactiveList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_MeshRenderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<MeshRenderer>(ActiveCheck.IncludingInactive), IncludingInactiveList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_Component()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Component>(ActiveCheck.IncludingInactive), IncludingInactiveListForAllComponents);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_Behaviour()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Behaviour>(ActiveCheck.IncludingInactive), IncludingInactiveList.Combine(BehaviourAddition).Combine(BehaviourAddition));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_CanvasGroup()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<CanvasGroup>(ActiveCheck.IncludingInactive), IncludingInactiveList);
		}

		// ------------------------------------------------------

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_BoxCollider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<BoxCollider>(ActiveCheck.InactiveOnly), InactiveOnlyList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_Collider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Collider>(ActiveCheck.InactiveOnly), InactiveOnlyList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_MeshRenderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<MeshRenderer>(ActiveCheck.InactiveOnly), InactiveOnlyList);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_Component()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Component>(ActiveCheck.InactiveOnly), InactiveOnlyListForAllComponents);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_Behaviour()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Behaviour>(ActiveCheck.InactiveOnly), InactiveOnlyList.Combine(BehaviourAddition));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_CanvasGroup()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<CanvasGroup>(ActiveCheck.InactiveOnly), InactiveOnlyList.Remove(IneffectiveComponentExclusion));
		}

		private IEnumerator TestFindObjectsOfType<T>(Func<List<T>> searchMethod, string[] expectedGameObjectPaths) where T : Component
		{
			yield return Setup();
			var foundComponents = searchMethod();

			var resultingGameObjectPaths = foundComponents.Select(item => item.gameObject.FullName()).ToList();
			resultingGameObjectPaths.Sort();
			Array.Sort(expectedGameObjectPaths);

			if (!resultingGameObjectPaths.SequenceEqual(expectedGameObjectPaths))
			{
				LogExpectedPaths(expectedGameObjectPaths);
				LogResult(foundComponents);
				expectedGameObjectPaths.LogList("Expected:");
				resultingGameObjectPaths.LogList("Found:");
				Assert.Fail("Found GameObject paths does not match the expected paths. See logs for details.");
			}
		}

		#endregion

		#region IsComponentEnabled

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_MeshRenderer()
		{
			yield return TestIsComponentEnabled<MeshRenderer>();
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_Renderer()
		{
			yield return TestIsComponentEnabled<Renderer>();
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_BoxCollider()
		{
			yield return TestIsComponentEnabled<BoxCollider>();
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_Collider()
		{
			yield return TestIsComponentEnabled<Collider>();
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_Behaviour()
		{
			yield return TestIsComponentEnabled<Behaviour>();
			var components = Scene.FindObjectsOfType<Behaviour>(ActiveCheck.IncludingInactive);
			Assert.True(components.FindComponentsByGameObjectPath("MultiBehaviour").Cast<MarkedTestBehaviour>().First(component => component.Mark == "Enabled One").IsComponentEnabled());
			Assert.False(components.FindComponentsByGameObjectPath("MultiBehaviour").Cast<MarkedTestBehaviour>().First(component => component.Mark == "Disabled One").IsComponentEnabled());
		}

		private IEnumerator TestIsComponentEnabled<T>() where T : Component
		{
			yield return Setup();
			var components = Scene.FindObjectsOfType<T>(ActiveCheck.IncludingInactive);

			Assert.True(components.FindSingleComponentByGameObjectPath("Cube").IsComponentEnabled());
			Assert.True(components.FindSingleComponentByGameObjectPath("Cube (GO Disabled)").IsComponentEnabled());
			Assert.False(components.FindSingleComponentByGameObjectPath("Cube (Comp Disabled)").IsComponentEnabled());
			Assert.True(components.FindSingleComponentByGameObjectPath("Container 1/ChildCube").IsComponentEnabled());
			Assert.True(components.FindSingleComponentByGameObjectPath("Container 1 (GO Disabled)/ChildCube").IsComponentEnabled());
			Assert.False(components.FindSingleComponentByGameObjectPath("Container 3/ChildCube (Comp Disabled)").IsComponentEnabled());
			Assert.False(components.FindSingleComponentByGameObjectPath("Container 3 (GO Disabled)/ChildCube (Comp Disabled)").IsComponentEnabled());
		}

		#endregion

		#region Tools

		private void LogResult<T>(ICollection<T> objects) where T : Component
		{
			Log.Info($"Listing '{objects.Count}' objects of type '{typeof(T).Name}':");
			foreach (var obj in objects)
			{
				Log.Info(obj.FullName(), obj.gameObject);
			}
		}

		private void LogExpectedPaths(ICollection<string> paths)
		{
			Log.Info($"Listing '{paths.Count}' expected paths':");
			foreach (var path in paths)
			{
				Log.Info(path);
			}
		}

		#endregion
	}

}
