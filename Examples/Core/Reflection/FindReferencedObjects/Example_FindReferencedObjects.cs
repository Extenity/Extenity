using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ExtenityExamples.Reflection
{

	public class Example_FindReferencedObjects : MonoBehaviour
	{
		[Serializable]
		public class CustomEvent : UnityEvent { }

		public Camera CameraLink;
		public Material MaterialLink;
		public Mesh MeshLink;
		public GameObject OtherGameObject;
		public GameObject[] OtherGameObjectArray;
		public List<GameObject> OtherGameObjectList;
		public UnityEvent UnityEventField;
		public CustomEvent CustomEventField;
	}

}
