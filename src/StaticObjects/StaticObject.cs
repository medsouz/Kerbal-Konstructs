using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticObject
	{
		public GameObject gameObject;
		public PQSCity pqsCity;
		public StaticModel model;

		/*public CelestialBody parentBody;
		public string groupName;

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
		public LaunchSites.SiteType siteType;*/

		public Dictionary<string, object> settings = new Dictionary<string, object>();

		public Boolean editing;

		public void update()
		{
			if (pqsCity != null)
			{
				pqsCity.repositionRadial = (Vector3) settings["RadialPosition"];
				pqsCity.repositionRadiusOffset = (float) settings["RadiusOffset"];
				pqsCity.reorientInitialUp = (Vector3) settings["Orientation"];
				pqsCity.reorientFinalAngle = (float) settings["RotationAngle"];
				pqsCity.Orientate();
			}
		}

		public object getSetting(string setting)
		{
			if (settings.ContainsKey(setting))
				return settings[setting];
			Debug.Log("Setting " + setting + " not found in instance of model " + model.config);
			object defaultValue = KKAPI.getInstanceSettings()[setting].getDefaultValue();
			settings.Add(setting, defaultValue);
			return defaultValue;
		}

		public void setSetting(string setting, object value)
		{
			if (settings.ContainsKey(setting))
			{
				settings[setting] = value;
			}
			else
			{
				settings.Add(setting, value);
			}
		}
	}
}
