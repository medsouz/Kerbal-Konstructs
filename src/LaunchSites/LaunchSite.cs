using System;
using UnityEngine;

namespace KerbalKonstructs.LaunchSites
{
	public class LaunchSite
	{
		public string name;
		public string author;
		public SiteType type;
		public Texture logo;
		public Texture icon;
		public string description;

		// ASH 28102014 - Added category
		public string category;

		public LaunchSite(string sName, string sAuthor, SiteType sType, Texture sLogo, Texture sIcon, string sDescription, string sDevice = "Other")
		{
			name = sName;
			author = sAuthor;
			type = sType;
			logo = sLogo;
			icon = sIcon;
			description = sDescription;
			// ASH 28102014 - Added category
			category = sDevice;
		}
	}

	public enum SiteType
	{
		VAB,
		SPH,
		Any
	}
}
