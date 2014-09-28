using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.SpaceCenters
{
	public class SpaceCenterManager
	{
		private static List<CustomSpaceCenter> spaceCenters = new List<CustomSpaceCenter>();
		private static SpaceCenter KSC;

		public static void setKSC()
		{
			KSC = SpaceCenter.Instance;
		}

		public static void addSpaceCenter(CustomSpaceCenter csc)
		{
			spaceCenters.Add(csc);
		}

		public static SpaceCenter getClosestSpaceCenter(Vector3 position)
		{
			SpaceCenter closest = KSC;
			float smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
			foreach (CustomSpaceCenter csc in spaceCenters)
			{
				Debug.Log(csc.SpaceCenterName);
				float dist = Vector3.Distance(position, csc.getStaticObject().gameObject.transform.position);
				if (dist < smallestDist)
				{
					closest = csc.getSpaceCenter();
					smallestDist = dist;
				}
			}
			return closest;
		}
	}
}
