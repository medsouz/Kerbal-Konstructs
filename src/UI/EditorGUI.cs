﻿using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
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
		Rect editorRect = new Rect(50, 100, 700, 400);
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

        // ASH I feel your pain. Want me to rewrite this?
		void drawToolWindow(int windowID)
		{
			GUI.Label(new Rect(21, 30, 203, 25), "Position");
			GUI.Label(new Rect(6, 50, 25, 15), "X:");
			xPos = GUI.TextField(new Rect(85, 50, 80, 25), xPos, 25);
			if (GUI.Button(new Rect(53, 50, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 50, 30, 25), "<<"))
			{
				selectedObject.position.x -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(167, 50, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 50, 30, 25), ">>"))
			{
				selectedObject.position.x += float.Parse(increment);
				updateSelection(selectedObject);
			}
			GUI.Label(new Rect(6, 80, 25, 15), "Y:");
			yPos = GUI.TextField(new Rect(85, 80, 80, 25), yPos, 25);
			if (GUI.Button(new Rect(53, 80, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 80, 30, 25), "<<"))
			{
				selectedObject.position.y -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(167, 80, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 80, 30, 25), ">>"))
			{
				selectedObject.position.y += float.Parse(increment);
				updateSelection(selectedObject);
			}
			GUI.Label(new Rect(6, 110, 25, 15), "Z:");
			zPos = GUI.TextField(new Rect(85, 110, 80, 25), zPos, 25);
			if (GUI.Button(new Rect(53, 110, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 110, 30, 25), "<<"))
			{
				selectedObject.position.z -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(167, 110, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 110, 30, 25), ">>"))
			{
				selectedObject.position.z += float.Parse(increment);
				updateSelection(selectedObject);
			}
			GUI.Label(new Rect(21, 140, 203, 25), "Altitude");

			altitude = GUI.TextField(new Rect(85, 160, 80, 25), altitude, 25);
			if (GUI.Button(new Rect(53, 160, 30, 25), "<") || GUI.RepeatButton(new Rect(21, 160, 30, 25), "<<"))
			{
				selectedObject.altitude -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(167, 160, 30, 25), ">") || GUI.RepeatButton(new Rect(199, 160, 30, 25), ">>"))
			{
				selectedObject.altitude += float.Parse(increment);
				updateSelection(selectedObject);
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
				selectedObject.altitude = (float)(selectedObject.parentBody.pqsController.GetSurfaceHeight(selectedObject.position) - selectedObject.parentBody.pqsController.radius);
				updateSelection(selectedObject);
			}
			GUI.enabled = !editingSite;
			if (GUI.Button(new Rect(201, 220, 115, 25), ((selectedObject.siteName != "") ? "Edit" : "Create") + " Launch Site"))
			{
				siteName = selectedObject.siteName;
				siteTrans = (selectedObject.siteTransform != "") ? selectedObject.siteTransform : selectedObject.model.defaultSiteTransform;
				siteDesc = selectedObject.siteDescription;
				siteType = selectedObject.siteType;
				siteTypeMenu.SelectedItemIndex = (int)siteType;
				siteLogo = selectedObject.siteLogo.Replace(selectedObject.model.path + "/", "");
				siteAuthor = (selectedObject.siteAuthor != "") ? selectedObject.siteAuthor : selectedObject.model.author;
				editingSite = true;
			}
				
			GUI.enabled = true;

			if (Event.current.keyCode == KeyCode.Return)
			{
				selectedObject.position.x = float.Parse(xPos);
				selectedObject.position.y = float.Parse(yPos);
				selectedObject.position.z = float.Parse(zPos);
				selectedObject.altitude = float.Parse(altitude);
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
				selectedObject.rotation = rot;
				rotation = rot.ToString();
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
            // ASH 15102014 Layout changes. medsouz, let me know if you like.
			GUILayout.BeginArea(new Rect(10, 25, 125, 365));
				//GUILayout.BeginHorizontal();
					GUI.enabled = !creating;
					if (GUILayout.Button("New Object", GUILayout.Width(115)))
						creating = true;
					//GUILayout.Space(5);
					GUI.enabled = creating;
					if (GUILayout.Button("Existing Object", GUILayout.Width(115)))
						creating = false;
					GUI.enabled = true;
				//GUILayout.EndHorizontal();
                    GUILayout.Space(10);
				if (GUILayout.Button("Save Objects", GUILayout.Width(115)))
					KerbalKonstructs.instance.saveObjects();
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect(135, 25, 540, 365));
				scrollPos = GUILayout.BeginScrollView(scrollPos);
				if (creating)
				{
					foreach (StaticModel model in KerbalKonstructs.instance.getStaticDB().getModels())
					{
                        // ASH 15102014 Removed redundant info from the path. Only need to know the content mod really
                        String[] modelpaths = model.path.Split('/');
                        String firstpath = modelpaths.Length > 0 ? modelpaths[0] : model.path;

						if (GUILayout.Button(model.meshName + " [" + firstpath + "]"))
						{
							StaticObject obj = new StaticObject();
							obj.gameObject = GameDatabase.Instance.GetModel(model.path + "/" + model.meshName);
							obj.altitude = (float)FlightGlobals.ActiveVessel.altitude;
							obj.parentBody = KerbalKonstructs.instance.getCurrentBody();
							obj.groupName = "Ungrouped";
							obj.position = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position);
							obj.rotation = 0;
							obj.orientation = Vector3.up;
							obj.visibleRange = 25000;
							obj.model = model;
							obj.siteName = "";
							obj.siteDescription = "";
							obj.siteTransform = "";
							obj.siteLogo = "";
							obj.siteIcon = "";
							obj.siteAuthor = "";

							KerbalKonstructs.instance.getStaticDB().addStatic(obj);
							KerbalKonstructs.instance.spawnObject(obj, true);
						}
					}
				}
				else
				{
					foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
					{
                        String[] modelpaths = obj.model.path.Split('/');
                        String firstpath = modelpaths.Length > 0 ? modelpaths[0] : obj.model.path;
						GUI.enabled = !(obj == selectedObject);
						if (GUILayout.Button(((obj.siteName != "") ? obj.siteName + "(" + obj.model.meshName + ")" : obj.model.meshName) + " [" + firstpath + "]"))
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
					Boolean addToDB = (selectedObject.siteName == "" && siteName != "");
					selectedObject.siteName = siteName;
					selectedObject.siteTransform = siteTrans;
					selectedObject.siteDescription = siteDesc;
					selectedObject.siteType = getSiteType(siteTypeMenu.SelectedItemIndex);
					selectedObject.siteLogo = (siteLogo != "") ? selectedObject.model.path + "/" + siteLogo : "";
					if (siteAuthor != selectedObject.model.author)
						selectedObject.siteAuthor = siteAuthor;
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
            // ASH Worth considering doing this for the launch selector?
		}

		public void updateSelection(StaticObject obj)
		{
			selectedObject = obj;
			xPos = obj.position.x.ToString();
			yPos = obj.position.y.ToString();
			zPos = obj.position.z.ToString();
			altitude = obj.altitude.ToString();
			rotation = obj.rotation.ToString();
			orientationMenu.SelectedItemIndex = getOrientation(obj.orientation);
			selectedObject.update();
		}

		public void setOrientation(int selection)
		{
			if (selectedObject != null)
			{
				switch (selection)
				{
					case 0:
						selectedObject.orientation = Vector3.up;
						break;
					case 1:
						selectedObject.orientation = Vector3.down;
						break;
					case 2:
						selectedObject.orientation = Vector3.left;
						break;
					case 3:
						selectedObject.orientation = Vector3.right;
						break;
					case 4:
						selectedObject.orientation = Vector3.forward;
						break;
					case 5:
						selectedObject.orientation = Vector3.back;
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
