using UnityEngine;
using Extenity.GameObjectToolbox;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class UIImageColorInitializer : MonoBehaviour
	{
		//public enum InitializationPlace
		//{
		//	Awake,
		//	Start,
		//}

		public Color InitialColor;
		//public InitializationPlace Method;

		//private void Awake()
		//{
		//	if (Method == InitializationPlace.Awake)
		//		SetValue();
		//}

		private void Start()
		{
			//if (Method == InitializationPlace.Start)
			SetValue();
		}

		public void SetValue()
		{
			transform.GetSingleComponentEnsured<Image>().color = InitialColor;
		}
	}

}
