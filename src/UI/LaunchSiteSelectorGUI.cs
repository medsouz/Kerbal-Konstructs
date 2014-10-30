using KerbalKonstructs.LaunchSites;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class LaunchSiteSelectorGUI
	{
		LaunchSite selectedSite;
		private SiteType editorType = SiteType.Any;

		// ASH 28102014 - Needs to be bigger for filter
		Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 700, 580);

		public void drawSelector()
		{
			if(Camera.main != null)//Camera.main is null when first loading a scene
				GUI.Window(0xB00B1E6, windowRect, drawSelectorWindow, "Launch Site Selector");

			if (windowRect.Contains(Event.current.mousePosition))
			{
				InputLockManager.SetControlLock(ControlTypes.EDITOR_LOCK, "KKEditorLock");
			}
			else
			{
				InputLockManager.RemoveControlLock("KKEditorLock");
			}
		}

		public Vector2 sitesScrollPosition;
		public Vector2 descriptionScrollPosition;

		// ASH 28102014 Changed scope so we can change it by Category filter
		public List<LaunchSite> sites;

		public void drawSelectorWindow(int id)
		{
			// ASH 28102014 Category filter handling added.
			GUILayout.BeginArea(new Rect(10, 25, 370, 550));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("RocketPads", GUILayout.Width(80)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "RocketPad");
			}
			GUILayout.Space(2);
			if (GUILayout.Button("Runways", GUILayout.Width(73)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "Runway");
			}
			GUILayout.Space(2);
			if (GUILayout.Button("Helipads", GUILayout.Width(73)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "Helipad");
			}
			GUILayout.Space(2);
			if (GUILayout.Button("Other", GUILayout.Width(65)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "Other");
			}
			GUILayout.Space(2);
			if (GUILayout.Button("ALL", GUILayout.Width(45)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "ALL");
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			sitesScrollPosition = GUILayout.BeginScrollView(sitesScrollPosition);
			// ASH 28102014 Category filter handling added
			//List<LaunchSite> sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType);

			if (sites == null) sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "ALL");

			foreach (LaunchSite site in sites)
				{
					GUI.enabled = !(selectedSite == site);
					if (GUILayout.Button(site.name, GUILayout.Height(30)))
					{
						selectedSite = site;
						LaunchSiteManager.setLaunchSite(site);
					}
				}
				GUILayout.EndScrollView();
			GUILayout.EndArea();

			//Fixes when the last item is selected and it leaves the GUI disabled
			GUI.enabled = true;

			if (selectedSite != null)
			{
				GUILayout.BeginArea(new Rect(385, 25, 310, 550));
				GUILayout.Label(selectedSite.logo, GUILayout.Height(280));
				GUILayout.Label(selectedSite.name + " By " + selectedSite.author);
				descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition);
				GUILayout.Label(selectedSite.description);
				GUILayout.EndScrollView();
				GUILayout.EndArea();
			}
			else
			{
				if (LaunchSiteManager.getLaunchSites().Count > 0)
				{
					selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
					LaunchSiteManager.setLaunchSite(selectedSite);
				}
			}
		}

		public void setEditorType(SiteType type)
		{
			editorType = (KerbalKonstructs.instance.launchFromAnySite) ? SiteType.Any : type;
			if (selectedSite != null)
			{
				if (selectedSite.type != editorType && selectedSite.type != SiteType.Any)
				{
					selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
				}
				LaunchSiteManager.setLaunchSite(selectedSite);
			}
		}
		
		// ASH and Ravencrow 28102014
		// Need to handle if Launch Selector is still open when switching from VAB to from SPH
		// otherwise abuse possible!
		public void Close()
		{
			sites = null;
		}
	}
}
