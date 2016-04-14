using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

    public class PloppableCommercial : CommercialBuildingAI
    {
        public int m_workplaceCount = 1;
        public int m_constructionCost = 1;
        public string m_subtype = "low";

        public override void GetWidthRange(out int minWidth, out int maxWidth)
        {
            minWidth = 1;
            maxWidth = 16;
        }

        public override void GetLengthRange(out int minLength, out int maxLength)
        {
            minLength = 1;
            maxLength = 16;
        }

        public override bool ClearOccupiedZoning()
        {
            return true;
        }

        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return base.m_info.GetUncheckedLocalizedTitle();
        }

        public override int GetConstructionCost()
        {
            int result = (m_constructionCost * 100);
            Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
            return result;
        }

        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {

            return null; //this will cause a check to fail in CheckBuildingLevel, and prevent the building form leveling. 
        }

        public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {

            if (Util.IsModEnabled(426163185ul))
            {
                base.CalculateWorkplaceCount(r, width, length, out level0, out level1, out level2, out level3);
            }
           else {

                int widths = m_workplaceCount;
                int lengths = 1;
               //fffdd

                ItemClass @class = this.m_info.m_class;
                int num = 0;
                level0 = 100;
                level1 = 0;
                level2 = 0;
                level3 = 0;
                ItemClass.SubService subService = @class.m_subService;
                if (subService != ItemClass.SubService.CommercialLow)
                {
                    if (subService != ItemClass.SubService.CommercialHigh)
                    {
                        if (subService != ItemClass.SubService.CommercialLeisure)
                        {
                            if (subService == ItemClass.SubService.CommercialTourist)
                            {
                                num = 100;
                                level0 = 20;
                                level1 = 20;
                                level2 = 30;
                                level3 = 30;
                            }
                        }
                        else
                        {
                            num = 100;
                            level0 = 30;
                            level1 = 30;
                            level2 = 20;
                            level3 = 20;
                        }
                    }
                    else if (@class.m_level == ItemClass.Level.Level1)
                    {
                        num = 100;
                        level0 = 0;
                        level1 = 40;
                        level2 = 50;
                        level3 = 10;
                    }
                    else if (@class.m_level == ItemClass.Level.Level2)
                    {
                        num = 100;
                        level0 = 0;
                        level1 = 20;
                        level2 = 50;
                        level3 = 30;
                    }
                    else
                    {
                        num = 100;
                        level0 = 0;
                        level1 = 0;
                        level2 = 40;
                        level3 = 60;
                    }
                }
                else if (@class.m_level == ItemClass.Level.Level1)
                {
                    num = 100;
                    level0 = 100;
                    level1 = 0;
                    level2 = 0;
                    level3 = 0;
                }
                else if (@class.m_level == ItemClass.Level.Level2)
                {
                    num = 100;
                    level0 = 20;
                    level1 = 60;
                    level2 = 20;
                    level3 = 0;
                }
                else
                {
                    num = 100;
                    level0 = 5;
                    level1 = 15;
                    level2 = 30;
                    level3 = 50;
                }
                if (num != 0)
                {
                    num = Mathf.Max(200, widths * lengths * num + r.Int32(100u)) / 100;
                    int num2 = level0 + level1 + level2 + level3;
                    if (num2 != 0)
                    {
                        level0 = (num * level0 + r.Int32((uint)num2)) / num2;
                        num -= level0;
                    }
                    num2 = level1 + level2 + level3;
                    if (num2 != 0)
                    {
                        level1 = (num * level1 + r.Int32((uint)num2)) / num2;
                        num -= level1;
                    }
                    num2 = level2 + level3;
                    if (num2 != 0)
                    {
                        level2 = (num * level2 + r.Int32((uint)num2)) / num2;
                        num -= level2;
                    }
                    level3 = num;
                }
            }
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {

            Util.buildingFlags(ref buildingData);

            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);

            Util.buildingFlags(ref buildingData);

        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)

        {

            Util.buildingFlags(ref buildingData);

            base.SimulationStep(buildingID, ref buildingData, ref frameData);

            Util.buildingFlags(ref buildingData);

        }
    }
}