using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Steamworks;

namespace PloppableRICO
{
    public class UIBuildingItem : UIPanel, IUIFastListRow
    {
        private UILabel m_name;

        private UISprite m_author;
        private UISprite m_local;

        private UILabel m_level;
        private UILabel nameLabel;
        private UIPanel m_background;

        private BuildingData m_building;
        private BuildingData m_building2;

        public UIPanel background
        {
            get
            {
                if (m_background == null)
                {
                    m_background = AddUIComponent<UIPanel>();
                    m_background.width = width;
                    m_background.height = 40;
                    m_background.relativePosition = Vector2.zero;

                    m_background.zOrder = 0;
                }

                return m_background;
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_name == null) return;

            background.width = width;
            m_name.relativePosition = new Vector3(10f, 5f);
            m_author.relativePosition = new Vector3(200f, 10f);
            m_local.relativePosition = new Vector3(240f, 10f);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            if (enabled) RICOSettingsPanel.instance.UpdateBuildingInfo(m_building);

        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);

        }

        private void SetupControls()
        {
            if (m_name != null) return;

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
            height = 40;

            m_name = AddUIComponent<UILabel>();
            m_name.width = 200;
            //nameLabel.textAlignment = UIHorizontalAlignment.Center;
            m_author = AddUIComponent<UISprite>();
            m_author.size = new Vector2(20, 20);
            m_author.spriteName = "AchievementCheckedFalse";

            m_local = AddUIComponent<UISprite>();
            m_local.size = new Vector2(20, 20);
            m_local.spriteName = "AchievementCheckedFalse";
        }

        #region IUIFastListRow implementation

        public void Display(object data, bool isRowOdd)
        {
            SetupControls();

            m_building = data as BuildingData;

            m_building2 = XMLManager.xmlData[(int)m_building.id];

            m_name.text = m_building.displayName;

            //m_name.text = XMLManager.xmlData[m_building.id].displayName;

            float maxLabelWidth = width - 120;

            if (m_building.hasAuthor) m_author.spriteName = "AchievementCheckedTrue";
            else m_author.spriteName = "AchievementCheckedFalse";

            if (isRowOdd)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                background.backgroundSprite = null;
            }
        }

        public void Select(bool isRowOdd)
        {
            background.backgroundSprite = "ListItemHighlight";
            background.color = new Color32(255, 255, 255, 255);
            if (enabled) RICOSettingsPanel.instance.UpdateBuildingInfo(m_building);
        }

        public void Deselect(bool isRowOdd)
        {

            if (isRowOdd)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                background.backgroundSprite = null;
            }
        }
        #endregion
    }
}

