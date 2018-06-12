using Extenity.UnityEditorToolbox.GraphPlotting;
using UnityEngine;

namespace ExtenityExamples.UnityEditorToolbox.GraphPlotting
{

	public class Example_GraphPlotterFromCode : MonoBehaviour
	{
		private Graph Graph;
		private Channel Channel1;
		private Channel Channel2;

		private void Start()
		{
			Graph = new Graph("Two Sine Waves", gameObject);
			Channel1 = new Channel(Graph, "Wavesome", Color.green);
			Channel2 = new Channel(Graph, "Bob Wave", Color.blue);
		}

		private void OnDestroy()
		{
			Channel.SafeClose(ref Channel1);
			Channel.SafeClose(ref Channel2);
			Graph.Close();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Graph.Add(new TagEntry(Time.time, "Space"));
			}
		}

		private void FixedUpdate()
		{
			var time = Time.time;
			var frame = Time.frameCount;
			Channel1.Sample(Mathf.Sin(3f * time), time, frame);
			Channel2.Sample(Mathf.Sin(4.5f * time), time, frame);
		}
	}

}
