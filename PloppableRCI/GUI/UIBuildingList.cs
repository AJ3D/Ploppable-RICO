using UnityEngine;
using ColossalFramework.UI;


namespace PloppableRICO
{
    public class UIBuildingItem : UIPanel, IUIFastListRow
    {
        private UILabel m_name;
        private UISprite m_mod;
        private UISprite m_author;
        private UISprite m_local;
        private UIPanel m_background;
        private BuildingData m_building;
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

            m_mod.relativePosition = new Vector3(280f, 10f);
            m_author.relativePosition = new Vector3(310f, 10f);
            m_local.relativePosition = new Vector3(340f, 10f);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            //This will update the panel when you hover over a fast list entry. 
            //if (enabled) RICOSettingsPanel.instance.UpdateBuildingInfo(m_building);
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            if (enabled) RICOSettingsPanel.instance.UpdateBuildingInfo(m_building);
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

            //Check boxes that indicate what settings are present. 

            m_mod = AddUIComponent<UISprite>();
            m_mod.size = new Vector2(20, 20);
            m_mod.spriteName = "AchievementCheckedFalse";

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

            m_name.text = m_building.displayName;

            float maxLabelWidth = width - 120;

            if (m_building.hasAuthor) m_author.spriteName = "AchievementCheckedTrue";
            else m_author.spriteName = "AchievementCheckedFalse";

            if (m_building.hasLocal) m_local.spriteName = "AchievementCheckedTrue";
            else m_local.spriteName = "AchievementCheckedFalse";

            if (m_building.hasMod) m_mod.spriteName = "AchievementCheckedTrue";
            else m_mod.spriteName = "AchievementCheckedFalse";

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

