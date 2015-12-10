using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProfilerSampleNumberChanger : MonoBehaviour
{
	public int MaxNumberOfSamplesPerFrame = 8000000;

	protected void Awake()
	{
		Profiler.maxNumberOfSamplesPerFrame = MaxNumberOfSamplesPerFrame;
	}
}
