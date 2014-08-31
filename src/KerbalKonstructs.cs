using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace KerbalKonstructs
{
	[KSPAddonFixed(KSPAddon.Startup.SpaceCentre, true, typeof(KerbalKonstructs))]
	public class KerbalKonstructs : MonoBehaviour
	{
		private CelestialBody currentBody;
		private StaticObject selectedObject;

		private StaticDatabase staticDB = new StaticDatabase();

		private CameraController camControl = new CameraController();
		private EditorGUI editor = new EditorGUI();

		void Awake()
		{
			//Assume that the Space Center is on Kerbin
			currentBody = Util.getCelestialBody("Kerbin");
			GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
			GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
			DontDestroyOnLoad(this);
			loadObjects();
			staticDB.loadObjectsForBody(currentBody.bodyName);
			InvokeRepeating("updateCache", 0, 1);
		}

		void onLevelWasLoaded(GameScenes data)
		{
			if (data.Equals(GameScenes.SPACECENTER))
			{
				//Assume that the Space Center is on Kerbin
				currentBody = Util.getCelestialBody("Kerbin");
				staticDB.onBodyChanged(currentBody);
			}
			else if (data.Equals(GameScenes.EDITOR))
			{
				staticDB.cacheAll();
			}
		}

		void onDominantBodyChange(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
		{
			currentBody = data.to;
			staticDB.onBodyChanged(data.to);
		}

		public void updateCache()
		{
			Vector3 playerPos;
			if (selectedObject != null)
			{
				playerPos = selectedObject.gameObject.transform.position;
			}
			else if (FlightGlobals.ActiveVessel != null)
			{
				playerPos = FlightGlobals.ActiveVessel.transform.position;
			}
			else
			{
				//HACKY: if there is no vessel use the camera, this could cause some issues
				playerPos = Camera.main.transform.position;
			}

			staticDB.updateCache(playerPos);
		}

		public void loadObjects()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");
			foreach(UrlDir.UrlConfig conf in configs)
			{
				string model = conf.config.GetValue("mesh");
				model = model.Substring(0, model.LastIndexOf('.'));
				string modelUrl = Path.GetDirectoryName(Path.GetDirectoryName(conf.url)) + "/" + model;
				//Debug.Log("Loading " + modelUrl);
				foreach (ConfigNode ins in conf.config.GetNodes("Instances"))
				{
					StaticObject obj = new StaticObject();
					obj.gameObject = GameDatabase.Instance.GetModel(modelUrl);
					string bodyName = ins.GetValue("CelestialBody");
					CelestialBody body = Util.getCelestialBody(bodyName);
					obj.parentBody = body;
					obj.position = ConfigNode.ParseVector3(ins.GetValue("RadialPosition"));
					obj.altitude = float.Parse(ins.GetValue("RadiusOffset"));
					obj.orientation = ConfigNode.ParseVector3(ins.GetValue("Orientation"));
					obj.rotation = float.Parse(ins.GetValue("RotationAngle"));
					obj.visibleRange = float.Parse(ins.GetValue("VisibilityRange"));

					//NEW VARIABLES 
					//KerbTown does not support group caching, for compatibility we will put these into "Ungrouped" group to be cached individually
					obj.groupName = ins.GetValue("Group") ?? "Ungrouped";

					staticDB.addStatic(obj);
					spawnObject(obj, false);
				}
			}
		}

		public void spawnObject(StaticObject obj, Boolean editing)
		{
			obj.gameObject.SetActive(editing);//Objects spawned at runtime should be active
			Transform[] gameObjectList = obj.gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();

			if (editing)
			{
				obj.editing = true;
				foreach (GameObject collider in colliderList)
				{
					collider.collider.enabled = false;
				}
				selectedObject = obj;
				InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
				camControl.enable(selectedObject.gameObject);
			}

			PQSCity.LODRange range = new PQSCity.LODRange
			{
				renderers = rendererList.ToArray(),
				objects = new GameObject[0],
				visibleRange = obj.visibleRange
			};
			obj.pqsCity = obj.gameObject.AddComponent<PQSCity>();
			obj.pqsCity.lod = new[] { range };
			obj.pqsCity.frameDelta = 1; //Unknown
			obj.pqsCity.repositionToSphere = true; //enable repositioning
			obj.pqsCity.repositionToSphereSurface = false; //Snap to surface?
			obj.pqsCity.repositionRadial = obj.position; //position
			obj.pqsCity.repositionRadiusOffset = obj.altitude; //height
			obj.pqsCity.reorientInitialUp = obj.orientation; //orientation
			obj.pqsCity.reorientFinalAngle = obj.rotation; //rotation x axis
			obj.pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
			obj.gameObject.transform.parent = obj.parentBody.transform;
			obj.pqsCity.sphere = obj.parentBody.pqsController;
			obj.pqsCity.order = 100;
			obj.pqsCity.modEnabled = true;
			obj.pqsCity.OnSetup();
			obj.pqsCity.Orientate();

			foreach (GameObject renderer in rendererList)
			{
				renderer.renderer.enabled = true;
			}
		}

		public void deselectObject()
		{
			selectedObject.editing = false;
			selectedObject = null;
			InputLockManager.RemoveControlLock("KKShipLock");
			camControl.disable();
		}

		void LateUpdate()
		{
			if (camControl.active)
			{
				camControl.updateCamera();
			}
		}

		void OnGUI()
		{
			//Use KSP's GUI skin
			GUI.skin = HighLogic.Skin;

			//Debug buttons
			if (GUI.Button(new Rect(270, 250, 150, 20), "Cache All"))
			{
				staticDB.cacheAll();
			}
			if (GUI.Button(new Rect(270, 280, 150, 20), "Load all for "+currentBody.bodyName))
			{
				staticDB.loadObjectsForBody(currentBody.bodyName);
			}

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				if (GUI.Button(new Rect(270, 350, 150, 20), "Place Object"))
				{
					StaticObject obj = new StaticObject();
					obj.gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					obj.altitude = (float)FlightGlobals.ActiveVessel.altitude;
					obj.parentBody = currentBody;
					obj.groupName = "New";
					obj.position = currentBody.transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position);
					obj.rotation = 0;
					obj.orientation = Vector3.up;
					obj.visibleRange = 25000;

					spawnObject(obj, true);
				}

				if (selectedObject != null)
				{
					if (GUI.Button(new Rect(270, 380, 150, 20), "Deselect Object"))
					{
						deselectObject();
						return;
					}

					//Editor Window
					editor.drawEditor(selectedObject);
				}
			}
		}


	}
}
