using KerbalKonstructs.LaunchSites;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class MapIconManager
	{
		public Texture VABIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/VABMapIcon", false);
		public Texture SPHIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SPHMapIcon", false);
		public Texture ANYIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/ANYMapIcon", false);
		private Boolean displayingTooltip = false;
		Rect mapManagerRect = new Rect(200, 150, 210, 225);

		public void drawManager()
		{
			mapManagerRect = GUI.Window(0xB00B2E3, mapManagerRect, drawMapManagerWindow, "Base Boss");
		}

		bool showOpen = true;
		bool showClosed = true;
		bool showRocketPads = true;
		bool showHelipads = true;
		bool showRunways = true;
		bool showOther = true;
		bool loadedPersistence = false;

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

		void drawMapManagerWindow(int windowID)
		{
			GUILayout.BeginArea(new Rect(5, 25, 190, 200));
				if (!loadedPersistence && isCareerGame())
				{
					PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
					loadedPersistence = true;
				}
				GUI.enabled = (isCareerGame());
				if (!isCareerGame())
				{
					showOpen = GUILayout.Toggle(true, "Show open bases");
					showClosed = GUILayout.Toggle(true, "Show closed bases");
				}
				else
				{
					showOpen = GUILayout.Toggle(showOpen, "Show open bases");
					showClosed = GUILayout.Toggle(showClosed, "Show closed bases");
				}
				GUI.enabled = true;
				GUILayout.Space(5);
				showRocketPads = GUILayout.Toggle(showRocketPads, "Show rocketpads");
				showHelipads = GUILayout.Toggle(showHelipads, "Show helipads");
				showRunways = GUILayout.Toggle(showRunways, "Show runways");
				showOther = GUILayout.Toggle(showOther, "Show other launchsites");
			GUILayout.EndArea();
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void drawIcons()
		{
			displayingTooltip = false;
			MapObject target = PlanetariumCamera.fetch.target;
			if (target.type == MapObject.MapObjectType.CELESTIALBODY)
			{
				List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
				foreach (LaunchSite site in sites)
				{
					PSystemSetup.SpaceCenterFacility facility = PSystemSetup.Instance.GetSpaceCenterFacility(site.name);
					if (facility != null)
					{
						PSystemSetup.SpaceCenterFacility.SpawnPoint sp = facility.GetSpawnPoint(site.name);
						if (sp != null)
						{
							if (facility.facilityPQS == target.celestialBody.pqsController)
							{
								if (!isOccluded(sp.spawnPointTransform.position, target.celestialBody))
								{
									Vector3 pos = MapView.MapCamera.camera.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(sp.spawnPointTransform.position));
									Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

									bool display = false;
									string openclosed = site.openclosestate;
									string category = site.category;

									if (showHelipads && category == "Helipad")
										display = true;
									if (showOther && category == "Other")
										display = true;
									if (showRocketPads && category == "RocketPad")
										display = true;
									if (showRunways && category == "Runway")
										display = true;

									if (display && isCareerGame())
									{
										if (!showOpen && openclosed == "Open")
											display = false;
										if (!showClosed && openclosed == "Closed")
											display = false;
									}

									if (display)
									{
										if (site.icon != null)
										{
											Graphics.DrawTexture(screenRect, site.icon);
										}
										else
										{
											switch (site.type)
											{
												case SiteType.VAB:
													Graphics.DrawTexture(screenRect, VABIcon);
													break;
												case SiteType.SPH:
													Graphics.DrawTexture(screenRect, SPHIcon);
													break;
												default:
													Graphics.DrawTexture(screenRect, ANYIcon);
													break;
											}
										}
										if (screenRect.Contains(Event.current.mousePosition) && !displayingTooltip)
										{
											//Only display one tooltip at a time
											displayingTooltip = true;
											GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 200, 20), site.name);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		//"Borrowed" from FinePrint
		//https://github.com/Arsonide/FinePrint/blob/master/Source/WaypointManager.cs#L53
		private bool isOccluded(Vector3d loc, CelestialBody body)
		{
			Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

			if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
				return false;

			return true;
		}
	}
}
