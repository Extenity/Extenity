using Extenity.UnityEditorToolbox.GraphPlotting;
using UnityEngine;

namespace ExtenityExamples.UnityEditorToolbox.GraphPlotting
{

	public class Example_GraphPlotterFromCode : MonoBehaviour
	{
		private Monitor Monitor;
		private Channel Channel1;
		private Channel Channel2;

		public float SomeRandomNumber;

		private void Start()
		{
			Monitor = new Monitor("Two Sine Waves");
			Channel1 = new Channel(Monitor, "Wavesome", Color.green);
			Channel2 = new Channel(Monitor, "Bob Wave", Color.blue);
		}

		private void OnDestroy()
		{
			Channel1.Close();
			Channel2.Close();
			Monitor.Close();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Monitor.Add(new TagEntry(Time.time, "Space"));
			}
		}

		private void FixedUpdate()
		{
			var time = Time.time;
			Channel1.Sample(Mathf.Sin(3f * time));
			Channel2.Sample(Mathf.Sin(4.5f * time));

			SomeRandomNumber = Random.value;
		}
	}

}
