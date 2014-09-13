using KerbalKonstructs.LaunchSites;
using System;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class LaunchSiteSelectorGUI
	{
		LaunchSite selectedSite;
		private SiteType editorType = SiteType.Any;

		public void drawSelector()
		{
			if(Camera.main != null)//Camera.main is null when first loading a scene
				GUI.Window(0xB00B1E6, new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 600, 500), drawSelectorWindow, "Launch Site Selector");
		}

		public Vector2 sitesScrollPosition;
		public Vector2 descriptionScrollPosition;

		public void drawSelectorWindow(int id)
		{
			GUILayout.BeginArea(new Rect(10, 25, 270, 465));
				sitesScrollPosition = GUILayout.BeginScrollView(sitesScrollPosition);
				foreach (LaunchSite site in LaunchSiteManager.getLaunchSites(editorType))
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
				GUILayout.BeginArea(new Rect(290, 25, 300, 465));
					GUILayout.Label(selectedSite.logo, GUILayout.Height(280));
					GUILayout.Label(selectedSite.name);
					GUILayout.Label("By "+selectedSite.author);
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
			editorType = type;
			if (selectedSite != null)
			{
				if (selectedSite.type != editorType && selectedSite.type != SiteType.Any)
				{
					selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
				}
				LaunchSiteManager.setLaunchSite(selectedSite);
			}
		}
	}
}
