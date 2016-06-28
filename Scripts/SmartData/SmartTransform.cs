using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdvancedInspector;

namespace SmartData
{

	[Serializable]
	[AdvancedInspector(false, false), Expandable(true)]
	public class SmartTransform : SmartVar<Transform>
	{
		#region Initialization

		public SmartTransform() : base() { }
		public SmartTransform(bool checkForChangesAutomatically) : base(checkForChangesAutomatically) { }

		#endregion

		[Inspect, Group("Events")]
		public SmartVarEvent OnLocationChanged = new SmartVarEvent();
		[Inspect, Group("Events")]
		public SmartVarEvent OnLocalLocationChanged = new SmartVarEvent();
		[Inspect, Group("Events")]
		public SmartVarEvent OnPositionChanged = new SmartVarEvent();
		[Inspect, Group("Events")]
		public SmartVarEvent OnRotationChanged = new SmartVarEvent();
		[Inspect, Group("Events")]
		public SmartVarEvent OnLocalPositionChanged = new SmartVarEvent();
		[Inspect, Group("Events")]
		public SmartVarEvent OnLocalRotationChanged = new SmartVarEvent();
		[Inspect, Group("Events")]
		public SmartVarEvent OnLocalScaleChanged = new SmartVarEvent();


		public override void CheckForChanges()
		{
			var locationChanged = false;
			var localLocationChanged = false;

			if (oldPosition != position)
			{
				var cachedValue = position;
				OnPositionChanged.Invoke(Reference, Tag);
				oldPosition = cachedValue;
				locationChanged = true;
			}
			if (oldRotation != rotation)
			{
				var cachedValue = rotation;
				OnRotationChanged.Invoke(Reference, Tag);
				oldRotation = cachedValue;
				locationChanged = true;
			}
			if (oldLocalPosition != localPosition)
			{
				var cachedValue = localPosition;
				OnLocalPositionChanged.Invoke(Reference, Tag);
				oldLocalPosition = cachedValue;
				localLocationChanged = true;
			}
			if (oldLocalRotation != localRotation)
			{
				var cachedValue = localRotation;
				OnLocalRotationChanged.Invoke(Reference, Tag);
				oldLocalRotation = cachedValue;
				localLocationChanged = true;
			}
			if (oldLocalScale != localScale)
			{
				var cachedValue = localScale;
				OnLocalScaleChanged.Invoke(Reference, Tag);
				oldLocalScale = cachedValue;
			}

			if (locationChanged)
			{
				OnLocationChanged.Invoke(Reference, Tag);
			}
			if (localLocationChanged)
			{
				OnLocalLocationChanged.Invoke(Reference, Tag);
			}
		}

		[Inspect, ReadOnly, Group("Details", priority: 5)]
		public Vector3 oldPosition { get; private set; }
		[Inspect, ReadOnly, Group("Details")]
		public Quaternion oldRotation { get; private set; }
		[Inspect, ReadOnly, Group("Details")]
		public Vector3 oldLocalPosition { get; private set; }
		[Inspect, ReadOnly, Group("Details")]
		public Quaternion oldLocalRotation { get; private set; }
		[Inspect, ReadOnly, Group("Details")]
		public Vector3 oldLocalScale { get; private set; }

		[Inspect, ReadOnly, Group("Details")]
		public Vector3 positionDelta { get { return position - oldPosition; } }
		[Inspect, ReadOnly, Group("Details")]
		public Vector3 localPositionDelta { get { return localPosition - oldLocalPosition; } }

		[Inspect, Group("Details")]
		public Vector3 position
		{
			get { return Reference != null ? Reference.position : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.position = value; }
		}

		[Inspect, Group("Details")]
		public Quaternion rotation
		{
			get { return Reference != null ? Reference.rotation : new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.rotation = value; }
		}

		[Inspect, Group("Details")]
		public Vector3 eulerAngles
		{
			get { return Reference != null ? Reference.eulerAngles : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.eulerAngles = value; }
		}

		[Inspect, Group("Details")]
		public Vector3 localPosition
		{
			get { return Reference != null ? Reference.localPosition : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.localPosition = value; }
		}

		[Inspect, Group("Details")]
		public Quaternion localRotation
		{
			get { return Reference != null ? Reference.localRotation : new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.localRotation = value; }
		}

		[Inspect, Group("Details")]
		public Vector3 localEulerAngles
		{
			get { return Reference != null ? Reference.localEulerAngles : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.localEulerAngles = value; }
		}

		[Inspect, Group("Details")]
		public Vector3 localScale
		{
			get { return Reference != null ? Reference.localScale : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.localScale = value; }
		}

		[Inspect, ReadOnly, Group("Details")]
		public Vector3 lossyScale
		{
			get { return Reference != null ? Reference.lossyScale : new Vector3(float.NaN, float.NaN, float.NaN); }
		}

		[Inspect, ReadOnly, Group("Details")]
		public Vector3 forward
		{
			get { return Reference != null ? Reference.forward : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.forward = value; }
		}

		[Inspect, ReadOnly, Group("Details")]
		public Vector3 right
		{
			get { return Reference != null ? Reference.right : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.right = value; }
		}

		[Inspect, ReadOnly, Group("Details")]
		public Vector3 up
		{
			get { return Reference != null ? Reference.up : new Vector3(float.NaN, float.NaN, float.NaN); }
			set { if (Reference != null) Reference.up = value; }
		}
	}

}
