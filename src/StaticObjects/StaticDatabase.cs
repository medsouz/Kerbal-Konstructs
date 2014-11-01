using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticDatabase
	{
		//Groups are stored by name within the body name
		private Dictionary<string, Dictionary<string, StaticGroup>> groupList = new Dictionary<string,Dictionary<string,StaticGroup>>();
		private List<StaticModel> modelList = new List<StaticModel>();
		private string activeBodyName = "";

		public void addStatic(StaticObject obj)
		{
			String bodyName = ((CelestialBody) obj.getSetting("CelestialBody")).bodyName;
			String groupName = (string) obj.getSetting("Group");

			//Debug.Log("Creating object in group " + obj.groupName);

			if (!groupList.ContainsKey(bodyName))
				groupList.Add(bodyName, new Dictionary<string, StaticGroup>());

			if (!groupList[bodyName].ContainsKey(groupName))
			{
				StaticGroup group = new StaticGroup(bodyName, groupName);
				//Ungrouped objects get individually cached. New acts the same as Ungrouped but stores unsaved statics instead.
				if (groupName == "Ungrouped")
				{
					group.alwaysActive = true;
					group.active = true;
				}
				groupList[bodyName].Add(groupName, group);
			}

			groupList[bodyName][groupName].addStatic(obj);
		}

		public void cacheAll()
		{
			// ASH 01112014 Need to handle this.
			Debug.Log("KK: cacheAll() activeBodyname is " + activeBodyName);
			if (activeBodyName == "")
				return;

			var body = KerbalKonstructs.instance.getCurrentBody();
			Debug.Log("KK: getCurrentBody()" + (body == null ? " = NULL" : (".bodyName = " + body.bodyName)));

			if (groupList.ContainsKey(activeBodyName))
			{
				// ASH 01112014 This is wrong.
				//foreach (StaticGroup group in groupList[KerbalKonstructs.instance.getCurrentBody().bodyName].Values)

				// ASH 01112014 This is right.
				foreach (StaticGroup group in groupList[activeBodyName].Values)
				{
					group.cacheAll();
					Debug.Log("KK: cacheAll for " + activeBodyName + " " + group.getGroupName());
					if (!group.alwaysActive)
					{
						group.active = false;
						Debug.Log("KK: group alwaysActive is FALSE. Group is deactivated.");
					}
					else
					{
						Debug.Log("KK: group alwaysActive is TRUE. Group stays active.");
					}
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
			String bodyName = ((CelestialBody)obj.getSetting("CelestialBody")).bodyName;
			String groupName = (string)obj.getSetting("Group");

			if (groupList.ContainsKey(bodyName))
			{
				if (groupList[bodyName].ContainsKey(groupName))
				{
					groupList[bodyName][groupName].deleteObject(obj);
				}
				else
				{
					Debug.Log("Group not found! " + groupName);
				}
			}
			else
			{
				Debug.Log("Body not found! " + bodyName);
			}
		}

		public List<StaticObject> getAllStatics()
		{
			List<StaticObject> objects = new List<StaticObject>();
			foreach (Dictionary<string, StaticGroup> groups in groupList.Values)
			{
				foreach (StaticGroup group in groups.Values)
				{
					foreach (StaticObject obj in group.getStatics())
					{
						objects.Add(obj);
					}
				}
			}
			return objects;
		}

		public void registerModel(StaticModel model)
		{
			modelList.Add(model);
		}

		public List<StaticModel> getModels()
		{
			return modelList;
		}

		public List<StaticObject> getObjectsFromModel(StaticModel model)
		{
			return (from obj in getAllStatics() where obj.model == model select obj).ToList();
		}

		public StaticObject getStaticFromGameObject(GameObject gameObject)
		{
			List<StaticObject> objList = (from obj in getAllStatics() where obj.gameObject == gameObject select obj).ToList();
			if (objList.Count >= 1)
			{
				if (objList.Count > 1)
					Debug.Log("WARNING: More than one StaticObject references to GameObject " + gameObject.name);
				return objList[0];
			}
			Debug.Log("WARNING: StaticObject doesn't exist for " + gameObject.name);
			return null;
		}
	}
}
