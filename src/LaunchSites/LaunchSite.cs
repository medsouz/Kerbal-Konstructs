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

		// ASH 28102014 - Added category
		public string category;
		// ASH Added career strategy
		public float opencost;
		public float closevalue;

		//public Vector3 radialposition;

		[PersistentField]
		public string openclosestate;

		public GameObject GameObject;

		public LaunchSite(string sName, string sAuthor, SiteType sType, Texture sLogo, Texture sIcon, string sDescription, string sDevice, float fOpenCost, float fCloseValue, string sOpenCloseState, /*Vector3 vRadial = default(Vector3)*/GameObject gameObject)
		{
			name = sName;
			author = sAuthor;
			type = sType;
			logo = sLogo;
			icon = sIcon;
			description = sDescription;
			// ASH 28102014 - Added category
			category = sDevice;
			// ASH Added career strategy
			opencost = fOpenCost;
			closevalue = fCloseValue;
			openclosestate = sOpenCloseState;
			//radialposition = vRadial;
			GameObject = gameObject;
		}
	}

	public enum SiteType
	{
		VAB,
		SPH,
		Any
	}
}
