using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.API;
using KerbalKonstructs.LaunchSites;

namespace KerbalKonstructs.SpaceCenters
{
	public class SpaceCenterManager
	{
		public static List<CustomSpaceCenter> spaceCenters = new List<CustomSpaceCenter>();
		public static SpaceCenter KSC;

		public static void setKSC()
		{
			KSC = SpaceCenter.Instance;
		}

		public static void addSpaceCenter(CustomSpaceCenter csc)
		{
			spaceCenters.Add(csc);
		}

		public static SpaceCenter getClosestSpaceCenter(Vector3 position)
		{
			//SpaceCenter closest = KSC;
			CustomSpaceCenter closest = null;
			float smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
			Debug.Log("Distance to KSC: " + smallestDist);

			// ASH Career mode strategy
			// Only open sites can do recoveries
			bool isCareer = false;
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				if (!KerbalKonstructs.instance.disableCareerStrategyLayer)
				{
					isCareer = true;
					PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				}
			}

			string sOpenCloseState = "Closed";

			foreach (CustomSpaceCenter csc in spaceCenters)
			{
				if (isCareer)
				{
					// ASH Get openclosestate of launchsite with same name as space centre
					sOpenCloseState = LaunchSiteManager.getSiteOpenCloseState(csc.SpaceCenterName);
				}

				float dist = Vector3.Distance(position, csc.getStaticObject().gameObject.transform.position);
				Debug.Log(csc.SpaceCenterName + " " + dist + " " + sOpenCloseState);
				//if (csc.getSpaceCenter() != null)
				{
					if (dist < smallestDist)
					{
						if (isCareer && sOpenCloseState == "Closed")
						{ }
						else
						{
							closest = csc;//.getSpaceCenter();
							smallestDist = dist;
							Debug.Log("closest updated to " + closest + " distance " + smallestDist);
						}
					}
				}
			}
			SpaceCenter sc;
			if (closest == null) 
				sc = KSC;
			else
			{
				Debug.Log("closest=" + closest);
				sc = closest.getSpaceCenter() ?? KSC;
			}
			Debug.Log("smallestDist=" + smallestDist);
			Debug.Log("returning closest space centre: " + sc);
			return sc;
		}
	}
}