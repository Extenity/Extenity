using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class ReferenceFilter : EditorWindow
{
	[MenuItem("Assets/What objects use this?", false, 20)]
	private static void OnSearchForReferences()
	{
		string final = "";
		var matches = new List<UnityEngine.Object>();

		int iid = Selection.activeInstanceID;
		if (AssetDatabase.IsMainAsset(iid))
		{
			// only main assets have unique paths
			string path = AssetDatabase.GetAssetPath(iid);
			// strip down the name
			final = System.IO.Path.GetFileNameWithoutExtension(path);
		}
		else
		{
			Debug.Log("Error Asset not found");
			return;
		}

		//loop through everything
		var allObjects = Resources.FindObjectsOfTypeAll(typeof(Object));
		for (int iObject = 0; iObject < allObjects.Length; iObject++)
		{
			Object go = allObjects[iObject];

			// All objects
			Object[] dependencies = EditorUtility.CollectDependencies(new[] { go });
			for (int i = 0; i < dependencies.Length; i++)
			{
				Object o = dependencies[i];
				if (o == null)
					continue;
				if (o.name == null)
					throw new ArgumentNullException("o.name");
				if (string.Compare(o.name.ToString(), final) == 0)
					matches.Add(go); // add it to our list to highlight
			}
		}
		Selection.objects = matches.ToArray();
		matches.Clear(); // clear the list
	}
}
