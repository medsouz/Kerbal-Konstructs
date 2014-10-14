using System;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticObject
	{
		public GameObject gameObject;
		public PQSCity pqsCity;

		public CelestialBody parentBody;
		public string groupName;
		public StaticModel model;

		public Vector3 position;
		public float altitude;
		public float visibleRange;
		public Vector3 orientation;
		public float rotation;
		public string siteName;
		public string siteTransform;
		public string siteDescription;
		public string siteLogo;
		public string siteIcon;
		public string siteAuthor;
        public string launchLength;
        public string launchWidth;
        public string maxMass;
        public string launchDevice;

		public LaunchSites.SiteType siteType;

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
