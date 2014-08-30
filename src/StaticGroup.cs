using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs
{
	class StaticGroup
	{
		private String groupName;
		private String bodyName;

		private List<StaticObject> childObjects = new List<StaticObject>();
		private Vector3 centerPoint = Vector3.zero;
		private float visiblityRange = 0;
		public Boolean alwaysActive = false;

		public StaticGroup(String name, String body)
		{
			groupName = name;
			bodyName = body;
		}

		public void addStatic(StaticObject obj)
		{
			childObjects.Add(obj);
			//TODO: recalculate center point and visibility range
		}
	}
}
