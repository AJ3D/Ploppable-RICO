using ColossalFramework;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PloppableRICO
{
    /// <summary>
    ///The base class of the RICO settings panel. Its based on SamsamTS's Building Themes panel. Many thanks to him for his work. 
    /// </summary>

    public class RICOSettingsPanel : UIPanel
    {

        #region Constant values
        private const float LEFT_WIDTH = 400;
        private const float RIGHT_WIDTH = 250;
        private const float MIDDLE_WIDTH = 250;
        private const float HEIGHT = 550;
        private const float SPACING = 5;
        private const float TITLE_HEIGHT = 40;
        #endregion

        public BuildingData currentSelection;
        private UITitleBar m_title;
        private UIBuildingFilter m_filter;
        private UIBuildingOptions m_buildingOptions;
        public UISavePanel m_savePanel;
        private UIBuildingPreview m_buildingPreview;
        private UIFastList m_buildingSelection;

        private static GameObject _gameObject;
        private static RICOSettingsPanel _instance;
        public static RICOSettingsPanel instance
        {
            get { return _instance; }
        }

        public static void Initialize()
        {
            try
            {
                // Destroy the UI if already exists
                _gameObject = GameObject.Find("RICOSettingsPanel");
                Destroy();

                // Creating our own gameObect, helps finding the UI in ModTools
                _gameObject = new GameObject("RICOSettingsPanel");
                _gameObject.transform.parent = UIView.GetAView().transform;
                _instance = _gameObject.AddComponent<RICOSettingsPanel>();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Destroy()
        {
            try
            {
                if (_gameObject != null)
                    GameObject.Destroy(_gameObject);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Toggle()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show(true);
            }
        }

        public override void Start()
        {
            base.Start();

            try
            {
                backgroundSprite = "UnlockingPanel2";
                isVisible = false;
                canFocus = true;
                isInteractive = true;
                width =  LEFT_WIDTH + MIDDLE_WIDTH + RIGHT_WIDTH + (SPACING * 4);
                height = TITLE_HEIGHT + HEIGHT + (SPACING * 2);
                relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

                SetupControls();
            }
            catch (Exception e)
            {
                    Destroy();
            }
        }

        private void SetupControls()
        {
            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "RICO Settings";
            m_title.iconSprite = "ToolbarIconZoomOutCity";

            // Filter
            m_filter = AddUIComponent<UIBuildingFilter>();
            m_filter.width = width - SPACING * 2;
            m_filter.height = 40;
            m_filter.relativePosition = new Vector3(SPACING, TITLE_HEIGHT);

            m_filter.eventFilteringChanged += (c, i) =>
            {
                if (i == -1) return;

                int listCount = m_buildingSelection.rowsData.m_size;
                float pos = m_buildingSelection.listPosition;

                m_buildingSelection.selectedIndex = -1;

                m_buildingSelection.rowsData = Filter();
              
            };

            UIPanel left = AddUIComponent<UIPanel>();
            left.width = LEFT_WIDTH;
            left.height = HEIGHT - m_filter.height;
            left.relativePosition = new Vector3(SPACING, TITLE_HEIGHT + m_filter.height + SPACING);

            UIPanel middle = AddUIComponent<UIPanel>();
            middle.width = MIDDLE_WIDTH;
            middle.height = HEIGHT - m_filter.height;
            middle.relativePosition = new Vector3(LEFT_WIDTH + (SPACING * 2), TITLE_HEIGHT + m_filter.height + SPACING);

            UIPanel right = AddUIComponent<UIPanel>();
            right.width = RIGHT_WIDTH;
            right.height = HEIGHT - m_filter.height;
            right.relativePosition = new Vector3(LEFT_WIDTH + MIDDLE_WIDTH + (SPACING * 3), TITLE_HEIGHT + m_filter.height + SPACING);


            m_buildingPreview = middle.AddUIComponent<UIBuildingPreview>();
            m_buildingPreview.width = middle.width;
            m_buildingPreview.height = (middle.height - SPACING) / 2;
            m_buildingPreview.relativePosition = Vector3.zero;

            m_savePanel = middle.AddUIComponent<UISavePanel>();
            m_savePanel.width = middle.width;
            m_savePanel.height = ((middle.height - SPACING) / 2) - 40; 
            m_savePanel.relativePosition = new Vector3(0, m_buildingPreview.height + SPACING);

            m_buildingSelection = UIFastList.Create<UIBuildingItem>(left);
            m_buildingSelection.backgroundSprite = "UnlockingPanel";
            m_buildingSelection.width = left.width;
            m_buildingSelection.height = left.height - 40;
            m_buildingSelection.canSelect = true; 
            m_buildingSelection.rowHeight = 40;
            m_buildingSelection.autoHideScrollbar = true;
            m_buildingSelection.relativePosition = Vector3.zero;
            m_buildingSelection.rowsData = new FastList<object>();
            m_buildingSelection.selectedIndex = -1;
         

            // Building Options
            m_buildingOptions = right.AddUIComponent<UIBuildingOptions>();
            m_buildingOptions.width = RIGHT_WIDTH;
            m_buildingOptions.height = right.height - 40;
            m_buildingOptions.relativePosition = Vector3.zero;

            try
            {
                m_buildingSelection.rowsData = GenerateFastList();
            }
            catch { }
        
        }

        public void UpdateBuildingInfo(BuildingData building) {

            //Debug.Log(building.name);

            currentSelection = XMLManager.xmlData[building.prefab];
            m_buildingOptions.SelectionChanged(currentSelection);
            m_savePanel.SelectionChanged(currentSelection);
            m_buildingPreview.Show(currentSelection);
        }

        public void Save() {
            m_buildingOptions.SaveRICO();

        }

        private FastList<object> Filter()
        {
            Debug.Log("Filtered Called");
            List<BuildingData> list = new List<BuildingData>();
            foreach (var bData in XMLManager.xmlData.Values)
            {
                if (bData != null)
                {
                    //var prefab = PrefabCollection<BuildingInfo>.GetLoaded(id.id);
                    list.Add(bData);
                    Debug.Log(bData.name);
                }
            }

            List<BuildingData> filtered = new List<BuildingData>();
            for (int i = 0; i < list.Count; i++)
            {
                BuildingData item = (BuildingData)list[i];

               
                // zone
                if (!m_filter.IsAllZoneSelected())
                {
                    Category category = item.category;
                    if (category == Category.None || !m_filter.IsZoneSelected(category)) continue;
                }

                // Name
                // if (!m_filter.buildingName.IsNullOrWhiteSpace() && !item.name.ToLower().Contains(m_filter.buildingName.ToLower())) continue;

                Debug.Log(item.category + " " + item.displayName);

                filtered.Add(item);
            }

            list = filtered;

            FastList<object> fastList = new FastList<object>();
            fastList.m_buffer = list.ToArray();
            fastList.m_size = list.Count;

            return fastList;
        }

        private FastList<object> GenerateFastList() {

            List<BuildingData> list = new List<BuildingData>();

            foreach (var bData in XMLManager.xmlData.Values)
            {
                if (bData != null)
                {
                    //var prefab = PrefabCollection<BuildingInfo>.GetLoaded(id.id);
                    list.Add(bData);
                    Debug.Log(bData.name);
                }
            }

            FastList<object> fastList = new FastList<object>();
            fastList.m_buffer = list.ToArray();
            fastList.m_size = list.Count;
            return fastList;
        }
    }
}
