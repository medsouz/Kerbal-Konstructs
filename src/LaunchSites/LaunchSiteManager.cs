using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// R and T LOG
// 14112014 ASH

namespace KerbalKonstructs.LaunchSites
{
	public class LaunchSiteManager
	{
		private static List<LaunchSite> launchSites = new List<LaunchSite>();
		public static Texture defaultLaunchSiteLogo = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/DefaultSiteLogo", false);

		static LaunchSiteManager()
		{
			// TODO Expose KSC runway and launchpad descriptions to KerbalKonstructs.cfg so players can change them
			// ASH 28102014 - Added category
			// ASH 12112014 - Added career open close costs and state
			launchSites.Add(new LaunchSite("Runway", "Squad", SiteType.SPH, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCRunway", false), null, "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface.", "Runway", 0, 0, "Open"));
			launchSites.Add(new LaunchSite("LaunchPad", "Squad", SiteType.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCLaunchpad", false), null, "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went...", "RocketPad", 0, 0, "Open"));
		}

		// KerbTown legacy code - TODO Review and update?
		public static void createLaunchSite(StaticObject obj)
		{
			if (obj.settings.ContainsKey("LaunchSiteName") && obj.gameObject.transform.Find((string) obj.getSetting("LaunchPadTransform")) != null)
			{
				Debug.Log("KK: Creating launch site " + obj.getSetting("LaunchSiteName"));
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
							
							// TODO This is still hard-code and needs to use the API properly
							// ASH 12112014 - Added career open close costs
							launchSites.Add(new LaunchSite((string)obj.getSetting("LaunchSiteName"), (obj.settings.ContainsKey("LaunchSiteAuthor")) ? (string)obj.getSetting("LaunchSiteAuthor") : (string)obj.model.getSetting("author"), (SiteType)obj.getSetting("LaunchSiteType"), logo, icon, (string)obj.getSetting("LaunchSiteDescription"), (string)obj.getSetting("Category"), (float)obj.getSetting("OpenCost"), (float)obj.getSetting("CloseValue")));
							Debug.Log("KK: Created launch site \"" + newFacility.name + "\" with transform " + obj.getSetting("LaunchSiteName") + "/" + obj.getSetting("LaunchPadTransform"));
						}
						else
						{
							Debug.Log("KK: Launch site " + obj.getSetting("LaunchSiteName") + " already exists.");
						}
					}
				}

				MethodInfo updateSitesMI = PSystemSetup.Instance.GetType().GetMethod("SetupFacilities", BindingFlags.NonPublic | BindingFlags.Instance);
				if (updateSitesMI == null)
					Debug.Log("KK: Failed to find SetupFacilities().");
				else
					updateSitesMI.Invoke(PSystemSetup.Instance, null);
			}
			else
			{
				Debug.Log("KK: Launch pad transform \"" + obj.getSetting("LaunchPadTransform") + "\" missing for " + obj.getSetting("LaunchSiteName"));
			}
		}

		// ASH 28102014 Added handling for Category filter
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
			
			// ASH 28102014 Return sites, not launchSites
			// return launchSites;
		}

		// ASH 28102014 Added handling for Category filter
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

		public static string getSiteOpenCloseState(string sSiteName)
		{
			string sOpenCloseState = "Closed";
			List<LaunchSite> sites = new List<LaunchSite>();
			foreach (LaunchSite site in launchSites)
			{
				if (site.name == sSiteName)
				{
					sOpenCloseState = site.openclosestate;
				}
			}
			return sOpenCloseState;
		}

		public static void setLaunchSite(LaunchSite site)
		{
			EditorLogic.fetch.launchSiteName = site.name;
		}

		public static List<LaunchSite> AllLaunchSites { get { return launchSites; } }
	}
}
