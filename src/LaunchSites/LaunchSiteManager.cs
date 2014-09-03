using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.LaunchSites
{
	public class LaunchSiteManager
	{
		//This is pretty much ripped from KerbTown, sorry
		static public void createLaunchSite(StaticObject obj)
		{
			if (obj.siteTransform != "")
			{
				Debug.Log("Creating launch site " + obj.siteName);
				obj.gameObject.transform.name = obj.siteName;
				obj.gameObject.name = obj.siteName;

				foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (fi.FieldType.Name == "LaunchSite[]")
					{
						PSystemSetup.LaunchSite[] sites = (PSystemSetup.LaunchSite[])fi.GetValue(PSystemSetup.Instance);
						if (PSystemSetup.Instance.GetLaunchSite(obj.siteName) == null)
						{
							PSystemSetup.LaunchSite newSite = new PSystemSetup.LaunchSite();
							newSite.launchPadName = obj.siteName;//obj.siteName + "/" + obj.siteTransform;
							newSite.name = obj.siteName;
							newSite.pqsName = obj.parentBody.bodyName;

							PSystemSetup.LaunchSite[] newSites = new PSystemSetup.LaunchSite[sites.Length + 1];
							for (int i = 0; i < sites.Length; ++i)
							{
								newSites[i] = sites[i];
							}
							newSites[newSites.Length - 1] = newSite;
							fi.SetValue(PSystemSetup.Instance, newSites);
							sites = newSites;
							Debug.Log("Created launch site \"" + newSite.name + "\" with transform " + newSite.launchPadName);
						}
						else
						{
							Debug.Log("Launch site " + obj.siteName + " already exists");
						}
					}
				}

				MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupLaunchSites", BindingFlags.NonPublic | BindingFlags.Instance);
				if (updateSitesMI == null)
					Debug.Log("Fail to find SetupLaunchSites().");
				else
					updateSitesMI.Invoke(PSystemSetup.Instance, null);
			}
			else
			{
				Debug.Log("Launch pad transform \"" + obj.siteTransform + "\" missing for " + obj.siteName);
			}
		}

		
	}
}
