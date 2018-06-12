using UnityEngine;
using Random = UnityEngine.Random;

namespace ExtenityExamples.UnityEditorToolbox.GraphPlotting
{

	public class Example_AnyComponentGraphPlotter : MonoBehaviour
	{
		public float SomeRandomNumber;
		private float SomeHiddenRandomNumber;

		private Example_Subclass HiddenSubobject = new Example_Subclass();

		public class Example_Subclass
		{
			public float SomeRandomNumberInSubclass;
		}

		private void FixedUpdate()
		{
			SomeRandomNumber = Random.value * 0.5f;
			SomeHiddenRandomNumber = Random.value * 0.5f + 0.5f;
			HiddenSubobject.SomeRandomNumberInSubclass = Random.value;
		}
	}

}
