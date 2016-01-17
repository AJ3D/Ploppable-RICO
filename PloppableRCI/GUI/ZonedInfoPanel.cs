using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PloppableRICO
{
	using ColossalFramework;
	using ColossalFramework.Globalization;
	using ColossalFramework.Math;
	using ColossalFramework.UI;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Timers;
	using UnityEngine;

	public class BuildingInfoWindow5 : UIPanel
	{

		public ZonedBuildingWorldInfoPanel baseBuildingWindow;
		FieldInfo baseSub;
		UILabel label2;
		InfoTabs InfoTabs;
		UIPanel Tabs;

		public override void Awake ()
		{

			InfoTabs = new InfoTabs ();
			var panal = UIView.Find<UIPanel> ("(Library) ZonedBuildingWorldInfoPanel");
			label2 = panal.AddUIComponent<UILabel> ();

			//namepan = UIView.Find<UITextField>("BuildingName");
			Tabs = panal.AddUIComponent<UIPanel> ();
			Tabs.size = new Vector2 (432, 25);
			Tabs.relativePosition = new Vector2 (13, -25);
			InfoTabs.DrawInfoTabs (Tabs);

			base.Awake ();

		}

		public override void Start ()
		{
			base.Start ();
		}
			
		public override void Update ()
		{
			var buildingId = GetParentInstanceId ().Building;
			InfoTabs.UpdateInfoPanelTabs (buildingId);		
			base.Update ();
		}
	
		private InstanceID GetParentInstanceId ()
		{
			if (baseSub == null) {
				baseSub = this.baseBuildingWindow.GetType ().GetField ("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			return (InstanceID)baseSub.GetValue (this.baseBuildingWindow);
		}

	}
}