using System;
using Extenity.MathToolbox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExtenityExamples.UnityEditorToolbox.GraphPlotting
{

	public class Example_AnyComponentGraphPlotter : MonoBehaviour
	{
#pragma warning disable 414

		public float SomeRandomNumber;
		private float SomeHiddenRandomNumber;
		[NonSerialized]
		public Quaternion SomeRotation;

		private Example_Subclass HiddenSubObject = new Example_Subclass();

#pragma warning restore 414

		public class Example_Subclass
		{
			public float SomeRandomNumberInSubclass;
		}

		private void FixedUpdate()
		{
			SomeRandomNumber = Random.value.Remap(0f, 1f, 0.00f, 0.25f);
			SomeHiddenRandomNumber = Random.value.Remap(0f, 1f, 0.25f, 0.50f);
			HiddenSubObject.SomeRandomNumberInSubclass = Random.value.Remap(0f, 1f, 0.50f, 0.75f);
			SomeRotation = Quaternion.Euler(Mathf.Sin(Time.time).Remap(-1, 1f, 0.75f, 1.00f), 0f, 0f);
		}
	}

}
