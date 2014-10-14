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

		Rect windowRect = new Rect(300, 50, 700, 580);

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
        
        // Changed scope so we can change it by filter ASH 14102014
        public List<LaunchSite> sites;
		
        public void drawSelectorWindow(int id)
		{
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

                if (sites == null) sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "ALL");

				foreach (LaunchSite site in sites)
				{
					GUI.enabled = !(selectedSite == site);
					if (GUILayout.Button(site.name, GUILayout.Height(30)))
					{
						selectedSite = site;
                        // Only do this if site is OPEN - if OPEN then
						LaunchSiteManager.setLaunchSite(site);
                        // else
                        // print up a message that this site is closed
                        // and auto-deselect
                        // HOW??
                        //Like this?
                        //selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
                        // No probably not cos filters
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
                    GUILayout.Label(selectedSite.name + " By " + selectedSite.author, GUILayout.ExpandWidth(true));
					descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition);
						GUILayout.Label(selectedSite.description);
					GUILayout.EndScrollView();
                    GUILayout.Label("Length: " + selectedSite.launchLength + " m | Width: " + selectedSite.launchWidth + " m"); 
                    GUILayout.Label("Recommended Mass: " + selectedSite.maxMass + " t");
                    GUILayout.Label("Recovery Rating: A-F | Launch Cost: X%");
                    // Button label should be determined by whether location is open or close
                    if (GUILayout.Button("OPEN/CLOSE for X Funds", GUILayout.ExpandWidth(true)))
                    {
                        // Complex shit. Needs to keep track of whether a location
                        // is open or closed per save
                        // Launch selector should not allow launching from a location that is not open
                        // CANNOT disable closed locations cos we need the second window populated
                        // unless maybe have a second column of buttons for just pulling up info - ?
                        // No will not work selected launch site is indicated by being disabled... hmmm
                        // Funds should be charged for opening a location and gained for closing a location
                    }
				GUILayout.EndArea();
			}
			else
			{
                if (LaunchSiteManager.getLaunchSites().Count > 0)
                {
                    selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
                    LaunchSiteManager.setLaunchSite(selectedSite);
                }
                else
                {
                        // TODO Need to handle list filter not having any entries and thus falling back to KSC runway
                        // or launchpad
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

        public void Close()
        {
            sites = null;
        }
	}
}
