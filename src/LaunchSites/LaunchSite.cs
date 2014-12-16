using System;
using UnityEngine;
using KerbalKonstructs.API;

namespace KerbalKonstructs.LaunchSites
{
	public class LaunchSite
	{
		[PersistentKey]
		public string name;

		public string author;
		public SiteType type;
		public Texture logo;
		public Texture icon;
		public string description;

		public string category;
		public float opencost;
		public float closevalue;

		[PersistentField]
		public string openclosestate;

		public GameObject GameObject;
		public PSystemSetup.SpaceCenterFacility facility;

		public LaunchSite(string sName, string sAuthor, SiteType sType, Texture sLogo, Texture sIcon, string sDescription, string sDevice, float fOpenCost, float fCloseValue, string sOpenCloseState, GameObject gameObject, PSystemSetup.SpaceCenterFacility newFacility)
		{
			name = sName;
			author = sAuthor;
			type = sType;
			logo = sLogo;
			icon = sIcon;
			description = sDescription;
			category = sDevice;
			opencost = fOpenCost;
			closevalue = fCloseValue;
			openclosestate = sOpenCloseState;
			GameObject = gameObject;
			facility = newFacility;
		}
	}

	public enum SiteType
	{
		VAB,
		SPH,
		Any
	}
}
