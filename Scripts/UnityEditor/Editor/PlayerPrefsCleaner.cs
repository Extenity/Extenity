using UnityEngine;
using System.Collections;

public class PlayerPrefsCleaner : MonoBehaviour
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F12) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			PlayerPrefs.DeleteAll();
		}
	}
}
