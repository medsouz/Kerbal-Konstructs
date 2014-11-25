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
			CustomSpaceCenter closest = null;

			float smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
			Debug.Log("KK: Distance to KSC is " + smallestDist);

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

				if (dist < smallestDist)
				{
					if (isCareer && sOpenCloseState == "Closed")
					{ }
					else
					{
						closest = csc;
						smallestDist = dist;
						Debug.Log("KK: closest updated to " + closest.SpaceCenterName + ", distance " + smallestDist);
					}
				}
			}

			SpaceCenter sc;

			if (closest == null) 
				sc = KSC;
			else
			{
				Debug.Log("KK: closest is " + closest.SpaceCenterName);
				sc = closest.getSpaceCenter() ?? KSC;
			}

			Debug.Log("KK: smallestDist is " + smallestDist);
			Debug.Log("KK: returning closest space centre: " + sc.name);

			return sc;
		}
	}
}