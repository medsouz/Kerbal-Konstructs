using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.UI;

namespace KerbalKonstructs
{
	[KSPAddonFixed(KSPAddon.Startup.SpaceCentre, true, typeof(KerbalKonstructs))]
	public class KerbalKonstructs : MonoBehaviour
	{
		public static KerbalKonstructs instance;

		private CelestialBody currentBody;
		private StaticObject selectedObject;

		private StaticDatabase staticDB = new StaticDatabase();

		private CameraController camControl = new CameraController();
		private EditorGUI editor = new EditorGUI();
		private LaunchSiteSelectorGUI selector = new LaunchSiteSelectorGUI();

		void Awake()
		{
			instance = this;
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
			else if (!data.Equals(GameScenes.FLIGHT))//Cache everywhere except the space center or during flight
			{
				staticDB.onBodyChanged(null);
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
				string author = conf.config.GetValue("author");
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
					obj.siteName = ins.GetValue("LaunchSiteName") ?? "";
					obj.siteTransform = ins.GetValue("LaunchPadTransform") ?? "";

					//NEW VARIABLES 
					//KerbTown does not support group caching, for compatibility we will put these into "Ungrouped" group to be cached individually
					obj.groupName = ins.GetValue("Group") ?? "Ungrouped";
					//Give credit yo
					obj.author = author;

					staticDB.addStatic(obj);
					spawnObject(obj, false);
					if (obj.siteName != "")
					{
						LaunchSiteManager.createLaunchSite(obj);
					}
				}
			}
		}

		public void spawnObject(StaticObject obj, Boolean editing)
		{
			obj.gameObject.SetActive(editing);//Objects spawned at runtime should be active
			Transform[] gameObjectList = obj.gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();

			setLayerRecursively(obj.gameObject, 15);

			if (editing)
			{
				obj.editing = true;
				foreach (GameObject collider in colliderList)
				{
					collider.collider.enabled = false;
				}
				if(selectedObject != null)
					deselectObject();
				selectedObject = obj;
				InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
				InputLockManager.SetControlLock(ControlTypes.EVA_INPUT, "KKEVALock");
				InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");
				InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "KKCamModes");
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
			obj.gameObject.transform.parent = obj.parentBody.pqsController.transform;
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
			Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();
			foreach (GameObject collider in colliderList)
			{
				collider.collider.enabled = true;
			}
			selectedObject = null;
			InputLockManager.RemoveControlLock("KKShipLock");
			InputLockManager.RemoveControlLock("KKEVALock");
			InputLockManager.RemoveControlLock("KKCamControls");
			InputLockManager.RemoveControlLock("KKCamModes");
			camControl.disable();
		}

		void LateUpdate()
		{
			if (camControl.active)
			{
				camControl.updateCamera();
			}
			if (selectedObject != null)
			{
				if (Input.GetKey(KeyCode.W))
				{
					selectedObject.position.y += editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.S))
				{
					selectedObject.position.y -= editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.D))
				{
					selectedObject.position.x += editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.A))
				{
					selectedObject.position.x -= editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.X))
				{
					selectedObject.position.z += editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.Z))
				{
					selectedObject.position.z -= editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.LeftShift))
				{
					selectedObject.altitude += editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.LeftControl))
				{
					selectedObject.altitude -= editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
			}
		}

		void OnGUI()
		{
			//Use KSP's GUI skin
			GUI.skin = HighLogic.Skin;

			//selector.drawSelector();

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

					staticDB.addStatic(obj);
					spawnObject(obj, true);
				}

				if (selectedObject != null)
				{
					//Editor Window
					editor.drawEditor(selectedObject);
				}
			}
		}

		public void deleteObject(StaticObject obj)
		{
			if (selectedObject == obj)
			{
				deselectObject();
			}
			staticDB.deleteObject(obj);
		}

		private static void setLayerRecursively(GameObject sGameObject, int newLayerNumber)
		{
			if ((sGameObject.collider != null && sGameObject.collider.enabled && !sGameObject.collider.isTrigger) || sGameObject.collider == null)
			{
				sGameObject.layer = newLayerNumber;
			}

			foreach (Transform child in sGameObject.transform)
			{
				setLayerRecursively(child.gameObject, newLayerNumber);
			}
		}

		public CelestialBody getCurrentBody()
		{
			return currentBody;
		}
	}
}
