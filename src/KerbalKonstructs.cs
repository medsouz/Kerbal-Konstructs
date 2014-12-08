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
		public StaticObject selectedObject;

		private Boolean atMainMenu = false;
		private CelestialBody currentBody;
		private StaticDatabase staticDB = new StaticDatabase();
		private CameraController camControl = new CameraController();
		private EditorGUI editor = new EditorGUI();
		private EditorGUI manager = new EditorGUI();
		private EditorGUI facilitymanager = new EditorGUI();
		private LaunchSiteSelectorGUI selector = new LaunchSiteSelectorGUI();
		private MapIconManager mapIconManager = new MapIconManager();

		// Show toggles
		private Boolean showEditor = false;
		private Boolean showSelector = false;
		private Boolean showBaseManager = false;
		private Boolean showMapManager = false;

		// App Buttons
		private ApplicationLauncherButton siteSelector;
		private ApplicationLauncherButton baseManager;
		private ApplicationLauncherButton mapManager;

		// Configurable variables
		[KSPField]
		public Boolean launchFromAnySite = false;
		[KSPField]
		public Boolean disableCareerStrategyLayer = false;
		[KSPField]
		public Boolean enableATC = true;
		[KSPField]
		public Boolean enableNGS = true;
		[KSPField]
		public Double staffHireCost = 1000;
		[KSPField]
		public Double staffRepRequirementMultiplier = 50;

		void Awake()
		{
			instance = this;
			Debug.Log("KK: Awake");

			// Game Event Additions
			GameEvents.onDominantBodyChange.Add(onDominantBodyChange);
			GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
			GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
			GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);
			GameEvents.OnFundsChanged.Add(OnDoshChanged);
			GameEvents.onVesselRecovered.Add(OnVesselRecovered);

			// Model API
			KKAPI.addModelSetting("mesh", new ConfigFile());
			ConfigGenericString authorConfig = new ConfigGenericString();
			authorConfig.setDefaultValue("Unknown");
			KKAPI.addModelSetting("author", authorConfig);
			KKAPI.addModelSetting("DefaultLaunchPadTransform", new ConfigGenericString());
			KKAPI.addModelSetting("title", new ConfigGenericString());
			KKAPI.addModelSetting("category", new ConfigGenericString());
			KKAPI.addModelSetting("cost", new ConfigFloat());
			KKAPI.addModelSetting("mass", new ConfigFloat());
			KKAPI.addModelSetting("crashTolerance", new ConfigFloat());
			KKAPI.addModelSetting("manufacturer", new ConfigGenericString());
			KKAPI.addModelSetting("description", new ConfigGenericString());
			KKAPI.addModelSetting("thumbnail", new ConfigGenericString());

			// START Instance API ******			
				// Position
				KKAPI.addInstanceSetting("CelestialBody", new ConfigCelestialBody());
				KKAPI.addInstanceSetting("RadialPosition", new ConfigVector3());
				KKAPI.addInstanceSetting("Orientation", new ConfigVector3());
				KKAPI.addInstanceSetting("RadiusOffset", new ConfigFloat());
				KKAPI.addInstanceSetting("RotationAngle", new ConfigFloat());

				// Visibility and Grouping
				ConfigFloat visibilityConfig = new ConfigFloat();
				visibilityConfig.setDefaultValue(25000f);
				KKAPI.addInstanceSetting("VisibilityRange", visibilityConfig);
				ConfigGenericString groupConfig = new ConfigGenericString();
				groupConfig.setDefaultValue("Ungrouped");
				KKAPI.addInstanceSetting("Group", groupConfig);

				// Launchsite
				KKAPI.addInstanceSetting("LaunchSiteName", new ConfigGenericString());
				KKAPI.addInstanceSetting("LaunchPadTransform", new ConfigGenericString());
				KKAPI.addInstanceSetting("LaunchSiteAuthor", new ConfigGenericString());
				ConfigGenericString descriptionConfig = new ConfigGenericString();
				descriptionConfig.setDefaultValue("No description available");
				KKAPI.addInstanceSetting("LaunchSiteDescription", descriptionConfig);
				KKAPI.addInstanceSetting("LaunchSiteLogo", new ConfigGenericString());
				KKAPI.addInstanceSetting("LaunchSiteIcon", new ConfigGenericString());
				KKAPI.addInstanceSetting("LaunchSiteType", new ConfigSiteType());
				ConfigGenericString category = new ConfigGenericString();
				category.setDefaultValue("Other");
				KKAPI.addInstanceSetting("Category", category);

				// Career Mode Strategy
				ConfigFloat openCost = new ConfigFloat();
				openCost.setDefaultValue(0f);
				KKAPI.addInstanceSetting("OpenCost", openCost);
				ConfigFloat closeValue = new ConfigFloat();
				closeValue.setDefaultValue(0f);
				KKAPI.addInstanceSetting("CloseValue", closeValue);
				ConfigGenericString opencloseState = new ConfigGenericString();
				opencloseState.setDefaultValue("Closed");
				KKAPI.addInstanceSetting("OpenCloseState", opencloseState);
				
				// Facility Unique ID and Role
				// Use RadialPosition
				// KKAPI.addInstanceSetting("FacilityUID", new ConfigGenericString());
				ConfigGenericString facilityrole = new ConfigGenericString();
				facilityrole.setDefaultValue("None");
				KKAPI.addModelSetting("FacilityRole", facilityrole);

				// Facility Ratings
				KKAPI.addModelSetting("StaffMax", new ConfigFloat());
				KKAPI.addInstanceSetting("StaffCurrent", new ConfigFloat());
				KKAPI.addModelSetting("LqFMax", new ConfigFloat());
				KKAPI.addInstanceSetting("LqFCurrent", new ConfigFloat());
				KKAPI.addModelSetting("OxFMax", new ConfigFloat());
				KKAPI.addInstanceSetting("OxFCurrent", new ConfigFloat());
				KKAPI.addModelSetting("MoFMax", new ConfigFloat());
				KKAPI.addInstanceSetting("MoFCurrent", new ConfigFloat());
				KKAPI.addModelSetting("ScienceOMax", new ConfigFloat());
				KKAPI.addInstanceSetting("ScienceOCurrent", new ConfigFloat());
				KKAPI.addModelSetting("RepOMax", new ConfigFloat());
				KKAPI.addInstanceSetting("RepOCurrent", new ConfigFloat());
				KKAPI.addModelSetting("FundsOMax", new ConfigFloat());
				KKAPI.addInstanceSetting("FundsOCurrent", new ConfigFloat());
				KKAPI.addModelSetting("RecoveryBMax", new ConfigFloat());
				KKAPI.addInstanceSetting("RecoveryBCurrent", new ConfigFloat());
				KKAPI.addModelSetting("LaunchBMax", new ConfigFloat());
				KKAPI.addInstanceSetting("LaunchBCurrent", new ConfigFloat());
				KKAPI.addModelSetting("TonsStMax", new ConfigFloat());
				KKAPI.addInstanceSetting("TonsStCurrent", new ConfigFloat());

			// END Instance API ******

			SpaceCenterManager.setKSC();

			loadConfig();
			saveConfig();
			
			DontDestroyOnLoad(this);
			loadObjects();
		}

		public Boolean CareerStrategyEnabled(Game gGame)
		{
			if (gGame.Mode == Game.Modes.CAREER)
			{
				if (!KerbalKonstructs.instance.disableCareerStrategyLayer)
					return true;
				else
					return false;
			}
			else
				return false;
		}

		void OnDoshChanged(double amount, TransactionReasons reason)
		{
			// Debug.Log("KK: Funds changed - " + amount + " because " + reason);
		}

		void OnVesselRecoveryRequested(Vessel data)
		{
			if (CareerStrategyEnabled(HighLogic.CurrentGame))
			{
				// Change the Space Centre to the nearest open base
				SpaceCenter csc = SpaceCenterManager.getClosestSpaceCenter(data.gameObject.transform.position);
				SpaceCenter.Instance = csc;
				Debug.Log("KK: event onVesselRecoveryRequested");
				Debug.Log("KK: Nearest SpaceCenter is " + SpaceCenter.Instance.name + " " + csc.name);
			}
		}

		void OnVesselRecovered(ProtoVessel vessel)
		{
			if (vessel == null)
				Debug.Log("KK: onVesselRecovered vessel was null but we don't care");

			if (CareerStrategyEnabled(HighLogic.CurrentGame))
			{
				// Put the KSC back as the Space Centre
				Debug.Log("KK: Resetting SpaceCenter to KSC");
				SpaceCenter.Instance = SpaceCenterManager.KSC;
			}
		}

		void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready)
			{
				bool vis;
				
				if (siteSelector == null || !ApplicationLauncher.Instance.Contains(siteSelector, out vis))				
					siteSelector = ApplicationLauncher.Instance.AddModApplication(onSiteSelectorOn, onSiteSelectorOff, onSiteSelectorOnHover, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/SiteToolbarIcon", false));

				if (baseManager == null || !ApplicationLauncher.Instance.Contains(baseManager, out vis))				
					baseManager = ApplicationLauncher.Instance.AddModApplication(onBaseManagerOn, onBaseManagerOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.FLIGHT, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/BaseManagerIcon", false));

				if (mapManager == null || !ApplicationLauncher.Instance.Contains(mapManager, out vis))
					mapManager = ApplicationLauncher.Instance.AddModApplication(onMapManagerOn, onMapManagerOff, doNothing, doNothing, doNothing, doNothing, ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.MAPVIEW, GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/BaseManagerIcon", false));
			}
		}

		void onSiteSelectorOnHover()
		{
			string hovermessage = "Selected launchsite is " + EditorLogic.fetch.launchSiteName;
			ScreenMessages.PostScreenMessage(hovermessage, 10, 0);
		}

		void onLevelWasLoaded(GameScenes data)
		{
			bool something = true;
			
			if (selectedObject != null)
			{
				Debug.Log("KK: Deselecting an object.");
				deselectObject(false);
				camControl.active = false;
			}

			// ASH 01112014 Toggle invoking of updateCache on and off for the flight scene only
			if (data.Equals(GameScenes.FLIGHT))
			{
				// ASH 04112014 Likely responsible for camera locks in the flight and space centre scenes
				InputLockManager.RemoveControlLock("KKEditorLock");
				updateCache();
				InvokeRepeating("updateCache", 0, 1);
				something = false;
			}
			else
			{
				CancelInvoke("updateCache");
			}

			if (data.Equals(GameScenes.SPACECENTER))
			{
				// ASH 04112014 Likely responsible for camera locks in the flight and space centre scenes
				InputLockManager.RemoveControlLock("KKEditorLock");
				currentBody = KKAPI.getCelestialBody("Kerbin");
				staticDB.onBodyChanged(KKAPI.getCelestialBody("Kerbin"));
				updateCache();

				if (CareerStrategyEnabled(HighLogic.CurrentGame))
				{
					Debug.Log("KK: Load launchsite openclose states for career game");
					PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				}
				
				something = false;
			}

			if (data.Equals(GameScenes.MAINMENU))
			{
				// Close all the launchsite objects
				Debug.Log("KK: Closing all launchsites");
				LaunchSiteManager.setAllLaunchsitesClosed();
				atMainMenu = true;
				something = false;
			}
			
			if (data.Equals(GameScenes.EDITOR) || data.Equals(GameScenes.SPH))
			{
				// Prevent abuse if selector left open when switching to from VAB and SPH
				selector.Close();

				// Default selected launchsite when switching between save games
				switch (data)
				{
					case GameScenes.SPH:
						selector.setEditorType(SiteType.SPH);
						if (atMainMenu)
						{
							Debug.Log("KK: First visit to SPH");
							LaunchSiteManager.setLaunchSite("Runway");
							atMainMenu = false;
						}
						break;
					case GameScenes.EDITOR:
						selector.setEditorType(SiteType.VAB);
						if (atMainMenu)
						{
							Debug.Log("KK: First visit to VAB");
							LaunchSiteManager.setLaunchSite("LaunchPad");
							atMainMenu = false;
						}
						break;
					default:
						selector.setEditorType(SiteType.Any);
						break;
				}
			}

			if (something)
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
				else if (Camera.main != null)
				{
					playerPos = Camera.main.transform.position;
				}
				else
				{
					Debug.Log("KK: KerbalKonstructs.updateCache could not determine playerPos. All hell now happens.");
				}

				staticDB.updateCache(playerPos);
			}
			else
			{
				Debug.Log("KK: KerbalKonstructs.updateCache not HighLogic.LoadedSceneIsGame. Why is this an if?");
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
					Debug.Log("KK: Found module: "+ins.name+" in "+conf.name);
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
					Debug.Log("KK: Adding module");
				}

				foreach (ConfigNode ins in conf.config.GetNodes("Instances"))
				{
					// Debug.Log("KK: Loading models");
					StaticObject obj = new StaticObject();
					obj.model = model;
					obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));

					if (obj.gameObject == null)
					{
						Debug.Log("KK: Could not find " + model.getSetting("mesh") + ".mu! Did the mod forget to include it or did you actually install it?");
						continue;
					}
					// Debug.Log("KK: mesh is " + (string)model.getSetting("mesh"));

					obj.settings = KKAPI.loadConfig(ins, KKAPI.getInstanceSettings());

					if (!obj.settings.ContainsKey("LaunchPadTransform") && obj.settings.ContainsKey("LaunchSiteName"))
					{
						
						if (model.settings.Keys.Contains("DefaultLaunchPadTransform"))
						{
							obj.settings.Add("LaunchPadTransform", model.getSetting("DefaultLaunchPadTransform"));
						}
						else
						{
							Debug.Log("KK: Launch site is missing a transform. Defaulting to " + obj.getSetting("LaunchSiteName") + "_spawn...");
							
							if (obj.gameObject.transform.Find(obj.getSetting("LaunchSiteName") + "_spawn") != null)
							{
								obj.settings.Add("LaunchPadTransform", obj.getSetting("LaunchSiteName") + "_spawn");
							}
							else
							{
								Debug.Log("KK: FAILED: " + obj.getSetting("LaunchSiteName") + "_spawn does not exist! Attempting to use any transform with _spawn in the name.");
								Transform lastResort = obj.gameObject.transform.Cast<Transform>().FirstOrDefault(trans => trans.name.EndsWith("_spawn"));
								
								if (lastResort != null)
								{
									Debug.Log("KK: Using " + lastResort.name + " as launchpad transform");
									obj.settings.Add("LaunchPadTransform", lastResort.name);
								}
								else
								{
									Debug.Log("KK: All attempts at finding a launchpad transform have failed (╯°□°）╯︵ ┻━┻ This static isn't configured for KK properly. Tell the modder.");
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
			foreach (StaticModel model in staticDB.getModels())
			{
				ConfigNode staticNode = new ConfigNode("STATIC");
				ConfigNode modelConfig = GameDatabase.Instance.GetConfigNode(model.config);
							
				modelConfig.RemoveNodes("Instances");

				foreach (StaticObject obj in staticDB.getObjectsFromModel(model))
				{
					ConfigNode inst = new ConfigNode("Instances");
					foreach (KeyValuePair<string, object> setting in obj.settings)
					{
						inst.AddValue(setting.Key, KKAPI.getInstanceSettings()[setting.Key].convertValueToConfig(setting.Value));
					}
					modelConfig.nodes.Add(inst);
				}

				staticNode.AddNode(modelConfig);
				staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + model.configPath, "Generated by Kerbal Konstructs");
			}
		}

		public void spawnObject(StaticObject obj, Boolean editing)
		{
			// Objects spawned at runtime should be active
			obj.gameObject.SetActive(editing);
			Transform[] gameObjectList = obj.gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();

			setLayerRecursively(obj.gameObject, 15);

			if (editing)
			{
				selectObject(obj);
			}

			float objvisibleRange = (float)obj.getSetting("VisibilityRange");
			if (objvisibleRange < 1)
			{
				objvisibleRange = 25000f;
				Debug.Log("KK: Object had no VisibilityRange set. Defaulting to 25000m");
			}

			PQSCity.LODRange range = new PQSCity.LODRange
			{
				renderers = rendererList.ToArray(),
				objects = new GameObject[0],
				visibleRange = objvisibleRange
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
							Debug.Log("KK: WARNING: Field " + fieldName + " does not exist in " + module.moduleClassname);
						}
					}
				}
				else
				{
					Debug.Log("KK: WARNING: Module " + module.moduleClassname + " could not be loaded in " + obj.gameObject.name);
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
			
			if(disableCam)
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
				
				// ASH 08112014 Fix clashing with camera zooming
				if (Input.GetKey(KeyCode.RightBracket))
				{
					alt += editor.getIncrement();
					changed = true;
				}
				if (Input.GetKey(KeyCode.LeftBracket))
				{
					alt -= editor.getIncrement();
					changed = true;
				}
				
				if (changed)
				{
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
			GUI.skin = HighLogic.Skin;

			if (showSelector && (HighLogic.LoadedScene.Equals(GameScenes.EDITOR) || HighLogic.LoadedScene.Equals(GameScenes.SPH)))//Disable scene selector when not in the editor
				selector.drawSelector();

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				if (showEditor)
				{
					editor.drawEditor(selectedObject);
				}

				if (showBaseManager)
				{
					manager.drawManager(selectedObject);
				}
			}

			if (MapView.MapIsEnabled)
			{
				if (showMapManager)
					mapIconManager.drawManager();

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

		public void selectObject(StaticObject obj, bool isEditing = true)
		{
			InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, "KKShipLock");
			InputLockManager.SetControlLock(ControlTypes.EVA_INPUT, "KKEVALock");
			InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");
			InputLockManager.SetControlLock(ControlTypes.CAMERAMODES, "KKCamModes");
			
			if (selectedObject != null)
				deselectObject();
			
			selectedObject = obj;
			if (isEditing)
			{
				selectedObject.editing = true;

				Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
				List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();

				foreach (GameObject collider in colliderList)
				{
					collider.collider.enabled = false;
				}
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
			else
			{
				Debug.Log("KK: setLayerRecursively did not set a layer to " + newLayerNumber + " because ??? if statement is a bloody mess");
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
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showSelector = true;
		}

		void onBaseManagerOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showBaseManager = true;
		}

		void onMapManagerOn()
		{
			PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
			showMapManager = true;
		}

		void onSiteSelectorOff()
		{
			showSelector = false;
			InputLockManager.RemoveControlLock("KKEditorLock");
			PersistenceFile<LaunchSite>.SaveList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
		}

		void onBaseManagerOff()
		{
			showBaseManager = false;
			if (selectedObject != null)
				deselectObject();
		}

		void onMapManagerOff()
		{
			showMapManager = false;
		}

		void doNothing()
		{
			// wow so robust
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
