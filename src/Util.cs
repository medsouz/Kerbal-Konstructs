using System;
using UnityEngine;

namespace KerbalKonstructs
{
	class Util
	{
		public static CelestialBody getCelestialBody(String name)
		{
			CelestialBody[] bodies = GameObject.FindObjectsOfType(typeof(CelestialBody)) as CelestialBody[];
			foreach (CelestialBody body in bodies)
			{
				if (body.bodyName == name)
					return body;
			}
			Debug.Log("Couldn't find body \"" + name + "\"");
			return null;
		}

        // ASH Going to need this for persistence
        public static String GetRootPath()
        {
            String path = KSPUtil.ApplicationRootPath;
            path = path.Replace("\\", "/");
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            return path;
        }
	}
}
