using System;
using UnityEngine;

namespace KerbalKonstructs
{
	class EditorGUI
	{
		StaticObject selectedObject;
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
		}

		public void drawEditor(StaticObject obj)
		{
			if (selectedObject != obj)
			{
				updateSelection(obj);
			}
			
			//It wanted a unique ID number ¯\_(ツ)_/¯
			editorRect = GUI.Window(0xB00B1E5, editorRect, drawEditorWindow, "Kerbal Konstructs Editor");
		}

		Rect editorRect = new Rect(70, 100, 306, 250);

		private static GUIStyle listStyle = new GUIStyle();
		private static GUIContent[] comboBoxList = {
										new GUIContent("Up"),
										new GUIContent("Down"),
										new GUIContent("Left"),
										new GUIContent("Right"),
										new GUIContent("Forward"),
										new GUIContent("Back")
									};
		ComboBox orientationMenu = new ComboBox(comboBoxList[0], comboBoxList, "button", "box", listStyle);
		void drawEditorWindow(int windowID)
		{
			GUI.Label(new Rect(6, 30, 203, 25), "Position");
			xPos = GUI.TextField(new Rect(70, 50, 80, 25), xPos, 25);
			if (GUI.Button(new Rect(38, 50, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 50, 30, 25), "<<"))
			{
				selectedObject.position.x -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if(GUI.Button(new Rect(152, 50, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 50, 30, 25), ">>"))
			{
				selectedObject.position.x += float.Parse(increment);
				updateSelection(selectedObject);
			}
			yPos = GUI.TextField(new Rect(70, 80, 80, 25), yPos, 25);
			if (GUI.Button(new Rect(38, 80, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 80, 30, 25), "<<"))
			{
				selectedObject.position.y -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(152, 80, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 80, 30, 25), ">>"))
			{
				selectedObject.position.y += float.Parse(increment);
				updateSelection(selectedObject);
			}
			zPos = GUI.TextField(new Rect(70, 110, 80, 25), zPos, 25);
			if (GUI.Button(new Rect(38, 110, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 110, 30, 25), "<<"))
			{
				selectedObject.position.z -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(152, 110, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 110, 30, 25), ">>"))
			{
				selectedObject.position.z += float.Parse(increment);
				updateSelection(selectedObject);
			}
			GUI.Label(new Rect(6, 140, 203, 25), "Altitude");

			altitude = GUI.TextField(new Rect(70, 160, 80, 25), altitude, 25);
			if (GUI.Button(new Rect(38, 160, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 160, 30, 25), "<<"))
			{
				selectedObject.altitude -= float.Parse(increment);
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(152, 160, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 160, 30, 25), ">>"))
			{
				selectedObject.altitude += float.Parse(increment);
				updateSelection(selectedObject);
			}
			//disable anything beneath the dropdown to prevent clicking through
			GUI.enabled = !orientationMenu.isClickedComboButton;
			GUI.Label(new Rect(220, 30, 80, 25), "Orientation");
			GUI.Label(new Rect(220, 80, 80, 25), "Increment");
			increment = GUI.TextField(new Rect(220, 100, 80, 25), increment, 25);
			GUI.Label(new Rect(220, 130, 80, 25), "Rotation");
			rotation = GUI.TextField(new Rect(220, 150, 80, 25), rotation, 25);
			GUI.enabled = true;

			GUI.Button(new Rect(6, 190, 80, 25), "Deselect");
			GUI.Button(new Rect(6, 220, 80, 25), "Delete");

			//Draw last so it properly overlaps
			orientationMenu.Show(new Rect(220, 50, 80, 25));
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void updateSelection(StaticObject obj)
		{
			selectedObject = obj;
			xPos = obj.position.x.ToString();
			yPos = obj.position.y.ToString();
			zPos = obj.position.z.ToString();
			altitude = obj.altitude.ToString();
			rotation = obj.rotation.ToString();
			selectedObject.update();
		}
	}
}
