using System;
using UnityEngine;

namespace KerbalKonstructs
{
	public class StaticObject
	{
		public GameObject gameObject;
		public PQSCity pqsCity;

		public CelestialBody parentBody;
		public string groupName;

		public Vector3 position;
		public float altitude;
		public float visibleRange;
		public Vector3 orientation;
		public float rotation;

		public Boolean editing;

		public void update()
		{
			if (pqsCity != null)
			{
				pqsCity.repositionRadial = position;
				pqsCity.repositionRadiusOffset = altitude;
				pqsCity.reorientInitialUp = orientation;
				pqsCity.reorientFinalAngle = rotation;
				pqsCity.Orientate();
			}
		}
	}
}
