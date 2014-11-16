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
		private static List<CustomSpaceCenter> spaceCenters = new List<CustomSpaceCenter>();
		private static SpaceCenter KSC;

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
			SpaceCenter closest = KSC;
			float smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);

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

				Debug.Log(csc.SpaceCenterName);
				float dist = Vector3.Distance(position, csc.getStaticObject().gameObject.transform.position);
				if (dist < smallestDist)
				{
					if (isCareer && sOpenCloseState == "Closed")
					{ }
					else
					{
						closest = csc.getSpaceCenter();
						smallestDist = dist;
					}
				}
			}
			return closest;
		}
	}
}
