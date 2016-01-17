using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace PloppableRICO
{

	/// <summary>
	/// This class draws and updates the tabs above the info panels. 
	/// </summary>

	public class InfoTabs
	{

		InstanceID BID = new InstanceID ();

		UISprite[] TabSprites = new UISprite[6];
		UIButton[] TabButtons = new UIButton[6];

		List<ushort> IDList;
		Building ParentBuilding;
		Building SubBuilding;

		ushort lastselected;

		int types = 5;
		int taboffset = 63;
		int taboffsetstart = 5;


		public void DrawInfoTabs (UIPanel Tabs)
		{

			IDList = new List<ushort> ();

			for (int i = 0; i <= types; i++) { //loop though the 6 tab buttons and initilize them. 

				TabButtons [i] = new UIButton ();
				TabButtons [i] = Tabs.AddUIComponent<UIButton> ();

				TabButtons [i].size = new Vector2 (58, 25);
				TabButtons [i].relativePosition = new Vector2 (taboffsetstart, 0);
				TabButtons [i].normalBgSprite = "SubBarButtonBase";
				TabButtons [i].disabledBgSprite = "SubBarButtonBaseDisabled";
				TabButtons [i].pressedBgSprite = "SubBarButtonBasePressed";
				TabButtons [i].hoveredBgSprite = "SubBarButtonBaseHovered";
				TabButtons [i].focusedBgSprite = "SubBarButtonBaseFocused";
				TabButtons [i].state = UIButton.ButtonState.Normal;
				//TabButtons[i].name = name;
				//button.tabStrip = true;
				TabButtons [i].isVisible = false;

				TabSprites [i] = new UISprite ();
				TabSprites [i] = TabButtons [i].AddUIComponent<UISprite> ();
				TabSprites [i].size = new Vector2 (35, 25);
				TabSprites [i].relativePosition = new Vector2 (12, 0);

				taboffsetstart = taboffsetstart + taboffset;
			}
		}

		public void UpdateInfoPanelTabs (ushort BuildingID)
		{
			//this method creates a list of the sub building chain, and applies the approprate settings to the tabs. 

			if (lastselected != BuildingID) { //Dont run if no new building selected

				Building SelectedBuilding = BuildingManager.instance.m_buildings.m_buffer [BuildingID];

				Debug.Log (SelectedBuilding.Info.name + " is selected");

				if ((SelectedBuilding.m_parentBuilding == 0) & (SelectedBuilding.m_subBuilding == 0)) { //if there are no subbuildings, hide tabs

					foreach (var button in TabButtons) {
						button.isVisible = false;
						lastselected = BuildingID;
					}
				} else { //but if there are sub/parent buildings, draw the tabs

					lastselected = BuildingID;

					foreach (var button in TabButtons) { //turn all tabs off to start. 
						button.isVisible = false;
						button.state = UIButton.ButtonState.Normal;
					}

					if (SelectedBuilding.m_parentBuilding != 0) { //work up the building chain, and grab all parent IDs. 

						IDList.Add (SelectedBuilding.m_parentBuilding);
						ParentBuilding = BuildingManager.instance.m_buildings.m_buffer [SelectedBuilding.m_parentBuilding];

			
						while (ParentBuilding.m_parentBuilding != 0) {
							IDList.Add (ParentBuilding.m_parentBuilding);
							ParentBuilding = BuildingManager.instance.m_buildings.m_buffer [ParentBuilding.m_parentBuilding];
						}
					}

					IDList.Reverse (); //Reverse so things are in order. The master lot will be first now. 

					IDList.Add (BuildingID); //add selected building id. 

					if (SelectedBuilding.m_subBuilding != 0) { // work down the building chain, and add sub building ID's. 
				
						IDList.Add (SelectedBuilding.m_subBuilding);
						SubBuilding = BuildingManager.instance.m_buildings.m_buffer [SelectedBuilding.m_subBuilding];

						while (SubBuilding.m_subBuilding != 0) {
					
							IDList.Add (SubBuilding.m_subBuilding);
							SubBuilding = BuildingManager.instance.m_buildings.m_buffer [SubBuilding.m_subBuilding];
						}
					}

					//IDlist is now an ordered list of entire parent and sub building chain for the selected building. We can use that info to draw the tabs. 

					Building TabBuilding;

					int counter = 0;
					foreach (var ID in IDList) { // loop through ID chain, and draw tabs. 

						TabBuilding = BuildingManager.instance.m_buildings.m_buffer [ID];

						if (!(TabBuilding.Info.m_buildingAI is DummyBuildingAI)) { //dont draw tabs for dummy buildings.

							TabButtons [counter].isVisible = true;
							TabButtons [counter].eventClick += (sender, e) => SelectSub (sender, e, ID);

							if (ID == lastselected) {
								TabButtons [counter].state = UIButton.ButtonState.Focused; //Highlight the tab of the selected building. 
							}
								
							if (TabBuilding.Info.m_buildingAI is MonumentAI) { //draw the sprite on the buttons. 
								SetSprites (TabSprites [counter], "FeatureMonumentLevel3");
							}

							if (TabBuilding.Info.m_buildingAI is PloppableOffice) {
								TabSprites [counter].atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;
								TabSprites [counter].spriteName = "ZoningOffice";
							}
							if (TabBuilding.Info.m_buildingAI is PloppableResidential) {
								TabSprites [counter].atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;
								TabSprites [counter].spriteName = "ZoningResidentialHigh";
							}
							if (TabBuilding.Info.m_buildingAI is PloppableExtractor) {
								TabSprites [counter].atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;
								TabSprites [counter].spriteName = "ZoningIndustrial";
							}
							if (TabBuilding.Info.m_buildingAI is PloppableCommercial) {
								TabSprites [counter].atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;
								TabSprites [counter].spriteName = "ZoningCommercialHigh";
							}

							counter = counter + 1;
						}
					}

					if (TabButtons [1].isVisible == false) { //if only one tab was drawn, then hide the one tab. This happens when you've got only dummy sub buildings. 
						TabButtons [0].isVisible = false;
					}
		
					IDList.Clear ();
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
		
			label.spriteName = sprite;

		}

		private void SelectSub (UIComponent component, UIMouseEventParameter eventParam, ushort ID)
		{
			
			BID.Building = ID;
			DefaultTool.OpenWorldInfoPanel (BID, new Vector2 (0, 0));
		}
	}
}

