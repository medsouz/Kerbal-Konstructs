using KerbalKonstructs.LaunchSites;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class MapIconManager
	{
		public Texture VABIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/VABMapIcon", false);
		public Texture SPHIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SPHMapIcon", false);
		public Texture ANYIcon = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/ANYMapIcon", false);
		private Boolean displayingTooltip = false;

		public void drawIcons()
		{
			displayingTooltip = false;
			MapObject target = PlanetariumCamera.fetch.target;
			if (target.type == MapObject.MapObjectType.CELESTIALBODY)
			{
				List<LaunchSite> sites = LaunchSiteManager.getLaunchSites();
				foreach (LaunchSite site in sites)
				{
					PSystemSetup.LaunchSite ls = PSystemSetup.Instance.GetLaunchSite(site.name);
					if (ls != null)
					{
						if (ls.launchPadPQS == target.celestialBody.pqsController)
						{
							if (!isOccluded(ls.launchPadTransform.position, target.celestialBody))
							{
								Vector3 pos = MapView.MapCamera.camera.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(ls.launchPadTransform.position));
								Rect screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);
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
