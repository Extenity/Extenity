using UnityEngine;
using System.Collections;

public class WindowTitleChanger : MonoBehaviour
{
	public string Title;

	private void Awake()
	{
		ChangeTitle();
	}

	public void ChangeTitle()
	{
		if (!string.IsNullOrEmpty(Title))
		{
			OperatingSystemTools.ChangeWindowTitle(Title);
		}
	}
}
