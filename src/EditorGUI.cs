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
			GUI.Button(new Rect(6, 50, 30, 25), "-5");
			GUI.Button(new Rect(38, 50, 30, 25), "-1");
			GUI.Button(new Rect(152, 50, 30, 25), "+1");
			GUI.Button(new Rect(184, 50, 30, 25), "+5");
			yPos = GUI.TextField(new Rect(70, 80, 80, 25), yPos, 25);
			GUI.Button(new Rect(6, 80, 30, 25), "-5");
			GUI.Button(new Rect(38, 80, 30, 25), "-1");
			GUI.Button(new Rect(152, 80, 30, 25), "+1");
			GUI.Button(new Rect(184, 80, 30, 25), "+5");
			zPos = GUI.TextField(new Rect(70, 110, 80, 25), zPos, 25);
			GUI.Button(new Rect(6, 110, 30, 25), "-5");
			GUI.Button(new Rect(38, 110, 30, 25), "-1");
			GUI.Button(new Rect(152, 110, 30, 25), "+1");
			GUI.Button(new Rect(184, 110, 30, 25), "+5");
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public void updateSelection(StaticObject obj)
		{
			selectedObject = obj;
			xPos = obj.position.x.ToString();
			yPos = obj.position.y.ToString();
			zPos = obj.position.z.ToString();
			altitude = obj.altitude.ToString();
		}
	}
}
