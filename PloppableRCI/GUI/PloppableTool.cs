using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ColossalFramework.UI;
using ColossalFramework.DataBinding;

namespace PloppableRICO
{
    /// <summary>
    /// This class draws the RICO panel, populates it with building buttons, and activates the building tool when buttons are clicked. 
    /// </summary>
    /// 
    public class PloppableTool : ToolBase
    {
        private static PloppableTool _instance;
        public static PloppableTool instance
        {
            get { return _instance; }
        }

        UIButton PloppableButton;
        UIPanel BuildingPanel;
        UITabstrip Tabs;
        UIButton BuildingButton;

        int types = 11;
        UISprite[] TabSprites = new UISprite[12];
        UIScrollablePanel[] BuildingPanels = new UIScrollablePanel[12];
        UIButton[] TabButtons = new UIButton[12];

        UIButton[] LeftButtons = new UIButton[12];
        UIButton[] RightButtons = new UIButton[12];

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
            "Leisure",
            "Tourist"
        };

		private UITextureAtlas ingame;
		private UITextureAtlas thumbnails;


        public static void Initialize()
        {
            if (_instance == null)
            {
                GameObject gameController = GameObject.FindWithTag("GameController");
                _instance = gameController.AddComponent<PloppableTool>();
                _instance.name = "PloppableTool";
                _instance.DrawPloppablePanel();
                _instance.PopulateAssets();
                _instance.enabled = false;
                GameObject.FindObjectOfType<ToolController>().Tools[0].enabled = true;

            }
        }

		public void DrawPloppablePanel()
		{
			if (PloppableButton == null) {

				ingame = Resources.FindObjectsOfTypeAll<UITextureAtlas>().FirstOrDefault(a => a.name == "Ingame");
				thumbnails = Resources.FindObjectsOfTypeAll<UITextureAtlas>().FirstOrDefault(a => a.name == "Thumbnails");

					PloppableButton = UIView.GetAView ().FindUIComponent<UITabstrip> ("MainToolstrip").AddUIComponent<UIButton> (); //main button on in game tool strip.
					PloppableButton.size = new Vector2 (43, 49);
					PloppableButton.eventClick += PloppablebuttonClicked;
					PloppableButton.normalBgSprite = "ToolbarIconGroup6Normal";
					PloppableButton.normalFgSprite = "IconPolicyBigBusiness";
					PloppableButton.focusedBgSprite = "ToolbarIconGroup6Focused";
					PloppableButton.hoveredBgSprite = "ToolbarIconGroup6Hovered";
					PloppableButton.pressedBgSprite = "ToolbarIconGroup6Pressed";
					PloppableButton.disabledBgSprite = "ToolbarIconGroup6Disabled";
					PloppableButton.relativePosition = new Vector2 (800, 0);
					PloppableButton.name = "PloppableButton";
                    PloppableButton.tooltip = "Ploppable RICO";
	
					BuildingPanel = UIView.GetAView ().FindUIComponent ("TSContainer").AddUIComponent<UIPanel> (); //this is the base panel. 
					BuildingPanel.backgroundSprite = "SubcategoriesPanel";
					BuildingPanel.isVisible = false;
					BuildingPanel.name = "PloppableBuildingPanel";
					BuildingPanel.size = new Vector2 (859, 109);
					BuildingPanel.relativePosition = new Vector2 (0, 0);

					Tabs = UIView.GetAView ().FindUIComponent ("PloppableBuildingPanel").AddUIComponent<UITabstrip> ();
					Tabs.size = new Vector2 (832, 25);
					Tabs.relativePosition = new Vector2 (13, -25);
					Tabs.pivot = UIPivotPoint.BottomCenter;
					Tabs.padding = new RectOffset (0, 3, 0, 0);
		

					for (int i = 0; i <= types; i++) {

						BuildingPanels [i] = new UIScrollablePanel (); //draw scrollable panels
						BuildingPanels [i] = BuildingPanel.AddUIComponent<UIScrollablePanel> ();
						BuildingPanels [i].size = new Vector2 (763, 109);
						BuildingPanels [i].relativePosition = new Vector2 (50, 0);
						BuildingPanels [i].name = Names [i] + "Panel";
						BuildingPanels [i].isVisible = false;
						BuildingPanels [i].autoLayout = true;
						BuildingPanels [i].autoLayoutStart = LayoutStart.BottomLeft;
						BuildingPanels [i].builtinKeyNavigation = true;
						BuildingPanels [i].autoLayoutDirection = LayoutDirection.Horizontal;
						BuildingPanels [i].clipChildren = true;
						BuildingPanels [i].freeScroll = false;
						BuildingPanels [i].horizontalScrollbar = new UIScrollbar ();

						BuildingPanels [i].scrollWheelAmount = 109;
						BuildingPanels [i].horizontalScrollbar.stepSize = 1f;
						BuildingPanels [i].horizontalScrollbar.incrementAmount = 109f;

                    LeftButtons[i] = new UIButton();
                    RightButtons[i] = new UIButton();

                    LeftButtons[i] = BuildingPanel.AddUIComponent<UIButton>();
                    RightButtons[i] = BuildingPanel.AddUIComponent<UIButton>();

                    LeftButtons[i].size = new Vector2(32, 32);
                    LeftButtons[i].size = new Vector2(32, 32);
                    LeftButtons[i].normalBgSprite = "ArrowLeft";
                    LeftButtons[i].pressedBgSprite = "ArrowLeftPressed";
                    LeftButtons[i].hoveredBgSprite = "ArrowLeftHovered";
                    LeftButtons[i].disabledBgSprite = "ArrowLeftDisabled";

                    LeftButtons[i].relativePosition = new Vector3(16, 33);
                    RightButtons[i].relativePosition = new Vector3(812, 33);
                    RightButtons[i].normalBgSprite = "ArrowRight";
                    RightButtons[i].pressedBgSprite = "ArrowRightPressed";
                    RightButtons[i].hoveredBgSprite = "ArrowRightHovered";
                    RightButtons[i].disabledBgSprite = "ArrowRightDisabled";



                    TabButtons [i] = new UIButton ();  //draw RICO tabstrip. 
						TabButtons [i] = Tabs.AddUIComponent<UIButton> ();
						TabButtons [i].size = new Vector2 (58, 25);
						TabButtons [i].normalBgSprite = "SubBarButtonBase";
						TabButtons [i].disabledBgSprite = "SubBarButtonBaseDisabled";
						TabButtons [i].pressedBgSprite = "SubBarButtonBasePressed";
						TabButtons [i].hoveredBgSprite = "SubBarButtonBaseHovered";
						TabButtons [i].focusedBgSprite = "SubBarButtonBaseFocused";
						TabButtons [i].state = UIButton.ButtonState.Normal;
						TabButtons [i].isEnabled = enabled;
						TabButtons [i].name = Names [i] + "Button";
						TabButtons [i].tabStrip = true;

						TabSprites [i] = new UISprite ();
						TabSprites [i] = TabButtons [i].AddUIComponent<UISprite> ();

						if (i <= 5) {
						TabSprites [i].atlas = thumbnails;
							SetSprites (TabSprites [i], "Zoning" + Names [i]);

						} else {
							SetSprites (TabSprites [i], "IconPolicy" + Names [i]);
						}
					
				}

				//Couldnt get this to work in the loop.
		
				TabButtons [0].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [0], TabButtons[0], TabSprites[0]);
				TabButtons [1].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [1], TabButtons[1],TabSprites[1]);
				TabButtons [2].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [2], TabButtons[2],TabSprites[2]);
				TabButtons [3].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [3], TabButtons[3],TabSprites[3]);
				TabButtons [4].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [4], TabButtons[4],TabSprites[4]);
				TabButtons [5].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [5], TabButtons[5],TabSprites[5]);
				TabButtons [6].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [6], TabButtons[6],TabSprites[6]);
				TabButtons [7].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [7], TabButtons[7],TabSprites[7]);
				TabButtons [8].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [8], TabButtons[8],TabSprites[8]);
				TabButtons [9].eventClick += (sender, e) => TabClicked (sender, e, BuildingPanels [9], TabButtons[9],TabSprites[9]);

                TabButtons[10].eventClick += (sender, e) => TabClicked(sender, e, BuildingPanels[10], TabButtons[10], TabSprites[10]);
                TabButtons[11].eventClick += (sender, e) => TabClicked(sender, e, BuildingPanels[11], TabButtons[11], TabSprites[11]);

                BuildingPanels [0].isVisible = true; //start with lowres panel visible. 

                if (!Util.isADinstalled()) { //if AD is not installed, hide extra tabs

                    TabButtons[10].isVisible = false;
                    TabButtons[11].isVisible = false;
                }

                UIButton showThemeManager = UIUtils.CreateButton(Tabs);
                showThemeManager.width = 100f;
                showThemeManager.text = "RICO";
                showThemeManager.eventClick += (c, p) => RICOSettingsPanel.instance.Toggle();

            }
	
		}

        public void PopulateAssets()
        {

            foreach (var buildingData in XMLManager.xmlData.Values)
            {
                if (buildingData != null)
                {
                    var prefab = PrefabCollection<BuildingInfo>.FindLoaded(buildingData.name);

                    //if (buildingData.hasLocal)
                    // {
                    // DrawBuildingButton(prefab, buildingData.local.UICategory);
                    //  RemoveUIButton(prefab);
                    //break;
                    //}
                    if (buildingData.hasAuthor)
                    {
                        DrawBuildingButton(prefab, buildingData.author.UICategory);
                        RemoveUIButton(prefab);
                    }
                    //}
                    //else if (buildingData.hasMod)
                    //{
                     ////   DrawBuildingButton(prefab, buildingData.mod.UICategory);
                       // RemoveUIButton(prefab);
                       // break;
                    //}
                }
            }
        }

        public void RemoveUIButton(BuildingInfo prefab)
        {

            UIView uiView = UIView.GetAView();
            UIComponent refButton = uiView.FindUIComponent(prefab.name);

            if (refButton != null)
            {
                refButton.isVisible = false;
                refButton.isEnabled = false;
                refButton.Disable();
            }

            //refresh panels after buttons are removed
            var monumentsPanels = GameObject.FindObjectsOfType<MonumentsPanel>();
            foreach (MonumentsPanel panel in monumentsPanels) {
                panel.RefreshPanel();
            }
        }

        public void DrawPanels (UIScrollablePanel panel, string name)
		{
			
			panel = UIView.GetAView ().FindUIComponent ("PloppableBuildingPanel").AddUIComponent<UIScrollablePanel> ();
			panel.size = new Vector2 (763, 109);
			panel.relativePosition = new Vector2 (50, 0);
			panel.Reset ();
		}

		public void SetSprites (UISprite labe, string sprite)
		{
			UISprite label = labe;
			label.isInteractive = false;
			label.relativePosition = new Vector2 (12, 0);
			label.spriteName = sprite;
			label.size = new Vector2 (35, 25);
		}

		void DrawBuildingButton (BuildingInfo BuildingPrefab, string type)
		{

			BuildingButton = new UIButton (); //draw button on appropriate panel. 
			if (type == "reslow") {
				BuildingButton = BuildingPanels [0].AddUIComponent<UIButton> ();
			}
			if (type == "reshigh") {
				BuildingButton = BuildingPanels [1].AddUIComponent<UIButton> ();
			}
			if (type == "comlow") {
				BuildingButton = BuildingPanels [2].AddUIComponent<UIButton> ();
			}
			if (type == "comhigh") {
				BuildingButton = BuildingPanels [3].AddUIComponent<UIButton> ();
			}
			if (type == "office") {
				BuildingButton = BuildingPanels [4].AddUIComponent<UIButton> ();
			}
			if (type == "industrial") {
				BuildingButton = BuildingPanels [5].AddUIComponent<UIButton> ();
			}
			if (type == "farming") {
				BuildingButton = BuildingPanels [6].AddUIComponent<UIButton> ();
			}
			if (type == "oil") {
				BuildingButton = BuildingPanels [8].AddUIComponent<UIButton> ();
			}
			if (type == "forest") {
				BuildingButton = BuildingPanels [7].AddUIComponent<UIButton> ();
			}
			if (type == "ore") {
				BuildingButton = BuildingPanels [9].AddUIComponent<UIButton> ();
			}
            if (type == "leisure")
            {
                if (Util.isADinstalled())
                {
                    BuildingButton = BuildingPanels[10].AddUIComponent<UIButton>();
                }
                else {
                    BuildingButton = BuildingPanels[3].AddUIComponent<UIButton>();
                }
            }
            if (type == "tourist")
            {
                if (Util.isADinstalled())
                {
                    BuildingButton = BuildingPanels[11].AddUIComponent<UIButton>();
                }
                else {
                    BuildingButton = BuildingPanels[3].AddUIComponent<UIButton>();
                }
            }

            BuildingButton.size = new Vector2 (109, 100); //apply settings to building buttons. 
			BuildingButton.atlas = BuildingPrefab.m_Atlas;

			BuildingButton.normalFgSprite = BuildingPrefab.m_Thumbnail;
			BuildingButton.focusedFgSprite = BuildingPrefab.m_Thumbnail + "Focused";
			BuildingButton.hoveredFgSprite = BuildingPrefab.m_Thumbnail + "Hovered";
			BuildingButton.pressedFgSprite = BuildingPrefab.m_Thumbnail + "Pressed";
			BuildingButton.disabledFgSprite = BuildingPrefab.m_Thumbnail + "Disabled";
			BuildingButton.objectUserData = BuildingPrefab;
			BuildingButton.horizontalAlignment = UIHorizontalAlignment.Center;
			BuildingButton.verticalAlignment = UIVerticalAlignment.Middle;
			BuildingButton.pivot = UIPivotPoint.TopCenter;
	

			string localizedTooltip = BuildingPrefab.GetLocalizedTooltip ();
			int hashCode = TooltipHelper.GetHashCode(localizedTooltip);
			UIComponent tooltipBox = GeneratedPanel.GetTooltipBox(hashCode);

			BuildingButton.tooltipAnchor = UITooltipAnchor.Anchored;
	
			BuildingButton.isEnabled = enabled;
			BuildingButton.tooltip = localizedTooltip;
			BuildingButton.tooltipBox = tooltipBox;
			BuildingButton.eventClick += (sender, e) => BuildingBClicked (sender, e, BuildingPrefab);
			BuildingButton.eventMouseHover += (sender, e) => BuildingBHovered (sender, e, BuildingPrefab);
		

		}

		void BuildingBClicked (UIComponent component, UIMouseEventParameter eventParam, BuildingInfo Binf)
		{
			

			var buildingTool = ToolsModifierControl.SetTool<BuildingTool> ();
			{
				buildingTool.m_prefab = Binf;
				buildingTool.m_relocate = 0;
				BuildingPanel.isVisible = true;
			}
		}

		void BuildingBHovered (UIComponent component, UIMouseEventParameter eventParam, BuildingInfo Binf)
		{

			var tooltipBoxa =  UIView.GetAView ().FindUIComponent<UIPanel> ("InfoAdvancedTooltip");
			var tooltipBox =  UIView.GetAView ().FindUIComponent<UIPanel> ("InfoAdvancedTooltipDetail");

			var spritea = tooltipBoxa.Find<UISprite> ("Sprite");
			var sprite = tooltipBox.Find<UISprite> ("Sprite");

			sprite.atlas = Binf.m_Atlas;
			spritea.atlas = Binf.m_Atlas;
		}

		void PloppablebuttonClicked (UIComponent component, UIMouseEventParameter eventParam)
		{
			component.Focus ();
			enabled = true;
			BuildingPanel.isVisible = true;
		}

		void TabClicked (UIComponent component, UIMouseEventParameter eventParam, UIScrollablePanel panel, UIButton button, UISprite sprite)
		{
			foreach (UIScrollablePanel pan in BuildingPanels){

				pan.isVisible = false;
			}

			panel.isVisible = true;

			for (int i = 0; i <= types; i++) {
				
				if (i <= 5) {
					TabSprites [i].spriteName =  "Zoning" + Names[i];

				} else {
					TabSprites[i].spriteName =  "IconPolicy" + Names[i];
				}
			}

            if (sprite.spriteName != "IconPolicyLeisure" || sprite.spriteName != "IconPolicyTourist") //There are no focused AD special com sprites
            {
                sprite.spriteName = sprite.spriteName + "Focused";
            }
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

	}
}




