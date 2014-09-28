using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.StaticObjects;
using System;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs
{
	public class CustomSpaceCenter : MonoBehaviour
	{
		public string SpaceCenterName;

		private SpaceCenter spaceCenter;
		private StaticObject staticObject;

		void Start()
		{
			staticObject = KerbalKonstructs.instance.getStaticDB().getStaticFromGameObject(gameObject);
			if (staticObject != null)
			{
				spaceCenter = gameObject.AddComponent<SpaceCenter>();
				spaceCenter.cb = staticObject.parentBody;
				FieldInfo lat = spaceCenter.GetType().GetField("\u0002", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				lat.SetValue(spaceCenter, spaceCenter.cb.GetLatitude(gameObject.transform.position));
				FieldInfo lon = spaceCenter.GetType().GetField("\u0003", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				lon.SetValue(spaceCenter, spaceCenter.cb.GetLongitude(gameObject.transform.position));
				FieldInfo srfVector = spaceCenter.GetType().GetField("\u0004", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				srfVector.SetValue(spaceCenter, spaceCenter.cb.GetRelSurfaceNVector(spaceCenter.Latitude, spaceCenter.Longitude));
				if (SpaceCenterName == null)
				{
					SpaceCenterName = "Unknown";
				}
				SpaceCenterManager.addSpaceCenter(this);
				Debug.Log("Added Space Center " + SpaceCenterName);
			}
			else
			{
				Debug.LogError("No StaticObject exists in CustomSpaceCenterObject. This should never happen!");
			}
			
		}

		public SpaceCenter getSpaceCenter()
		{
			return spaceCenter;
		}

		public StaticObject getStaticObject()
		{
			return staticObject;
		}
	}
}
