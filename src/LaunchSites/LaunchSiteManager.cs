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
			// ASH 28102014 - Added category
			launchSites.Add(new LaunchSite("Runway", "Squad", SiteType.SPH, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCRunway", false), null, "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.", "Runway"));
			launchSites.Add(new LaunchSite("LaunchPad", "Squad", SiteType.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCLaunchpad", false), null, "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...", "RocketPad"));
		}

		//This is pretty much ripped from KerbTown, sorry
		public static void createLaunchSite(StaticObject obj)
		{
			if (obj.settings.ContainsKey("LaunchSiteName") && obj.gameObject.transform.Find((string) obj.getSetting("LaunchPadTransform")) != null)
			{
				Debug.Log("Creating launch site " + obj.getSetting("LaunchSiteName"));
				obj.gameObject.transform.name = (string) obj.getSetting("LaunchSiteName");
				obj.gameObject.name = (string) obj.getSetting("LaunchSiteName");

				foreach (FieldInfo fi in PSystemSetup.Instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (fi.FieldType.Name == "SpaceCenterFacility[]")
					{
						PSystemSetup.SpaceCenterFacility[] facilities = (PSystemSetup.SpaceCenterFacility[])fi.GetValue(PSystemSetup.Instance);
						if (PSystemSetup.Instance.GetSpaceCenterFacility((string) obj.getSetting("LaunchSiteName")) == null)
						{
							PSystemSetup.SpaceCenterFacility newFacility = new PSystemSetup.SpaceCenterFacility();
							newFacility.name = (string) obj.getSetting("LaunchSiteName");
							newFacility.facilityName = obj.gameObject.name;
							newFacility.facilityPQS = ((CelestialBody) obj.getSetting("CelestialBody")).pqsController;
							newFacility.facilityTransformName = obj.gameObject.name;
							newFacility.pqsName = ((CelestialBody) obj.getSetting("CelestialBody")).pqsController.name;
							PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint = new PSystemSetup.SpaceCenterFacility.SpawnPoint();
							spawnPoint.name = (string) obj.getSetting("LaunchSiteName");
							spawnPoint.spawnTransformURL = (string) obj.getSetting("LaunchPadTransform");
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
							if(obj.settings.ContainsKey("LaunchSiteLogo"))
								logo = GameDatabase.Instance.GetTexture(obj.model.path + "/" + obj.getSetting("LaunchSiteLogo"), false);
							if(obj.settings.ContainsKey("LaunchSiteIcon"))
								icon = GameDatabase.Instance.GetTexture(obj.model.path + "/" + obj.getSetting("LaunchSiteIcon"), false);
							
							// ASH 28102014 TODO This is still hard-code and needs to use the API properly.
							launchSites.Add(new LaunchSite((string)obj.getSetting("LaunchSiteName"), (obj.settings.ContainsKey("LaunchSiteAuthor")) ? (string)obj.getSetting("LaunchSiteAuthor") : (string)obj.model.getSetting("author"), (SiteType)obj.getSetting("LaunchSiteType"), logo, icon, (string)obj.getSetting("LaunchSiteDescription"), (string)obj.getSetting("Category")));
							Debug.Log("Created launch site \"" + newFacility.name + "\" with transform " + obj.getSetting("LaunchSiteName") + "/" + obj.getSetting("LaunchPadTransform"));
						}
						else
						{
							Debug.Log("Launch site " + obj.getSetting("LaunchSiteName") + " already exists");
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
				Debug.Log("Launch pad transform \"" + obj.getSetting("LaunchPadTransform") + "\" missing for " + obj.getSetting("LaunchSiteName"));
			}
		}

		// ASH 28102014 Added handling for new Category filter
		public static List<LaunchSite> getLaunchSites(String usedFilter = "ALL")
		{
			List<LaunchSite> sites = new List<LaunchSite>();
			foreach (LaunchSite site in launchSites)
			{
				if (usedFilter.Equals("ALL"))
				{
					sites.Add(site);
				}
				else
				{
					if (site.category.Equals(usedFilter))
					{
						sites.Add(site);
					}
				}
			}
			return sites;
			// ASH 28102014
			//return launchSites;
		}

		// ASH 28102014 Added handling for new Category filter
		public static List<LaunchSite> getLaunchSites(SiteType type, Boolean allowAny = true, String appliedFilter = "ALL")
		{
			List<LaunchSite> sites = new List<LaunchSite>();
			foreach (LaunchSite site in launchSites)
			{
				if (site.type.Equals(type) || (site.type.Equals(SiteType.Any) && allowAny))
				{
					if (appliedFilter.Equals("ALL"))
					{
						sites.Add(site);
					}
					else
					{
						if (site.category.Equals(appliedFilter))
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
