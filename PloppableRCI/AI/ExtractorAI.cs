using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace PloppableRICO
{
    public class PloppableExtractor : IndustrialExtractorAI, IWorkplaceLevelCalculator
    {
        public bool m_pollutionEnabled = true;
        public int m_constructionCost = 1;
        public int m_workplaceCount = 1;
        public RICOBuilding m_ricoData;
        public int[] workplaceCount;

        // In this house, jobs get done
        public override int GetConstructionCost()
        {
            return WorkplaceAIHelper.GetConstructionCost(m_constructionCost, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
        }

        protected override int GetConstructionTime()
        {
            return 0;
        }

        public override void CalculateWorkplaceCount(ItemClass.Level level, ColossalFramework.Math.Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            // See IndustrialAI.cs
            if (workplaceCount != null)
                WorkplaceAIHelper.SetWorkplaceLevels(out level0, out level1, out level2, out level3, workplaceCount);
            else
            {
                WorkplaceAIHelper.CalculateWorkplaceCount(level, m_ricoData, this, r, width, length, out level0, out level1, out level2, out level3);
                workplaceCount = new int[] { level0, level1, level2, level3 };
            }
        }

        public void CalculateBaseWorkplaceCount(ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            base.CalculateWorkplaceCount(level, r, width, length, out level0, out level1, out level2, out level3); ;
        }

        public override void GetPollutionRates(ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            groundPollution = 0;
            noisePollution = 0;

            if (!m_pollutionEnabled)
                return;

            base.GetPollutionRates(level, productionRate, cityPlanningPolicies, out groundPollution, out noisePollution);
        }

        public override bool CheckUnlocking()
        {
            return true;
        }

        // Not much to see from here on
        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            Util.buildingFlags(ref buildingData);

            if ((buildingData.m_flags & Building.Flags.Completed) == Building.Flags.None)
            {
                bool flag = (buildingData.m_flags & Building.Flags.Upgrading) != Building.Flags.None;
                int constructionTime = this.GetConstructionTime();
                if (constructionTime == 0)
                {
                    frameData.m_constructState = 255;
                }
                else
                {
                    frameData.m_constructState = (byte)Mathf.Min(255, (int)frameData.m_constructState + 1088 / constructionTime);
                }
                if (frameData.m_constructState == 255)
                {
                    this.BuildingCompleted(buildingID, ref buildingData);
                    GuideController properties3 = Singleton<GuideManager>.instance.m_properties;
                    if (properties3 != null)
                    {
                        Singleton<BuildingManager>.instance.m_buildingLevelUp.Deactivate(buildingID, true);
                    }
                }
                else if (flag)
                {
                    GuideController properties4 = Singleton<GuideManager>.instance.m_properties;
                    if (properties4 != null)
                    {
                        Singleton<BuildingManager>.instance.m_buildingLevelUp.Activate(properties4.m_buildingLevelUp, buildingID);
                    }
                }
                if (flag)
                {
                    this.SimulationStepActive(buildingID, ref buildingData, ref frameData);
                }
            }
            else
            {
                this.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            }

            Util.buildingFlags(ref buildingData);
        }

    
    protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {

            Util.buildingFlags(ref buildingData);

            base.SimulationStepActive(buildingID, ref  buildingData, ref frameData);

            Util.buildingFlags(ref buildingData);

        }

        public override bool ClearOccupiedZoning()
        {
            return true;
        }

        public override void GetWidthRange(out int minWidth, out int maxWidth)
        {
            base.GetWidthRange(out minWidth, out maxWidth);
            minWidth = 1;
            maxWidth = 16;
        }

        public override void GetLengthRange(out int minLength, out int maxLength)
        {
            base.GetLengthRange(out minLength, out maxLength);
            minLength = 1;
            maxLength = 16;
        }
       
        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return base.m_info.GetUncheckedLocalizedTitle();
        }

        /*
        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {

            return null; //this will cause a check to fail in CheckBuildingLevel, and prevent the building form leveling. 
        }
        */
    }
}