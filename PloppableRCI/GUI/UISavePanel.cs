
using ColossalFramework.UI;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace PloppableRICO
{
    /// <summary>
    ///This panel is in the middle column on the bottom. It contains the save and reset buttons, and will possibly contain more settings in the future. 
    /// </summary>

    public class UISavePanel : UIScrollablePanel
    {

        public BuildingData currentSelection;
        public UIButton save;
        public UIButton addLocal;
        public UIButton removeLocal;
        public UIButton reset;

        private static UISavePanel _instance;
        public static UISavePanel instance

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
            autoLayoutPadding.left = 5;
            autoLayoutPadding.right = 5;
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
        public void SelectionChanged(BuildingData buildingData)
        {
            currentSelection = buildingData;
        }

        private void SetupControls()
        {
            save = UIUtils.CreateButton(this);
            save.text = "Save";
            save.width = 140;

            addLocal = UIUtils.CreateButton(this);
            addLocal.text = "Add Local";
            addLocal.width = 140;

            addLocal.eventClick += (c, p) =>
            {
                if (!currentSelection.hasLocal)
                {

                    currentSelection.local = new RICOBuilding();
                    currentSelection.hasLocal = true;

                    //Set some basic settings for assets with no settings
                    currentSelection.local.name = currentSelection.name;
                    currentSelection.local.ricoEnabled = true;
                    currentSelection.local.service = "residential";
                    currentSelection.local.subService = "low";
                    currentSelection.local.level = 1;
                    currentSelection.local.uiCategory = "reslow";
                    currentSelection.local.constructionCost = 10;
                    currentSelection.local.homeCount = 10;
                    currentSelection.local.workplaces = new int[] {10, -1 ,-1, -1};


                    //If selected asset has author settings, copy those to local
                    if (currentSelection.hasAuthor)
                    {
                        currentSelection.local = (RICOBuilding)currentSelection.author.Clone();
                    }
                    else if (currentSelection.hasMod)
                    {
                        currentSelection.local = (RICOBuilding)currentSelection.mod.Clone();
                    }

                    currentSelection.local.name = currentSelection.name;
                    //currentSelection.local = (PloppableRICODefinition.Building)newlocal.Clone();

                    if (enabled) RICOSettingsPanel.instance.UpdateBuildingInfo(currentSelection);
                    if (enabled) RICOSettingsPanel.instance.UpdateSelection();
                    Save();
                }
            };

            removeLocal = UIUtils.CreateButton(this);
            removeLocal.eventClick += (c, p) =>
            {
                currentSelection.local = null;
                currentSelection.hasLocal = false;
                RICOSettingsPanel.instance.UpdateBuildingInfo(currentSelection);

                if (enabled) RICOSettingsPanel.instance.UpdateSelection();
                Save();

            };
            removeLocal.text = "Remove Local";
            removeLocal.width = 140;

            save.eventClick += (c, p) =>
            {
                Save();
            };
        }

        public void Save()
        {

            RICOSettingsPanel.instance.Save();

            if (!File.Exists("LocalRICOSettings.xml"))
            {

                var newlocalSettings = new PloppableRICODefinition();
                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));

                using (XmlWriter writer = XmlWriter.Create("LocalRICOSettings.xml"))
                {
                    xmlSerializer.Serialize(writer, newlocalSettings);
                }
            }

            if (File.Exists("LocalRICOSettings.xml"))
            {
                PloppableRICODefinition localSettings;
                var newlocalSettings = new PloppableRICODefinition();

                var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));

                using (StreamReader streamReader = new System.IO.StreamReader("LocalRICOSettings.xml"))
                {
                    localSettings = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                }

                //Loop though all buildings in the file. If they arent the current selection, write them back to file. 
                foreach (var buildingDef in localSettings.Buildings)
                {
                    if (buildingDef.name != currentSelection.name)
                    {
                        newlocalSettings.Buildings.Add(buildingDef);
                    }
                }

                //If current selection has local settings, add them to file. 
                if (currentSelection.hasLocal)
                {
                    newlocalSettings.Buildings.Add(currentSelection.local);
                }

                using (TextWriter writer = new StreamWriter("LocalRICOSettings.xml"))
                {
                    xmlSerializer.Serialize(writer, newlocalSettings);
                }

            }

        }
    }
}