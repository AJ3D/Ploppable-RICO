using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PloppableRICO
{
	/// <summary>
	/// The foundation of this class is from EMF's Extended Buiding Informaion mod. Many thanks to him for his work. 
	/// </summary>

	public class TabUpdater : MonoBehaviour
	{
		
		FieldInfo baseSub;

		InfoTabs Tabs;

		ushort ServicebuildingId;
		ushort ZonedbuildingId;

		public ushort ZonedLast;
		public ushort ServiceLast;

		CityServiceWorldInfoPanel m_servicePanel;
		ZonedBuildingWorldInfoPanel m_zonedPanel;

		public CityServiceWorldInfoPanel servicePanel {

			get { return m_servicePanel; }
			set {m_servicePanel = value;}
		}

		public ZonedBuildingWorldInfoPanel zonedPanel {
				
			get { return m_zonedPanel; }
			set {m_zonedPanel = value; }
		}

	
		public void Update ()
		{

			if ((servicePanel == null) || (zonedPanel == null)) {
				return;
			}

			ServicebuildingId = GetServiceInstanceId ().Building;  //grab last selected Service building
			ZonedbuildingId = GetZonedInstanceId ().Building; //grab last selected zoned building
			 
			if (ZonedLast != ZonedbuildingId) { //if different buildings are selected, update the tabs. 
				Tabs = UIView.Find<InfoTabs> ("InfoTabs");
				Tabs.UpdateInfoPanelTabs (ZonedbuildingId);
				ZonedLast = ZonedbuildingId;
			}

			if (ServiceLast != ServicebuildingId) {
				Tabs = UIView.Find<InfoTabs> ("InfoTabs");
				Tabs.UpdateInfoPanelTabs (ServicebuildingId);
				ServiceLast = ServicebuildingId;
			}
		
		}

		private InstanceID GetServiceInstanceId ()
		{
			if (baseSub == null) {
				baseSub = m_servicePanel.GetType ().GetField ("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);

			}
			return (InstanceID)baseSub.GetValue (m_servicePanel);
		}

		private InstanceID GetZonedInstanceId () 
		{
			if (baseSub == null) {
				baseSub = m_zonedPanel.GetType ().GetField ("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);

			}
			return (InstanceID)baseSub.GetValue (m_zonedPanel);
		}
	}

}
	

