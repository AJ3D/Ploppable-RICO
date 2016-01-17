using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PloppableRICO
{
	// The foundation of this class is from EMF's Extended Buiding Informaion mod. I've added the UI elements. Many thanks to him for his work.

	public class ServiceInfoWindow : MonoBehaviour
	{
		UILabel info;
		UILabel label1;
		FieldInfo baseSub;
		UIPanel Tabs;
		InfoTabs InfoTabs;
		InstanceID BID = new InstanceID ();

		CityServiceWorldInfoPanel m_servicePanel;

		public CityServiceWorldInfoPanel servicePanel {
			
			get { return m_servicePanel; }
			set {

				InfoTabs = new InfoTabs ();

				var stats = value.Find<UIPanel> ("StatsPanel");
				info = stats.Find<UILabel> ("Info");
				label1 = stats.AddUIComponent<UILabel> ();
				label1.color = info.color;
				label1.textColor = info.textColor;
				label1.textScale = info.textScale;
				label1.relativePosition = new Vector3 (0, info.height + info.relativePosition.y - 40);
				label1.size = new Vector2 (230, 84);
				label1.font = info.font;

				Tabs = UIView.GetAView ().FindUIComponent ("(Library) CityServiceWorldInfoPanel").AddUIComponent<UIPanel> ();
				Tabs.size = new Vector2 (432, 25);
				Tabs.relativePosition = new Vector2 (13, -25);

				InfoTabs.DrawInfoTabs (Tabs);

				m_servicePanel = value;
			}
		}

		int lastSelected;

			
		public void Update ()
		{
			if (servicePanel == null) {
				return;
			}
	
			var buildingId = GetParentInstanceId ().Building;
			if (this.enabled && info.isVisible && BuildingManager.instance != null && ((SimulationManager.instance.m_currentFrameIndex & 15u) == 15u || lastSelected != buildingId)) {
				//Debug.Log ("running");

				lastSelected = buildingId;


				InfoTabs.UpdateInfoPanelTabs (buildingId);


			}
		}


		private InstanceID GetParentInstanceId ()
		{
			if (baseSub == null) {
				baseSub = m_servicePanel.GetType ().GetField ("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);

			}
			return (InstanceID)baseSub.GetValue (m_servicePanel);
		}

	}

}
	

