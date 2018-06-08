using UnityEngine;
using MonitorComponents;

namespace ExtenityExamples.UnityEditorToolbox.GraphPlotting
{

	public class Example_GraphPlotterFromCode : MonoBehaviour
	{
		private Monitor Monitor;
		private MonitorInput MonitorInput1;
		private MonitorInput MonitorInput2;

		private void Awake()
		{
			Monitor = new Monitor("Two Sine Waves");
			MonitorInput1 = new MonitorInput(Monitor, "Wavesome", Color.green);
			MonitorInput2 = new MonitorInput(Monitor, "Bob Wave", Color.blue);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Monitor.Add(new MonitorEvent()
				{
					text = "Space",
					time = Time.time
				});
			}
		}

		private void FixedUpdate()
		{
			var time = Time.time;
			MonitorInput1.Sample(Mathf.Sin(Mathf.PI * time));
			MonitorInput2.Sample(Mathf.Sin(Mathf.PI * 1.5f * time));
		}
	}

}
