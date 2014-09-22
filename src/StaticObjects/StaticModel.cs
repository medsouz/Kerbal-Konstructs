using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticModel
	{
		public string author;
		public string path;
		public string meshName;
		public string config;
		public string configPath;
		public string defaultSiteTransform;
		public List<StaticModule> modules = new List<StaticModule>();
	}
}
