using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using LibNoise.Unity.Operator;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	class EditorGUI
	{
		StaticObject selectedObject;
		private Boolean editingSite = false;
		private String xPos, yPos, zPos, altitude, rotation, customgroup = "";
		private String increment = "1";

		public EditorGUI()
		{
			listStyle.normal.textColor = Color.white;
			listStyle.onHover.background =
			listStyle.hover.background = new Texture2D(2, 2);
			listStyle.padding.left =
			listStyle.padding.right =
			listStyle.padding.top =
			listStyle.padding.bottom = 4;

			orientationMenu = new ComboBox(orientationOptions[0], orientationOptions, "button", "box", setOrientation, listStyle);
			siteTypeMenu = new ComboBox(siteTypeOptions[0], siteTypeOptions, "button", "box", null, listStyle);
		}

		public void drawEditor(StaticObject obj)
		{
			if (obj != null)
			{
				if (selectedObject != obj)
					updateSelection(obj);

				//It wanted a unique ID number ¯\_(ツ)_/¯
				toolRect = GUI.Window(0xB00B1E5, toolRect, drawToolWindow, "KK Instance Editor");

				if(editingSite)
					siteEditorRect = GUI.Window(0xB00B1E8, siteEditorRect, drawSiteEditorWindow, "KK Launchsite Editor");
			}
			editorRect = GUI.Window(0xB00B1E7, editorRect, drawEditorWindow, "Kerbal Konstructs Statics Editor");
		}

		Rect toolRect = new Rect(150, 25, 300, 325);
		Rect editorRect = new Rect(10, 25, 520, 520);
		Rect siteEditorRect = new Rect(400, 50, 330, 350);

		private GUIStyle listStyle = new GUIStyle();
		private GUIContent[] orientationOptions = {
										new GUIContent("Up"),
										new GUIContent("Down"),
										new GUIContent("Left"),
										new GUIContent("Right"),
										new GUIContent("Forward"),
										new GUIContent("Back")
									};
		ComboBox orientationMenu;

		void drawToolWindow(int windowID)
		{
			Vector3 position = Vector3.zero;
			float alt = 0;
			float newRot = 0;
			bool shouldUpdateSelection = false;
			bool manuallySet = false;

			GUILayout.BeginArea(new Rect(10, 25, 275, 300));
				GUILayout.BeginHorizontal();
					GUILayout.Label("Position");
					GUILayout.FlexibleSpace();
					GUILayout.Label("Increment");
					increment = GUILayout.TextField(increment, 4, GUILayout.Width(50));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("X:");
					GUILayout.FlexibleSpace();
					xPos = GUILayout.TextField(xPos, 25, GUILayout.Width(70));
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
					yPos = GUILayout.TextField(yPos, 25, GUILayout.Width(70));
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
					zPos = GUILayout.TextField(zPos, 25, GUILayout.Width(70));
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
					altitude = GUILayout.TextField(altitude, 25, GUILayout.Width(70));
					// 
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
					//GUILayout.Label("Orientation");
					//disable anything beneath the dropdown to prevent clicking through
					// GUI.enabled = !orientationMenu.isClickedComboButton;
					GUILayout.Label("Rot.");
					GUILayout.FlexibleSpace();
					rotation = GUILayout.TextField(rotation, 4, GUILayout.Width(70));
					GUI.enabled = false;
					if (GUILayout.RepeatButton("<<", GUILayout.Width(30)) || GUILayout.Button("<", GUILayout.Width(30)))
					{
					}
					if (GUILayout.Button(">", GUILayout.Width(30)) || GUILayout.RepeatButton(">>", GUILayout.Width(30)))
					{
					}
					GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					if (GUILayout.Button("Deselect", GUILayout.Width(130)))
					{
						KerbalKonstructs.instance.deselectObject();
					}
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Delete", GUILayout.Width(130)))
					{
						KerbalKonstructs.instance.deleteObject(selectedObject);
					}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					if (GUILayout.Button("Snap to Surface", GUILayout.Width(130)))
					{
						var pqsc = ((CelestialBody)selectedObject.getSetting("CelestialBody")).pqsController;
						alt = (float)(pqsc.GetSurfaceHeight((Vector3)selectedObject.getSetting("RadialPosition")) - pqsc.radius - (float)selectedObject.getSetting("RadiusOffset"));
						shouldUpdateSelection = true;
					}
					GUILayout.FlexibleSpace();
					GUI.enabled = !editingSite;
					if (GUILayout.Button(((selectedObject.settings.ContainsKey("LaunchSiteName")) ? "Edit" : "Make") + " Launchsite", GUILayout.Width(130)))
					{
						siteName = (string)selectedObject.getSetting("LaunchSiteName");
						siteTrans = (selectedObject.settings.ContainsKey("LaunchPadTransform")) ? (string)selectedObject.getSetting("LaunchPadTransform") : (string)selectedObject.model.getSetting("DefaultLaunchPadTransform");
						siteDesc = (string)selectedObject.getSetting("LaunchSiteDescription");
						siteType = (SiteType) selectedObject.getSetting("LaunchSiteType");
						siteTypeMenu.SelectedItemIndex = (int)siteType;
						siteLogo = ((string) selectedObject.getSetting("LaunchSiteLogo"));//.Replace(selectedObject.model.path + "/", "");
						siteAuthor = (selectedObject.settings.ContainsKey("author")) ? (string)selectedObject.getSetting("author") : (string)selectedObject.model.getSetting("author");
						editingSite = true;
					}				
					GUI.enabled = true;
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
					}

					selectedObject.setSetting("RadialPosition", position);
					selectedObject.setSetting("RadiusOffset", alt);
					selectedObject.setSetting("RotationAngle", newRot);
					updateSelection(selectedObject);
				}

				//Draw last so it properly overlaps
				//orientationMenu.Show(new Rect(235, 50, 80, 25));

			GUILayout.EndArea();

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		Vector2 scrollPos;
		Boolean creating = false;
		Boolean showLocal = false;

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
				//GUILayout.EndArea();
				//GUILayout.BeginArea(new Rect(135, 25, 540, 365));
			
				scrollPos = GUILayout.BeginScrollView(scrollPos);
					if (creating)
					{
						foreach (StaticModel model in KerbalKonstructs.instance.getStaticDB().getModels())
						{
							// ASH 07112014 Removed redundant info from the path.
							// String[] modelpaths = model.path.Split('/');
							// String firstpath = modelpaths.Length > 0 ? modelpaths[0] : model.path;

							if (GUILayout.Button(model.getSetting("title") + " : " + model.getSetting("mesh")))
							// if (GUILayout.Button(model.getSetting("mesh") + " [" + firstpath + "]"))
							{						
								StaticObject obj = new StaticObject();
								obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.getSetting("mesh"));
								obj.setSetting("RadiusOffset", (float) FlightGlobals.ActiveVessel.altitude);
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
					else
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
							}
							
							if (isLocal)
							{
								// ASH 07112014 Removed redundant info from the path. Only need to know the content mod really
								// String[] modelpaths = obj.model.path.Split('/');
								// String firstpath = modelpaths.Length > 0 ? modelpaths[0] : obj.model.path;

								// ASH 08112014 No point in disabling the button
								// GUI.enabled = obj != selectedObject;
								if (GUILayout.Button("[" + obj.getSetting("Group") + "] " + (obj.settings.ContainsKey("LaunchSiteName") ? obj.getSetting("LaunchSiteName") + " : " + obj.model.getSetting("title") : obj.model.getSetting("title"))))
								{
									//TODO: Move PQS target to object position
									KerbalKonstructs.instance.selectObject(obj);
								}
							}
						}
						GUI.enabled = true;
					}
				GUILayout.EndScrollView();
				
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

		void setLocalsGroup(string sGroup)
		{
			if (sGroup == "")
				return;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				var dist = Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, obj.gameObject.transform.position);
				if (dist < 10000f)
				{
					KerbalKonstructs.instance.getStaticDB().changeGroup(obj, sGroup);
				}					
			}
		}

		string siteName, siteTrans, siteDesc, siteAuthor, siteLogo;
		SiteType siteType;
		Vector2 descScroll;

		private GUIContent[] siteTypeOptions = {
										new GUIContent("VAB"),
										new GUIContent("SPH"),
										new GUIContent("ANY")
									};
		ComboBox siteTypeMenu;

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
					selectedObject.setSetting("LaunchSiteType", getSiteType(siteTypeMenu.SelectedItemIndex));
					if(siteLogo != "")
						selectedObject.setSetting("LaunchSiteLogo", siteLogo);
					if (siteAuthor != (string)selectedObject.model.getSetting("author"))
						selectedObject.setSetting("LaunchSiteAuthor", siteAuthor);
					if(addToDB)
					{
						LaunchSiteManager.createLaunchSite(selectedObject);
					}
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
			orientationMenu.SelectedItemIndex = getOrientation((Vector3)obj.getSetting("Orientation"));
			selectedObject.update();
		}

		public void setOrientation(int selection)
		{
			if (selectedObject != null)
			{
				//TODO: do this with an array
				switch (selection)
				{
					case 0:
						selectedObject.setSetting("Orientation", Vector3.up);
						break;
					case 1:
						selectedObject.setSetting("Orientation", Vector3.down);
						break;
					case 2:
						selectedObject.setSetting("Orientation", Vector3.left);
						break;
					case 3:
						selectedObject.setSetting("Orientation", Vector3.right);
						break;
					case 4:
						selectedObject.setSetting("Orientation", Vector3.forward);
						break;
					case 5:
						selectedObject.setSetting("Orientation", Vector3.back);
						break;
				}
				selectedObject.update();
			}
		}

		public int getOrientation(Vector3 rot)
		{
			if(rot.Equals(Vector3.up))
			{
				return 0;
			}
			else if (rot.Equals(Vector3.down))
			{
				return 1;
			}
			else if (rot.Equals(Vector3.left))
			{
				return 2;
			}
			else if (rot.Equals(Vector3.right))
			{
				return 3;
			}
			else if (rot.Equals(Vector3.forward))
			{
				return 4;
			}
			else if (rot.Equals(Vector3.back))
			{
				return 5;
			}
			//If the static has a custom orientation then just display "up", I will add support for custom orientations in the future
			return 0;
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
