using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using UnityEngine;
using ICities;

namespace PloppableRICO
{

	public class PloppableExtractor : IndustrialExtractorAI

	{
        public bool m_pollutionEnabled = true;
        public int m_constructionCost = 1;
        public int m_workplaceCount = 1;

		public override void GetWidthRange (out int minWidth, out int maxWidth)
		{
			base.GetWidthRange (out minWidth, out maxWidth);
			minWidth = 1;
			maxWidth = 16;
		}

        public override string GenerateName(ushort buildingID, InstanceID caller)
		{
			return base.m_info.GetUncheckedLocalizedTitle ();
		}

		public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data){

			return null; //this will cause a check to fail in CheckBuildingLevel, and prevent the building form leveling. 
		}

        public override void GetLengthRange (out int minLength, out int maxLength)
		{
			base.GetLengthRange (out minLength, out maxLength);
			minLength = 1;
			maxLength = 16;
		}

		public override int GetConstructionCost()
		{
			int result = (m_constructionCost * 100);
			Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref result, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
			return result;
		}

		public override bool ClearOccupiedZoning ()
		{
			return true;
		}

        public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {

            if (Util.IsModEnabled(426163185ul))
            {
                base.CalculateWorkplaceCount(r, width, length, out level0, out level1, out level2, out level3);
            }
            else {
                int widths = m_workplaceCount;

                ItemClass @class = this.m_info.m_class;
                int num = 0;
                level0 = 0;
                level1 = 0;
                level2 = 0;
                level3 = 0;
                if (@class.m_subService == ItemClass.SubService.IndustrialGeneric)
                {
                    if (@class.m_level == ItemClass.Level.Level1)
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
                }
                else if (@class.m_subService == ItemClass.SubService.IndustrialFarming)
                {
                    num = 100;
                    level0 = 100;
                    level1 = 0;
                    level2 = 0;
                    level3 = 0;
                }
                else if (@class.m_subService == ItemClass.SubService.IndustrialForestry)
                {
                    num = 100;
                    level0 = 100;
                    level1 = 0;
                    level2 = 0;
                    level3 = 0;
                }
                else if (@class.m_subService == ItemClass.SubService.IndustrialOre)
                {
                    num = 100;
                    level0 = 20;
                    level1 = 60;
                    level2 = 20;
                    level3 = 0;
                }
                else if (@class.m_subService == ItemClass.SubService.IndustrialOil)
                {
                    num = 100;
                    level0 = 20;
                    level1 = 60;
                    level2 = 20;
                    level3 = 0;
                }
                if (num != 0)
                {
                    num = Mathf.Max(200, widths * 1 * num + r.Int32(100u)) / 100;
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

        public override void SimulationStep (ushort buildingID, ref Building buildingData, ref Building.Frame frameData)

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

		protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData){

            Util.buildingFlags(ref buildingData);

            base.SimulationStepActive(buildingID, ref  buildingData, ref frameData);

            Util.buildingFlags(ref buildingData);

		}

        public override void GetPollutionRates(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            groundPollution = 0;
            noisePollution = 0;

            if (!m_pollutionEnabled)
            {
                groundPollution = 0;
                noisePollution = 0;
            }
            else {
                base.GetPollutionRates(productionRate, cityPlanningPolicies, out groundPollution, out noisePollution);
            }
        }

    }
}