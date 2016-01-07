using ColossalFramework.IO;
using ColossalFramework;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Math;
using System.Threading;


namespace PloppableRICO
{
	
	public class PloppableTool : ToolBase
	{
		UIButton PloppableButton;
		BuildingInfo BuildingIm;
		UIPanel BuildingPanel;
		UIPanel Tabs;
		bool m_active;
		UIScrollablePanel SBuildingPanel;
		UIButton ResTab;
		UIButton BBut;
		UIButton TabButton;

		int types = 9;
		UISprite TabSprite;
		UISprite[] TabSprites = new UISprite[10];
		UIScrollablePanel[] BuildingPanels = new UIScrollablePanel[10];
		UIButton[] TabButtons = new UIButton[10];

		string[] Names = new string[]{
		
			"ResidentialLow",
			"ResidentialHigh",
			"CommercialLow",
			"CommercialHigh",
			"Office",
			"Industrial",
			"Farming",
			"Forest",
			"Oil",
			"Ore",
		};

		int taboffset = 63;
		int taboffsetstart = 5;


		public void InitGui (List<string[]> BNames)
		{

			UIView uiView = UIView.GetAView ();
			UIComponent refButton = uiView.FindUIComponent ("Policies");
			UIComponent tsBar = uiView.FindUIComponent ("TSBar");


			if (PloppableButton == null) {

				PloppableButton = UIView.GetAView ().FindUIComponent<UITabstrip> ("MainToolstrip").AddUIComponent<UIButton> ();
				PloppableButton.size = new Vector2 (43, 49);
				PloppableButton.eventClick += PloppablebuttonClicked;
				PloppableButton.normalBgSprite = "ZoningOptionMarquee";
				PloppableButton.focusedFgSprite = "ToolbarIconGroup6Focused";
				PloppableButton.hoveredFgSprite = "ToolbarIconGroup6Hovered";
				PloppableButton.relativePosition = new Vector2 (800, 0);
				PloppableButton.name = "PloppableButton";

				BuildingPanel = UIView.GetAView ().FindUIComponent ("TSContainer").AddUIComponent<UIPanel> ();
				BuildingPanel.backgroundSprite = "SubcategoriesPanel";
				BuildingPanel.isVisible = false;
				BuildingPanel.name = "PloppableBuildingPanel";
				BuildingPanel.size = new Vector2 (859, 109);
				BuildingPanel.relativePosition = new Vector2 (0, 0);

				Tabs = UIView.GetAView ().FindUIComponent ("PloppableBuildingPanel").AddUIComponent<UIPanel> ();
				Tabs.size = new Vector2 (832, 25);
				Tabs.relativePosition = new Vector2 (13, -25);

				for (int i = 0; i <= types; i++) {

					//BuildingPanels[i].Reset ();

					BuildingPanels[i] = new UIScrollablePanel ();
					BuildingPanels[i] = BuildingPanel.AddUIComponent<UIScrollablePanel> ();
					BuildingPanels[i].size = new Vector2 (763, 109);
					BuildingPanels[i].relativePosition = new Vector2 (50, 0);
					BuildingPanels[i].name = Names [i] + "Panel";
					BuildingPanels[i].isVisible = false;


					TabButtons[i] = new UIButton ();
					TabButtons[i] = Tabs.AddUIComponent<UIButton> ();
					TabButtons[i].size = new Vector2 (58, 25);
					TabButtons[i].relativePosition = new Vector2 (taboffsetstart, 0);
					TabButtons[i].normalBgSprite = "SubBarButtonBase";
					TabButtons[i].disabledBgSprite = "SubBarButtonBaseDisabled";
					TabButtons[i].pressedBgSprite = "SubBarButtonBasePressed";
					TabButtons[i].hoveredBgSprite = "SubBarButtonBaseHovered";
					TabButtons[i].focusedBgSprite = "SubBarButtonBaseFocused";
					TabButtons[i].state = UIButton.ButtonState.Normal;
					TabButtons[i].isEnabled = enabled;
					TabButtons[i].name = Names[i] + "Button";

					TabSprite = TabButtons[i].AddUIComponent<UISprite>();

					if (i <= 5) {
						SetSprites (TabSprite, "Zoning" + Names[i]);

					} else {
						SetSprites (TabSprite, "DistrictSpecialization" + Names[i]);
					}
			
					taboffsetstart = taboffsetstart + taboffset;
				}
					
		
				TabButtons[0].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[0]);
				TabButtons[1].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[1]);
				TabButtons[2].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[2]);
				TabButtons[3].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[3]);
				TabButtons[4].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[4]);
				TabButtons[5].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[5]);
				TabButtons[6].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[6]);
				TabButtons[7].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[7]);
				TabButtons[8].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[8]);
				TabButtons[9].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels[9]);


				string[][] Bdnames = new string[][]{ };
				Bdnames = BNames.ToArray ();
				int Offset = 0;

				int tcounter = 0;



				foreach (string[] test in Bdnames) {

					Debug.Log (Convert.ToString (test));
					BuildingIm = PrefabCollection<BuildingInfo>.FindLoaded (test[0]);
					this.GenBButton (BuildingIm, Offset, test[1]);
					Offset = Offset + 109;
					//counter = counter + 1;
				}
					
			}
	
		}
//End of InitGUI


		public void DrawPanels(UIScrollablePanel panel, string name){
			
			panel = UIView.GetAView ().FindUIComponent ("PloppableBuildingPanel").AddUIComponent<UIScrollablePanel> ();
			panel.size = new Vector2 (763, 109);
			panel.relativePosition = new Vector2 (50, 0);
			panel.Reset ();
		}

		public void SetSprites (UISprite labe, string sprite)
		{
			UISprite label = labe;
			label.atlas = UIView.GetAView ().FindUIComponent<UIButton> ("CommercialLow").atlas;
			label.relativePosition = new Vector2 (12, 0);
			label.spriteName = sprite;
			label.size = new Vector2 (35, 25);
		}


		void GenBButton (BuildingInfo BuildingIm, int Offset, string type)
		{

			BBut = new UIButton ();
			if (type == "Residential") {
				BBut = BuildingPanels [1].AddUIComponent<UIButton> ();
			}
			if (type == "Office") {
				BBut = BuildingPanels [4].AddUIComponent<UIButton> ();
			}

			//BBut.name = name;
			BBut.size = new Vector2 (109, 100);
			BBut.atlas = BuildingIm.m_Atlas;
			BBut.normalFgSprite = BuildingIm.m_Thumbnail;
			BBut.focusedFgSprite = BuildingIm.m_Thumbnail + "Focused";
			BBut.hoveredFgSprite = BuildingIm.m_Thumbnail + "Hovered";
			BBut.pressedFgSprite = BuildingIm.m_Thumbnail + "Pressed";
			BBut.disabledFgSprite = BuildingIm.m_Thumbnail + "Disabled";
			BBut.isEnabled = enabled;
			BBut.tooltip = BuildingIm.m_InfoTooltipThumbnail;

			BBut.tooltipAnchor = UITooltipAnchor.Anchored;

			BBut.tooltipBox = UIView.GetAView ().FindUIComponent<UIPanel> ("InfoAdvancedTooltipDetail");
			BBut.relativePosition = new Vector2 (Offset, 5);
			BBut.eventClick += (sender, e) => BuildingBClicked (sender, e, BuildingIm);



			Debug.Log (Convert.ToString (name));

		}

		void BuildingBClicked (UIComponent component, UIMouseEventParameter eventParam, BuildingInfo Binf)
		{

			BuildingTool buildingTool = ToolsModifierControl.SetTool<BuildingTool> ();
			{
				buildingTool.m_prefab = Binf;
				buildingTool.m_relocate = 0;
				BuildingPanel.isVisible = true;
			}
		}

		void PloppablebuttonClicked (UIComponent component, UIMouseEventParameter eventParam)
		{
			component.Focus ();
			enabled = true;
			BuildingPanel.isVisible = true;
		}

		void TabClicked (UIComponent component, UIMouseEventParameter eventParam, UIScrollablePanel panel )
		{
			for (int i = 0; i <= types; i++) {
				BuildingPanels [i].isVisible = false;
			}
			//panel.isVisible = true;
			panel.isVisible = true;
			Debug.Log("ButtonClicked" + panel.name);
		}

		protected override void OnDisable ()
		{
			if (BuildingPanel != null)
				BuildingPanel.isVisible = false;
			base.OnDisable ();
		}

		protected override void OnEnable ()
		{
			UIView.GetAView ().FindUIComponent<UITabstrip> ("MainToolstrip").selectedIndex = -1;
			base.OnEnable ();
		}

		protected override void Awake ()
		{
			//this.m_dataLock = new object();

			base.Awake ();

		}


}
}




