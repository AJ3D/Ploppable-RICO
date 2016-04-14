using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace PloppableRICO
{
    public class EnableRICOPanel : UIScrollablePanel
    {

        public BuildingData currentSelection;

        public UIButton save;
        public UIButton addLocal;
        public UIButton removeLocal;
        public UIButton reset;

        private static EnableRICOPanel _instance;
        public static EnableRICOPanel instance

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
            save = UIUtils.CreateButton(this);
            save.text = "Save";

            save.eventClick += (c, state) =>
            {
                XMLManager.SaveLocal(currentSelection);

            };
        

            reset = UIUtils.CreateButton(this);
            reset.text = "Reset";

        }
    }
}