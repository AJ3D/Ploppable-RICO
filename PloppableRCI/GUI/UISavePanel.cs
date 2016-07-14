
using ColossalFramework.UI;
using System.IO;
using System.Xml.Serialization;

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

            addLocal = UIUtils.CreateButton(this);
            addLocal.text = "Add Local";

            addLocal.eventClick += (c, p) =>
            {
                if (currentSelection.hasLocal == false) {

                    var newlocal = new RICOBuilding();
                    currentSelection.hasLocal = true;
                    currentSelection.local = newlocal;
                    currentSelection.local.name = currentSelection.name;

                }

            };

            removeLocal = UIUtils.CreateButton(this);
            removeLocal.text = "Remove Local";

            save.eventClick += (c, p) =>
            {
                //Serialize the new RICO settings. 
                //XMLManager.SaveLocal(currentSelection.local);
                RICOSettingsPanel.instance.Save();

                if (File.Exists("LocalRICOSettings.xml") && currentSelection.local != null)
                {
                    
                    PloppableRICODefinition localSettings;
                    var newlocalSettings = new PloppableRICODefinition();

                    var xmlSerializer = new XmlSerializer(typeof(PloppableRICODefinition));

                    using (StreamReader streamReader = new System.IO.StreamReader("LocalRICOSettings.xml"))
                    {
                        localSettings = xmlSerializer.Deserialize(streamReader) as PloppableRICODefinition;
                    }
                    
                    foreach (var buildingDef in localSettings.Buildings)
                    {
                        if (buildingDef.name != currentSelection.local.name)
                        {
                            newlocalSettings.Buildings.Add(buildingDef);
                        }
                    }
                    
                    //newBuildingData.name = newBuildingData.name;
                    newlocalSettings.Buildings.Add(currentSelection.local);

                    using (TextWriter writer = new StreamWriter("LocalRICOSettings.xml"))
                    {
                        xmlSerializer.Serialize(writer, newlocalSettings);
                    }
                    
                }

            };

            reset = UIUtils.CreateButton(this);
            reset.text = "Reset";

        }
    }
}