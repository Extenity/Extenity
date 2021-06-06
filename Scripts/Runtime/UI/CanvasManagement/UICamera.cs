using Extenity.DesignPatternsToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class UICamera : SingletonUnity<UICamera>
	{
		public Camera Camera;

		protected void Awake()
		{
			InitializeSingleton();
		}
	}

}
