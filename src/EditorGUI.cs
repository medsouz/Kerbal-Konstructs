using System;
using UnityEngine;

namespace KerbalKonstructs
{
	class EditorGUI
	{
		StaticObject selectedObject;
		private String xPos, yPos, zPos, altitude;


		public void drawEditor(StaticObject obj)
		{
			if (selectedObject != obj)
			{
				updateSelection(obj);
			}
			
			//It wanted a unique ID number ¯\_(ツ)_/¯
			editorRect = GUI.Window(0xB00B1E5, editorRect, drawEditorWindow, "Kerbal Konstructs Editor");
		}

		Rect editorRect = new Rect(70, 100, 500, 150);
		void drawEditorWindow(int windowID)
		{
			GUI.Label(new Rect(6, 30, 203, 25), "Position");
			xPos = GUI.TextField(new Rect(70, 50, 80, 25), xPos, 25);
			if (GUI.Button(new Rect(38, 50, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 50, 30, 25), "<<"))
			{
				selectedObject.position.x--;
				updateSelection(selectedObject);
			}
			if(GUI.Button(new Rect(152, 50, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 50, 30, 25), ">>"))
			{
				selectedObject.position.x++;
				updateSelection(selectedObject);
			}
			yPos = GUI.TextField(new Rect(70, 80, 80, 25), yPos, 25);
			if (GUI.Button(new Rect(38, 80, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 80, 30, 25), "<<"))
			{
				selectedObject.position.y--;
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(152, 80, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 80, 30, 25), ">>"))
			{
				selectedObject.position.y++;
				updateSelection(selectedObject);
			}
			zPos = GUI.TextField(new Rect(70, 110, 80, 25), zPos, 25);
			if (GUI.Button(new Rect(38, 110, 30, 25), "<") || GUI.RepeatButton(new Rect(6, 110, 30, 25), "<<"))
			{
				selectedObject.position.z--;
				updateSelection(selectedObject);
			}
			if (GUI.Button(new Rect(152, 110, 30, 25), ">") || GUI.RepeatButton(new Rect(184, 110, 30, 25), ">>"))
			{
				selectedObject.position.z++;
				updateSelection(selectedObject);
			}
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void updateSelection(StaticObject obj)
		{
			selectedObject = obj;
			xPos = obj.position.x.ToString();
			yPos = obj.position.y.ToString();
			zPos = obj.position.z.ToString();
			altitude = obj.altitude.ToString();
			selectedObject.update();
		}
	}
}
