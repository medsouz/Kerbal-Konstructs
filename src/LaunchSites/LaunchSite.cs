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

        // ASH 15102014 Added new parameters. Damn KerbTown had sucky architecture. I love legacy code.
        // OO means I should not have to change multiple classes just to add some parameters. Gah.
        public string launchLength;
        public string launchWidth;
        public string maxMass;
        public string launchDevice;

		public LaunchSite(string sName, string sAuthor, SiteType sType, Texture sLogo, Texture sIcon, string sDescription, string sLength, string sWidth, string sMass, string sDevice)
		{
			name = sName;
			author = sAuthor;
			type = sType;
			logo = sLogo;
			icon = sIcon;
			description = sDescription;

            // ASH 15102014 Added new parameters. Again. Sigh.
            launchLength = sLength;
            launchWidth = sWidth;
            maxMass = sMass;
            launchDevice = sDevice;
		}
	}

	public enum SiteType
	{
		VAB,
		SPH,
		Any
	}
}
