using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.StaticObjects;
using System.Reflection;
using UnityEngine;
using KerbalKonstructs.LaunchSites;

namespace KerbalKonstructs
{
	public class CustomSpaceCenter //: MonoBehaviour
	{
		public string SpaceCenterName;

		private SpaceCenter spaceCenter;
		private StaticObject staticObject;
		private GameObject gameObject;

/*
		void Start()
		{
			staticObject = KerbalKonstructs.instance.getStaticDB().getStaticFromGameObject(gameObject);
			if (staticObject != null)
			{
				spaceCenter = gameObject.AddComponent<SpaceCenter>();
				spaceCenter.cb = (CelestialBody) staticObject.getSetting("CelestialBody");
				FieldInfo lat = spaceCenter.GetType().GetField("\u0002", BindingFlags.NonPublic | BindingFlags.Instance);
				lat.SetValue(spaceCenter, spaceCenter.cb.GetLatitude(gameObject.transform.position));
				FieldInfo lon = spaceCenter.GetType().GetField("\u0003", BindingFlags.NonPublic | BindingFlags.Instance);
				lon.SetValue(spaceCenter, spaceCenter.cb.GetLongitude(gameObject.transform.position));
				FieldInfo srfVector = spaceCenter.GetType().GetField("\u0004", BindingFlags.NonPublic | BindingFlags.Instance);
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
		*/
		
		public static void CreateFromLaunchsite(string name, GameObject go)
		{
			StaticObject staticObject = KerbalKonstructs.instance.getStaticDB().getStaticFromGameObject(go);
			if (staticObject != null)
			{
				/*
				SpaceCenter spaceCenter = go.AddComponent<SpaceCenter>();
				spaceCenter.cb = (CelestialBody)staticObject.getSetting("CelestialBody");
				spaceCenter.name = name;

				FieldInfo lat = spaceCenter.GetType().GetField("\u0002", BindingFlags.NonPublic | BindingFlags.Instance);
				lat.SetValue(spaceCenter, spaceCenter.cb.GetLatitude(go.transform.position));
				FieldInfo lon = spaceCenter.GetType().GetField("\u0003", BindingFlags.NonPublic | BindingFlags.Instance);
				lon.SetValue(spaceCenter, spaceCenter.cb.GetLongitude(go.transform.position));
				FieldInfo srfVector = spaceCenter.GetType().GetField("\u0004", BindingFlags.NonPublic | BindingFlags.Instance);
				srfVector.SetValue(spaceCenter, spaceCenter.cb.GetRelSurfaceNVector(spaceCenter.Latitude, spaceCenter.Longitude));
				*/
				var csc = new CustomSpaceCenter();
				//csc.spaceCenter = spaceCenter;
				csc.SpaceCenterName = name;
				csc.staticObject = staticObject;
				csc.gameObject = go;

				Debug.Log("Added Space Center " + name);
				SpaceCenterManager.addSpaceCenter(csc);
			}
		}

		public SpaceCenter getSpaceCenter()
		{
			if (spaceCenter == null)
			{
				spaceCenter = gameObject.AddComponent<SpaceCenter>();
				spaceCenter.cb = (CelestialBody)staticObject.getSetting("CelestialBody");
				spaceCenter.name = SpaceCenterName;
				

				FieldInfo lat = spaceCenter.GetType().GetField("\u0002", BindingFlags.NonPublic | BindingFlags.Instance);
				lat.SetValue(spaceCenter, spaceCenter.cb.GetLatitude(gameObject.transform.position));
				FieldInfo lon = spaceCenter.GetType().GetField("\u0003", BindingFlags.NonPublic | BindingFlags.Instance);
				lon.SetValue(spaceCenter, spaceCenter.cb.GetLongitude(gameObject.transform.position));
				FieldInfo srfVector = spaceCenter.GetType().GetField("\u0004", BindingFlags.NonPublic | BindingFlags.Instance);
				srfVector.SetValue(spaceCenter, spaceCenter.cb.GetRelSurfaceNVector(spaceCenter.Latitude, spaceCenter.Longitude));
			}
			return spaceCenter;
		}

		public StaticObject getStaticObject()
		{
			return staticObject;
		}
	}
}
