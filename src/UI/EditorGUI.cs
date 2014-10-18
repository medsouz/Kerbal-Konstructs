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
		private String xPos, yPos, zPos, altitude, rotation;
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
				toolRect = GUI.Window(0xB00B1E5, toolRect, drawToolWindow, "Kerbal Konstructs Editor Tools");

				if(editingSite)
					siteEditorRect = GUI.Window(0xB00B1E8, siteEditorRect, drawSiteEditorWindow, "Kerbal Konstruct Site Editor");
			}
			editorRect = GUI.Window(0xB00B1E7, editorRect, drawEditorWindow, "Kerbal Konstruct Editor");
		}

		Rect toolRect = new Rect(50, 50, 336, 250);
		Rect editorRect = new Rect(50, 350, 500, 295);
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

		//TODO: rewrite this to use magical GUILayout code
		//I wish I knew GUILayout was a thing when I made this :(
		void drawToolWindow(int windowID)
		{
			GUI.Label(new Rect(21, 30, 203, 25), "Position");
			GUI.Label(new Rect(6, 50, 25, 15), "X:");
			xPos = GUI.TextField(new Rect(85, 50, 80, 25), xPos, 25);
			Vector3 position = Vector3.zero;
			float alt = 0;
			float newRot = 0;
			bool shouldUpdateSelection = false;
			bool manuallySet = false;
			if (GUI.Button(new Rect(53, 50, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 50, 30, 25), "<<"))
			{
				position.x -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUI.Button(new Rect(167, 50, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 50, 30, 25), ">>"))
			{
				position.x += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUI.Label(new Rect(6, 80, 25, 15), "Y:");
			yPos = GUI.TextField(new Rect(85, 80, 80, 25), yPos, 25);
			if (GUI.Button(new Rect(53, 80, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 80, 30, 25), "<<"))
			{
				position.y -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUI.Button(new Rect(167, 80, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 80, 30, 25), ">>"))
			{
				position.y += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUI.Label(new Rect(6, 110, 25, 15), "Z:");
			zPos = GUI.TextField(new Rect(85, 110, 80, 25), zPos, 25);
			if (GUI.Button(new Rect(53, 110, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 110, 30, 25), "<<"))
			{
				position.z -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUI.Button(new Rect(167, 110, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 110, 30, 25), ">>"))
			{
				position.z += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUI.Label(new Rect(21, 140, 203, 25), "Altitude");

			altitude = GUI.TextField(new Rect(85, 160, 80, 25), altitude, 25);
			if (GUI.Button(new Rect(53, 160, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 160, 30, 25), "<<"))
			{
				alt -= float.Parse(increment);
				shouldUpdateSelection = true;
			}
			if (GUI.Button(new Rect(167, 160, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 160, 30, 25), ">>"))
			{
				alt += float.Parse(increment);
				shouldUpdateSelection = true;
			}
			GUI.Label(new Rect(235, 30, 80, 25), "Orientation");
			//disable anything beneath the dropdown to prevent clicking through
			GUI.enabled = !orientationMenu.isClickedComboButton;
			GUI.Label(new Rect(235, 80, 80, 25), "Increment");
			increment = GUI.TextField(new Rect(235, 100, 80, 25), increment, 25);
			GUI.Label(new Rect(235, 130, 80, 25), "Rotation");
			rotation = GUI.TextField(new Rect(235, 150, 80, 25), rotation, 25);
			GUI.enabled = true;

			if (GUI.Button(new Rect(21, 190, 80, 25), "Deselect"))
			{
				KerbalKonstructs.instance.deselectObject();
			}
			if (GUI.Button(new Rect(21, 220, 80, 25), "Delete"))
			{
				KerbalKonstructs.instance.deleteObject(selectedObject);
			}
			GUI.enabled = false;
			GUI.Button(new Rect(106, 190, 90, 25), "Save Local");
			GUI.Button(new Rect(106, 220, 90, 25), "Save Global");
			GUI.enabled = true;
			if (GUI.Button(new Rect(201, 190, 115, 25), "Snap to Surface"))
			{
				alt = (float)(((CelestialBody)selectedObject.getSetting("CelestialBody")).pqsController.GetSurfaceHeight((Vector3)selectedObject.getSetting("RadialPosition")) - ((CelestialBody)selectedObject.getSetting("CelestialBody")).pqsController.radius);
				shouldUpdateSelection = true;
			}
			GUI.enabled = !editingSite;
			if (GUI.Button(new Rect(201, 220, 115, 25), ((selectedObject.settings.ContainsKey("LaunchSiteName")) ? "Edit" : "Create") + " Launch Site"))
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
			orientationMenu.Show(new Rect(235, 50, 80, 25));
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		Vector2 scrollPos;
		Boolean creating = false;

		void drawEditorWindow(int id)
		{
			GUILayout.BeginArea(new Rect(10, 25, 240, 265));
				GUILayout.BeginHorizontal();
					GUI.enabled = !creating;
					if (GUILayout.Button("New Object", GUILayout.Width(115)))
						creating = true;
					GUILayout.Space(5);
					GUI.enabled = creating;
					if (GUILayout.Button("Existing Object", GUILayout.Width(115)))
						creating = false;
					GUI.enabled = true;
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Save Objects", GUILayout.Width(115)))
					KerbalKonstructs.instance.saveObjects();
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect(255, 25, 240, 265));
				scrollPos = GUILayout.BeginScrollView(scrollPos);
				if (creating)
				{
					foreach (StaticModel model in KerbalKonstructs.instance.getStaticDB().getModels())
					{
						if (GUILayout.Button(model.getSetting("mesh") + " [" + model.path + "]"))
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
						GUI.enabled = obj != selectedObject;
						if (GUILayout.Button(((obj.settings.ContainsKey("LaunchSiteName")) ? obj.getSetting("LaunchSiteName") + "(" + obj.model.getSetting("mesh") + ")" : obj.model.getSetting("mesh")) + " [" + obj.model.path + "]"))
						{
							//TODO: Move PQS target to object position
							KerbalKonstructs.instance.selectObject(obj);
						}
					}
					GUI.enabled = true;
				}
				GUILayout.EndScrollView();
			GUILayout.EndArea();
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
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
