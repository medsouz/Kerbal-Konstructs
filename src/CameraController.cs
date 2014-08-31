using System;
using UnityEngine;

namespace KerbalKonstructs
{
	class CameraController
	{

		public FlightCamera cam;
		public Transform oldTarget;
		public Boolean active = false;

		private float x = 0; 
		private float y = 0;

		public void enable(GameObject targ)
		{
			cam = FlightCamera.fetch;
			if (cam)
			{
				cam.DeactivateUpdate();
				oldTarget = cam.transform.parent;
				cam.transform.parent = targ.transform;
				x = targ.transform.eulerAngles.x;
				y = targ.transform.eulerAngles.y;
				active = true;
			}
			else
			{
				Debug.LogError("FlightCamera doesn't exist!");
			}
		}

		public void disable()
		{
			cam.ActivateUpdate();
			cam.transform.parent = oldTarget;
			active = false;
		}

		//TODO: Make this less shaky
		public void updateCamera()
		{
			if (Input.GetMouseButton(1))
			{
				x += Input.GetAxis("Mouse X") * cam.orbitSensitivity * 10.0f;
				y -= Input.GetAxis("Mouse Y") * cam.orbitSensitivity * 10.0f;
			}

			cam.transform.localRotation = Quaternion.Euler(y, x, 0);
			//TODO: Don't hardcode zoom
			cam.transform.localPosition = (Quaternion.Euler(y, x, 0)) * new Vector3(0.0f, 0.0f, -10);
		}
	}
}
