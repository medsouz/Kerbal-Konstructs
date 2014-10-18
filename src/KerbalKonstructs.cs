using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.UI;
using System.Reflection;
using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.API;
using KerbalKonstructs.API.Config;

namespace KerbalKonstructs
{
	[KSPAddonFixed(KSPAddon.Startup.MainMenu, true, typeof(KerbalKonstructs))]
	public class KerbalKonstructs : MonoBehaviour
	{
		public static KerbalKonstructs instance;
		public static string installDir = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbalKonstructs));

		private CelestialBody currentBody;
		public StaticObject selectedObject;

		private StaticDatabase staticDB = new StaticDatabase();

		private CameraController camControl = new CameraController();
		private EditorGUI editor = new EditorGUI();
		private Boolean showEditor = false;
		private LaunchSiteSelectorGUI selector = new LaunchSiteSelectorGUI();
		private Boolean showSelector = false;
		private MapIconManager mapIconManager = new MapIconManager();
		private ApplicationLauncherButton siteSelector;

		//Configurable variables
		[KSPField]
		public Boolean launchFromAnySite = false;

		void Awake()
		{
			instance = this;
			//Setup configuration
			KKAPI.addModelSetting("mesh", new ConfigFile());
			ConfigGenericString authorConfig = new ConfigGenericString();
			authorConfig.setDefaultValue("Unknown");
			KKAPI.addModelSetting("author", authorConfig);
			KKAPI.addModelSetting("DefaultLaunchPadTransform", new ConfigGenericString());

			KKAPI.addInstanceSetting("CelestialBody", new ConfigCelestialBody());
			KKAPI.addInstanceSetting("RadialPosition", new ConfigVector3());
			KKAPI.addInstanceSetting("Orientation", new ConfigVector3());
			KKAPI.addInstanceSetting("RadiusOffset", new ConfigFloat());
			KKAPI.addInstanceSetting("RotationAngle", new ConfigFloat());
			ConfigFloat visibilityConfig = new ConfigFloat();
			visibilityConfig.setDefaultValue(25000f);
			KKAPI.addInstanceSetting("VisibilityRange", visibilityConfig);
			KKAPI.addInstanceSetting("LaunchSiteName", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchPadTransform", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchSiteAuthor", new ConfigGenericString());
			ConfigGenericString groupConfig = new ConfigGenericString();
			groupConfig.setDefaultValue("Ungrouped");
			KKAPI.addInstanceSetting("Group", groupConfig);
			ConfigGenericString descriptionConfig = new ConfigGenericString();
			descriptionConfig.setDefaultValue("No description available");
			KKAPI.addInstanceSetting("LaunchSiteDescription", descriptionConfig);
			KKAPI.addInstanceSetting("LaunchSiteLogo", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchSiteIcon", new ConfigGenericString());
			KKAPI.addInstanceSetting("LaunchSiteType", new ConfigSiteType());

			loadConfig();
			saveConfig();
			GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
			GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
			GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
			GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
			DontDestroyOnLoad(this);
			loadObjects();
			InvokeRepeating("updateCache", 0, 1);
			SpaceCenterManager.setKSC();
		}

		void OnVesselRecoveryRequested(Vessel data)
		{
			SpaceCenter csc = SpaceCenterManager.getClosestSpaceCenter(data.gameObject.transform.position);
			SpaceCenter.Instance = csc;
		}

		void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready)
			{
				//Just keep adding the button whenever the ApplicationLauncher is added to prevent it from disappearing, this is ineffecient but I don't care enough to come up with a better method.
				if (siteSelector != null)
					ApplicationLauncher.Instance.RemoveModApplication(siteSelector);
				siteSelector = ApplicationLauncher.Instance.AddModApplication(onSiteSelectorOn, onSiteSelectorOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SiteToolbarIcon", false));
			}
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
				currentBody = KKAPI.getCelestialBody("Kerbin");
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
			if (HighLogic.LoadedSceneIsGame)
			{
				Vector3 playerPos = Vector3.zero;
				if (selectedObject != null)
				{
					playerPos = selectedObject.gameObject.transform.position;
				}
				else if (FlightGlobals.ActiveVessel != null)
				{
					playerPos = FlightGlobals.ActiveVessel.transform.position;
				}
				else if (Camera.main != null)//Camera.main goes null when switching scenes
				{
					//HACKY: if there is no vessel use the camera, this could cause some issues
					playerPos = Camera.main.transform.position;
				}
				staticDB.updateCache(playerPos);
			}
		}

		public void loadObjects()
		{
			UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("STATIC");
			foreach(UrlDir.UrlConfig conf in configs)
			{
				StaticModel model = new StaticModel();
				model.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
				model.config = conf.url;
				model.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
				model.settings = KKAPI.loadConfig(conf.config, KKAPI.getModelSettings());

				foreach (ConfigNode ins in conf.config.GetNodes("MODULE"))
				{
					Debug.Log("Found module: "+ins.name+" in "+conf.name);
					StaticModule module = new StaticModule();
					foreach (ConfigNode.Value value in ins.values)
					{
						switch (value.name)
						{
							case "namespace":
								module.moduleNamespace = value.value;
								break;
							case "name":
								module.moduleClassname = value.value;
								break;
							default:
								module.moduleFields.Add(value.name, value.value);
								break;
						}
					}
					model.modules.Add(module);
				}
				foreach (ConfigNode ins in conf.config.GetNodes("Instances"))
				{
					StaticObject obj = new StaticObject();
					obj.model = model;
					obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));

					obj.settings = KKAPI.loadConfig(ins, KKAPI.getInstanceSettings());

					if (!obj.settings.ContainsKey("LaunchPadTransform") && obj.settings.ContainsKey("LaunchSiteName"))
					{
						if (model.settings.Keys.Contains("DefaultLaunchPadTransform"))
						{
							obj.settings.Add("LaunchPadTransform", model.getSetting("DefaultLaunchPadTransform"));
						}
						else
						{
							Debug.Log("Launch site is missing a transform. Defaulting to " + obj.getSetting("LaunchSiteName") + "_spawn...");
							if (obj.gameObject.transform.Find(obj.getSetting("LaunchSiteName") + "_spawn") != null)
							{
								obj.settings.Add("LaunchPadTransform", obj.getSetting("LaunchSiteName") + "_spawn");
							}
							else
							{
								Debug.Log("FAILED: " + obj.getSetting("LaunchSiteName") + "_spawn does not exist! Attempting to use any transform with _spawn in the name.");
								Transform lastResort = obj.gameObject.transform.Cast<Transform>().FirstOrDefault(trans => trans.name.EndsWith("_spawn"));
								if (lastResort != null)
								{
									Debug.Log("Using " + lastResort.name + " as launchpad transform");
									obj.settings.Add("LaunchPadTransform", lastResort.name);
								}
								else
								{
									Debug.Log("All attempts at finding a launchpad transform have failed (╯°□°）╯︵ ┻━┻");
								}
							}
						}
					}

					staticDB.addStatic(obj);
					spawnObject(obj, false);
					if (obj.settings.ContainsKey("LaunchSiteName"))
					{
						LaunchSiteManager.createLaunchSite(obj);
					}
				}
				staticDB.registerModel(model);
			}
		}

		public void saveObjects()
		{
			//TODO: REIMPLEMENT THIS YOU LAZY FUCK
			/*foreach (StaticModel model in staticDB.getModels())
			{
				Debug.Log("Saving "+model.config);
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
						if (obj.siteIcon != "")
							inst.AddValue("LaunchSiteIcon", obj.siteIcon.Replace(obj.model.path + "/", ""));
						inst.AddValue("LaunchSiteType", obj.siteType.ToString().ToUpper());
						if(obj.siteAuthor != "")
							inst.AddValue("LaunchSiteAuthor", obj.siteAuthor);
					}
					modelConfig.nodes.Add(inst);
				}

				staticNode.AddNode(modelConfig);
				staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + model.configPath, "Generated by Kerbal Konstructs - https://github.com/medsouz/Kerbal-Konstructs");
			}*/
		}

		public void spawnObject(StaticObject obj, Boolean editing)
		{
			obj.gameObject.SetActive(editing);//Objects spawned at runtime should be active
			Transform[] gameObjectList = obj.gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();

			setLayerRecursively(obj.gameObject, 15);

			if (editing)
			{
				selectObject(obj);
			}

			PQSCity.LODRange range = new PQSCity.LODRange
			{
				renderers = rendererList.ToArray(),
				objects = new GameObject[0],
				visibleRange = (float)obj.getSetting("VisibilityRange")
			};
			obj.pqsCity = obj.gameObject.AddComponent<PQSCity>();
			obj.pqsCity.lod = new[] { range };
			obj.pqsCity.frameDelta = 1; //Unknown
			obj.pqsCity.repositionToSphere = true; //enable repositioning
			obj.pqsCity.repositionToSphereSurface = false; //Snap to surface?
			obj.pqsCity.repositionRadial = (Vector3)obj.getSetting("RadialPosition"); //position
			obj.pqsCity.repositionRadiusOffset = (float)obj.getSetting("RadiusOffset"); //height
			obj.pqsCity.reorientInitialUp = (Vector3)obj.getSetting("Orientation"); //orientation
			obj.pqsCity.reorientFinalAngle = (float)obj.getSetting("RotationAngle"); //rotation x axis
			obj.pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
			obj.gameObject.transform.parent = ((CelestialBody)obj.getSetting("CelestialBody")).pqsController.transform;
			obj.pqsCity.sphere = ((CelestialBody)obj.getSetting("CelestialBody")).pqsController;
			obj.pqsCity.order = 100;
			obj.pqsCity.modEnabled = true;
			obj.pqsCity.OnSetup();
			obj.pqsCity.Orientate();

			foreach (StaticModule module in obj.model.modules)
			{
				Type moduleType = AssemblyLoader.loadedAssemblies.SelectMany(asm => asm.assembly.GetTypes()).FirstOrDefault(t => t.Namespace == module.moduleNamespace && t.Name == module.moduleClassname);
				MonoBehaviour mod = obj.gameObject.AddComponent(moduleType) as MonoBehaviour;

				if (mod != null)
				{
					foreach (string fieldName in module.moduleFields.Keys)
					{
						FieldInfo field = mod.GetType().GetField(fieldName);
						if (field != null)
						{
							field.SetValue(mod, Convert.ChangeType(module.moduleFields[fieldName], field.FieldType));
						}
						else
						{
							Debug.Log("WARNING: Field " + fieldName + " does not exist in " + module.moduleClassname);
						}
					}
				}
				else
				{
					Debug.Log("WARNING: Module " + module.moduleClassname + " could not be loaded in " + obj.gameObject.name);
				}
			}

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
				Vector3 pos = Vector3.zero;
				float alt = 0;
				bool changed = false;
				if (Input.GetKey(KeyCode.W))
				{
					pos.y += editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.S))
				{
					pos.y -= editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.D))
				{
					pos.x += editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.A))
				{
					pos.x -= editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.E))
				{
					pos.z += editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.Q))
				{
					pos.z -= editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.LeftShift))
				{
					alt += editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.LeftControl))
				{
					alt -= editor.getIncrement();
					changed = true;
				}
				if (changed)
				{
					//This should probably be changed...
					pos += (Vector3) selectedObject.getSetting("RadialPosition");
					alt += (float) selectedObject.getSetting("RadiusOffset");
					selectedObject.setSetting("RadialPosition", pos);
					selectedObject.setSetting("RadiusOffset", alt);
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

			if (MapView.MapIsEnabled)
			{
				mapIconManager.drawIcons();
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
			//Make sure the editor doesn't think you're still mousing over the site selector
			InputLockManager.RemoveControlLock("KKEditorLock");
		}

		void doNothing()
		{
			//wow so robust
		}

		public StaticDatabase getStaticDB()
		{
			return staticDB;
		}

		public bool loadConfig()
		{
			ConfigNode cfg = ConfigNode.Load(installDir + @"\KerbalKonstructs.cfg".Replace('/', '\\'));
			if (cfg != null)
			{
				foreach (FieldInfo f in GetType().GetFields())
				{
					if (Attribute.IsDefined(f, typeof(KSPField)))
					{
						if(cfg.HasValue(f.Name))
							f.SetValue(this, Convert.ChangeType(cfg.GetValue(f.Name), f.FieldType));
					}
				}
				return true;
			}
			return false;
		}

		public void saveConfig()
		{
			ConfigNode cfg = new ConfigNode();
			foreach (FieldInfo f in GetType().GetFields())
			{
				if (Attribute.IsDefined(f, typeof(KSPField)))
				{
					cfg.AddValue(f.Name, f.GetValue(this));
				}
			}
			Directory.CreateDirectory(installDir);
			cfg.Save(installDir + "/KerbalKonstructs.cfg", "Kerbal Konstructs - https://github.com/medsouz/Kerbal-Konstructs");
		}
	}
}
