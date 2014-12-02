using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;

// R and T LOG
// 14112014 ASH

namespace KerbalKonstructs.UI
{
	class EditorGUI
	{
		StaticObject selectedObject;
		private String xPos, yPos, zPos, altitude, rotation, customgroup = "";
		private String increment = "1";

		public Texture tBilleted = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/billeted", false);

		Vector2 scrollPos;
		private Boolean editingSite = false;
		Boolean creating = false;
		Boolean showLocal = false;
		Boolean managingFacility = false;

		Rect toolRect = new Rect(150, 25, 300, 325);
		Rect editorRect = new Rect(10, 25, 520, 520);
		Rect siteEditorRect = new Rect(400, 50, 330, 350);
		Rect managerRect = new Rect(10, 25, 400, 405);
		Rect facilityRect = new Rect(150, 75, 350, 400);

		private GUIStyle listStyle = new GUIStyle();

		string siteName, siteTrans, siteDesc, siteAuthor, siteLogo;
		SiteType siteType;
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

		void drawFacilityManagerWindow(int windowID)
		{
			string sFacilityName = (string)selectedObject.model.getSetting("title");
			string sFacilityRole = (string)selectedObject.getSetting("FacilityRole");

			float fStaffMax = (float)selectedObject.getSetting("StaffMax");
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
					// GUI.enabled = false;
					// GUILayout.TextField(string.Format("{0}", fStaffCurrent), GUILayout.Width(20));
					GUI.enabled = true;
					// if (fStaffCurrent > 0)
					for (int i = 1; i <= fStaffCurrent; i++)
					{
						GUILayout.Label(tBilleted, GUILayout.Height(32), GUILayout.Width(23));
					}
				GUILayout.EndHorizontal();

				GUILayout.Label("To hire a kerbal costs 1000 Funds and requires Rep equal to the current number of staff x 50. Firing a kerbal costs nothing.");

				GUILayout.BeginHorizontal();
					GUI.enabled = (fStaffCurrent < fStaffMax);
					if (GUILayout.Button("Hire 1", GUILayout.Width(120)))
					{
						double dFunds = Funding.Instance.Funds;
						double dRep = Reputation.Instance.reputation;

						if (dFunds < 1000)
						{
							ScreenMessages.PostScreenMessage("Insufficient funds to hire more staff!", 10, 0);
						}
						else
							if (dRep < (fStaffCurrent*50))
							{
								ScreenMessages.PostScreenMessage("Insufficient rep to hire more staff!", 10, 0);
							}
							else
							{
								selectedObject.setSetting("StaffCurrent", fStaffCurrent + 1);
								Funding.Instance.AddFunds(-1000, TransactionReasons.Cheating);
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
		}

		void drawBaseManagerWindow(int windowID)
		{
			string Base;
			float Range;

			GUILayout.BeginArea(new Rect(10, 30, 380, 380));
				GUILayout.Space(3);
				GUILayout.Box("Settings");

				GUILayout.BeginHorizontal();
					KerbalKonstructs.instance.enableATC = GUILayout.Toggle(KerbalKonstructs.instance.enableATC, "Enable ATC");
				GUILayout.EndHorizontal();

				/* if(GUILayout.Button("Test Persistence"))
				{
					List<StaticObject> instancelist = new List<StaticObject>();
					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
						instancelist.Add(obj);
					}
					PersistenceFile<StaticObject>.SaveList(instancelist, "FACILITIES", "KKFacilities");						
				} */

				GUILayout.Box("Base");

				if (isCareerGame())
				{
					GUILayout.BeginHorizontal();
						GUILayout.Label("Nearest Open Base: ", GUILayout.Width(125));
						LaunchSiteManager.getNearestOpenBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range);
						GUILayout.Label(Base + " at ", GUILayout.Width(140));
						GUI.enabled = false;
						GUILayout.TextField(" " + Range + " ", GUILayout.Width(80));
						GUI.enabled = true;
						GUILayout.Label("m");
					GUILayout.EndHorizontal();

					GUILayout.Space(2);
				}

				GUILayout.BeginHorizontal();
					GUILayout.Label("Nearest Base: ", GUILayout.Width(125));
					LaunchSiteManager.getNearestBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Range);
					GUILayout.Label(Base + " at ", GUILayout.Width(140));
					GUI.enabled = false;
					GUILayout.TextField(" " + Range + " ", GUILayout.Width(80));
					GUI.enabled = true;
					GUILayout.Label("m");
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

		void drawToolWindow(int windowID)
		{
			Vector3 position = Vector3.zero;
			float alt = 0;
			float newRot = 0;
			bool shouldUpdateSelection = false;
			bool manuallySet = false;

			GUILayout.BeginArea(new Rect(10, 25, 275, 310));
				GUILayout.BeginHorizontal();
					GUILayout.Label("Position");
					GUILayout.FlexibleSpace();
					GUILayout.Label("Increment");
					increment = GUILayout.TextField(increment, 5, GUILayout.Width(50));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("X:");
					GUILayout.FlexibleSpace();
					xPos = GUILayout.TextField(xPos, 25, GUILayout.Width(80));
					GUI.enabled = true;
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30)) || GUILayout.Button("<", GUILayout.Width(30)))
					{
						position.x -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30)) || GUILayout.RepeatButton(">>", GUILayout.Width(30)))
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
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30)) || GUILayout.Button("<", GUILayout.Width(30)))
					{
						position.y -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30)) || GUILayout.RepeatButton(">>", GUILayout.Width(30)))
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
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30)) || GUILayout.Button("<", GUILayout.Width(30)))
					{
						position.z -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30)) || GUILayout.RepeatButton(">>", GUILayout.Width(30)))
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
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30)) || GUILayout.Button("<", GUILayout.Width(30)))
					{
						alt -= float.Parse(increment);
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30)) || GUILayout.RepeatButton(">>", GUILayout.Width(30)))
					{
						alt += float.Parse(increment);
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Rot.");
					GUILayout.FlexibleSpace();
					rotation = GUILayout.TextField(rotation, 4, GUILayout.Width(80));

					// ASH Added very handy rotation buttons
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30)))
					{
						newRot -= 1.0f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button("<", GUILayout.Width(30)))
					{
						newRot -= float.Parse(increment) / 10f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.Button(">", GUILayout.Width(30)))
					{
						newRot += float.Parse(increment) / 10f;
						shouldUpdateSelection = true;
					}
					if (GUILayout.RepeatButton(">>", GUILayout.Width(30)))
					{
						newRot += 1.0f;
						shouldUpdateSelection = true;
					}
				GUILayout.EndHorizontal();
				
				//GUILayout.FlexibleSpace();
				GUILayout.Space(10);

				var pqsc = ((CelestialBody)selectedObject.getSetting("CelestialBody")).pqsController;

				GUILayout.BeginHorizontal();
					if (GUILayout.Button("Snap to Surface", GUILayout.Width(130)))
					{
						alt = 1.0f + ((float)(pqsc.GetSurfaceHeight((Vector3)selectedObject.getSetting("RadialPosition")) - pqsc.radius - (float)selectedObject.getSetting("RadiusOffset")));
						shouldUpdateSelection = true;
					}
					GUILayout.FlexibleSpace();

					GUI.enabled = !editingSite;

					if (GUILayout.Button(((selectedObject.settings.ContainsKey("LaunchSiteName")) ? "Edit" : "Make") + " Launchsite", GUILayout.Width(130)))
					{
						siteName = (string)selectedObject.getSetting("LaunchSiteName");
						siteTrans = (selectedObject.settings.ContainsKey("LaunchPadTransform")) ? (string)selectedObject.getSetting("LaunchPadTransform") : (string)selectedObject.model.getSetting("DefaultLaunchPadTransform");
						siteDesc = (string)selectedObject.getSetting("LaunchSiteDescription");
						siteType = (SiteType)selectedObject.getSetting("LaunchSiteType");
						siteTypeMenu.SelectedItemIndex = (int)siteType;
						siteLogo = ((string)selectedObject.getSetting("LaunchSiteLogo"));
						siteAuthor = (selectedObject.settings.ContainsKey("author")) ? (string)selectedObject.getSetting("author") : (string)selectedObject.model.getSetting("author");
						editingSite = true;
					}
					
					GUI.enabled = true;
				GUILayout.EndHorizontal();

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
				
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Delete", GUILayout.Width(80)))
					{
						KerbalKonstructs.instance.deleteObject(selectedObject);
					}
				GUILayout.EndHorizontal();

				if (Event.current.keyCode == KeyCode.Return)
				{
					manuallySet = true;
					position.x = float.Parse(xPos);
					position.y = float.Parse(yPos);
					position.z = float.Parse(zPos);
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

						// 10112014 ASH Handle new rotation button limits
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
					}

					selectedObject.setSetting("RadialPosition", position);
					selectedObject.setSetting("RadiusOffset", alt);
					selectedObject.setSetting("RotationAngle", newRot);
					updateSelection(selectedObject);
				}

			GUILayout.EndArea();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
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
						// ASH 07112014 Removed redundant info
						if (GUILayout.Button(model.getSetting("title") + " : " + model.getSetting("mesh")))
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
							KerbalKonstructs.instance.spawnObject(obj, true);
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
								// ASH Ooopsie. Bug fix where off-Kerbin instances were always considered local.
						}
							
						if (isLocal)
						{
							// ASH 07112014 Removed redundant info
							// ASH 08112014 No point in disabling the button								
							// GUI.enabled = obj != selectedObject;
							if (GUILayout.Button("[" + obj.getSetting("Group") + "] " + (obj.settings.ContainsKey("LaunchSiteName") ? obj.getSetting("LaunchSiteName") + " : " + obj.model.getSetting("title") : obj.model.getSetting("title"))))
							{
								// TODO Move PQS target to object position
								KerbalKonstructs.instance.selectObject(obj);
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

		void drawSiteEditorWindow(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Name: ");
				siteName = GUILayout.TextField(siteName);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Pad Transform: ");
				siteTrans = GUILayout.TextField(siteTrans);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Site Type:");
				GUILayout.FlexibleSpace();
				Rect rect = GUILayoutUtility.GetRect(siteTypeOptions[0], "button", GUILayout.Width(50));
			GUILayout.EndHorizontal();

			GUI.enabled = !siteTypeMenu.isClickedComboButton;
			GUILayout.BeginHorizontal();
				GUILayout.Label("Author: ");
				siteAuthor = GUILayout.TextField(siteAuthor);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Logo: ");
				siteLogo = GUILayout.TextField(siteLogo);
			GUILayout.EndHorizontal();

			GUILayout.Label("Site Description: ");
			descScroll = GUILayout.BeginScrollView(descScroll);
				siteDesc = GUILayout.TextArea(siteDesc, GUILayout.ExpandHeight(true));
			GUILayout.EndScrollView();

			GUI.enabled = true;
			GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save", GUILayout.Width(115)))
				{
					Boolean addToDB = (selectedObject.settings.ContainsKey("LaunchSiteName") && siteName != "");
					selectedObject.setSetting("LaunchSiteName", siteName);
					selectedObject.setSetting("LaunchPadTransform", siteTrans);
					selectedObject.setSetting("LaunchSiteDescription", siteDesc);
					selectedObject.setSetting("OpenCloseState", "Open");
					selectedObject.setSetting("LaunchSiteType", getSiteType(siteTypeMenu.SelectedItemIndex));
					if(siteLogo != "")
						selectedObject.setSetting("LaunchSiteLogo", siteLogo);
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

			siteTypeMenu.Show(rect);

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
