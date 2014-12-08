using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;

// R and T LOG
// 14112014 ASH

namespace KerbalKonstructs.UI
{
	class EditorGUI
	{
		StaticObject selectedObject;
		private String xPos, yPos, zPos, altitude, rotation, customgroup = "";
		private String visrange = "";
		private String increment = "1";
		public Boolean enableColliders = false;

		public Texture tBilleted = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/billeted", false);
		public Texture tCopyPos = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/copypos", false);
		public Texture tPastePos = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/pastepos", false);
		public Texture tIconClosed = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteclosed", false);
		public Texture tIconOpen = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteopen", false);
		public Texture tLeftOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/lefton", false);
		public Texture tLeftOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/leftoff", false);
		public Texture tRightOn = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/righton", false);
		public Texture tRightOff = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/rightoff", false);

		Vector2 scrollPos;
		private Boolean editingSite = false;
		private float fOldRange = 0f;
		Boolean creating = false;
		Boolean showLocal = false;
		Boolean managingFacility = false;
		Boolean onNGS = false;

		Rect toolRect = new Rect(150, 25, 290, 410);
		Rect editorRect = new Rect(10, 25, 520, 520);
		Rect siteEditorRect = new Rect(400, 50, 340, 480);
		Rect managerRect = new Rect(10, 25, 400, 405);
		Rect facilityRect = new Rect(150, 75, 350, 400);
		Rect NGSRect = new Rect(250, 50, 350, 160);

		private GUIStyle listStyle = new GUIStyle();
		private GUIStyle navStyle = new GUIStyle();
		public LaunchSite lTargetSite = null;

		string siteName, siteTrans, siteDesc, siteAuthor;
		float flOpenCost, flCloseValue;
		SiteType siteType;
		string siteCategory;
		Vector2 descScroll;

		private GUIContent[] siteTypeOptions = {
										new GUIContent("VAB"),
										new GUIContent("SPH"),
										new GUIContent("ANY")
									};
		ComboBox siteTypeMenu;

		public EditorGUI()
		{
			listStyle.normal.textColor = Color.white;
			listStyle.onHover.background =
			listStyle.hover.background = new Texture2D(2, 2);
			listStyle.padding.left =
			listStyle.padding.right =
			listStyle.padding.top =
			listStyle.padding.bottom = 4;

			navStyle.padding.left = 0;
			navStyle.padding.right = 0;
			navStyle.padding.top = 1;
			navStyle.padding.bottom = 3;
			
			siteTypeMenu = new ComboBox(siteTypeOptions[0], siteTypeOptions, "button", "box", null, listStyle);
		}

		public void drawManager(StaticObject obj)
		{
			if (obj != null)
			{
				if (selectedObject != obj)
					updateSelection(obj);

				if (managingFacility)
					facilityRect = GUI.Window(0xB00B1E1, facilityRect, drawFacilityManagerWindow, "Base Boss Facility Manager");
			}

			if (onNGS)
			{
				NGSRect = GUI.Window(0xB00B1E9, NGSRect, drawNGSWindow, "", navStyle);
			}

			managerRect = GUI.Window(0xB00B1E2, managerRect, drawBaseManagerWindow, "Base Boss");

		}

		public void drawEditor(StaticObject obj)
		{
			if (obj != null)
			{
				if (selectedObject != obj)
					updateSelection(obj);

				toolRect = GUI.Window(0xB00B1E3, toolRect, drawToolWindow, "KK Instance Editor");

				if (editingSite)
				{
						siteEditorRect = GUI.Window(0xB00B1E4, siteEditorRect, drawSiteEditorWindow, "KK Launchsite Editor");
				}
			}

			editorRect = GUI.Window(0xB00B1E5, editorRect, drawEditorWindow, "Kerbal Konstructs Statics Editor");
		}

		public Boolean isCareerGame()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				// disableCareerStrategyLayer is configurable in KerbalKonstructs.cfg
				if (!KerbalKonstructs.instance.disableCareerStrategyLayer)
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		public float fRangeToTarget = 0f;
		public bool bClosing = false;
		public int iCorrection = 3;
		public Texture tTextureLeft = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/leftoff", false);
		public Texture tTextureRight = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/rightoff", false);
		public Texture tTextureMiddle = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/siteclosed", false);
		public string sTargetSiteName = "NO TARGET";

		private float angle1, angle2;
		private float angle3, angle4;

		private Vector3 vCrft;
		private Vector3 vSPos;
		private Vector3 vHead;

		void prepNGS()
		{
			if (lTargetSite != null)
			{
				sTargetSiteName = lTargetSite.name;
				fRangeToTarget = LaunchSiteManager.getDistanceToBase(FlightGlobals.ActiveVessel.GetTransform().position, lTargetSite);
				if (fRangeToTarget > fOldRange) bClosing = false;
				if (fRangeToTarget < fOldRange) bClosing = true;

				fOldRange = fRangeToTarget;

				if (bClosing)
					tTextureMiddle = tIconOpen;
				else
					tTextureMiddle = tIconClosed;

				Vector3 vcraftpos = FlightGlobals.ActiveVessel.GetTransform().position;
				vCrft = vcraftpos;
				Vector3 vsitepos = lTargetSite.GameObject.transform.position;
				vSPos = vsitepos;
				Vector3 vHeading = (Vector3)FlightGlobals.ActiveVessel.transform.up;
				vHead = vHeading;

				iCorrection = GetCourseCorrection(vcraftpos, vsitepos, vHeading);
				if (iCorrection == 1)
				{
					tTextureLeft = tLeftOn;
					tTextureRight = tRightOff;
				}
				if (iCorrection == 2)
				{
					tTextureLeft = tLeftOff;
					tTextureRight = tRightOn;
				}
				if (iCorrection == 3)
				{
					tTextureLeft = tLeftOff;
					tTextureRight = tRightOff;
				}
			}
			else
			{
				tTextureLeft = tLeftOff;
				tTextureRight = tRightOff;
			}
		}

		void drawNGSWindow(int windowID)
		{
			GUILayout.Box(sTargetSiteName, GUILayout.Height(20));
			GUILayout.Box(fRangeToTarget + " m", GUILayout.Height(20));

			GUILayout.BeginHorizontal();
				GUILayout.Box(tTextureLeft, GUILayout.Height(20));
				GUILayout.Box(tTextureMiddle, GUILayout.Height(20));
				GUILayout.Box(tTextureRight, GUILayout.Height(20));			
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Box("A1 vCraft/vSite: " + angle1);
				GUILayout.Box("A2 vHeading/000: " + angle2);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Box("A3 vCraft/vSite (0z): " + angle3);
				GUILayout.Box("A4 vHeading/000 (0z): " + angle4);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("VECTORS ");
				GUILayout.Box("Craft: " + vCrft.x + ", " + vCrft.y + ", " + vCrft.z);
				GUILayout.Box("Base: " + vSPos.x + ", " + vSPos.y + ", " + vSPos.z);
				GUILayout.Box("Head: " + vHead.x + ", " + vHead.y + ", " + vHead.z);
			GUILayout.EndHorizontal();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));

			prepNGS();
		}

		public int GetCourseCorrection(Vector3 vCraft, Vector3 vSite, Vector3 vHeading)
		{
			angle1 = Vector3.Angle(vCraft, vSite);
			angle2 = Vector3.Angle(vHeading, new Vector3(0, 1, 0));

			vCraft.z = 0;
			vSite.z = 0;

			angle3 = Vector3.Angle(vCraft, vSite);
			angle4 = Vector3.Angle(vHeading, new Vector3(0, 1, 0));
			float angle = angle3 - angle4;
			if (angle < -90) angle += 180;
			else if (angle > 90) angle -= 180;
			
			if (angle > 5)
				return 2;
			if (angle < -5)
				return 1;

			return 3;
		}

		void drawFacilityManagerWindow(int windowID)
		{
			string sFacilityName = (string)selectedObject.model.getSetting("title");
			string sFacilityRole = (string)selectedObject.model.getSetting("FacilityRole");

			float fStaffMax = (float)selectedObject.model.getSetting("StaffMax");
			float fStaffCurrent = (float)selectedObject.getSetting("StaffCurrent");

			GUILayout.Box(sFacilityName);

			if (fStaffMax > 0)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label("Max Billeted");
					GUI.enabled = false;
					GUILayout.TextField(string.Format("{0}", fStaffMax), GUILayout.Width(20));
					GUI.enabled = true;
					GUILayout.Space(20);
					GUILayout.Label("Current");
					GUI.enabled = true;

					for (int i = 1; i <= fStaffCurrent; i++)
					{
						GUILayout.Label(tBilleted, GUILayout.Height(32), GUILayout.Width(23));
					}
				GUILayout.EndHorizontal();

				double dHiring = KerbalKonstructs.instance.staffHireCost;
				double dRepMultiplier = KerbalKonstructs.instance.staffRepRequirementMultiplier;

				GUILayout.Label("To hire a kerbal costs " + dHiring + " Funds and requires Rep equal to the current number of staff x " + dRepMultiplier + ". Firing a kerbal costs nothing.");

				GUILayout.BeginHorizontal();
					GUI.enabled = (fStaffCurrent < fStaffMax);
					if (GUILayout.Button("Hire 1", GUILayout.Width(120)))
					{
						double dFunds = Funding.Instance.Funds;
						double dRep = Reputation.Instance.reputation;

						if (dFunds < dHiring)
						{
							ScreenMessages.PostScreenMessage("Insufficient funds to hire more staff!", 10, 0);
						}
						else
							if (dRep < (fStaffCurrent*dRepMultiplier))
							{
								ScreenMessages.PostScreenMessage("Insufficient rep to hire more staff!", 10, 0);
							}
							else
							{
								selectedObject.setSetting("StaffCurrent", fStaffCurrent + 1);
								Funding.Instance.AddFunds(-dHiring, TransactionReasons.Cheating);
							}
					}
					GUI.enabled = true;
					GUILayout.FlexibleSpace();
					GUI.enabled = (fStaffCurrent > 0);
					if (GUILayout.Button("Fire 1", GUILayout.Width(120)))
					{
						selectedObject.setSetting("StaffCurrent", fStaffCurrent - 1);
					}
					GUI.enabled = true;
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(10);
			if (GUILayout.Button("Save and Close"))
			{
				managingFacility = false;
				KerbalKonstructs.instance.saveObjects();
			}
			if (GUILayout.Button("Cancel"))
			{
				managingFacility = false;
			}
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		void drawBaseManagerWindow(int windowID)
		{
			string Base;
			float Range;
			LaunchSite lNearest;
			LaunchSite lBase;

			GUILayout.BeginArea(new Rect(10, 30, 380, 380));
				GUILayout.Space(3);
				GUILayout.Box("Settings");

				GUILayout.BeginHorizontal();
					KerbalKonstructs.instance.enableATC = GUILayout.Toggle(KerbalKonstructs.instance.enableATC, "Enable ATC", GUILayout.Width(175));
					KerbalKonstructs.instance.enableNGS = GUILayout.Toggle(KerbalKonstructs.instance.enableNGS, "Enable NGS", GUILayout.Width(175));
					onNGS = (KerbalKonstructs.instance.enableNGS);
				GUILayout.EndHorizontal();

				GUILayout.Box("Base");

				if (isCareerGame())
				{
					GUILayout.BeginHorizontal();
						GUILayout.Label("Nearest Open Base: ", GUILayout.Width(100));
						LaunchSiteManager.getNearestOpenBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range, out lNearest);
						GUILayout.Label(Base + " at ", GUILayout.Width(130));
						GUI.enabled = false;
						GUILayout.TextField(" " + Range + " ", GUILayout.Width(75));
						GUI.enabled = true;
						GUILayout.Label("m");
						if (KerbalKonstructs.instance.enableNGS)
						{
							if (GUILayout.Button("NGS",GUILayout.Height(21)))
							{
								lTargetSite = lNearest;
							}
						}
					GUILayout.EndHorizontal();

					GUILayout.Space(2);
				}

				GUILayout.BeginHorizontal();
					GUILayout.Label("Nearest Base: ", GUILayout.Width(100));
					LaunchSiteManager.getNearestBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range, out lBase);
					GUILayout.Label(Base + " at ", GUILayout.Width(130));
					GUI.enabled = false;
					GUILayout.TextField(" " + Range + " ", GUILayout.Width(75));
					GUI.enabled = true;
					GUILayout.Label("m");
					if (KerbalKonstructs.instance.enableNGS)
					{
						if (GUILayout.Button("NGS", GUILayout.Height(21)))
						{
							lTargetSite = lBase;
						}
					}
				GUILayout.EndHorizontal();

				if (isCareerGame())
				{
					if (Range < 2000)
					{
						string sClosed;
						float fOpenCost;
						bool bLanded = (FlightGlobals.ActiveVessel.Landed);
						LaunchSiteManager.getSiteOpenCloseState(Base, out sClosed, out fOpenCost);
						fOpenCost = fOpenCost / 2f;

						if (bLanded && sClosed == "Closed")
						{
							if (GUILayout.Button("Open Base for " + fOpenCost + " Funds"))
							{
								double currentfunds = Funding.Instance.Funds;

								if (fOpenCost > currentfunds)
								{
									ScreenMessages.PostScreenMessage("Insufficient funds to open this site!", 10, 0);
								}
								else
								{
									// Charge some funds
									Funding.Instance.AddFunds(-fOpenCost, TransactionReasons.Cheating);

									// Open the site - save to instance
									LaunchSiteManager.setSiteOpenCloseState(Base, "Open");
								}
							}
						}

						if (bLanded && sClosed == "Open")
						{
							GUI.enabled = false;
							GUILayout.Button("Base is Open");
							GUI.enabled = true;
						}

						GUILayout.Space(2);
					}
				}
										
				GUILayout.Box("Facilities");

				scrollPos = GUILayout.BeginScrollView(scrollPos);
					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						bool isLocal = true;
						if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
						{
							var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
							isLocal = dist < 2000f;
						}
						else
							isLocal = false;

						if (isLocal)
						{
							if (GUILayout.Button((string)obj.model.getSetting("title")))
							{
								KerbalKonstructs.instance.selectObject(obj, false);
								managingFacility = true;
							}
						}
					}
				GUILayout.EndScrollView();

				GUILayout.Space(5);
			GUILayout.EndArea();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		string savedxpos = "";
		string savedypos = "";
		string savedzpos = "";
		string savedalt = "";
		string savedrot = "";
		bool savedpos = false;
		bool pospasted = false;

		void drawToolWindow(int windowID)
		{
			Vector3 position = Vector3.zero;
			float alt = 0;
			float newRot = 0;
			float vis = 0;
			bool shouldUpdateSelection = false;
			bool manuallySet = false;

			//GUILayout.BeginArea(new Rect(10, 25, 275, 310));
			GUILayout.Box((string)selectedObject.model.getSetting("title"));
			GUILayout.Label("Hit enter after typing a value to apply.");

				GUILayout.BeginHorizontal();
					GUILayout.Label("Position   ");
					GUILayout.Space(15);
					if (GUILayout.Button(tCopyPos, GUILayout.Width(23), GUILayout.Height(23)))
					{
						savedpos = true;
						savedxpos = xPos;
						savedypos = yPos;
						savedzpos = zPos;
						savedalt = altitude;
						savedrot = rotation;
						Debug.Log("KK: Instance position copied");
					}
					if (GUILayout.Button(tPastePos, GUILayout.Width(23), GUILayout.Height(23)))
					{
						if (savedpos)
						{
							pospasted = true;
							xPos = savedxpos;
							yPos = savedypos;
							zPos = savedzpos;
							altitude = savedalt;
							rotation = savedrot;
							Debug.Log("KK: Instance position pasted");
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.Label("Increment");
					increment = GUILayout.TextField(increment, 5, GUILayout.Width(50));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("X:");
					GUILayout.FlexibleSpace();
					xPos = GUILayout.TextField(xPos, 25, GUILayout.Width(80));
					GUI.enabled = true;
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
					{
						position.x -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
					{
						position.x += float.Parse(increment);
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Y:");
					GUILayout.FlexibleSpace();
					yPos = GUILayout.TextField(yPos, 25, GUILayout.Width(80));
					GUI.enabled = true;
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
					{
						position.y -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
					{
						position.y += float.Parse(increment);
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Z:");
					GUILayout.FlexibleSpace();
					zPos = GUILayout.TextField(zPos, 25, GUILayout.Width(80));
					GUI.enabled = true;
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
					{
						position.z -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
					{
						position.z += float.Parse(increment);
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Alt.");
					GUILayout.FlexibleSpace();
					altitude = GUILayout.TextField(altitude, 25, GUILayout.Width(80));
					GUI.enabled = true;
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(21)))
					{
						alt -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(21)) || GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(21)))
					{
						alt += float.Parse(increment);
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				var pqsc = ((CelestialBody)selectedObject.getSetting("CelestialBody")).pqsController;
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Snap to Terrain", GUILayout.Width(130), GUILayout.Height(21)))
					{
						alt = 1.0f + ((float)(pqsc.GetSurfaceHeight((Vector3)selectedObject.getSetting("RadialPosition")) - pqsc.radius - (float)selectedObject.getSetting("RadiusOffset")));
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Rot.");
					GUILayout.FlexibleSpace();
					rotation = GUILayout.TextField(rotation, 4, GUILayout.Width(80));

					// ASH Added very handy rotation buttons
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30), GUILayout.Height(23)))
					{
						newRot -= 1.0f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(23)))
					{
						newRot -= float.Parse(increment) / 10f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(23)))
					{
						newRot += float.Parse(increment) / 10f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.RepeatButton(">>", GUILayout.Width(30), GUILayout.Height(23)))
					{
						newRot += 1.0f;
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				//GUILayout.Space(5);

				GUILayout.BeginHorizontal();
					GUILayout.Label("Vis.");
					GUILayout.FlexibleSpace();
					visrange = GUILayout.TextField(visrange, 6, GUILayout.Width(80));
					// GUILayout.Label("m");
					if (GUILayout.Button("Min", GUILayout.Width(30), GUILayout.Height(23)))
					{
						vis -= 100000f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button("-", GUILayout.Width(30), GUILayout.Height(23)))
					{
						vis -= 2500f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(23)))
					{
						vis += 2500f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button("Max", GUILayout.Width(30), GUILayout.Height(23)))
					{
						vis += 100000f;
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				//GUILayout.Space(2);
				
				GUILayout.BeginHorizontal();
					enableColliders = GUILayout.Toggle(enableColliders, "Enable Colliders", GUILayout.Width(140));

					Transform[] gameObjectList = selectedObject.gameObject.GetComponentsInChildren<Transform>();
					List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();

					if (enableColliders)
					{
						foreach (GameObject collider in colliderList)
						{
							collider.collider.enabled = true;
						}
					}
					if (!enableColliders)
					{
						foreach (GameObject collider in colliderList)
						{
							collider.collider.enabled = false;
						}
					}

					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Duplicate", GUILayout.Width(130)))
					{
						KerbalKonstructs.instance.saveObjects();
						StaticModel oModel = selectedObject.model;
						float fOffset = (float)selectedObject.getSetting("RadiusOffset");
						Vector3 vPosition = (Vector3)selectedObject.getSetting("RadialPosition");
						float fAngle = (float)selectedObject.getSetting("RotationAngle");
						KerbalKonstructs.instance.deselectObject();
						spawnInstance(oModel, fOffset, vPosition, fAngle);
					}
				GUILayout.EndHorizontal();
			
				//GUILayout.Space(5);

				GUI.enabled = !editingSite;

				string sLaunchPadTransform = (string)selectedObject.getSetting("LaunchPadTransform");
				string sDefaultPadTransform = (string)selectedObject.model.getSetting("DefaultLaunchPadTransform");
				string sLaunchsiteDesc = (string)selectedObject.getSetting("LaunchSiteDescription");
				string sModelDesc = (string)selectedObject.model.getSetting("description");

				if (sLaunchPadTransform == "" && sDefaultPadTransform == "")
					GUI.enabled = false;

				if (GUILayout.Button(((selectedObject.settings.ContainsKey("LaunchSiteName")) ? "Edit" : "Make") + " Launchsite"))
				{
					// Edit or make a launchsite
					siteName = (string)selectedObject.getSetting("LaunchSiteName");
					siteTrans = (selectedObject.settings.ContainsKey("LaunchPadTransform")) ? sLaunchPadTransform : sDefaultPadTransform;
						
					if (sLaunchsiteDesc != "")
						siteDesc = sLaunchsiteDesc;
					else
						siteDesc = sModelDesc;

					siteCategory = (string)selectedObject.getSetting("Category");
					siteType = (SiteType)selectedObject.getSetting("LaunchSiteType");
					flOpenCost = (float)selectedObject.getSetting("OpenCost");
					flCloseValue = (float)selectedObject.getSetting("CloseValue");
					stOpenCost = string.Format("{0}", flOpenCost);
					stCloseValue = string.Format("{0}", flCloseValue);
					siteAuthor = (selectedObject.settings.ContainsKey("author")) ? (string)selectedObject.getSetting("author") : (string)selectedObject.model.getSetting("author");
					Debug.Log("KK: Making or editing a launchsite");
					editingSite = true;
				}
					
				GUI.enabled = true;

				GUILayout.BeginHorizontal();
					if (GUILayout.Button("Save", GUILayout.Width(130)))
						KerbalKonstructs.instance.saveObjects();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Deselect", GUILayout.Width(130)))
					{
						// ASH Auto-save on deselect
						KerbalKonstructs.instance.saveObjects();
						KerbalKonstructs.instance.deselectObject();
					}
				GUILayout.EndHorizontal();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Delete Instance", GUILayout.Height(23)))
				{
					KerbalKonstructs.instance.deleteObject(selectedObject);
				}

				if (Event.current.keyCode == KeyCode.Return || (pospasted))
				{
					pospasted = false;
					manuallySet = true;
					position.x = float.Parse(xPos);
					position.y = float.Parse(yPos);
					position.z = float.Parse(zPos);
					vis = float.Parse(visrange);
					alt = float.Parse(altitude);
					float rot = float.Parse(rotation);
					while (rot > 360 || rot < 0)
					{
						if (rot > 360)
						{
							rot -= 360;
						}
						else if (rot < 0)
						{
							rot += 360;
						}
					}
					newRot = rot;
					rotation = rot.ToString();
					shouldUpdateSelection = true;
				}

				if (shouldUpdateSelection)
				{
					if (!manuallySet)
					{
						position += (Vector3)selectedObject.getSetting("RadialPosition");						
						alt += (float)selectedObject.getSetting("RadiusOffset");
						newRot += (float)selectedObject.getSetting("RotationAngle");
						vis += (float)selectedObject.getSetting("VisibilityRange");

						while (newRot > 360 || newRot < 0)
						{ 
							if (newRot > 360)
							{
								newRot -= 360;
							}
							else if (newRot < 0)
							{
								newRot += 360;
							}
						}

						while (vis > 100000 || vis < 1000)
						{
							if (vis > 100000)
							{
								vis = 100000;
							}
							else if (vis < 1000)
							{
								vis = 1000;
							}
						}
					}

					selectedObject.setSetting("RadialPosition", position);
					selectedObject.setSetting("RadiusOffset", alt);
					selectedObject.setSetting("RotationAngle", newRot);
					selectedObject.setSetting("VisibilityRange", vis);
					updateSelection(selectedObject);
				}

			// GUILayout.EndArea();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public StaticObject spawnInstance(StaticModel model)
		{
			StaticObject obj = new StaticObject();
			obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));
			obj.setSetting("RadiusOffset", (float)FlightGlobals.ActiveVessel.altitude);
			obj.setSetting("CelestialBody", KerbalKonstructs.instance.getCurrentBody());
			obj.setSetting("Group", "Ungrouped");
			obj.setSetting("RadialPosition", KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position));
			obj.setSetting("RotationAngle", 0f);
			obj.setSetting("Orientation", Vector3.up);
			obj.setSetting("VisibilityRange", 25000f);
			obj.model = model;

			KerbalKonstructs.instance.getStaticDB().addStatic(obj);
			enableColliders = false;
			KerbalKonstructs.instance.spawnObject(obj, true);
			return obj;
		}

		public StaticObject spawnInstance(StaticModel model, float fOffset, Vector3 vPosition, float fAngle)
		{
			StaticObject obj = new StaticObject();
			obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));
			obj.setSetting("RadiusOffset", fOffset);
			obj.setSetting("CelestialBody", KerbalKonstructs.instance.getCurrentBody());
			obj.setSetting("Group", "Ungrouped");
			obj.setSetting("RadialPosition", vPosition);
			obj.setSetting("RotationAngle", fAngle);
			obj.setSetting("Orientation", Vector3.up);
			obj.setSetting("VisibilityRange", 25000f);
			obj.model = model;

			KerbalKonstructs.instance.getStaticDB().addStatic(obj);
			enableColliders = false;
			KerbalKonstructs.instance.spawnObject(obj, true);
			return obj;
		}

		void drawEditorWindow(int id)
		{
			// ASH 07112014 Layout changes
			GUILayout.BeginArea(new Rect(10, 25, 500, 485));
			GUILayout.BeginHorizontal();
				GUI.enabled = !creating;
				if (GUILayout.Button("Spawn New", GUILayout.Width(115)))
				{
					creating = true;
					showLocal = false;
				}
				GUILayout.Space(10);
				GUI.enabled = creating || showLocal;
				if (GUILayout.Button("All Instances", GUILayout.Width(108)))
				{
					creating = false;
					showLocal = false;
				}
				GUI.enabled = true;
				GUILayout.Space(2);
				GUI.enabled = creating || !showLocal;
				if (GUILayout.Button("Local Instances", GUILayout.Width(108)))
				{
					creating = false;
					showLocal = true;
				}
				GUI.enabled = true;
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Save Objects", GUILayout.Width(115)))
					KerbalKonstructs.instance.saveObjects();
			GUILayout.EndHorizontal();

			scrollPos = GUILayout.BeginScrollView(scrollPos);
				if (creating)
				{
					foreach (StaticModel model in KerbalKonstructs.instance.getStaticDB().getModels())
					{
						if (GUILayout.Button(model.getSetting("title") + " : " + model.getSetting("mesh")))
						{
							spawnInstance(model);
						}
					}
				}

				if (!creating)
				{
					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						bool isLocal = true;

						if (showLocal)
						{
							if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
							{
								var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
								isLocal = dist < 10000f;
							}
							else
								isLocal = false;
						}
							
						if (isLocal)
						{
							if (GUILayout.Button("[" + obj.getSetting("Group") + "] " + (obj.settings.ContainsKey("LaunchSiteName") ? obj.getSetting("LaunchSiteName") + " : " + obj.model.getSetting("title") : obj.model.getSetting("title"))))
							{
								enableColliders = true;
								KerbalKonstructs.instance.selectObject(obj, false);
							}
						}
					}
				}
			GUILayout.EndScrollView();
			GUI.enabled = true;
				
			// Set locals to group function
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Group:");
				GUILayout.Space(5);
				GUI.enabled = showLocal;
				customgroup = GUILayout.TextField(customgroup, 25, GUILayout.Width(150));
				GUI.enabled = true;
				GUILayout.Space(5);
				GUI.enabled = showLocal;
				if (GUILayout.Button("Set as Group", GUILayout.Width(115)))
				{
					setLocalsGroup(customgroup);
				}
				GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.EndArea();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		// Set locals to group function
		void setLocalsGroup(string sGroup)
		{
			if (sGroup == "")
				return;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if (obj.pqsCity.sphere == FlightGlobals.currentMainBody.pqsController)
				{
					var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
					if (dist < 10000f)
					{
						KerbalKonstructs.instance.getStaticDB().changeGroup(obj, sGroup);
					}
				}
			}
		}

		string stOpenCost;
		string stCloseValue;

		void drawSiteEditorWindow(int id)
		{
			GUILayout.Box((string)selectedObject.model.getSetting("title"));
			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Name: ", GUILayout.Width(120));
				siteName = GUILayout.TextField(siteName);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Category: ", GUILayout.Width(120));
				GUILayout.Label(siteCategory, GUILayout.Width(80));
				GUILayout.FlexibleSpace();
				GUI.enabled = !(siteCategory == "RocketPad");
				if (GUILayout.Button("RP"))
					siteCategory = "RocketPad";
				GUI.enabled = !(siteCategory == "Runway");
				if (GUILayout.Button("RW"))
					siteCategory = "Runway";
				GUI.enabled = !(siteCategory == "Helipad");
				if (GUILayout.Button("HP"))
					siteCategory = "Helipad";
				GUI.enabled = !(siteCategory == "Other");
				if (GUILayout.Button("OT"))
					siteCategory = "Other";
			GUILayout.EndHorizontal();

			GUI.enabled = true;

			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Type: ", GUILayout.Width(120));
				if (siteType == (SiteType)0)
					GUILayout.Label("VAB", GUILayout.Width(40));
				if (siteType == (SiteType)1)
					GUILayout.Label("SPH", GUILayout.Width(40));
				if (siteType == (SiteType)2)
					GUILayout.Label("Any", GUILayout.Width(40));
				GUILayout.FlexibleSpace();
				GUI.enabled = !(siteType == (SiteType)0);
				if (GUILayout.Button("VAB"))
					siteType = ((SiteType)0);
				GUI.enabled = !(siteType == (SiteType)1);
				if (GUILayout.Button("SPH"))
					siteType = ((SiteType)1);
				GUI.enabled = !(siteType == (SiteType)2);
				if (GUILayout.Button("Any"))
					siteType = ((SiteType)2);
			GUILayout.EndHorizontal();

			GUI.enabled = true;
			
			GUILayout.BeginHorizontal();
				GUILayout.Label("Author: ", GUILayout.Width(120));
				siteAuthor = GUILayout.TextField(siteAuthor);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Open Cost: ", GUILayout.Width(120));
				stOpenCost = GUILayout.TextField(stOpenCost);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Close Value: ", GUILayout.Width(120));
				stCloseValue = GUILayout.TextField(stCloseValue);
			GUILayout.EndHorizontal();

			GUILayout.Label("Description: ");
			descScroll = GUILayout.BeginScrollView(descScroll);
				siteDesc = GUILayout.TextArea(siteDesc, GUILayout.ExpandHeight(true));
			GUILayout.EndScrollView();

			GUI.enabled = true;
			GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save", GUILayout.Width(115)))
				{
					Boolean addToDB = (selectedObject.settings.ContainsKey("LaunchSiteName") && siteName != "");
					selectedObject.setSetting("LaunchSiteName", siteName);
					selectedObject.setSetting("LaunchSiteType", siteType);
					selectedObject.setSetting("LaunchPadTransform", siteTrans);
					selectedObject.setSetting("LaunchSiteDescription", siteDesc);
					selectedObject.setSetting("OpenCost", float.Parse(stOpenCost));
					selectedObject.setSetting("CloseValue", float.Parse(stCloseValue));
					selectedObject.setSetting("OpenCloseState", "Open");
					selectedObject.setSetting("Category", siteCategory);
					if (siteAuthor != (string)selectedObject.model.getSetting("author"))
						selectedObject.setSetting("LaunchSiteAuthor", siteAuthor);
					
					if(addToDB)
					{
						LaunchSiteManager.createLaunchSite(selectedObject);
					}
					KerbalKonstructs.instance.saveObjects();
					
					List<LaunchSite> basesites = LaunchSiteManager.getLaunchSites();
					PersistenceFile<LaunchSite>.SaveList(basesites, "LAUNCHSITES", "KK");
					editingSite = false;
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Cancel", GUILayout.Width(115)))
				{
					editingSite = false;
				}
			GUILayout.EndHorizontal();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void updateSelection(StaticObject obj)
		{
			selectedObject = obj;
			xPos = ((Vector3)obj.getSetting("RadialPosition")).x.ToString();
			yPos = ((Vector3)obj.getSetting("RadialPosition")).y.ToString();
			zPos = ((Vector3)obj.getSetting("RadialPosition")).z.ToString();
			altitude = ((float)obj.getSetting("RadiusOffset")).ToString();
			rotation = ((float)obj.getSetting("RotationAngle")).ToString();
			visrange = ((float)obj.getSetting("VisibilityRange")).ToString();
			selectedObject.update();
		}

		public float getIncrement()
		{
			return float.Parse(increment);
		}

		public SiteType getSiteType(int selection)
		{
			switch(selection)
			{
				case 0:
					return SiteType.VAB;
				case 1:
					return SiteType.SPH;
				default:
					return SiteType.Any;
			}
		}
	}
}
