using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;

namespace PloppableAI
{
	public class PloppableExperiment : IUserMod 
	{

		public string Name 
		{
			get { return "PloppableRCI"; }
		}

		public string Description 
		{
			get { return "Adds Ploppable RCI"; }
		}
	}

	//This is my current attempts at making the UI button with some code copied from various mods. Still learning how it all works. 

	public class PloppableTool : ToolBase
	{
		UIButton mainButton;
		BuildingInfo BuildingIm;
		UIPanel BuildingPanel;

		UITabstrip Tabs;
		bool m_active;
		UIScrollablePanel ScrollPanel;
		UIButton ResTab;
	
		UIButton BBut;
	
		protected override void Awake()
		{
			//this.m_dataLock = new object();
			this.m_active = false;
			base.Awake();
		}

		public void InitGui (System.Collections.Generic.List<string> BNames) {

			mainButton = UIView.GetAView ().FindUIComponent<UIButton> ("MarqueeBulldozer");

			if (mainButton == null) {
				
				var RCIButton = UIView.GetAView ().FindUIComponent<UIMultiStateButton> ("BulldozerButton");

				mainButton = RCIButton.parent.AddUIComponent<UIButton> ();
				mainButton.name = "RCIButton";
				mainButton.size = new Vector2 (36, 36);
				mainButton.normalBgSprite = "ZoningOptionMarquee";
				mainButton.focusedFgSprite = "ToolbarIconGroup6Focused";
				mainButton.hoveredFgSprite = "ToolbarIconGroup6Hovered";
				mainButton.relativePosition = new Vector2 (
					RCIButton.relativePosition.x + RCIButton.width / 2.0f - mainButton.width - RCIButton.width,
					RCIButton.relativePosition.y + RCIButton.height / 2.0f - mainButton.height / 2.0f
				);
				

				mainButton.eventClick += buttonClicked;

				BuildingPanel = UIView.GetAView ().FindUIComponent ("TSContainer").AddUIComponent<UIPanel> ();

				BuildingPanel.backgroundSprite = "SubcategoriesPanel";
				BuildingPanel.isVisible = false;
				BuildingPanel.name = "BuildingPanel";
				BuildingPanel.size = new Vector2 (859, 109);
				BuildingPanel.relativePosition = new Vector2 (0, 0);

				ScrollPanel = UIView.GetAView ().FindUIComponent ("BuildingPanel").AddUIComponent<UIScrollablePanel> ();
				ScrollPanel.size = new Vector2 (763, 109);
				ScrollPanel.relativePosition = new Vector2 (50, 0);

				Tabs = UIView.GetAView ().FindUIComponent ("BuildingPanel").AddUIComponent<UITabstrip> ();
				Tabs.size = new Vector2 (832, 25);
				Tabs.relativePosition = new Vector2 (13, -25);

				ResTab = Tabs.AddUIComponent<UIButton> ();
				ResTab.size = new Vector2 (58, 25);
				ResTab.relativePosition = new Vector2 (2, 0);
				ResTab.normalBgSprite = "SubBarButtonBase";
				ResTab.pressedBgSprite = "SubBarButtonBasePressed";
				ResTab.hoveredBgSprite = "SubBarButtonBaseHovered";
				ResTab.focusedBgSprite = "SubBarButtonBaseFocused";

				ResTab.normalFgSprite = "ZoningResidentialHigh";
				ResTab.pressedFgSprite = "ZoningResidentialHighPressed";
				ResTab.hoveredFgSprite = "ZoningResidentialHighHovered";
				ResTab.tabStrip = true;


				string[] Bdnames = new string[]{};
				Bdnames = BNames.ToArray ();
				int Offset = 0;
				int counter = 0;

				foreach (string test in Bdnames) {

					Debug.Log (Convert.ToString (test));
					BuildingIm = PrefabCollection<BuildingInfo>.FindLoaded (test);
					this.GenBButton (BuildingIm, Offset);
					Offset = Offset + 109;
					counter = counter + 1;
				}
			}

		}
		

		void GenBButton (BuildingInfo BuildingIm, int Offset){
			
			BBut = new UIButton ();
			BBut = ScrollPanel.AddUIComponent<UIButton> ();
			BBut.name = name;
			BBut.size = new Vector2 (109, 100);
			BBut.atlas = BuildingIm.m_Atlas;
			BBut.normalFgSprite = BuildingIm.m_Thumbnail;
			BBut.focusedFgSprite = BuildingIm.m_Thumbnail + "Focused";
			BBut.hoveredFgSprite = BuildingIm.m_Thumbnail + "Hovered";
			BBut.pressedFgSprite = BuildingIm.m_Thumbnail + "Pressed";
			BBut.disabledFgSprite = BuildingIm.m_Thumbnail + "Disabled";
			BBut.isEnabled = enabled;
			BBut.relativePosition = new Vector2 (Offset,5);
			BBut.eventClick += (sender, e) => BuildingBClicked(sender, e, BuildingIm);

			 

			Debug.Log (Convert.ToString (name));
			
		}

		void BuildingBClicked(UIComponent component, UIMouseEventParameter eventParam, BuildingInfo Binf ){

			BuildingTool buildingTool = ToolsModifierControl.SetTool<BuildingTool>();
			{
				buildingTool.m_prefab = Binf;
				buildingTool.m_relocate = 0;
			}
		}

		void buttonClicked(UIComponent component, UIMouseEventParameter eventParam)
		{
			this.enabled = true;
			BuildingPanel.isVisible = true;
		}
		protected override void OnDisable()
		{
			if (BuildingPanel != null)
				BuildingPanel.isVisible = false;
			base.OnDisable();
		}
		protected override void OnEnable()
		{
			UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
			base.OnEnable();
		}


	}
}