using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class DummyGraphic : Graphic
	{
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();
		}
	}

}
