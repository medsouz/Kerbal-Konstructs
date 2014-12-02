using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.API;
using KerbalKonstructs.LaunchSites;

// R and T Log
// ASH 14112014

namespace KerbalKonstructs.UI
{
	public class LaunchSiteSelectorGUI
	{
		public Texture tIconClosed = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteclosed", false);
		public Texture tIconOpen = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);

		LaunchSite selectedSite;
		private SiteType editorType = SiteType.Any;

		private Boolean isOpen = false;
		private float iFundsOpen = 0;

		// ASH 28102014 - Needs to be bigger for filter
		Rect windowRect = new Rect(((Screen.width - Camera.main.rect.x) / 2) + Camera.main.rect.x - 125, (Screen.height / 2 - 250), 700, 580);

		public Boolean isCareerGame()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				// disableCareerStrategyLayer is configurable in KerbalKonstructs.cfg
				if (!KerbalKonstructs.instance.disableCareerStrategyLayer)
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		public void drawSelector()
		{
			//Camera.main is null when first loading a scene
			//if (Camera.main != null)
			//{
			windowRect = GUI.Window(0xB00B1E3, windowRect, drawSelectorWindow, "Launch Site Selector");
			//}

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
			// ASH 07112014 Disabling of restricted categories added.
			GUILayout.BeginArea(new Rect(10, 25, 370, 550));
			GUILayout.BeginHorizontal();
			if (editorType == SiteType.SPH)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("RocketPads", GUILayout.Width(80)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "RocketPad");
			}
			GUI.enabled = true;
			GUILayout.Space(2);
			if (editorType == SiteType.VAB)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Runways", GUILayout.Width(73)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "Runway");
			}
			GUI.enabled = true;
			GUILayout.Space(2);
			if (editorType == SiteType.VAB)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Helipads", GUILayout.Width(73)))
			{
				sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "Helipad");
			}
			GUI.enabled = true;
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
			if (sites == null) sites = (editorType == SiteType.Any) ? LaunchSiteManager.getLaunchSites() : LaunchSiteManager.getLaunchSites(editorType, true, "ALL");

			foreach (LaunchSite site in sites)
				{
					if (isCareerGame())
						GUILayout.BeginHorizontal();
					// Light icons in the launchsite list only shown in career so only need horizontal for two elements for that mode
					
					GUI.enabled = !(selectedSite == site);
					if (GUILayout.Button(site.name, GUILayout.Height(30)))
					{
						selectedSite = site;
						
						// ASH Career Mode Unlocking
						// In career the launchsite is not set by the launchsite list but rather in the launchsite description
						// panel on the right
						if (!isCareerGame())
							LaunchSiteManager.setLaunchSite(site);
					}
					GUI.enabled = true;
					if (isCareerGame())
					{
						// if site is closed show red light
						// if site is open show green light

						// If a site has an open cost of 0 it's always open
						if (site.openclosestate == "Open" || site.opencost == 0)
						{
							site.openclosestate = "Open";
							GUILayout.Label(tIconOpen, GUILayout.Height(30), GUILayout.Width(30));
						}
						else
						{
							GUILayout.Label(tIconClosed, GUILayout.Height(30), GUILayout.Width(30));
						}
						// Light icons in the launchsite list only shown in career
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.EndScrollView();
			GUILayout.EndArea();

			GUI.enabled = true;

			if (selectedSite != null)
			{
				drawRightSelectorWindow();
			}
			else
			{
				if (LaunchSiteManager.getLaunchSites().Count > 0)
				{
					selectedSite = LaunchSiteManager.getLaunchSites(editorType)[0];
					// ASH Career Mode Unlocking
					// In career the launchsite is not set by the launchsite list but rather in the launchsite description
					// panel on the right
					// if (!isCareerGame())
					LaunchSiteManager.setLaunchSite(selectedSite);

					// ASH 05112014 Fixes the selector centering issue on the right panel... probably
					drawRightSelectorWindow();
				}
				else
				{
					Debug.Log("KK: ERROR Launch Selector cannot find KSC Runway or Launch Pad! PANIC! Runaway! Hide!");
				}
			}
		}

		// ASH 05112014 Having the right panel always drawn might fix the selector centering issue on the right panel
		private void drawRightSelectorWindow()
		{
			GUILayout.BeginArea(new Rect(385, 25, 310, 550));
				GUILayout.Label(selectedSite.logo, GUILayout.Height(280));
				GUILayout.Label(selectedSite.name + " By " + selectedSite.author);
				descriptionScrollPosition = GUILayout.BeginScrollView(descriptionScrollPosition);
				GUILayout.Label(selectedSite.description);
				GUILayout.EndScrollView();

				float iFundsClose = 0;
				// Career mode - get cost to open and value of opening from launchsite (defined in the cfg)
				iFundsOpen = selectedSite.opencost;
				iFundsClose = selectedSite.closevalue;

				bool isAlwaysOpen = false;
				bool cannotBeClosed = false;

				// Career mode
				// If a launchsite is 0 to open it is always open
				if (iFundsOpen == 0)
					isAlwaysOpen = true;

				// If it is 0 to close you cannot close it
				if (iFundsClose == 0)
					cannotBeClosed = true;
				
				if (isCareerGame())
				{	
					// Determine if a site is open or closed
					// If persistence says the site is open then isOpen = true;
					// If persistence file says nothing or site is closed then isOpen = false;
					// STUB IN KerbalKonstructs OnSiteSelectorOn()
					// Easier to just load the openclosestate of all launchsites on to the from so its ready when we get here

					isOpen = (selectedSite.openclosestate == "Open");

					GUI.enabled = !isOpen;
					
					if (!isAlwaysOpen)
					{
						if (GUILayout.Button("Open Site for " + iFundsOpen + " Funds"))
						{
							// TODO What if there isn't enough funds?

							double currentfunds = Funding.Instance.Funds;

							if (iFundsOpen > currentfunds)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this site!", 10, 0);
							}
							else
							{
								// Open the site - save to instance
								selectedSite.openclosestate = "Open";

								// Charge some funds
								Funding.Instance.AddFunds(-iFundsOpen, TransactionReasons.Cheating);

								// Save new state to persistence
								PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
							}
						}
					}
					GUI.enabled = true;
					
					GUI.enabled = isOpen;
					if (!cannotBeClosed)
					{
						if (GUILayout.Button("Close Site for " + iFundsClose + " Funds"))
						{
							// Close the site - save to instance
							// Pay back some funds
							Funding.Instance.AddFunds(iFundsClose, TransactionReasons.Cheating);
							selectedSite.openclosestate = "Closed";

							// Save new state to persistence
							PersistenceFile<LaunchSite>.SaveList(sites, "LAUNCHSITES", "KK");
						}
					}
					GUI.enabled = true;

					// If the site is open and it isn't the selected launchsite, enable the set as launchsite button
					// in the right pane
					GUI.enabled = (isOpen || isAlwaysOpen) && !(selectedSite.name == EditorLogic.fetch.launchSiteName);					
					if (GUILayout.Button("Set as Launchsite"))
					{
						LaunchSiteManager.setLaunchSite(selectedSite);
					}
					GUI.enabled = true;
				}
			GUILayout.EndArea();
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
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
				// ASH Career Mode Unlocking
				// In career the launchsite is not set by the launchsite list but rather in the launchsite description
				// panel on the right
				// if (!isCareerGame())
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