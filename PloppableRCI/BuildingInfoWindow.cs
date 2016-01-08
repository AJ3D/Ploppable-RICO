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
		ushort selectedBuilding;

		UIPanel Tabs;
		UIButton Button3;
		UIButton Button4;
		UISprite Sprite1;
		UISprite Sprite2;
		UITextField namepan;

	
		InstanceID BID = new InstanceID();


		public override void Awake()
		{
			var panal = UIView.Find<UIPanel>("(Library) ZonedBuildingWorldInfoPanel");
			label2 = panal.AddUIComponent<UILabel> ();

			//namepan = UIView.Find<UITextField>("BuildingName");

			Tabs = panal.AddUIComponent<UIPanel> ();
			Tabs.size = new Vector2 (432, 25);
			Tabs.relativePosition = new Vector2 (13, -25);
			Button3 = Tabs.AddUIComponent<UIButton> ();
			Sprite1 = Button3.AddUIComponent<UISprite> ();
			Button4 = Tabs.AddUIComponent<UIButton> ();
			Sprite2 = Button4.AddUIComponent<UISprite> ();

			Sprite2.atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;

			DrawTabs (Button3, 5, "Button3");
			DrawTabs (Button4, 68, "Button4");

			base.Awake();

		}


		public override void Start()
		{
			base.Start();
		}
			

		public override void Update()
		{
			var buildingId = GetParentInstanceId ().Building;

			Building data = BuildingManager.instance.m_buildings.m_buffer [buildingId];

			Building ParB = BuildingManager.instance.m_buildings.m_buffer [data.m_parentBuilding];

			//namepan.text = "test";

			if (data.Info.m_buildingAI is PloppableRICO.PloppableResidential || data.Info.m_buildingAI is PloppableRICO.PloppableOffice || data.Info.m_buildingAI is PloppableRICO.PloppableExtractor) {

				Button4.state = UIButton.ButtonState.Focused;


				if (data.Info.m_buildingAI is PloppableRICO.PloppableResidential) {
					SetSprites (Sprite2, "ZoningResidentialHigh");
				}

				if (data.Info.m_buildingAI is PloppableRICO.PloppableOffice) {
					SetSprites (Sprite2, "ZoningOffice");
				}
				if (data.Info.m_buildingAI is PloppableRICO.PloppableExtractor) {
					SetSprites (Sprite2, "ZoningIndustrial");
				}


				SetSprites (Sprite1, "FeatureMonumentLevel3");

				Button4.isVisible = true;
				Button3.isVisible = true;
				Button3.eventClick += (sender, e) => SelectPar(sender, e, data.m_parentBuilding);
				Button4.eventClick += (sender, e) => SelectPar(sender, e, buildingId);

			} else {
				
				Button4.isVisible = false;
				Button3.isVisible = false;
			}
				
			base.Update();
		}

		private void SelectPar(UIComponent component, UIMouseEventParameter eventParam, ushort ID){

			BID.Building = ID;
			DefaultTool.OpenWorldInfoPanel (BID, new Vector2 (0, 0));
			Button4.state = UIButton.ButtonState.Focused;
			//Button1.state = UIButton.ButtonState.Normal;
		}


			
		private InstanceID GetParentInstanceId()
		{
			if (baseSub == null)
			{
				baseSub = this.baseBuildingWindow.GetType().GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			return (InstanceID)baseSub.GetValue(this.baseBuildingWindow);
		}

		public void DrawTabs(UIButton butto, int offset, string name){

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
			//button.isVisible = false;

		}
		public void SetSprites(UISprite labe, string sprite){
			UISprite label = labe;
			label.relativePosition = new Vector2 (12, 0);
			label.spriteName = sprite;
			label.size = new Vector2 (35, 25);
		}

	}
}