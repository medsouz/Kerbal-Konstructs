using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticDatabase
	{
		//Groups are stored by name within the body name
		private Dictionary<string, Dictionary<string, StaticGroup>> groupList = new Dictionary<string,Dictionary<string,StaticGroup>>();
		private string activeBodyName = "";

		public void addStatic(StaticObject obj)
		{
			String bodyName = obj.parentBody.bodyName;
			String groupName = obj.groupName;

			//Debug.Log("Creating object in group " + obj.groupName);

			if (!groupList.ContainsKey(bodyName))
				groupList.Add(bodyName, new Dictionary<string, StaticGroup>());

			if (!groupList[bodyName].ContainsKey(groupName))
			{
				StaticGroup group = new StaticGroup(bodyName, groupName);
				//Ungrouped objects get individually cached. New acts the same as Ungrouped but stores unsaved statics instead.
				if (obj.groupName == "Ungrouped" || obj.groupName == "New")
				{
					group.alwaysActive = true;
					group.active = true;
				}
				groupList[bodyName].Add(groupName, group);
			}

			groupList[obj.parentBody.bodyName][obj.groupName].addStatic(obj);
		}

		public void cacheAll()
		{
			if (groupList.ContainsKey(activeBodyName))
			{
				foreach (StaticGroup group in groupList[KerbalKonstructs.instance.getCurrentBody().bodyName].Values)
				{
					group.cacheAll();
					if (!group.alwaysActive)
						group.active = false;
				}
			}
		}

		public void loadObjectsForBody(String bodyName)
		{
			activeBodyName = bodyName;
			if (groupList.ContainsKey(bodyName))
			{
				foreach (KeyValuePair<String, StaticGroup> bodyGroups in groupList[bodyName])
				{
					bodyGroups.Value.active = true;
				}
			}
			else
			{
				Debug.Log("No statics exist for " + bodyName);
			}
		}

		public void onBodyChanged(CelestialBody body)
		{
			if (body != null)
			{
				if (body.bodyName != activeBodyName)
				{
					cacheAll();
					loadObjectsForBody(body.bodyName);
				}
			}
			else
			{
				cacheAll();
				activeBodyName = "";
			}
		}

		public void updateCache(Vector3 playerPos)
		{
			if (groupList.ContainsKey(activeBodyName))
			{
				foreach (StaticGroup group in groupList[activeBodyName].Values)
				{
					if (!group.alwaysActive)
					{
						float dist = Vector3.Distance(group.getCenter(), playerPos);
						Boolean active = dist < group.getVisibilityRange();
						if (active != group.active && active == false)
						{
							Debug.Log("Caching group " + group.getGroupName());
							group.cacheAll();
						}
						group.active = active;
					}
					if (group.active)
					{
						group.updateCache(playerPos);
					}
				}
			}
		}

		public void deleteObject(StaticObject obj)
		{
			if (groupList.ContainsKey(obj.parentBody.bodyName))
			{
				if (groupList[obj.parentBody.bodyName].ContainsKey(obj.groupName))
				{
					groupList[obj.parentBody.bodyName][obj.groupName].deleteObject(obj);
				}
				else
				{
					Debug.Log("Group not found! " + obj.groupName);
				}
			}
			else
			{
				Debug.Log("Body not found! " + obj.parentBody.bodyName);
			}
		}
	}
}
