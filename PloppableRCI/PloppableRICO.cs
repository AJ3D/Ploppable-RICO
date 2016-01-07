using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;

using UnityEngine;
using ColossalFramework.UI;


namespace PloppableRICO
{
	public class PloppableRCI : IUserMod
	{
		public string Name
		{
			get
			{
				return "Ploppable RICO";
			}
		}
		public string Description
		{
			get
			{
				return "Allows Plopping of Residential Buildings";
			}
		}
	}

}
