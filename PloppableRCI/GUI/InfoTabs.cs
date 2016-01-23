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

	public class InfoTabs : UITabstrip
	{

		InstanceID BID = new InstanceID ();

		UISprite[] TabSprites = new UISprite[6];
		UIButton[] TabButtons = new UIButton[6];



		List<ushort> IDList;
		Building ParentBuilding;
		Building SubBuilding;

		public ushort lastselected = 1;

		int types = 5;

		public override void Start ()
		{
			IDList = new List<ushort> ();
			this.size = new Vector2 (432, 25);
			this.relativePosition = new Vector2 (13, -25);
			this.name = "InfoTabs";
			this.startSelectedIndex = 0;
			this.padding = new RectOffset (0, 3, 0, 0);

			for (int i = 0; i <= types; i++) { //loop though the 6 tab buttons and initilize them. 

				TabButtons [i] = new UIButton ();
				TabButtons [i] = this.AddUIComponent<UIButton> ();

				TabButtons [i].size = new Vector2 (58, 25);

				TabButtons [i].normalBgSprite = "SubBarButtonBase";
				TabButtons [i].disabledBgSprite = "SubBarButtonBaseDisabled";
				TabButtons [i].pressedBgSprite = "SubBarButtonBasePressed";
				TabButtons [i].hoveredBgSprite = "SubBarButtonBaseHovered";
				TabButtons [i].focusedBgSprite = "SubBarButtonBaseFocused";
				TabButtons [i].name = name;
				TabButtons [i].isVisible = false;

				TabSprites [i] = new UISprite ();
				TabSprites [i] = TabButtons [i].AddUIComponent<UISprite> ();
				TabSprites [i].size = new Vector2 (35, 25);
				TabSprites [i].isInteractive = false;
				TabSprites [i].relativePosition = new Vector2 (12, 0);
			}
		}

		public void UpdateInfoPanelTabs (ushort BuildingID)
		{
			//this method creates a list of the sub building chain, and applies the approprate settings to the tabs. 

			Building SelectedBuilding = BuildingManager.instance.m_buildings.m_buffer [BuildingID];

			if ((SelectedBuilding.m_parentBuilding == 0) & (SelectedBuilding.m_subBuilding == 0)) { 

				foreach (var button in TabButtons) { //if there are no subbuildings, hide tabs
					button.isVisible = false;
				}
			} else { //but if there are sub/parent buildings, draw the tabs

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
						TabButtons [counter].name = BuildingID.ToString ();

					

						if (TabBuilding.Info.m_buildingAI is MonumentAI) { //draw the sprite on the buttons. 
							TabSprites [counter].spriteName = "FeatureMonumentLevel3";
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

						if (ID == BuildingID) {

							this.selectedIndex = counter;
							TabSprites [counter].spriteName = TabSprites [counter].spriteName + "Focused";
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

		private void SelectSub (UIComponent component, UIMouseEventParameter eventParam, ushort ID)
		{

			BID.Building = ID;
			DefaultTool.OpenWorldInfoPanel (BID, new Vector2 (0, 0));
		}
	}
}
