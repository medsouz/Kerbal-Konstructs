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
		public StaticObject selectedObject;

		private StaticDatabase staticDB = new StaticDatabase();

		private CameraController camControl = new CameraController();
		private EditorGUI editor = new EditorGUI();
		private Boolean showEditor = false;
		private LaunchSiteSelectorGUI selector = new LaunchSiteSelectorGUI();
		private Boolean showSelector = false;

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
			ApplicationLauncher.Instance.AddModApplication(onSiteSelectorOn, onSiteSelectorOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SiteToolbarIcon", false));
		}

		void onLevelWasLoaded(GameScenes data)
		{
			//TODO: fix camera when switching scenes if an object is selected
			if (selectedObject != null)
			{
				deselectObject(false);
				camControl.active = false;
			}

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
			else if (data.Equals(GameScenes.FLIGHT))
			{
				//Fixes statics not showing up on launch sometimes
				//Still not 100% sure what causes that issue
				staticDB.onBodyChanged(currentBody);
				updateCache();
			}

			if (data.Equals(GameScenes.EDITOR) || data.Equals(GameScenes.SPH))
			{
				switch (data)
				{
					case GameScenes.SPH:
						selector.setEditorType(SiteType.SPH);
						break;
					case GameScenes.EDITOR:
						selector.setEditorType(SiteType.VAB);
						break;
					default:
						selector.setEditorType(SiteType.Any);
						break;
				}
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
				StaticModel model = new StaticModel();
				model.author = conf.config.GetValue("author") ?? "Unknown";
				model.meshName = conf.config.GetValue("mesh");
				model.meshName = model.meshName.Substring(0, model.meshName.LastIndexOf('.'));//remove file extension
				model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
				model.config = conf.url;
				model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
				model.defaultSiteTransform = conf.config.GetValue("DefaultLaunchPadTransform") ?? "";
				foreach (ConfigNode ins in conf.config.GetNodes("Instances"))
				{
					StaticObject obj = new StaticObject();
					obj.model = model;
					obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.meshName);
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

					if (obj.siteTransform == "" && obj.siteName != "")
					{
						if (model.defaultSiteTransform != "")
						{
							obj.siteTransform = model.defaultSiteTransform;
						}
						else
						{
							Debug.Log("Launch site is missing a transform. Defaulting to " + obj.siteName + "_spawn...");
							if (obj.gameObject.transform.Find(obj.siteName + "_spawn") != null)
							{
								obj.siteTransform = obj.siteName + "_spawn";
							}
							else
							{
								Debug.Log("FAILED: "+ obj.siteName + "_spawn does not exist! Attempting to use any transform with _spawn in the name.");
								Transform lastResort = obj.gameObject.transform.Cast<Transform>().FirstOrDefault(trans => trans.name.EndsWith("_spawn"));
								if (lastResort != null)
								{
									Debug.Log("Using " + lastResort.name + " as launchpad transform");
									obj.siteTransform = lastResort.name;
								}
								else
								{
									Debug.Log("All attempts at finding launchpad transform have failed (╯°□°）╯︵ ┻━┻");
								}
							}
						}
					}

					//NEW VARIABLES 
					//KerbTown does not support group caching, for compatibility we will put these into "Ungrouped" group to be cached individually
					obj.groupName = ins.GetValue("Group") ?? "Ungrouped";
					//Site description
					obj.siteDescription = ins.GetValue("LaunchSiteDescription") ?? "No description available";
					//Site icon
					String icon = ins.GetValue("LaunchSiteLogo") ?? "";
					obj.siteLogo = (icon != "") ? model.path + "/" + icon : "";
					//Site type: VAB, SPH, or ANY
					switch (ins.GetValue("LaunchSiteType") ?? "ANY")
					{
						case "VAB":
							obj.siteType = SiteType.VAB;
							break;
						case "SPH":
							obj.siteType = SiteType.SPH;
							break;
						default:
							obj.siteType = SiteType.Any;
							break;
					}

					staticDB.addStatic(obj);
					spawnObject(obj, false);
					if (obj.siteName != "")
					{
						LaunchSiteManager.createLaunchSite(obj);
					}
				}
				staticDB.registerModel(model);
			}
		}

		public void saveObjects()
		{
			foreach (StaticModel model in staticDB.getModels())
			{
				ConfigNode staticNode = new ConfigNode("STATIC");
				ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);
				modelConfig.RemoveNodes("Instances");

				foreach (StaticObject obj in staticDB.getObjectsFromModel(model))
				{
					ConfigNode inst = new ConfigNode("Instances");
					inst.AddValue("CelestialBody", obj.parentBody.bodyName);
					inst.AddValue("RadialPosition", ConfigNode.WriteVector(obj.position));
					inst.AddValue("RadiusOffset", obj.altitude.ToString());
					inst.AddValue("Orientation", ConfigNode.WriteVector(obj.orientation));
					inst.AddValue("RotationAngle", obj.rotation.ToString());
					inst.AddValue("VisibilityRange", obj.visibleRange.ToString());
					inst.AddValue("Group", obj.groupName);
					if (obj.siteName != "")
					{
						inst.AddValue("LaunchSiteName", obj.siteName);
						inst.AddValue("LaunchPadTransform", obj.siteTransform);
						inst.AddValue("LaunchSiteDescription", obj.siteDescription);
						inst.AddValue("LaunchSiteLogo", obj.siteLogo.Replace(obj.model.path + "/", ""));//Strip path from image
						inst.AddValue("LaunchSiteType", obj.siteType.ToString().ToUpper());
					}
					modelConfig.nodes.Add(inst);
				}

				staticNode.AddNode(modelConfig);
				staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + model.configPath, "Generated by Kerbal Konstructs - https://github.com/medsouz/Kerbal-Konstructs");
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
				selectObject(obj);
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

		public void deselectObject(Boolean disableCam = true)
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
			if(disableCam)//if you disable the camera when switching scenes shit will go down
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
				if (Input.GetKey(KeyCode.E))
				{
					selectedObject.position.z += editor.getIncrement();
					editor.updateSelection(selectedObject);
				}
				if (Input.GetKey(KeyCode.Q))
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

			if(Input.GetKeyDown(KeyCode.K) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				if (selectedObject != null)
					deselectObject();

				showEditor = !showEditor;
			}
		}

		void OnGUI()
		{
			//Use KSP's GUI skin
			GUI.skin = HighLogic.Skin;

			if (showSelector && (HighLogic.LoadedScene.Equals(GameScenes.EDITOR) || HighLogic.LoadedScene.Equals(GameScenes.SPH)))//Disable scene selector when not in the editor
				selector.drawSelector();

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				if (showEditor)
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

		public void selectObject(StaticObject obj)
		{
			InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
			InputLockManager.SetControlLock(ControlTypes.EVA_INPUT, "KKEVALock");
			InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");
			InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "KKCamModes");
			if (selectedObject != null)
				deselectObject();
			selectedObject = obj;
			selectedObject.editing = true;
			Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();
			foreach (GameObject collider in colliderList)
			{
				collider.collider.enabled = false;
			}
			if(camControl.active)
				camControl.disable();
			camControl.enable(obj.gameObject);
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

		void onSiteSelectorOn()
		{
			showSelector = true;
		}

		void onSiteSelectorOff()
		{
			showSelector = false;
		}

		void doNothing()
		{
			//wow so robust
		}

		public StaticDatabase getStaticDB()
		{
			return staticDB;
		}
	}
}
