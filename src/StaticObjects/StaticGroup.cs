using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	class StaticGroup
	{
		private String groupName;
		private String bodyName;

		private List<StaticObject> childObjects = new List<StaticObject>();
		private Vector3 centerPoint = Vector3.zero;
		private float visiblityRange = 0;
		public Boolean alwaysActive = false;
		public Boolean active = false;

		public StaticGroup(String name, String body)
		{
			groupName = name;
			bodyName = body;
		}

		public void addStatic(StaticObject obj)
		{
			childObjects.Add(obj);
			updateCacheSettings();
		}

		public void updateCacheSettings()
		{
			float highestVisibility = 0;
			float furthestDist = 0;
			Vector3 center = Vector3.zero;
			foreach (StaticObject obj in childObjects)
			{
				if ((float) obj.getSetting("VisibilityRange") > highestVisibility)
					highestVisibility = (float) obj.getSetting("VisibilityRange");

				center += obj.gameObject.transform.position;
			}
			center /= childObjects.Count;

			foreach (StaticObject obj in childObjects)
			{
				float dist = Vector3.Distance(center, obj.gameObject.transform.position);
				if (dist > furthestDist)
					furthestDist = dist;
			}

			visiblityRange = highestVisibility + furthestDist;
			centerPoint = center;
		}

		public void cacheAll()
		{
			foreach (StaticObject obj in childObjects)
			{
				obj.gameObject.SetActive(false);
			}
		}

		public void updateCache(Vector3 playerPos)
		{
			foreach (StaticObject obj in childObjects)
			{
				float dist = Vector3.Distance(obj.gameObject.transform.position, playerPos);
				bool visible = (dist < (float) obj.getSetting("VisibilityRange"));
				if (visible != obj.gameObject.activeSelf)
				{
					//Debug.Log("Setting " + obj.gameObject.name + " to visible=" + visible);
					obj.gameObject.SetActive(visible);
				}
			}
		}

		public Vector3 getCenter()
		{
			return centerPoint;
		}

		public float getVisibilityRange()
		{
			return visiblityRange;
		}

		public String getGroupName()
		{
			return groupName;
		}

		internal void deleteObject(StaticObject obj)
		{
			if (childObjects.Contains(obj))
			{
				childObjects.Remove(obj);
				MonoBehaviour.Destroy(obj.gameObject);
			}
			else
			{
				Debug.Log("Tried to delete an object that doesn't exist in this group!");
			}
		}

		public List<StaticObject> getStatics()
		{
			return childObjects;
		}
	}
}
