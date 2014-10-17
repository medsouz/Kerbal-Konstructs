using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.LaunchSites
{
	public class LaunchSiteManager
	{
		private static List<LaunchSite> launchSites = new List<LaunchSite>();
		public static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/DefaultSiteLogo", false);

		static LaunchSiteManager()
		{
			//Accepting contributions to change my horrible descriptions

            // ASH 15102014
            // Added new parameters. Also do not change the name again. Apparently they are unique IDs. Squad, you suck sometimes.
			launchSites.Add(new LaunchSite("Runway", "Squad", SiteType.SPH, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCRunway", false), null, "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.", "X", "X", "NA", "Runway"));
            launchSites.Add(new LaunchSite("LaunchPad", "Squad", SiteType.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCLaunchpad", false), null, "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...", "X", "X", "NA", "RocketPad"));
		}

		//This is pretty much ripped from KerbTown, sorry
		public static void createLaunchSite(StaticObject obj)
		{
			if (obj.siteTransform != "" && obj.gameObject.transform.Find(obj.siteTransform) != null)
			{
				Debug.Log("Creating launch site " + obj.siteName);
				obj.gameObject.transform.name = obj.siteName;
				obj.gameObject.name = obj.siteName;

				foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (fi.FieldType.Name == "SpaceCenterFacility[]")
					{
						PSystemSetup.SpaceCenterFacility[] facilities = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
						if (PSystemSetup.Instance.GetSpaceCenterFacility(obj.siteName) == null)
						{
							PSystemSetup.SpaceCenterFacility newFacility = new PSystemSetup.SpaceCenterFacility();
							newFacility.name = obj.siteName;
							newFacility.facilityName = obj.gameObject.name;
							newFacility.facilityPQS = obj.parentBody.pqsController;
							newFacility.facilityTransformName = obj.gameObject.name;
							newFacility.pqsName = obj.parentBody.pqsController.name;
							PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
							spawnPoint.name = obj.siteName;
							spawnPoint.spawnTransformURL = obj.siteTransform;
							newFacility.spawnPoints = new PSystemSetup.SpaceCenterFacility.SpawnPoint[1];
							newFacility.spawnPoints[0] = spawnPoint;
							PSystemSetup.SpaceCenterFacility[] newFacilities = new PSystemSetup.SpaceCenterFacility[facilities.Length + 1];
							for (int i = 0; i < facilities.Length; ++i)
							{
								newFacilities[i] = facilities[i];
							}
							newFacilities[newFacilities.Length - 1] = newFacility;
							fi.SetValue(PSystemSetup.Instance, newFacilities);
							facilities = newFacilities;
							Texture logo = defaultLaunchSiteLogo;
							Texture icon = null;
							if(obj.siteLogo != "")
								logo = GameDatabase.Instance.GetTexture(obj.siteLogo, false);
							if (obj.siteIcon != "")
								icon = GameDatabase.Instance.GetTexture(obj.siteIcon, false);

                            // ASH 15102014 Added new parameters
							launchSites.Add(new LaunchSite(obj.siteName, (obj.siteAuthor != "") ? obj.siteAuthor : obj.model.author, obj.siteType, logo, icon, obj.siteDescription, obj.launchLength, obj.launchWidth, obj.maxMass, obj.launchDevice));
							Debug.Log("Created launch site \"" + newFacility.name + "\" with transform " + obj.siteName + "/" + obj.siteTransform);
						}
						else
						{
							Debug.Log("Launch site " + obj.siteName + " already exists");
						}
					}
				}

				MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
				if (updateSitesMI == null)
					Debug.Log("Fail to find SetupFacilities().");
				else
					updateSitesMI.Invoke(PSystemSetup.Instance, null);
			}
			else
			{
				Debug.Log("Launch pad transform \"" + obj.siteTransform + "\" missing for " + obj.siteName);
			}
		}

        public static List<LaunchSite> getLaunchSites(String usedFilter = "ALL")
		{
            // ASH 15102014 Added handling for new LaunchDevice filter
            List<LaunchSite> sites = new List<LaunchSite>();
            foreach (LaunchSite site in launchSites)
            {
                if (usedFilter.Equals("ALL"))
                {
                    sites.Add(site);
                }
                else
                {
                    if (site.launchDevice.Equals(usedFilter))
                    {
                        sites.Add(site);
                    }
                }
            }
            return sites;
			//return launchSites;
		}

		public static List<LaunchSite> getLaunchSites(SiteType type, Boolean allowAny = true, String appliedFilter = "ALL")
		{
            // ASH 15102014 Added handling for new LaunchDevice filter
			List<LaunchSite> sites = new List<LaunchSite>();
			foreach (LaunchSite site in launchSites)
			{
				if(site.type.Equals(type) || (site.type.Equals(SiteType.Any) && allowAny))
				{
                    if (appliedFilter.Equals("ALL"))
                    {
                        sites.Add(site);
                    }
                    else
                    {
                        if (site.launchDevice.Equals(appliedFilter))
                        {
                            sites.Add(site);
                        }
                    }
				}
			}
			return sites;
		}

		public static void setLaunchSite(LaunchSite site)
		{
			EditorLogic.fetch.launchSiteName = site.name;
		}
	}
}
