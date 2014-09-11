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
			launchSites.Add(new LaunchSite("KSC Runway", "Squad", SiteType.SPH, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCRunway", false), "The KSC runway is a concrete runway measuring about 2.5km long and 70m wide, on a magnetic heading of 90/270. It is not uncommon to see burning chunks of metal sliding across the surface."));
			launchSites.Add(new LaunchSite("KSC Launchpad", "Squad", SiteType.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/KSCLaunchpad", false), "The KSC launchpad is a platform used to fire screaming Kerbals into the kosmos. There was a tower here at one point but for some reason nobody seems to know where it went..."));
		}

		//This is pretty much ripped from KerbTown, sorry
		public static void createLaunchSite(StaticObject obj)
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
							newSite.launchPadName = obj.siteName + "/" + obj.siteTransform;
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
							Texture logo = defaultLaunchSiteLogo;
							if(obj.siteLogo != "")
								logo = GameDatabase.Instance.GetTexture(obj.siteLogo, false);
							launchSites.Add(new LaunchSite(obj.siteName, obj.author, obj.siteType, logo, obj.siteDescription));
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

		public static List<LaunchSite> getLaunchSites()
		{
			return launchSites;
		}

		public static List<LaunchSite> getLaunchSites(SiteType type, Boolean allowAny = true)
		{
			List<LaunchSite> sites = new List<LaunchSite>();
			foreach (LaunchSite site in launchSites)
			{
				if(site.type.Equals(type) || (site.type.Equals(SiteType.Any) && allowAny))
				{
					sites.Add(site);
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
