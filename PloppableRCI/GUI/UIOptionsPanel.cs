using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace PloppableRICO
{
    /// <summary>
    ///The far right column of the settigns panel. Contains the drop downs and entry fields that allows players to assign RICO settings. 
    /// </summary>

    public class UIBuildingOptions : UIScrollablePanel

    {

        string[] Service = new string[]{
        "None",
        "Residential",
        "Industrial",
        "Office",
        "Commercial",
        "Extractor"
        };

        string[] OfficeSub = new string[]{
        "None",
        };

        string[] ResSub = new string[]{
        "High",
        "Low",
        };

        string[] ComSub = new string[]{
        "High",
        "Low",
        "Tourist",
        "Leisure",
        };

        string[] IndustrialSub = new string[]{
        "Generic",
        "Forest",
        "Oil",
        "Ore",
        "Farming"
        };

        string[] ExtractorSub = new string[]{
        "Forest",
        "Oil",
        "Ore",
        "Farming"
        };

        string[] Level = new string[]{
        "1",
        "2",
        "3",
        };

        string[] resLevel = new string[]{
        "1",
        "2",
        "3",
        "4",
        "5"
        };

        string[] extLevel = new string[]{
        "1",
        };

        string[] UICategory = new string[]{
           "Residential Low",
           "Residential High",
           "Commercial Low",
           "Commercial High",
           "Office",
           "Industrial",
            "Farming",
            "Oil",
            "Forest",
            "Ore",
            "Tourist",
            "Leisure",
        };

        public bool disableEvents;
        public PloppableRICODefinition.Building currentSelection;
        //Enable RICO
        public UICheckBox ricoEnabled;
        public UIPanel enableRICOPanel;

        public UIDropDown service;
        public UIDropDown subService;
        public UIDropDown level;
        public UIDropDown uiCategory;

        //Education Ratio
        public UICheckBox educationRatiosEnabled;
        public UIPanel educationPanel;

        public UITextField uneducated;
        public UITextField educated;
        public UITextField welleducated;
        public UITextField highlyeducated;

        public UITextField manual;

        public UICheckBox manualHomesEnabled;
        public UIPanel manualHomesPanel;
        public UITextField homes;

        //Pollution
        public UICheckBox pollutionEnabled;
        public UIPanel pollutionPanel;

        public UICheckBox popBalanceEnabled;
        public UICheckBox manualWorkersEnabled;
        public UIPanel manualPanel;

        //Construction
        public UICheckBox constructionCostEnabled;
        public UIPanel constructionPanel;
        public UITextField construction;

        public UIButton save;

        private static UIBuildingOptions _instance;
        public static UIBuildingOptions instance

        {
            get { return _instance; }
        }

        public override void Start()
        {
            base.Start();

            _instance = this;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            //padding = new RectOffset(5, 5, 5, 0);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;
            builtinKeyNavigation = true;
            clipChildren = true;
            freeScroll = false;
            scrollWheelDirection = UIOrientation.Vertical;
            verticalScrollbar = new UIScrollbar();
            scrollWheelAmount = 10;
            verticalScrollbar.stepSize = 1f;
            verticalScrollbar.incrementAmount = 10f;
            SetupControls();
        }

        private void SetupControls()
        {
        
            ricoEnabled = UIUtils.CreateCheckBox(this, "Enable RICO" );
            enableRICOPanel = UIUtils.CreatePanel(this, 120, ricoEnabled);

            service = UIUtils.CreateDropDown(enableRICOPanel, 0, "Service");
            service.items = Service;
            service.selectedIndex = 0;
            service.eventSelectedIndexChanged += UpdateService; 

            subService = UIUtils.CreateDropDown(enableRICOPanel, 30, "Sub-Service");
            subService.selectedIndex = 0;

            uiCategory = UIUtils.CreateDropDown(enableRICOPanel, 60, "UI Category");
            uiCategory.selectedIndex = 0;
            uiCategory.items = UICategory;

            level = UIUtils.CreateDropDown(enableRICOPanel, 90, "Level");
            level.selectedIndex = 0;
            level.items = Level;

            //Construction Cost
            constructionCostEnabled = UIUtils.CreateCheckBox(this, "Enable Construction Cost");
            constructionPanel = UIUtils.CreatePanel(this, 30, constructionCostEnabled);
            construction = UIUtils.CreateTextField(constructionPanel, 0, "Construction Cost");

            //Manual Panel

            manualWorkersEnabled = UIUtils.CreateCheckBox(this, "Enable Manual Worker Count");
            manualPanel = UIUtils.CreatePanel(this, 30, manualWorkersEnabled);
            manual = UIUtils.CreateTextField(manualPanel, 0, "Worker Count");

            //Manual Homes
            manualHomesEnabled = UIUtils.CreateCheckBox(this, "Enable Manual Home Count");
            manualHomesPanel = UIUtils.CreatePanel(this, 30, manualHomesEnabled);
            homes = UIUtils.CreateTextField(manualHomesPanel, 0, "Home Count");


            //Education Ratio Panel
            educationRatiosEnabled = UIUtils.CreateCheckBox(this, "Enable Education");
            educationPanel = UIUtils.CreatePanel(this, 120, educationRatiosEnabled);
            uneducated = UIUtils.CreateTextField(educationPanel, 0, "Uneducated");
            educated = UIUtils.CreateTextField(educationPanel, 30, "Educated");
            welleducated = UIUtils.CreateTextField(educationPanel, 60, "Well Educated");
            highlyeducated = UIUtils.CreateTextField(educationPanel, 90, "Highly Educated");

            pollutionEnabled = UIUtils.CreateCheckBox(this, "Enable Pollution");
            ricoEnabled.eventCheckChanged += ChecksChanged;
        }

        public void ChecksChanged(UIComponent c, bool text) {

            //Update options panel if you disable RICO. 

            if (!disableEvents)
            {
                if (!text)
                {
                    NoSettings();
                }
                else
                {
                    UpdateElements(currentSelection.service);
                    UpdateValues(currentSelection);
                }
            }
        }

        public void UpdateService(UIComponent c, int value)
        {
            //Update options panel if the service is changed. 

            if (!disableEvents)
            {
                if (value == 0) UpdateElements("none");
                else if (value == 1) UpdateElements("residential");
                else if (value == 2) UpdateElements("industrial");
                else if (value == 3) UpdateElements("office");
                else if (value == 4) UpdateElements("commercial");
                else if (value == 5) UpdateElements("extractor");
            }
        }

        public void SaveRICO() {

            //Reads current settings from UI elements, and saves them to the XMLData. 

            if (service.selectedIndex == 0) currentSelection.service = "none";
            else if (service.selectedIndex == 1) currentSelection.service = "residential";
            else if (service.selectedIndex == 2) currentSelection.service = "industrial";
            else if (service.selectedIndex == 3) currentSelection.service = "office";
            else if (service.selectedIndex == 4) currentSelection.service = "commercial";
            else if (service.selectedIndex == 5) currentSelection.service = "extractor";

            currentSelection.constructionCost = int.Parse(construction.text);
            currentSelection.workplaceCount = int.Parse(manual.text);
            currentSelection.homeCount = int.Parse(homes.text);


            if (uiCategory.selectedIndex == 0)  currentSelection.UICategory = "reslow";
            else if (uiCategory.selectedIndex == 1) currentSelection.UICategory = "reshigh"; 
            else if (uiCategory.selectedIndex == 2) currentSelection.UICategory = "comlow"; 
            else if (uiCategory.selectedIndex == 3) currentSelection.UICategory = "comhigh";
            else if (uiCategory.selectedIndex == 4) currentSelection.UICategory = "office";
            else if (uiCategory.selectedIndex == 5) currentSelection.UICategory = "industrial";
            else if (uiCategory.selectedIndex == 6) currentSelection.UICategory = "farming";
            else if (uiCategory.selectedIndex == 7) currentSelection.UICategory = "oil";
            else if (uiCategory.selectedIndex == 8) currentSelection.UICategory = "forest";
            else if (uiCategory.selectedIndex == 9) currentSelection.UICategory = "ore";
            else if (uiCategory.selectedIndex == 10) currentSelection.UICategory = "leisure";
            else if (uiCategory.selectedIndex == 11) currentSelection.UICategory = "tourist";

            currentSelection.constructionCostEnabled = constructionCostEnabled.isChecked;
            currentSelection.manualHomeEnabled = manualHomesEnabled.isChecked;
            currentSelection.manualWorkerEnabled = manualWorkersEnabled.isChecked;
            currentSelection.ricoEnabled = ricoEnabled.isChecked;

            Debug.Log("Saved Data");

        }

        public void SelectionChanged(BuildingData buildingData) {

            //When dropdowns are updated, this disables the event logic
            disableEvents = true;

            NoSettings();

            //If selected asset has local settings, update option UI elements with those settings. 
            if (buildingData.hasLocal)
            {
                UpdateElements(buildingData.local.service);
                UpdateValues(buildingData.local);
               
                currentSelection = buildingData.local;
                disableEvents = false;
                return;
            }
            else if (buildingData.hasAuthor)
            {
                UpdateElements(buildingData.author.service);
                currentSelection = buildingData.author;
                UpdateValues(buildingData.author);
                disableEvents = false;
                return;
            }
            else if (buildingData.hasMod)
            {
                UpdateElements(buildingData.mod.service);
                currentSelection = buildingData.mod;
                disableEvents = false;
                return;
            }
            else {

                NoSettings();
            }

            disableEvents = false;
        }

        public void NoSettings() {

            //Hide all options if selected building has no RICO settings. 

            ricoEnabled.isChecked = false;

            educationRatiosEnabled.isChecked = false;
            educationRatiosEnabled.parent.isVisible = false;

            constructionCostEnabled.isChecked = false;
            constructionCostEnabled.parent.isVisible = false;

            pollutionEnabled.isChecked = false;
            pollutionEnabled.parent.isVisible = false;

            manualWorkersEnabled.isChecked = false;
            manualWorkersEnabled.parent.isVisible = false;

            manualHomesEnabled.isChecked = false;
            manualHomesEnabled.parent.isVisible = false;
        }

        public void UpdateValues(PloppableRICODefinition.Building buildingData)
        {
            //Updates the values in the RICO options panel to match the selected building. 
          
                if (buildingData.service == "residential")
                {
                    manualHomesEnabled.isChecked = buildingData.manualHomeEnabled;
                    service.selectedIndex = 1;
                    if (buildingData.subService == "high") subService.selectedIndex = 0;
                    else subService.selectedIndex = 1;
                }

                else if (buildingData.service == "industrial")
                {
                    if (buildingData.manualWorkerEnabled) manualWorkersEnabled.isChecked = true;
                    service.selectedIndex = 2;
                    //subService.items = IndustrialSub;
                }

                else if (buildingData.service == "office")
                {
                    manualWorkersEnabled.isChecked = buildingData.manualWorkerEnabled;
                    Debug.Log(buildingData.manualWorkerEnabled);
                    service.selectedIndex = 3;
                    subService.selectedIndex = 0;
                }

                else if (buildingData.service == "commercial")
                {
                    if (buildingData.manualWorkerEnabled) manualWorkersEnabled.isChecked = true;
                    service.selectedIndex = 4;
                    //subService.items = ComSub;
                }

                else if (buildingData.service == "extractor")
                {
                    if (buildingData.manualWorkerEnabled) manualWorkersEnabled.isChecked = true;
                    service.selectedIndex = 5;
                    subService.items = ExtractorSub;
                }

                if (buildingData.UICategory == "reslow") uiCategory.selectedIndex = 0;
                else if (buildingData.UICategory == "reshigh") uiCategory.selectedIndex = 1;
                else if (buildingData.UICategory == "comlow") uiCategory.selectedIndex = 2;
                else if (buildingData.UICategory == "comhigh") uiCategory.selectedIndex = 3;
                else if (buildingData.UICategory == "office") uiCategory.selectedIndex = 4;
                else if (buildingData.UICategory == "industrial") uiCategory.selectedIndex = 5;
                else if (buildingData.UICategory == "farming") uiCategory.selectedIndex = 6;
                else if (buildingData.UICategory == "oil") uiCategory.selectedIndex = 7;
                else if (buildingData.UICategory == "forest") uiCategory.selectedIndex = 8;
                else if (buildingData.UICategory == "ore") uiCategory.selectedIndex = 9;
                else if (buildingData.UICategory == "leisure") uiCategory.selectedIndex = 10;
                else if (buildingData.UICategory == "tourist") uiCategory.selectedIndex = 11;

                level.selectedIndex = (buildingData.level - 1);
                manual.text = buildingData.workplaceCount.ToString();
                homes.text = buildingData.homeCount.ToString();
                construction.text = buildingData.constructionCost.ToString();

               ricoEnabled.isChecked = buildingData.ricoEnabled;
               constructionCostEnabled.isChecked = buildingData.constructionCostEnabled;
            
          
        }

        public void UpdateElements(string service) {

            //Reconfigure the RICO options panel to display relevant options for a given service.
            //This simply hides/shows different option fields for the various services. 

                educationRatiosEnabled.parent.isVisible = true;
                educationRatiosEnabled.isVisible = true;

                constructionCostEnabled.parent.isVisible = true;
                constructionCostEnabled.isVisible = true;

                pollutionEnabled.parent.isVisible = false;
                pollutionEnabled.isVisible = false;

                manualWorkersEnabled.parent.isVisible = true;
                manualWorkersEnabled.isVisible = true;


                if (service == "residential")
                {
                    educationPanel.isVisible = false;

                    educationRatiosEnabled.parent.isVisible = false;
                    educationRatiosEnabled.isVisible = false;

                    manualHomesEnabled.parent.isVisible = true;
                    manualHomesEnabled.isVisible = true;

                    manualWorkersEnabled.isVisible = false;
                    manualWorkersEnabled.parent.isVisible = false;

                    level.items = resLevel;
                    subService.items = ResSub;
                }
                else if (service == "office")
                {
                    level.items = Level;
                    subService.items = OfficeSub;
                }
                else if (service == "industrial")
                {
                    pollutionEnabled.parent.isVisible = true;

                    level.items = Level;
                    subService.items = IndustrialSub;
                }
                else if (service == "extractor")
                {
                    pollutionEnabled.parent.isVisible = true;

                    level.items = extLevel;
                    subService.items = ExtractorSub;
                }
                else if (service == "commercial")
                {

                    level.items = Level;
                    subService.items = ComSub;
                }
                else if (service == "none")
                {
                    educationRatiosEnabled.isVisible = false;
                    constructionCostEnabled.isVisible = false;
                    pollutionEnabled.isVisible = false;
                    manualWorkersEnabled.isVisible = false;
                }
            
        }
    }
}