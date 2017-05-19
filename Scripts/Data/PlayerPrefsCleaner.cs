using UnityEngine;

namespace Extenity.DataToolbox
{

	public class PlayerPrefsCleaner : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F12) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				PlayerPrefs.DeleteAll();
			}
		}
	}

}
