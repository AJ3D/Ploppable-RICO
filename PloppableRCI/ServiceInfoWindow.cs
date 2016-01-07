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
		UIButton Button1;
		UIButton Button2;
		UISprite Sprite1;
		UISprite Sprite2;
		InstanceID BID = new InstanceID ();
	

		CityServiceWorldInfoPanel m_servicePanel;

		public CityServiceWorldInfoPanel servicePanel {
			get { return m_servicePanel; }
			set {
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
				Button1 = Tabs.AddUIComponent<UIButton> ();
				Sprite1 = Button1.AddUIComponent<UISprite> ();
				Button2 = Tabs.AddUIComponent<UIButton> ();
				Sprite2 = Button2.AddUIComponent<UISprite> ();

				Sprite2.atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;

				DrawTabs (Button1, 5, "Button1");
				DrawTabs (Button2, 68, "Button2");

		
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

				Building data = BuildingManager.instance.m_buildings.m_buffer [buildingId];


				Building SubB = BuildingManager.instance.m_buildings.m_buffer [data.m_subBuilding];


				if (SubB.Info.m_buildingAI is PloppableRICO.PloppableResidential || SubB.Info.m_buildingAI is PloppableRICO.PloppableOffice) {

					Button1.state = UIButton.ButtonState.Focused;

					Button1.isVisible = true;
					Button2.isVisible = true;

					if (SubB.Info.m_buildingAI is PloppableRICO.PloppableResidential) {
						SetSprites (Sprite2, "ZoningResidentialHigh");
					}

					if (SubB.Info.m_buildingAI is PloppableRICO.PloppableOffice) {
						SetSprites (Sprite2, "ZoningOffice");
					}

					SetSprites (Sprite1, "FeatureMonumentLevel3");

					Button2.eventClick += (sender, e) => SelectSub (sender, e, data.m_subBuilding);
					//Button1.eventClick += (sender, e) => SelectSub(sender, e, buildingId);

				} else {
					Button2.isVisible = false;
					Button1.isVisible = false;
				}
			}
		}

		public void DrawTabs (UIButton butto, int offset, string name)
		{

			UIButton button = butto;
			button.size = new Vector2 (58, 25);
			button.relativePosition = new Vector2 (offset, 0);
			button.normalBgSprite = "SubBarButtonBase";
			button.disabledBgSprite = "SubBarButtonBaseDisabled";
			button.pressedBgSprite = "SubBarButtonBasePressed";
			button.hoveredBgSprite = "SubBarButtonBaseHovered";
			button.focusedBgSprite = "SubBarButtonBaseFocused";
			button.state = UIButton.ButtonState.Normal;
			button.name = name;
			//button.tabStrip = true;
			button.isVisible = false;

		}

		public void SetSprites (UISprite labe, string sprite)
		{
			UISprite label = labe;
			label.relativePosition = new Vector2 (12, 0);
			label.spriteName = sprite;
			label.size = new Vector2 (35, 25);
		}

	
		private void SelectSub (UIComponent component, UIMouseEventParameter eventParam, ushort ID)
		{
			
			BID.Building = ID;
			DefaultTool.OpenWorldInfoPanel (BID, new Vector2 (0, 0));
			//Button1.state = UIButton.ButtonState.Focused;
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
	

