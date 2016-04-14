using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace PloppableRICO
{
    public class UIBuildingOptions : UIScrollablePanel
    {

        public BuildingData currentSelection;
        //Enable RICO
        public UICheckBox ricoEnabled;
        public UIPanel enableRICOPanel;

        public UIDropDown service;
        public UIDropDown subService;
        public UIDropDown level;
        public UIDropDown uiCategory;

        //Educatino Ratio
        public UICheckBox educationRatiosEnabled;
        public UIPanel educationPanel;

        public UITextField uneducated;
        public UITextField educated;
        public UITextField welleducated;
        public UITextField highlyeducated;

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
            service.AddItem("None");
            service.AddItem("Residential");
            service.AddItem("Industrial");
            service.AddItem("Office");
            service.AddItem("Commercial");
            service.AddItem("Extractor");
            service.selectedIndex = 0;
            service.eventSelectedIndexChanged += DropDownsChanged;

            subService = UIUtils.CreateDropDown(enableRICOPanel, 30, "Sub-Service");
            subService.AddItem("Generic");
            subService.AddItem("Forest");
            subService.AddItem("Oil");
            subService.AddItem("Ore");
            subService.AddItem("Farming");
            subService.selectedIndex = 0;

            uiCategory = UIUtils.CreateDropDown(enableRICOPanel, 60, "UI Category");
            uiCategory.AddItem("None");
            uiCategory.AddItem("Residential Low");
            uiCategory.AddItem("Residential High");
            uiCategory.AddItem("Industrial");
            uiCategory.AddItem("Office");
            uiCategory.AddItem("Commercial High");
            uiCategory.AddItem("Commercial Low");
            uiCategory.AddItem("Oil");
            uiCategory.AddItem("Farming");
            uiCategory.AddItem("Forest");
            uiCategory.AddItem("Ore");
            uiCategory.AddItem("Tourist");
            uiCategory.AddItem("Leisure");
            uiCategory.selectedIndex = 0;
            uiCategory.eventSelectedIndexChanged += DropDownsChanged;

            //Construction Cost
            constructionCostEnabled = UIUtils.CreateCheckBox(this, "Enable Construction Cost");
            constructionPanel = UIUtils.CreatePanel(this, 30, constructionCostEnabled);
            construction = UIUtils.CreateTextField(constructionPanel, 0, "Construction Cost");

            //Manual Panel
            manualWorkersEnabled = UIUtils.CreateCheckBox(this, "Enable Manual Worker Count");
            manualPanel = UIUtils.CreatePanel(this, 30, manualWorkersEnabled);

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

        public void ChecksChanged(UIComponent c, bool state) {

            if (c = ricoEnabled) {
            }
            if (c = ricoEnabled)
            {
            }
            if (c = ricoEnabled)
            {
            }
            if (c = ricoEnabled)
            {
            }
        }

        public void DropDownsChanged(UIComponent c, int value) {

                if (value == 0) currentSelection.local.service = "none";
                if (value == 1) currentSelection.local.service = "residential";
                if (value == 2) currentSelection.local.service = "industrial";
                if (value == 3) currentSelection.local.service = "office";
                if (value == 4) currentSelection.local.service = "commercial";
                if (value == 5) currentSelection.local.service = "extractor";

                UpdateElements(currentSelection.local);
        }

        public void Save() {

            //save current xml settings for selected prefab
        }

        public void SelectionChanged(BuildingData buildingData) {

            if (buildingData.hasMod)
            {
                UpdateElements(buildingData.mod);
            }
            else if (buildingData.hasAuthor)
            {
                UpdateElements(buildingData.author);
            }
            else if (buildingData.hasLocal)
            {
                UpdateElements(buildingData.local);
            }
        }

        public void NoSettings() {

            ricoEnabled.isChecked = false;
            educationRatiosEnabled.isChecked = false;
            educationRatiosEnabled.parent.isVisible = false;
            constructionCostEnabled.isChecked = false;
            constructionCostEnabled.parent.isVisible = false;
            pollutionEnabled.isChecked = false;
            pollutionEnabled.parent.isVisible = false;
            manualWorkersEnabled.isChecked = false;
            manualWorkersEnabled.parent.isVisible = false;
        }

        public void UpdateElements(PloppableRICODefinition.Building buildingData) {

  
            pollutionEnabled.isVisible = true;
            manualWorkersEnabled.isVisible = true;
            educationRatiosEnabled.isVisible = true;

            if (buildingData.ricoEnabled == true)
            {
                ricoEnabled.isChecked = true;
            }
            else {
                NoSettings();
            }

            if (buildingData.service == "residential")
            {
                pollutionEnabled.isChecked = false;
                pollutionEnabled.parent.isVisible = false;

                manualWorkersEnabled.isChecked = false;
                manualWorkersEnabled.parent.isVisible = false;

                constructionCostEnabled.isChecked = false;
                educationRatiosEnabled.parent.isVisible = false;
                service.selectedIndex = 1;
            }
            else if (buildingData.service == "office")
            {

                manualWorkersEnabled.parent.isVisible = true;
                educationRatiosEnabled.parent.isVisible = true;

                service.selectedIndex = 3;
            }
            else if (buildingData.service == "industrial")
            {
                service.selectedIndex = 3;
            }
            else if (buildingData.service == "commercial")
            {
                service.selectedIndex = 3;
            }
            else if (buildingData.service == "none")
            {
                service.selectedIndex = 0;
                educationRatiosEnabled.isChecked = false;
                educationRatiosEnabled.isVisible = false;
                constructionCostEnabled.isChecked = false;
                constructionCostEnabled.isVisible = false;
                pollutionEnabled.isChecked = false;
                pollutionEnabled.isVisible = false;
                manualWorkersEnabled.isChecked = false;
                manualWorkersEnabled.isVisible = false;
            }

                if (buildingData.educationRatioEnabled == true) educationRatiosEnabled.isChecked = true;
                else educationRatiosEnabled.isChecked = false;

                if (buildingData.constructionCostEnabled == true) constructionCostEnabled.isChecked = true;
                else constructionCostEnabled.isChecked = false;

                if (buildingData.pollutionEnabled == true) pollutionEnabled.isChecked = true;
                else pollutionEnabled.isChecked = false;

                if (buildingData.manualCountEnabled == true) manualWorkersEnabled.isChecked = true;
                else manualWorkersEnabled.isChecked = false;

                if (buildingData.ricoEnabled == true) ricoEnabled.isChecked = true;
                else {
                    ricoEnabled.isChecked = false;
                    educationRatiosEnabled.isChecked = false;
                    constructionCostEnabled.isChecked = false;
                    pollutionEnabled.isChecked = false;
                    manualWorkersEnabled.isChecked = false;
                }
          }
    }
}