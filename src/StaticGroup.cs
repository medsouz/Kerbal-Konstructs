using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs
{
	class StaticGroup
	{
		private String groupName;
		private String bodyName;

		private List<StaticObject> childObjects = new List<StaticObject>();
		private Vector3 centerPoint = Vector3.zero;
		private float visiblityRange = 0;
		public Boolean alwaysActive = false;

		public StaticGroup(String name, String body)
		{
			groupName = name;
			bodyName = body;
		}

		public void addStatic(StaticObject obj)
		{
			childObjects.Add(obj);
			//TODO: recalculate center point and visibility range
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
				bool visible = (dist < obj.visibleRange);
				if (visible != obj.gameObject.activeSelf)
				{
					//Debug.Log("Setting " + obj.gameObject.name + " to visible=" + visible);
					obj.gameObject.SetActive(visible);
				}
			}
		}
	}
}
