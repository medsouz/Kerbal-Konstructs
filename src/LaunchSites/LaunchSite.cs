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
		public string description;

		public LaunchSite(string sName, string sAuthor, SiteType sType, Texture sLogo, string sDescription)
		{
			name = sName;
			author = sAuthor;
			type = sType;
			logo = sLogo;
			description = sDescription;
		}
	}

	public enum SiteType
	{
		VAB,
		SPH,
		Any
	}
}
