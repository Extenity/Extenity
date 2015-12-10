using UnityEngine;
using UnityEditor;
public class AutoSnap : EditorWindow
{
	private Vector3 prevPosition;
	private bool doSnap = true;
	private float snapValue = 1;

	[MenuItem("Edit/Auto Snap %_l")]
	static void Init()
	{
		var window = (AutoSnap)EditorWindow.GetWindow(typeof(AutoSnap));
		window.maxSize = new Vector2(200, 100);
	}

	public void OnGUI()
	{
		doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
		snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
	}

	public void Update()
	{
		if (doSnap
		&& !EditorApplication.isPlaying
		&& Selection.transforms.Length > 0
		&& Selection.transforms[0].position != prevPosition)
		{
			Snap();
			prevPosition = Selection.transforms[0].position;
		}
	}

	private void Snap()
	{
		var transforms = Selection.transforms;
		for (int i = 0; i < transforms.Length; i++)
		{
			var transform = transforms[i];
			var t = transform.transform.position;
			t.x = Round(t.x);
			t.y = Round(t.y);
			t.z = Round(t.z);
			transform.transform.position = t;
		}
	}

	private float Round(float input)
	{
		return snapValue * Mathf.Round((input / snapValue));
	}
}
