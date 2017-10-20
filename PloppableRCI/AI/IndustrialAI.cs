using System;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;

namespace PloppableRICO
{
    public class PloppableIndustrial : IndustrialBuildingAI, PloppableRICO.IWorkplaceLevelCalculator
	{
		public int m_constructionCost = 1;
		public int m_workplaceCount = 1;
        public bool m_pollutionEnabled = true;
        public RICOBuilding m_ricoData;
        public int[] workplaceCount;

        // the important part
        public override int GetConstructionCost()
        {
            return PloppableRICO.WorkplaceAIHelper.GetConstructionCost(m_constructionCost, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
        }

        protected override int GetConstructionTime()
        {
            return 0;
        }

        public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {

            // For some reason CalculateWorkplaceCount gets called every two seconds or so for every rico building currently plopped.
            // It's the bottleneck of the mod.
            // So we cache the calculation results, which might save a few cpu cycles.
            if (workplaceCount != null)
                PloppableRICO.WorkplaceAIHelper.SetWorkplaceLevels(out level0, out level1, out level2, out level3, workplaceCount);
            else
            {
                PloppableRICO.WorkplaceAIHelper.CalculateWorkplaceCount(m_ricoData, this, r, width, length, out level0, out level1, out level2, out level3);
                workplaceCount = new int[] { level0, level1, level2, level3 };
            }
        }

        public void CalculateBaseWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            base.CalculateWorkplaceCount(r, width, length, out level0, out level1, out level2, out level3); ;
        }

        public override void GetPollutionRates(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            groundPollution = 0;
            noisePollution = 0;

            if (!m_pollutionEnabled)
                return;

            base.GetPollutionRates(productionRate, cityPlanningPolicies, out groundPollution, out noisePollution);
        }

        //// PlayerBuildingAI
        //public override bool GetFireParameters( ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance )
        //{
        //    Util.TRACE( " **** FIRE ****" );
        //    fireHazard = m_ricoData.fireHazard;
        //    fireSize = m_ricoData.fireSize;
        //    fireTolerance = m_ricoData.fireTolerance;
        //    return m_ricoData.fireHazard != 0;
        //}

        // I'd really love to refactor this boilerplate out, but meh, no multiple inheritance nor mixins
        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            PloppableRICO.Util.buildingFlags(ref buildingData);

            base.SimulationStep(buildingID, ref buildingData, ref frameData);

            PloppableRICO.Util.buildingFlags(ref buildingData);

        }

        public override bool CheckUnlocking()
        {
            return true;
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            PloppableRICO.Util.buildingFlags(ref buildingData);

            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);

            PloppableRICO.Util.buildingFlags(ref buildingData);

        }

        public override bool ClearOccupiedZoning()
        {
            return true;
        }

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

        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return base.m_info.GetUncheckedLocalizedTitle();
        }
        
        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {

            return null; //this will cause a check to fail in CheckBuildingLevel, and prevent the building form leveling. 
        }
    }
}