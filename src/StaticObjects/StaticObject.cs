﻿using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticObject
	{
		[PersistentKey]
		public Vector3 RadialPosition;

		[PersistentField]
		public CelestialBody CelestialBody;
		[PersistentField]
		public float StaffCurrent;

		public GameObject gameObject;
		public PQSCity pqsCity;
		public StaticModel model;

		[KSPField]
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
			Debug.Log("KK: Setting " + setting + " not found in instance of model " + model.config);
			object defaultValue = KKAPI.getInstanceSettings()[setting].getDefaultValue();

			if (defaultValue != null)
			{
				settings.Add(setting, defaultValue);
				return defaultValue;
			}
			else
			{
				Debug.Log("KK: Setting " + setting + " not found in instance API. BUG BUG BUG.");
				return null;
			}
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
