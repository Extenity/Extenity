using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extenity;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Logger = Extenity.Logger;
#if UNITY_EDITOR
using Extenity.AssetToolbox.Editor;
#endif

namespace ExtenityTests.GameObjectToolbox
{

	public class Test_GameObjectTools : ExtenityTestBase
	{
		private Scene Scene => SceneManager.GetActiveScene();

		#region Initialization and Cleanup

		protected override void OnInitialize()
		{
#if UNITY_EDITOR
			AssetDatabaseTools.InstantiatePrefabWithTheSameNameOfThisScript();
#else
			throw new System.NotImplementedException("Find a way to load test assets for device compilations.");
#endif
			base.OnInitialize();
		}

		#endregion

		#region FindObjectsOfType

		#region List - ActiveOnly

		private static readonly string[] List_ActiveOnly_BoxCollider =
		{
			"Container 1/ChildCube/GrandchildCube|BoxCollider",
			"Container 1/ChildCube|BoxCollider",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|BoxCollider",
			"Container 4/ChildCube|BoxCollider",
			"Container 5/ChildCube|BoxCollider",
			"Cube|BoxCollider",
		};

		private static readonly string[] List_ActiveOnly_MeshRenderer =
		{
			"Container 1/ChildCube/GrandchildCube|MeshRenderer",
			"Container 1/ChildCube|MeshRenderer",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|MeshRenderer",
			"Container 4/ChildCube|MeshRenderer",
			"Container 5/ChildCube|MeshRenderer",
			"Cube|MeshRenderer",
		};

		private static readonly string[] List_ActiveOnly_Component =
		{
			"Container 1/ChildCube/GrandchildCube|BoxCollider",
			"Container 1/ChildCube/GrandchildCube|CanvasGroup",
			"Container 1/ChildCube/GrandchildCube|EmptyTestBehaviour",
			"Container 1/ChildCube/GrandchildCube|MeshFilter",
			"Container 1/ChildCube/GrandchildCube|MeshRenderer",
			"Container 1/ChildCube/GrandchildCube|Transform",
			"Container 1/ChildCube|BoxCollider",
			"Container 1/ChildCube|CanvasGroup",
			"Container 1/ChildCube|EmptyTestBehaviour",
			"Container 1/ChildCube|MeshFilter",
			"Container 1/ChildCube|MeshRenderer",
			"Container 1/ChildCube|Transform",
			"Container 1|Transform",
			"Container 2|Transform",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|BoxCollider",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|CanvasGroup",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|MeshFilter",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|MeshRenderer",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|Transform",
			"Container 3/ChildCube (Comp Disabled)|MeshFilter",
			"Container 3/ChildCube (Comp Disabled)|Transform",
			"Container 3|Transform",
			"Container 4/ChildCube|BoxCollider",
			"Container 4/ChildCube|CanvasGroup",
			"Container 4/ChildCube|EmptyTestBehaviour",
			"Container 4/ChildCube|MeshFilter",
			"Container 4/ChildCube|MeshRenderer",
			"Container 4/ChildCube|Transform",
			"Container 4|Transform",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|MeshFilter",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|Transform",
			"Container 5/ChildCube|BoxCollider",
			"Container 5/ChildCube|CanvasGroup",
			"Container 5/ChildCube|EmptyTestBehaviour",
			"Container 5/ChildCube|MeshFilter",
			"Container 5/ChildCube|MeshRenderer",
			"Container 5/ChildCube|Transform",
			"Container 5|Transform",
			"Cube (Comp Disabled)|MeshFilter",
			"Cube (Comp Disabled)|Transform",
			"Cube|BoxCollider",
			"Cube|CanvasGroup",
			"Cube|EmptyTestBehaviour",
			"Cube|MeshFilter",
			"Cube|MeshRenderer",
			"Cube|Transform",
			"Empty|Transform",
			"MultiBehaviour|MarkedTestBehaviour|Enabled One",
			"MultiBehaviour|Transform",
		};

		private static readonly string[] List_ActiveOnly_Behaviour =
		{
			"Container 1/ChildCube/GrandchildCube|CanvasGroup",
			"Container 1/ChildCube/GrandchildCube|EmptyTestBehaviour",
			"Container 1/ChildCube|CanvasGroup",
			"Container 1/ChildCube|EmptyTestBehaviour",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|CanvasGroup",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 4/ChildCube|CanvasGroup",
			"Container 4/ChildCube|EmptyTestBehaviour",
			"Container 5/ChildCube|CanvasGroup",
			"Container 5/ChildCube|EmptyTestBehaviour",
			"Cube|CanvasGroup",
			"Cube|EmptyTestBehaviour",
			"MultiBehaviour|MarkedTestBehaviour|Enabled One",
		};

		private static readonly string[] List_ActiveOnly_CanvasGroup =
		{
			"Container 1/ChildCube/GrandchildCube|CanvasGroup",
			"Container 1/ChildCube|CanvasGroup",
			"Container 3/ChildCube (Comp Disabled)/GrandchildCube|CanvasGroup",
			"Container 4/ChildCube|CanvasGroup",
			"Container 5/ChildCube|CanvasGroup",
			"Cube|CanvasGroup",
		};

		#endregion

		#region List - InactiveOnly

		private static readonly string[] List_InactiveOnly_BoxCollider =
		{
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|BoxCollider",
			"Container 1 (GO Disabled)/ChildCube|BoxCollider",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|BoxCollider",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|BoxCollider",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|BoxCollider",
			"Container 2/ChildCube (GO Disabled)|BoxCollider",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|BoxCollider",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|BoxCollider",
			"Container 3/ChildCube (Comp Disabled)|BoxCollider",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|BoxCollider",
			"Container 4 (GO Disabled)/ChildCube|BoxCollider",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|BoxCollider",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|BoxCollider",
			"Container 5 (GO Disabled)/ChildCube|BoxCollider",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|BoxCollider",
			"Cube (Comp Disabled)|BoxCollider",
			"Cube (GO Disabled)|BoxCollider",
		};

		private static readonly string[] List_InactiveOnly_MeshRenderer =
		{
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|MeshRenderer",
			"Container 1 (GO Disabled)/ChildCube|MeshRenderer",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|MeshRenderer",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|MeshRenderer",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|MeshRenderer",
			"Container 2/ChildCube (GO Disabled)|MeshRenderer",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|MeshRenderer",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|MeshRenderer",
			"Container 3/ChildCube (Comp Disabled)|MeshRenderer",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|MeshRenderer",
			"Container 4 (GO Disabled)/ChildCube|MeshRenderer",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|MeshRenderer",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|MeshRenderer",
			"Container 5 (GO Disabled)/ChildCube|MeshRenderer",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|MeshRenderer",
			"Cube (Comp Disabled)|MeshRenderer",
			"Cube (GO Disabled)|MeshRenderer",
		};

		private static readonly string[] List_InactiveOnly_Component =
		{
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|BoxCollider",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|CanvasGroup",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|EmptyTestBehaviour",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|MeshFilter",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|MeshRenderer",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|Transform",
			"Container 1 (GO Disabled)/ChildCube|BoxCollider",
			"Container 1 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 1 (GO Disabled)/ChildCube|EmptyTestBehaviour",
			"Container 1 (GO Disabled)/ChildCube|MeshFilter",
			"Container 1 (GO Disabled)/ChildCube|MeshRenderer",
			"Container 1 (GO Disabled)/ChildCube|Transform",
			"Container 1 (GO Disabled)|Transform",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|BoxCollider",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|CanvasGroup",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|MeshFilter",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|MeshRenderer",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|Transform",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|BoxCollider",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|CanvasGroup",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|MeshFilter",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|MeshRenderer",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|Transform",
			"Container 2 (GO Disabled)|Transform",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|BoxCollider",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|CanvasGroup",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|MeshFilter",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|MeshRenderer",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|Transform",
			"Container 2/ChildCube (GO Disabled)|BoxCollider",
			"Container 2/ChildCube (GO Disabled)|CanvasGroup",
			"Container 2/ChildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 2/ChildCube (GO Disabled)|MeshFilter",
			"Container 2/ChildCube (GO Disabled)|MeshRenderer",
			"Container 2/ChildCube (GO Disabled)|Transform",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|BoxCollider",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|CanvasGroup",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|MeshFilter",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|MeshRenderer",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|Transform",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|BoxCollider",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|CanvasGroup",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|MeshFilter",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|MeshRenderer",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|Transform",
			"Container 3 (GO Disabled)|Transform",
			"Container 3/ChildCube (Comp Disabled)|BoxCollider",
			"Container 3/ChildCube (Comp Disabled)|CanvasGroup",
			"Container 3/ChildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 3/ChildCube (Comp Disabled)|MeshRenderer",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|BoxCollider",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|CanvasGroup",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|MeshFilter",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|MeshRenderer",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|Transform",
			"Container 4 (GO Disabled)/ChildCube|BoxCollider",
			"Container 4 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 4 (GO Disabled)/ChildCube|EmptyTestBehaviour",
			"Container 4 (GO Disabled)/ChildCube|MeshFilter",
			"Container 4 (GO Disabled)/ChildCube|MeshRenderer",
			"Container 4 (GO Disabled)/ChildCube|Transform",
			"Container 4 (GO Disabled)|Transform",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|BoxCollider",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|CanvasGroup",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|MeshFilter",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|MeshRenderer",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|Transform",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|BoxCollider",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|CanvasGroup",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|MeshFilter",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|MeshRenderer",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|Transform",
			"Container 5 (GO Disabled)/ChildCube|BoxCollider",
			"Container 5 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 5 (GO Disabled)/ChildCube|EmptyTestBehaviour",
			"Container 5 (GO Disabled)/ChildCube|MeshFilter",
			"Container 5 (GO Disabled)/ChildCube|MeshRenderer",
			"Container 5 (GO Disabled)/ChildCube|Transform",
			"Container 5 (GO Disabled)|Transform",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|BoxCollider",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|CanvasGroup",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|MeshRenderer",
			"Cube (Comp Disabled)|BoxCollider",
			"Cube (Comp Disabled)|CanvasGroup",
			"Cube (Comp Disabled)|EmptyTestBehaviour",
			"Cube (Comp Disabled)|MeshRenderer",
			"Cube (GO Disabled)|BoxCollider",
			"Cube (GO Disabled)|CanvasGroup",
			"Cube (GO Disabled)|EmptyTestBehaviour",
			"Cube (GO Disabled)|MeshFilter",
			"Cube (GO Disabled)|MeshRenderer",
			"Cube (GO Disabled)|Transform",
			"Empty (GO Disabled)|Transform",
			"MultiBehaviour|MarkedTestBehaviour|Disabled One",
		};

		private static readonly string[] List_InactiveOnly_Behaviour =
		{
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|CanvasGroup",
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|EmptyTestBehaviour",
			"Container 1 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 1 (GO Disabled)/ChildCube|EmptyTestBehaviour",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|CanvasGroup",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|CanvasGroup",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|CanvasGroup",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 2/ChildCube (GO Disabled)|CanvasGroup",
			"Container 2/ChildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|CanvasGroup",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|EmptyTestBehaviour",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|CanvasGroup",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 3/ChildCube (Comp Disabled)|CanvasGroup",
			"Container 3/ChildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|CanvasGroup",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 4 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 4 (GO Disabled)/ChildCube|EmptyTestBehaviour",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|CanvasGroup",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|EmptyTestBehaviour",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|CanvasGroup",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|EmptyTestBehaviour",
			"Container 5 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 5 (GO Disabled)/ChildCube|EmptyTestBehaviour",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|CanvasGroup",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|EmptyTestBehaviour",
			"Cube (Comp Disabled)|CanvasGroup",
			"Cube (Comp Disabled)|EmptyTestBehaviour",
			"Cube (GO Disabled)|CanvasGroup",
			"Cube (GO Disabled)|EmptyTestBehaviour",
			"MultiBehaviour|MarkedTestBehaviour|Disabled One",
		};

		private static readonly string[] List_InactiveOnly_CanvasGroup =
		{
			"Container 1 (GO Disabled)/ChildCube/GrandchildCube|CanvasGroup",
			"Container 1 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)/GrandchildCube|CanvasGroup",
			"Container 2 (GO Disabled)/ChildCube (GO Disabled)|CanvasGroup",
			"Container 2/ChildCube (GO Disabled)/GrandchildCube|CanvasGroup",
			"Container 2/ChildCube (GO Disabled)|CanvasGroup",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)/GrandchildCube|CanvasGroup",
			"Container 3 (GO Disabled)/ChildCube (Comp Disabled)|CanvasGroup",
			"Container 3/ChildCube (Comp Disabled)|CanvasGroup",
			"Container 4 (GO Disabled)/ChildCube/GrandchildCube (GO Disabled)|CanvasGroup",
			"Container 4 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 4/ChildCube/GrandchildCube (GO Disabled)|CanvasGroup",
			"Container 5 (GO Disabled)/ChildCube/GrandchildCube (Comp Disabled)|CanvasGroup",
			"Container 5 (GO Disabled)/ChildCube|CanvasGroup",
			"Container 5/ChildCube/GrandchildCube (Comp Disabled)|CanvasGroup",
			"Cube (Comp Disabled)|CanvasGroup",
			"Cube (GO Disabled)|CanvasGroup",
		};

		#endregion

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_A1_BoxCollider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<BoxCollider>(ActiveCheck.ActiveOnly), List_ActiveOnly_BoxCollider);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_A2_Collider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Collider>(ActiveCheck.ActiveOnly), List_ActiveOnly_BoxCollider);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_B1_MeshRenderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<MeshRenderer>(ActiveCheck.ActiveOnly), List_ActiveOnly_MeshRenderer);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_B2_Renderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Renderer>(ActiveCheck.ActiveOnly), List_ActiveOnly_MeshRenderer);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_C_Component()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Component>(ActiveCheck.ActiveOnly), List_ActiveOnly_Component);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_D_Behaviour()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Behaviour>(ActiveCheck.ActiveOnly), List_ActiveOnly_Behaviour);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_ActiveOnly_E_CanvasGroup()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<CanvasGroup>(ActiveCheck.ActiveOnly), List_ActiveOnly_CanvasGroup);
		}

		// ------------------------------------------------------

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_A1_BoxCollider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<BoxCollider>(ActiveCheck.IncludingInactive), List_ActiveOnly_BoxCollider.Combine(List_InactiveOnly_BoxCollider));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_A2_Collider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Collider>(ActiveCheck.IncludingInactive), List_ActiveOnly_BoxCollider.Combine(List_InactiveOnly_BoxCollider));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_B1_MeshRenderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<MeshRenderer>(ActiveCheck.IncludingInactive), List_ActiveOnly_MeshRenderer.Combine(List_InactiveOnly_MeshRenderer));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_B2_Renderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Renderer>(ActiveCheck.IncludingInactive), List_ActiveOnly_MeshRenderer.Combine(List_InactiveOnly_MeshRenderer));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_C_Component()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Component>(ActiveCheck.IncludingInactive), List_ActiveOnly_Component.Combine(List_InactiveOnly_Component));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_D_Behaviour()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Behaviour>(ActiveCheck.IncludingInactive), List_ActiveOnly_Behaviour.Combine(List_InactiveOnly_Behaviour));
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_IncludingInactive_E_CanvasGroup()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<CanvasGroup>(ActiveCheck.IncludingInactive), List_ActiveOnly_CanvasGroup.Combine(List_InactiveOnly_CanvasGroup));
		}

		// ------------------------------------------------------

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_A1_BoxCollider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<BoxCollider>(ActiveCheck.InactiveOnly), List_InactiveOnly_BoxCollider);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_A2_Collider()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Collider>(ActiveCheck.InactiveOnly), List_InactiveOnly_BoxCollider);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_B1_MeshRenderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<MeshRenderer>(ActiveCheck.InactiveOnly), List_InactiveOnly_MeshRenderer);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_B2_Renderer()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Renderer>(ActiveCheck.InactiveOnly), List_InactiveOnly_MeshRenderer);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_C_Component()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Component>(ActiveCheck.InactiveOnly), List_InactiveOnly_Component);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_D_Behaviour()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<Behaviour>(ActiveCheck.InactiveOnly), List_InactiveOnly_Behaviour);
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator FindObjectsOfType_InactiveOnly_E_CanvasGroup()
		{
			yield return TestFindObjectsOfType(() => Scene.FindObjectsOfType<CanvasGroup>(ActiveCheck.InactiveOnly), List_InactiveOnly_CanvasGroup);
		}

		private IEnumerator TestFindObjectsOfType<T>(Func<List<T>> searchMethod, string[] expectedComponentPaths) where T : Component
		{
			var foundComponents = searchMethod();

			// Get full names of found components. Also get MarkedTestBehaviour data
			// as an extra to see if we got "Enabled One" or "Disabled One".
			var resultingComponentPaths = foundComponents
				.Where(item => item.gameObject.name != "Code-based tests runner")
				.Select(item =>
				{
					var name = item.FullName();
					if (item is MarkedTestBehaviour marked)
					{
						name += "|" + marked.Mark;
					}
					return name;
				}).ToList();

			// Sort
			resultingComponentPaths.Sort();
			Array.Sort(expectedComponentPaths);

			if (!resultingComponentPaths.SequenceEqual(expectedComponentPaths))
			{
				Log.Error("Something went wrong and here are the details. See below for the error.");
				using (Log.IndentedScope)
				{
					LogExpectedPaths(expectedComponentPaths);
					LogResult(foundComponents);
					expectedComponentPaths.LogList("Expected:");
					resultingComponentPaths.LogList("Found:");
				}
				Assert.Fail("Found GameObject paths does not match the expected paths. See logs for details.");
			}

			yield break;
		}

		#endregion

		#region IsComponentEnabled

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_A1_BoxCollider()
		{
			yield return TestIsComponentEnabled<BoxCollider>();
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_A2_Collider()
		{
			yield return TestIsComponentEnabled<Collider>();
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_B1_MeshRenderer()
		{
			yield return TestIsComponentEnabled<MeshRenderer>();
		}
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_B2_MeshRenderer()
		{
			yield return TestIsComponentEnabled<Renderer>();
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator IsComponentEnabled_MarkedTestBehaviour()
		{
			var components = Scene.FindObjectsOfType<MarkedTestBehaviour>(ActiveCheck.IncludingInactive);
			Assert.AreEqual(2, components.Count);
			Assert.True(components.First(component => component.Mark == "Enabled One").IsComponentEnabled());
			Assert.False(components.First(component => component.Mark == "Disabled One").IsComponentEnabled());

			yield break;
		}

		private IEnumerator TestIsComponentEnabled<T>() where T : Component
		{
			var components = Scene.FindObjectsOfType<T>(ActiveCheck.IncludingInactive);

			Assert.True(components.FindSingleComponentByGameObjectPath("Cube").IsComponentEnabled());
			Assert.True(components.FindSingleComponentByGameObjectPath("Cube (GO Disabled)").IsComponentEnabled());
			Assert.False(components.FindSingleComponentByGameObjectPath("Cube (Comp Disabled)").IsComponentEnabled());
			Assert.True(components.FindSingleComponentByGameObjectPath("Container 1/ChildCube").IsComponentEnabled());
			Assert.True(components.FindSingleComponentByGameObjectPath("Container 1 (GO Disabled)/ChildCube").IsComponentEnabled());
			Assert.False(components.FindSingleComponentByGameObjectPath("Container 3/ChildCube (Comp Disabled)").IsComponentEnabled());
			Assert.False(components.FindSingleComponentByGameObjectPath("Container 3 (GO Disabled)/ChildCube (Comp Disabled)").IsComponentEnabled());

			yield break;
		}

		#endregion

		#region Tools

		private void LogResult<T>(ICollection<T> objects) where T : Component
		{
			Log.Info($"Listing '{objects.Count}' objects of type '{typeof(T).Name}':");
			using (Log.IndentedScope)
			{
				foreach (var obj in objects)
				{
					Log.InfoWithContext(obj.gameObject, obj.FullName());
				}
			}
		}

		private void LogExpectedPaths(ICollection<string> paths)
		{
			Log.Info($"Listing '{paths.Count}' expected paths':");
			using (Log.IndentedScope)
			{
				foreach (var path in paths)
				{
					Log.Info(path);
				}
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Test_GameObjectTools));

		#endregion
	}

}
