using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PloppableRICO
{

	public class ServiceInfoWindow : MonoBehaviour
	{
		UILabel info;
		UILabel label1;
		FieldInfo baseSub;
		FieldInfo bSub;
		UITabstrip Tabs;
		UIButton ResTab;
		InstanceID BID = new InstanceID();
	
		Building data;
		int num;

		//CityServiceWorldInfoPanel tester = UIView.GetAView ().FindUIComponent ("(Library) CityServiceWorldInfoPanel");


		CityServiceWorldInfoPanel m_servicePanel;
		public CityServiceWorldInfoPanel servicePanel
		{
			get { return m_servicePanel; }
			set
			{
				var stats = value.Find<UIPanel>("StatsPanel");
			


				info = stats.Find<UILabel>("Info");
				label1 = stats.AddUIComponent<UILabel>();
				label1.color = info.color;
				label1.textColor = info.textColor;
				label1.textScale = info.textScale;
				label1.relativePosition = new Vector3(0, info.height + info.relativePosition.y - 40);
				label1.size = new Vector2(230, 84);
				label1.font = info.font;

				Tabs = UIView.GetAView ().FindUIComponent ("(Library) CityServiceWorldInfoPanel").AddUIComponent<UITabstrip> ();
				Tabs.size = new Vector2 (432, 25);
				Tabs.relativePosition = new Vector2 (13, -25);
				DrawTabs ("ZoningResidentialHigh", 5, "SResidential", 30534);
				ResTab.isVisible = false;
		
				m_servicePanel = value;
			}
		}

		int lastSelected;

			
		public void Update()
		{
			if (servicePanel == null) {
				return;
			}
	
			var buildingId = GetParentInstanceId ().Building;
			if (this.enabled && info.isVisible && BuildingManager.instance != null && ((SimulationManager.instance.m_currentFrameIndex & 15u) == 15u || lastSelected != buildingId)) {
				Debug.Log ("running");
				lastSelected = buildingId;

				Building data = BuildingManager.instance.m_buildings.m_buffer [buildingId];

				BID.Building = data.m_subBuilding;

				//UITextField Bname = new UITextField ();
				//Bname = UIView.GetAView ().FindUIComponent<UITextField>("BuildingName");
				//Bname.text = data.Info.name.ToString ();

				Building SubB = BuildingManager.instance.m_buildings.m_buffer [data.m_subBuilding];

				if (SubB.Info.m_buildingAI is PloppableRICO.PloppableResidential || SubB.Info.m_buildingAI is PloppableRICO.PloppableOffice) {
					ResTab.isVisible = true;
					ResTab.eventClick += (sender, e) => SelectSub(sender, e, data.m_subBuilding);
				} else {
					ResTab.isVisible = false;
				}
			}
		}
		private InstanceID GetParentInstanceId()
		{
			if (baseSub == null)
			{
				baseSub = m_servicePanel.GetType().GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);

			}
			return (InstanceID)baseSub.GetValue(m_servicePanel);
		}
		public void DrawTabs(string Sprite, int offset, string name, ushort ID){

			ResTab = new UIButton ();
			ResTab = Tabs.AddUIComponent<UIButton> ();
			ResTab.size = new Vector2 (58, 25);
			ResTab.relativePosition = new Vector2 (offset, 0);
			//ResTab.atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;
			ResTab.normalBgSprite = "SubBarButtonBase";
			ResTab.pressedBgSprite = "SubBarButtonBasePressed";
			ResTab.hoveredBgSprite = "SubBarButtonBaseHovered";
			ResTab.focusedBgSprite = "SubBarButtonBaseFocused";
			//ResTab.normalFgSprite = Sprite;
			//ResTab.pressedFgSprite = Sprite + "Pressed";
			//ResTab.hoveredFgSprite = Sprite + "Hovered";
			//ResTab.focusedFgSprite = Sprite + "Focused";
			//ResTab.tabStrip = true;
		}
			
	
		private void SelectSub(UIComponent component, UIMouseEventParameter eventParam, ushort ID){
			
			label1.text = ID.ToString() ;
			BID.Building = ID;
			DefaultTool.OpenWorldInfoPanel (BID, new Vector2 (0, 0));
		
			//WorldInfoPanel.Show<CityServiceWorldInfoPanel> (new Vector2 (200, 200), BID);
		}

	}

}
	

