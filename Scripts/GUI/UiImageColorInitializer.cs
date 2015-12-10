using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiImageColorInitializer : MonoBehaviour
{
	public enum InitializationPlace
	{
		Awake,
		Start,
	}

	public Color InitialColor;
	public InitializationPlace Method;

	private void Awake()
	{
		if (Method == InitializationPlace.Awake)
		{
			SetValue();
		}
	}

	private void Start()
	{
		if (Method == InitializationPlace.Start)
		{
			SetValue();
		}
	}

	public void SetValue()
	{
		transform.GetComponentEnsured<Image>().color = InitialColor;
	}
}
